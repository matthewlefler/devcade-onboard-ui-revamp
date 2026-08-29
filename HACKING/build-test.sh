# builds the container
INIT_PATH=$(pwd)
GIT_ROOT_PATH=$(git rev-parse --show-toplevel)
cd $GIT_ROOT_PATH && echo "cd'ed to: " && pwd &&

# export project
mkdir -p $GIT_ROOT_PATH/HACKING/tmp/godot
#"Usage:" $0 "<godot_executable> <path_to_root_folder> <out_path>" 
$GIT_ROOT_PATH/dcu/config-files/export-all godot-mono-4.7 $GIT_ROOT_PATH $GIT_ROOT_PATH/HACKING/tmp/godot &&

echo "building test image" &&
podman build -f ./HACKING/Dockerfile.test -t dcu-devcade-onboard:latest-test .

cd $INIT_PATH