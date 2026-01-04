#!/usr/bin/env bash

if [ "$EUID" -eq 0 ]
  then echo "This script must not be run as root"
  exit
fi

# Update system
echo "## Updating system ##"
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

# Reset any changes so there's no issues checking out a new version
git reset --hard

# Get latest tag name
latestTag=$(git describe --tags "$(git rev-list --tags --max-count=1)")

# Checkout latest tag
git checkout -q $latestTag

# Stop service
if [ -f /etc/systemd/system/jrencoder.service ]; then
  echo "## Stopping service ##"
  sudo systemctl stop jrencoder.service
fi

# Build project
# Must be built as debug, release does not work for some reason
echo "## Building project ##"
mkdir -p $HOME/jrencoder
$HOME/.dotnet/dotnet build --nologo --configuration Debug --property:OutputPath=$HOME/jrencoder/ -property:WarningLevel=0 $HOME/wsjr-data-encoder/JrEncoder.sln

# Check if systemd service exists already
if [ ! -f /etc/systemd/system/jrencoder.service ]; then
  echo "## Installing service ##"
  first_install=true
else
  echo "## Updating service ##"
fi

# Create/update systemd service
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
WorkingDirectory=$HOME/jrencoder/

[Install]
WantedBy=multi-user.target
EOL

# Reload systemctl
sudo systemctl daemon-reload

# Enable at boot
sudo systemctl enable jrencoder.service

# Get wifi IP address
ip=$(ip -4 addr show wlan0 | grep -oP '(?<=inet\s)\d+(\.\d+){3}')

# Check if the service was just installed for the first time
if [ "$first_install" = true ] ; then

  # Service was just installed, a reboot is required
  printf "\n\nInstall complete!\n"
  echo "Reboot your pi to start the program\n"
  echo "The configuration UI will be available at http://$ip:5000"

else

  echo "## Starting service ##"
  sudo systemctl start jrencoder.service

  printf "\n\nInstall complete!\n"
  echo "The configuration UI is available at http://$ip:5000"

fi


