# builds the container
echo "building base image";
cd ../dcu && ./build.sh && cd ../HACKING && 
echo "building test image" &&
podman build -f ./Dockerfile.test -t dcu-devcade-onboard:latest-test .