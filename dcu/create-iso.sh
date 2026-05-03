# only prompts for password once
sudo bash -c '
  podman build --network=host --build-arg-file=./.env -t dcu-devcade-onboard:latest . &&
  podman images &&
  echo -e "\n" &&
  mkdir -p output &&
  podman run --rm -it --privileged \
    -v /var/lib/containers/storage:/var/lib/containers/storage \
    -v $(pwd)/config.toml:/config.toml \
    -v $(pwd)/output:/output \
    quay.io/centos-bootc/bootc-image-builder:latest \
    --rootfs ext4 \
    --type iso \
    localhost/dcu-devcade-onboard:latest
'
