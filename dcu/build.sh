echo "remember to build using sudo if you are going to run create-iso.sh" && \
podman build --build-arg-file=./.env -t dcu-devcade-onboard:latest .
# builds the container