DEVCADE_AUTOLOGIN_INSTALLED=1
if [[-z “$DISPLAY” ]] && [[ $(tty) = /dev/tty1 ]]; then
    . startx
    logout 
fi
