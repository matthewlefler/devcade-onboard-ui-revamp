FROM debian:buster

RUN apt-get -y update
RUN apt-get -y install vim wget git fonts-liberation fontconfig-config tmux xterm pulseaudio
RUN wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb \
     -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-6.0 aspnetcore-runtime-6.0 dotnet-sdk-3.1 aspnetcore-runtime-3.1
RUN dpkg-reconfigure fontconfig-config
