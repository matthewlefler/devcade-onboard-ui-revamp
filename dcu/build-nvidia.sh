echo "remember to build using sudo if you are going to run create-iso.sh" && \
podman build --network=host --build-arg-file=./.env -t dcu-devcade-onboard:latest . && \
podman build --network=host -f ./Dockerfile.nvidia -t dcu-devcade-onboard:latest-nvidia .
# builds the container
