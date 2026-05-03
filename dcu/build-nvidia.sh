podman build -f ./Dockerfile.nvidia --build-arg-file=./.env -t dcu-devcade-onboard:latest-nvidia .
# builds the container