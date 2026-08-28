# podman run -p 2200:22 -it devcade-onboard:latest /bin/bash
# sudo xhost +local:podman

touch .env_var

echo "" > .env_var
echo "WAYLAND_DISPLAY=$WAYLAND_DISPLAY" >> .env_var
echo "XDG_RUNTIME_DIR=$XDG_RUNTIME_DIR" >> .env_var
echo "DISPLAY=$DISPLAY" >> .env_var

podman run -it --rm \
  -v $XDG_RUNTIME_DIR/$WAYLAND_DISPLAY:$XDG_RUNTIME_DIR/$WAYLAND_DISPLAY:rw \
  -v /tmp/.X11-unix:/tmp/.X11-unix:rw \
  -v .env_var:/tmp/.env_var:ro \
  dcu-devcade-onboard:latest-test

rm -f .env_var
