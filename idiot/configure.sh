#!/bin/bash

set -e

# Install Dotnet SDK
cd /home/devcade
wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update && \
  apt-get install -y dotnet-sdk-6.0

# Set up Onboard
cd devcade-onboard;

# Build onboard
cd onboard
dotnet publish -c release -r linux-x64 --self-contained
mv bin/Release/net6.0/linux-x64/publish /home/devcade
cd ..

# TODO: Set up publish.sh? IDK man it should work fine as is.

cp idiot/.env /home/devcade; 
cp idiot/xinitrc ~/.xinitrc;
git submodule update --init --recursive;
cd idiot/xlogin;
make install;
systemctl enable xlogin@devcade;

