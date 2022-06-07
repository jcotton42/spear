#!/bin/sh

docker compose -p spear-devenv \
    -f docker-compose.yml \
    -f .devcontainer/docker-compose.devcontainer-common.yml \
    -f .devcontainer/docker-compose.devcontainer-rider.yml \
    "$@"
