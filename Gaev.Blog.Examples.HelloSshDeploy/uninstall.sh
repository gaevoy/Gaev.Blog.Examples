#!/bin/bash
ssh root@192.168.2.4 'bash -s' <<'ENDSSH'
systemctl stop HelloSshDeploy
systemctl disable HelloSshDeploy 
rm /etc/systemd/system/HelloSshDeploy.service 
systemctl daemon-reload
systemctl reset-failed
rm -rf /apps/HelloSshDeploy
ENDSSH