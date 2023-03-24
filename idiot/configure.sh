#!/bin/bash

set -e

# Install Dotnet SDK
cd /home/devcade
wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-6.0

# Set up Onboard
cd devcade-onboard;

# Build onboard
cd onboard
dotnet publish -c release -r linux-x64 --self-contained
rm -rf /home/devcade/publish
mv bin/Release/net6.0/linux-x64/publish /home/devcade
cd ..
idiot_dir="/home/devcade/devcade-onboard/idiot"

# Env file
cp $idiot_dir/.env /home/devcade; 

# Set up xinitrc
cp $idiot_dir/xinitrc /home/devcade/.xinitrc

# Auto-login (scuffed)
sudo mkdir -p /etc/systemd/system/getty@tty1.service.d/
cat << EOF > /etc/systemd/system/getty@tty1.service.d/override.conf
[Service]
Type=Simple
ExecStart=
ExecStart=-/sbin/agetty --autologin devcade --noclear %I 38400 linux
EOF

# Add a thingy to the bashrc
until ! grep 'DEVCADE_AUTOLOGIN_INSTALLED' /home/devcade/.bashrc  ; do {
  cat bashrc-check.sh >> /home/devcade/.bashrc
}

# Enable auto-login
sudo systemctl enable getty@tty1
