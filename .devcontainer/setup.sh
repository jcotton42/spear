#!/bin/sh

printf 'vscode\nvscode\n' | sudo passwd vscode
sudo chown vscode:vscode ~/.microsoft
