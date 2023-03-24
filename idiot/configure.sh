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

# TODO: Set up publish.sh? IDK man it should work fine as is.

cp /home/devcade/idiot/.env /home/devcade; 
cp /home/devcade/idiot/xinitrc ~/.xinitrc;
git submodule update --init --recursive;
cd /home/devcade/idiot/xlogin;
sudo make install;
sudo systemctl enable xlogin@devcade;

