echo "remeber to build using sudo if you are going to run create-iso.sh" && \
podman build --build-arg-file=./.env -t dcu-devcade-onboard:latest . && \
podman build -f ./Dockerfile.nvidia -t dcu-devcade-onboard:latest-nvidia .
# builds the container