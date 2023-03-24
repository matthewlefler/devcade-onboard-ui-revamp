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

cp /home/devcade/devcade-onboard/idiot/.env /home/devcade; 

# Set up xinitrc
cat << EOF > ~/.xinitrc;
#!/bin/sh

function configure_display() {
	xrandr -q >> ~/onboard.log 2>&1
	DEVCADE_OUTPUT=$(xrandr -q | grep ' connected ' | awk '{print $1}')
	echo "using output: $DEVCADE_OUTPUT" >> ~/onboard.log 2>&1
	#DEVCADE_OUTPUT=DP-2

	xrandr --newmode "2560x1080_75.00" 294.00 2560 2744 3016 3472 1080 1083 1093 1130 -hsync +vsync --verbose
	xrandr --addmode $DEVCADE_OUTPUT 2560x1080_75.00 --verbose
	xrandr --output $DEVCADE_OUTPUT --mode 2560x1080_75.00 --rotate left  --verbose
}

configure_display

source ~/.env
openbox & compton & pulseaudio &
~/publish/onboard >> ~/onboard.log 2>&1
EOF

# Auto-login
sudo mkdir -p /etc/systemd/system/getty@tty1.service.d/
cat << EOF > /etc/systemd/system/getty@tty1.service.d/override.conf
[Service]
Type=Simple
ExecStart=
ExecStart=-/sbin/agetty --autologin devcade --noclear %I 38400 linux
EOF

until ! grep 'DEVCADE_AUTOLOGIN_INSTALLED' ; do {
cat << EOF >> /home/devcade/.bashrc
DEVCADE_AUTOLOGIN_INSTALLED=1
if [[-z “$DISPLAY” ]] && [[ $(tty) = /dev/tty1 ]]; then
    . startx
    logout 
fi 
EOF
}

sudo systemctl enable getty@tty1.service.d
