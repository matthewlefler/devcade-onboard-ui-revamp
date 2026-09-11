# builds the container
GIT_ROOT_PATH=$(git rev-parse --show-toplevel)

# export project
mkdir -p $GIT_ROOT_PATH/HACKING/tmp/onboard
#"Usage:" $0 "<godot_executable> <path_to_root_folder> <out_path>" 
$GIT_ROOT_PATH/dcu/config-files/export-all godot-mono-4.7 $GIT_ROOT_PATH $GIT_ROOT_PATH/HACKING/tmp/onboard &&

echo "building test image" &&
podman build -f ./Dockerfile.test -t dcu-devcade-onboard:latest-test .
