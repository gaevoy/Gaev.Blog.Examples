#!/bin/bash
ssh root@app.gaevoy.com 'bash -s' <<'ENDSSH'
systemctl stop GaevKeybaseSignIn
systemctl disable GaevKeybaseSignIn 
rm /etc/systemd/system/GaevKeybaseSignIn.service 
systemctl daemon-reload
systemctl reset-failed
rm -rf /apps/GaevKeybaseSignIn
ENDSSH