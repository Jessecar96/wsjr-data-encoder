#!/usr/bin/env bash

if [ "$EUID" -eq 0 ]
  then echo "This script must not be run as root"
  exit
fi

# Update OS
echo "## Updating OS ##"
sudo apt-get update
sudo apt-get upgrade -yq

# Install git
echo "## Installing git ##"
sudo apt-get install -yq git

# Check if .dotnet folder exists
echo "## Checking for .NET ##"
if ! command -v dotnet 2>&1 >/dev/null
then

  # Install .NET LTS
  echo "## Installing .NET ##"
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel LTS

  # Add to PATH
  # shellcheck disable=SC2016
  echo 'export DOTNET_ROOT=$HOME/.dotnet' >> $HOME/.bashrc
  # shellcheck disable=SC2016
  echo 'export PATH=$PATH:$HOME/.dotnet' >> $HOME/.bashrc
  source "$HOME/.bashrc"

fi

# Enable SPI, this requires reboot
echo "## Enabling SPI ##"
sudo sed -i '/spi=on/s/^#//g' /boot/firmware/config.txt

# git clone project
if [ ! -d $HOME/wsjr-data-encoder ]; then
  echo "## Cloning repo ##"
  cd $HOME
  git clone https://github.com/Jessecar96/wsjr-data-encoder.git $HOME/wsjr-data-encoder
  cd $HOME/wsjr-data-encoder
fi

# Get new tags from remote
echo "## Checking out latest version ##"
cd $HOME/wsjr-data-encoder
git fetch --tags

# Get latest tag name
latestTag=$(git describe --tags "$(git rev-list --tags --max-count=1)")

# Checkout latest tag
git checkout -q $latestTag

# Build project
# Must be built as debug, release does not work for some reason
echo "## Building project ##"
mkdir -p $HOME/jrencoder
dotnet build --nologo --configuration Debug --property:OutputPath=$HOME/jrencoder/ -property:WarningLevel=0 $HOME/wsjr-data-encoder/JrEncoder.sln

# Create systemd service
echo "## Installing service ##"
cat << EOL | sudo tee /etc/systemd/system/jrencoder.service
[Unit]
Description=Weather Star Data Encoder
After=network.target

[Service]
Type=simple
Restart=always
RestartSec=1
User=${USER}
ExecStart=$HOME/.dotnet/dotnet $HOME/jrencoder/JrEncoder.dll

[Install]
WantedBy=multi-user.target
EOL

# Reload systemctl
sudo systemctl daemon-reload

# Enable at boot
sudo systemctl enable jrencoder.service

# Generate config file
echo "## Generating config file ##"
cd $HOME/jrencoder
$HOME/.dotnet/dotnet $HOME/jrencoder/JrEncoder.dll --create-config

printf "\n\nInstall complete!\n"
echo "config.json file was created in $HOME/jrencoder/"
echo "Edit your config.json file using nano/vi/vim then reboot your pi to start the program"
