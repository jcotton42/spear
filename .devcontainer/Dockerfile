# [Choice] .NET version: 6.0, 5.0, 3.1, 6.0-bullseye, 5.0-bullseye, 3.1-bullseye, 6.0-focal, 5.0-focal, 3.1-focal
ARG VARIANT="7.0"
FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-${VARIANT} AS vscode
ENV NUGET_XMLDOC_MODE=none
# [Choice] Node.js version: none, lts/*, 16, 14, 12, 10
ARG NODE_VERSION="none"
RUN if [ "${NODE_VERSION}" != "none" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

COPY library-scripts/docker-debian.sh /tmp/library-scripts/
ENV DOCKER_BUILDKIT=1
RUN apt-get update && bash /tmp/library-scripts/docker-debian.sh
ENTRYPOINT ["/usr/local/share/docker-init.sh"]

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1

FROM vscode AS rider
COPY startup.sh /startup.sh
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install openssh-server \
    && systemctl enable ssh \
    && chmod +x /startup.sh

CMD ["/startup.sh"]
