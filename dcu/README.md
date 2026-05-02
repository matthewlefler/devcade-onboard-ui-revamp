Documentation can be found here: https://devcade-docs.csh.rit.edu/#/Hardware/installing-dcu

# bootc containerization
see https://docs.fedoraproject.org/en-US/bootc/getting-started/   
for image building tool documentation https://github.com/osbuild/bootc-image-builder   
**bootc** is a tool for creating bootable containers, basically meaning putting a container on top of a kernel.   
In this case Fedora is used as a base image, and the devcade-onboard is built on top of it using a Dockerfile based container. 
## Installing
For most use cases this is unnecessary, and you can skip this
### For official installation documentation see:
see https://bootc.dev/bootc/packaging-and-integration.html
### Arch:

First try:
```bash
paru bootc
```

If that fails build the program manually:
```bash
git clone https://github.com/bootc-dev/bootc.git # clone the git repository
cd bootc
sudo make install # build and install the program
```
Then install dependencies
```
bootupd, skopo
```
## Installing to a partition
First build the container as always:
```bash
sudo ./build.sh
```
To install to a mounted partition 
```bash
sudo bootc install to-filesystem /path/to/mounted/partition --source-imgref containers-storage:localhost/dcu-devcade-onboard:latest
```
If you want a boot-loader to be created, mount your efi partition to /boot/efi in the mounted partition:   
*Note:* this creates new boot entries that can be selected in your **BIOS** and boots using grub
```bash
mount /dev/<efi-partition-name> /path/to/mounted/partition/boot/efi
```
### Issues
If issues arise *un-mount* the partition, reformat it to `sudo mkfs -t ext4 /dev/<partition-name>`, mount it and try again   
*Note:* bootc will mount the partition to an internal file path, in my case `/run/bootc/storage`, use `findmnt | grep <partition-name>` to find it otherwise
# Testing
## Using podman
Refer to: https://github.com/containers/podman/blob/main/docs/tutorials/podman_tutorial.md

*Quick note:* these images are around 5-6 GiB in size at the moment. 
To remove unused images run `podman image purge`. 
The reason why this command is not run automatically is that it could remove images that the user wants to keep unintentionally.
## Testing the container
For a quick way to test things like: 
- file structure
- file ownership
- package installation
- Dockerfile correctness
build and run the container locally
### Using predefined scripts
First build the container with:
```bash
./build.sh
```
Then run it with:
```bash
./run.sh
```
### Manually
First build the container with:
```bash
podman build --build-arg-file=./.env -t dcu-devcade-onboard:latest .
```
Then run it with:
```bash
podman run -p 2200:22 -it dcu-devcade-onboard:latest /bin/bash
```
### SSH
to SSH into the container run:
```bash
ssh -p 2200 devcade@localhost
```
to ssh while forwarding the display server
```bash
ssh -p 2200 -X devcade@localhost
```
### ISO
The only true way to test the container is to boot is though "real" hardware, e.g. a virtual machine.
#### Requirements
- compatible virtual machine software
	- all it needs to be able to do is boot from an **ISO** file
- podman
- sudo
- an internet connection
- 10-15 minutes (may take longer or shorter depending on hardware)
#### Creation
```bash
./create-iso.sh
```
if prompted for your password enter it, the container to create the ISO requires elevated privileges, and by extension building the dcu-devcade-onboard image requires it. *Note: that copying the container to the root user is possible but it was found to be slower than just having the root user build it.*   
#### Virtual Machine
##### Setup
After the script finishes an ISO file will exist as `./output/bootiso/install.iso`. In this example i will use `virtmanager` but other virtual machine software should work. 
1. Start `virtmanager`, click on **create a new virtual machine** in the top left (the computer icon with the star). This will open a pop-up window. 
2. Next select **local media install** and click **Forward**. 
3. Then click **browse**, if the path `/path/to/git/repo/dcu/output/bootiso/install.iso` is there you can select it, if not click **Browse Local** and find it there. 
4. Then click **Choose Volume** and if it does not detect the operating system uncheck **Automatically detect from the installation media / source** and manually select it from the selections, currently this is **Fedora 43** but check to make sure
5. Then click **Forward** again and select the number of CPUs and memory to dedicate to this VM, i find the default 2 and 4 GiB work for me, but higher numbers will make it faster.
6. Click **Forward** again and allocate storage to the machine, again the defaults have been fine to me, but if run out of storage, you can increase the number of GiB allocated.
7. Click **Forward** again and give it a name, network selection is also possible here, but the default is fine again. 
8. Click **Finish** to create the new virtual machine
At this point the machine will start booting, this does take some time to do so, so take a break and scroll Reddit or something. 

After the machine finishes installing the container and booting, you should be in a WM where the onboard is running
##### Useful info
To **stop** the machine click the **power button icon** at the top of the virtual machine's output display, or shut down the machine from the command line. 
To **release the mouse** press `left alt + left ctrl`, or try `left ctrl + left alt + g`. 
##### Issues
If you get any issues, such as not being able to select an OS make sure the libvirtd service is started: `systemctl start libvirtd`
##### SSH 
Click on **view** then **details** and click on the sub category: **NIC ab:cd:ef:gh:ij:kl** and the IP address to use is listed as **IP address: 192.xxx.xxx.xxx** 
Then run:
```bash
ssh devcade@ip-address
```
If any issues arise, running in verbose mode will reveal more information:
```bash
ssh -v devcade@ip-address
```
