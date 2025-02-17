#!/usr/bin/env bash

if [ "$EUID" -eq 0 ]
  then echo "This script must not be run as root"
  exit
fi

# Update OS
sudo apt-get update
sudo apt-get upgrade -y

# Install git
sudo apt-get install git

# Check if .dotnet folder exists
if [ ! -d $HOME/.dotnet ]; then

  # Install .NET LTS
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel LTS

  # Add to PATH
  echo 'export DOTNET_ROOT=$HOME/.dotnet' >> $HOME/.bashrc
  echo 'export PATH=$PATH:$HOME/.dotnet' >> $HOME/.bashrc
  source $HOME/.bashrc

fi

# Enable SPI, this requires reboot
sudo sed -i '/spi=on/s/^#//g' /boot/firmware/config.txt

# git clone project
cd $HOME
git clone https://github.com/Jessecar96/wsjr-data-encoder.git $HOME/wsjr-data-encoder
cd $HOME/wsjr-data-encoder

# Get new tags from remote
git fetch --tags

# Get latest tag name
latestTag=$(git describe --tags "$(git rev-list --tags --max-count=1)")

# Checkout latest tag
git checkout $latestTag

# Build project
# Must be built as debug, release does not work for some reason
mkdir -p $HOME/jrencoder
dotnet build --nologo --configuration Debug --property:OutputPath=$HOME/jrencoder/ -property:WarningLevel=0 $HOME/wsjr-data-encoder/JrEncoder.sln

# Create systemd service
cat << EOL | sudo tee /etc/systemd/system/jrencoder.service
[Unit]
Description=Weather Star Data Encoder
After=network.target

[Service]
Type=simple
Restart=always
RestartSec=1
User=${USER}
ExecStartPre=/bin/sleep 30
ExecStart=$HOME/.dotnet/dotnet $HOME/jrencoder/JrEncoder.dll

[Install]
WantedBy=multi-user.target
EOL

# Reload systemctl
sudo systemctl daemon-reload

# Enable at boot
sudo systemctl enable jrencoder.service

# Generate config file
cd $HOME/jrencoder
$HOME/.dotnet/dotnet $HOME/jrencoder/JrEncoder.dll --create-config

echo "Install complete!"
echo "config.json file was created in $HOME/jrencoder/"
echo "Edit your config.json file using nano/vi/vim then reboot your pi to start the program"