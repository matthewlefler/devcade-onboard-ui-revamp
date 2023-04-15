#!/bin/sh

configure_display() {
	xrandr -q >> ~/onboard.log 2>&1
	DEVCADE_OUTPUT=$(xrandr -q | grep ' connected ' | awk '{print $1}')
	echo "using output: $DEVCADE_OUTPUT" >> ~/onboard.log 2>&1

	xrandr --output $DEVCADE_OUTPUT --rotate left
}

. ~/.env
configure_display

openbox & compton & $(pulseaudio || echo "pulseaudio already started") &
~/publish/onboard >> ~/onboard.log 2>&1
