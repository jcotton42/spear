#!/bin/bash

echo -e "${SSH_PASSWORD}\n${SSH_PASSWORD}" | passwd "${SSH_USERNAME}"
service ssh start
tail -f /dev/null
