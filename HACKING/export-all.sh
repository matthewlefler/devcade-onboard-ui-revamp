#!/bin/bash

# first arg is --help or -h             or # of args != 3
if [[ $1 == --help ]] || [[ $1 == -h ]] || [[ $# -ne 3 ]]; then
    echo "Usage:" $0 "<godot_executable> <path_to_root_folder> <out_path>" 
    exit
fi

# backend
# cargo build the backend cause yeah

# godot frontend
$1 --path $2/onboard/godot-frontend --headless --export-release "Linux" $3/godot_frontend
# notification service
$1 --path $2/onboard/notifications-service --headless --export-release "Linux" $3/notification_service

exit