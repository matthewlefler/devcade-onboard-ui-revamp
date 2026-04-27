FROM quay.io/fedora/fedora-bootc:latest

### User Creation
ARG PASSWORD_VAR
ARG USERNAME_VAR
# create devcade user
RUN useradd devcade && \
#         username:password
    echo "${USERNAME_VAR}:${PASSWORD_VAR}" | chpasswd && \
# add to sudoer's group
    sudo usermod -aG wheel devcade

WORKDIR /var/home/${USERNAME_VAR}
ENV HOME /var/home/${USERNAME_VAR}

### required software
RUN rpm-ostree install -y \
    openbox gcc git wget unzip dotnet-sdk-10.0 \
    xrandr grep openssh-server openssl-devel glib2-devel \
    flatpak-devel libnfc-devel make cmake \
    autoconf automake libtool xauth
# install libfreefare-devel, as it is not in the package manager yet
RUN git clone https://github.com/nfc-tools/libfreefare.git && \
    cd libfreefare && \
    autoreconf -vis && \
    ./configure --prefix=/usr && \
    make && make install && \
    cd .. && rm -r libfreefare
    
# Rust
RUN curl https://sh.rustup.rs -sSf | bash -s -- -y 
ENV PATH="${HOME}/.cargo/bin:${PATH}"
# git repository
RUN git clone https://github.com/matthewlefler/devcade-onboard-ui-revamp.git
# godot 4.4.1
RUN wget https://github.com/godotengine/godot/releases/download/4.4.1-stable/Godot_v4.4.1-stable_mono_linux_x86_64.zip && \
    unzip Godot_v4.4.1-stable_mono_linux_x86_64.zip && \
    rm Godot_v4.4.1-stable_mono_linux_x86_64.zip

### copy required files 
# openbox config file
COPY ./config-files/rc.xml ./.config/openbox/rc.xml
# autologin service
COPY ./config-files/tty1_service_override.conf /etc/systemd/system/getty@tty1.service.d/override.conf
# sshd config
COPY ./config-files/sshd_config /etc/ssh/sshd_config
# restart services 
#   - enable autologin
#   - start ssh server
RUN systemctl enable getty@tty1; \
    systemctl enable sshd
EXPOSE 22
# export script, xinitrc, bashrc, onboard script
COPY ["./config-files/export-all.sh", "./config-files/.xinitrc", "./config-files/.bashrc", "./config-files/onboard", "./"]

# export template
COPY ./config-files/linux_debug.x86_64 ./.local/share/godot/export_templates/4.4.1.stable.mono/linux_release.x86_64

RUN ls
# export-all.sh <godot-path> <src path> <out path>
RUN ./export-all.sh \
    ${HOME}/Godot_v4.4.1-stable_mono_linux_x86_64/Godot_v4.4.1-stable_mono_linux.x86_64 \
    ${HOME}/devcade-onboard-ui-revamp \
    ${HOME}/
