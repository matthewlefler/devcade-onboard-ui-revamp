#!/bin/bash
set -e

cd /home/devcade/devcade-onboard/onboard

git pull origin main; # Update

# Build
dotnet publish -c release -r linux-x64 --self-contained
rm -rf /home/devcade/publish

# Install
mv bin/Release/net6.0/linux-x64/publish /home/devcade
