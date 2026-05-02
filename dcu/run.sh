# podman run -p 2200:22 -it devcade-onboard:latest /bin/bash
podman run --rm -it \
  --systemd=always \
  --privileged \
  -v /tmp/.X11-unix:/tmp/.X11-unix \
  -e DISPLAY=$DISPLAY \
  -v $HOME/.Xauthority:/home/devcade/.Xauthority \
  dcu-devcade-onboard