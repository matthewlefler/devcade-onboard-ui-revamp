FROM quay.io/fedora/fedora-bootc:latest

COPY ./config-files/sleep.conf /etc/systemd/sleep.conf

### User Creation
ARG PASSWORD_VAR
ARG USERNAME_VAR
# create devcade user
RUN useradd ${USERNAME_VAR} && \
#                username:password
    echo "${USERNAME_VAR}:${PASSWORD_VAR}" | chpasswd && \
# add to sudoer's group
    usermod -aG wheel ${USERNAME_VAR}

WORKDIR /var/home/${USERNAME_VAR}
ENV HOME /var/home/${USERNAME_VAR}

### required software
RUN dnf install -y \
    openbox picom awk scrot xterm pamixer \
    gcc git wget unzip dotnet-sdk-10.0 \
    grep openssh-server openssl-devel fail2ban \
    glib2-devel flatpak-devel libnfc-devel \
    make cmake \
    autoconf automake libtool \
    xrandr @base-x 

# install libfreefare-devel, as it is not in the package manager yet
RUN git clone https://github.com/nfc-tools/libfreefare.git && \
    cd libfreefare && \
    autoreconf -vis && \
    ./configure --prefix=/usr && \
    make && make install && \
    cd .. && rm -r libfreefare

# root services config file
COPY ./config-files/sshd.local /etc/fail2ban/jail.d/sshd.local

USER ${USERNAME_VAR}
# Rust
RUN curl https://sh.rustup.rs -sSf | bash -s -- -y 
ENV PATH="${HOME}/.cargo/bin:${PATH}"
# git repository
RUN git clone https://github.com/matthewlefler/devcade-onboard-ui-revamp.git
# godot 4.4.1
RUN wget https://github.com/godotengine/godot/releases/download/4.4.1-stable/Godot_v4.4.1-stable_mono_linux_x86_64.zip && \
    unzip Godot_v4.4.1-stable_mono_linux_x86_64.zip && \
    rm Godot_v4.4.1-stable_mono_linux_x86_64.zip

# file required for xauth
RUN touch .Xauthority

### copy required files 
# openbox config file
COPY --chown=${USERNAME_VAR}:${USERNAME_VAR} \
    ./config-files/rc.xml ./.config/openbox/rc.xml
# autologin service
COPY ./config-files/tty1_service_override.conf /etc/systemd/system/getty@tty1.service.d/override.conf
# sshd config
COPY ./config-files/sshd_config /etc/ssh/sshd_config
# restart services 
#   - start ssh server
#   - enable autologin
RUN systemctl enable sshd; \
    systemctl enable getty@tty1

# expose port for SSH connections
EXPOSE 22
# export script, xinitrc, bashrc, onboard script
COPY --chown=${USERNAME_VAR}:${USERNAME_VAR} \
    ["./config-files/export-all.sh", "./config-files/.xinitrc", "./config-files/.bashrc", "./config-files/onboard", "./"]

# export template
COPY --chown=${USERNAME_VAR}:${USERNAME_VAR} \
    ["./config-files/linux_debug.x86_64", "./config-files/linux_release.x86_64", "./.local/share/godot/export_templates/4.4.1.stable.mono/"]

RUN cp ./devcade-onboard-ui-revamp/onboard/.env.template ./.env

# export-all.sh <godot-path> <src path> <out path>
RUN ./export-all.sh \
    ${HOME}/Godot_v4.4.1-stable_mono_linux_x86_64/Godot_v4.4.1-stable_mono_linux.x86_64 \
    ${HOME}/devcade-onboard-ui-revamp \
    ${HOME}/

USER root
# niceity software
RUN dnf install -y \
    vim fastfetch \
    && dnf clean all

RUN chown -R ${USERNAME_VAR}:${USERNAME_VAR} ${HOME}

# make it known as a bootc container
LABEL containers.bootc=1
LABEL bootc.rootfs=/sysroot
