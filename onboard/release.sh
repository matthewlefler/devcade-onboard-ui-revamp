#!/bin/bash
dotnet publish -c Release -r linux-x64 --no-self-contained && \
scp -r /Devcade-onboard/onboard/bin/Release/netcoreapp3.1/linux-x64/publish/ devcade@devcade.csh.rit.edu:~/onboard-new

