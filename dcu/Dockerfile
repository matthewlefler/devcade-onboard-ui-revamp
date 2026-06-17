FROM quay.io/fedora/fedora-bootc:43

# user and file systemd configs
COPY ./config-files/files.conf /usr/lib/tmpfiles.d/custom_files.conf
COPY ./config-files/users.conf /usr/lib/sysusers.d/custom_users.conf

# disable sleep/hiberate features
RUN systemctl mask sleep.target suspend.target hibernate.target hybrid-sleep.target

### required software
RUN dnf install -y                                                  \
    openbox picom awk scrot xterm pamixer xdg-desktop-portal-gtk    \
    gcc-c++ git wget unzip dotnet-sdk-10.0                          \
    grep openssh-server openssl-devel fail2ban                      \
    glib2-devel flatpak-devel libnfc-devel                          \
    make cmake                                                      \
    autoconf automake libtool                                       \
    xrandr @base-x

# install libfreefare-devel, as it is not in the package manager yet
RUN git clone https://github.com/nfc-tools/libfreefare.git &&   \
    cd libfreefare &&                                           \
    autoreconf -vis &&                                          \
    ./configure --prefix=/usr &&                                \
    make && make install &&                                     \
    cd .. && rm -r libfreefare

# required flatpak runtimes
RUN flatpak remote-add --if-not-exists flathub https://dl.flathub.org/repo/flathub.flatpakrepo &&   \
    flatpak install -y flathub org.freedesktop.Platform/x86_64/22.08

# root services config file
# ban people from ssh'ing after some number of attempts for some amount of time (see contents of file for specifics)
COPY ./config-files/sshd.local /etc/fail2ban/jail.d/sshd.local
# ssh server configuration file
COPY ./config-files/sshd_config /etc/ssh/sshd_config
RUN systemctl enable fail2ban; \
    systemctl enable sshd

# Rust
RUN curl https://sh.rustup.rs -sSf | bash -s -- -y 
ENV PATH="${HOME}/.cargo/bin:${PATH}"

# godot 4.4.1-stable mono
RUN wget "https://github.com/godotengine/godot/releases/download/4.4.1-stable/Godot_v4.4.1-stable_mono_linux_x86_64.zip" &&             \
    unzip Godot_v4.4.1-stable_mono_linux_x86_64.zip &&                                                                                  \
    mv Godot_v4.4.1-stable_mono_linux_x86_64/Godot_v4.4.1-stable_mono_linux.x86_64 /usr/bin/Godot_v4.4.1-stable_mono_linux.x86_64 &&    \
    mv Godot_v4.4.1-stable_mono_linux_x86_64/GodotSharp /usr/bin/GodotSharp;                                                            \
    rm Godot_v4.4.1-stable_mono_linux_x86_64.zip

# file required for xauth
RUN touch .Xauthority

### copy required files 
# NOTE: required symlinks for many global config files are created in dcu/config-files/files.conf

# openbox config file
COPY ./config-files/rc.xml /usr/openbox/etc/rc.xml

# x11 init file
COPY ./config-files/.xinitrc /usr/X11/xinit/etc/.xinitrc

# bash init file
COPY ./config-files/.bashrc /usr/bash/etc/.bashrc

# autologin service
COPY ./config-files/tty1_service_override.conf /etc/systemd/system/getty@tty1.service.d/override.conf

# required custom executables
COPY ./config-files/export-all ./config-files/onboard /usr/bin/

# export templates
COPY ./config-files/linux_debug.x86_64 ./config-files/linux_release.x86_64 /usr/share/godot/export_templates/4.4.1.stable.mono/
# symlink required for the export functionality of the godot cli, 
# I blame godot for not letting me change the install location of the export templates
RUN mkdir -p /root/.local/share/godot/export_templates && \
    ln -s -f /usr/share/godot/export_templates/4.4.1.stable.mono /root/.local/share/godot/export_templates/

# devcade-onboard git repository
ARG BRANCH_VAR
RUN git clone https://github.com/matthewlefler/devcade-onboard-ui-revamp.git && \
    cd devcade-onboard-ui-revamp && git switch ${BRANCH_VAR}

RUN cp ./devcade-onboard-ui-revamp/onboard/.env.template /home/devcade/.env && \
    chown devcade: /home/devcade/.env

# export-all <godot-path> <absolute src path> <absolute out path>
RUN export-all                              \
    Godot_v4.4.1-stable_mono_linux.x86_64   \
    /devcade-onboard-ui-revamp              \
    /usr/bin/ &&                            \
    rm -r /devcade-onboard-ui-revamp

# enable autologin
RUN systemctl enable getty@tty1

# niceity software
RUN dnf install -y                      \
    vim fastfetch btop lspci usbutils   \
    && dnf clean all

# make it known as a bootc container
LABEL containers.bootc=1
LABEL bootc.rootfs=/sysroot

RUN bootc container lint
