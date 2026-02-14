#!/bin/bash

# first arg is --help or -h             or # of args != 3
if [[ $1 == --help ]] || [[ $1 == -h ]] || [[ $# -ne 3 ]]; then
    echo "Usage:" $0 "<godot_executable> <path_to_frontend_godot_project> <out_path_including_name>" 
    exit
fi

$1 --path $2 --headless --export-release "Linux" $3