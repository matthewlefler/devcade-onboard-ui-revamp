#!/bin/sh

# /etc/X11/xinit/xinitrc
#
# global xinitrc file, used by all X sessions started by xinit (startx)

# invoke global X session script
#. /etc/X11/Xsession

echo "\n==========Making new x mode:==========\n" > onboard.log
#xrandr --newmode "1080x2560_75.00" 299.75 1080 1176 1288 1496 2560 2563 2573 2674 -hsync +vsync | tee -a onboard.log
xrandr --newmode "2560x1080_75.00" 294.00 2560 2744 3016 3472 1080 1083 1093 1130 -hsync +vsync --verbose | tee -a onboard.log
xrandr | tee -a onboard.log

echo "\n==========Adding mode to hdmi:==========\n" | tee -a onboard.log
#xrandr --addmode HDMI-2 1080x2560_75.00 | tee -a onboard.log
xrandr --addmode HDMI-2 2560x1080_75.00 --verbose | tee -a onboard.log
xrandr | tee -a onboard.log

echo "\n==========Setting output:==========\n" | tee -a onboard.log
#xrandr --output HDMI-2 --mode 1080x2560 --rate 75.00 | tee -a onboard.log
xrandr --output HDMI-2 --mode 2560x1080_75.00 --rotate right  --verbose | tee -a onboard.log
xrandr | tee -a onboard.log

echo "\nChecking input devices:\n"
xinput | tee -a onboard.log

#echo "\nxrandr output:\n"
#xrandr | tee -a onboard.log

source /home/devcade/.env

openbox &
compton &

echo "\nonboard log:\n"
/home/devcade/publish/onboard | tee -a /home/devcade/onboard.log
