# bootc container structure
Base image is `quay.io/fedora/fedora-bootc:43`  
A bootc image with the fedora 43 kernel
## Main programs
### devcade_onboard
This is the bash script that runs on startup of the `openbox` window manager,
it starts `devcade_backend` and `devcade_frontend` and kills the other if one of them dies then exits

It is stored at `/usr/bin/devcade_onboard`
### devcade_backend
The backend of the two executables, handles communication with the public API and the `devcade_frontend` and Games over unix sockets

It is stored at `/usr/bin/devcade_backend`
### devcade_frontend
The frontend of the two executables, the GUI that people interact with. Currently is a Godot based project

It is stored at `/usr/bin/devcade_frontend`

## Config files
| config file                 | machine path | 
|-----------------------------|--------------|
|`files.conf`                 | `/usr/lib/tmpfiles.d/custom_files.conf` |
|`users.conf`                 | `/usr/lib/sysusers.d/custom_users.conf` |
|`sshd.local`                 | `/etc/fail2ban/jail.d/sshd.local` |
|`sshd_config`                | `/etc/ssh/sshd_config` |
|`prepare-root.conf`          | `/usr/lib/ostree/prepare-root.conf` |
|`rc.xml`                     | `/etc/xdg/openbox/rc.xml` |
|`.xinitrc`                   | `/etc/X11/xinit/xinitrc` |
|`.bashrc`                    | `/etc/bashrc` |
|`tty1_service_override.conf` | `/etc/systemd/system/getty@tty1.service.d/override.conf` |

