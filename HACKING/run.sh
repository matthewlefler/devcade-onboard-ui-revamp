# !/bin/bash

# export required enviorment variables
mkdir -p ./tmp
touch ./tmp/.env_var

echo "" > ./tmp/.env_var
echo "WAYLAND_DISPLAY=$WAYLAND_DISPLAY" >> ./tmp/.env_var
echo "XDG_RUNTIME_DIR=$XDG_RUNTIME_DIR" >> ./tmp/.env_var
echo "DISPLAY=$DISPLAY" >> ./tmp/.env_var

podman run -it --rm \
  -v $XDG_RUNTIME_DIR/$WAYLAND_DISPLAY:$XDG_RUNTIME_DIR/$WAYLAND_DISPLAY:rw \
  -v /tmp/.X11-unix:/tmp/.X11-unix:rw \
  -v ./tmp/.env_var:/tmp/.env_var:ro \
  -v ./test.env:/var/home/devcade/.env:rw \
  --net=host \
  --name dcu-devcade-onboard \
  localhost/dcu-devcade-onboard:latest-test

rm -f ./tmp/.env_var
