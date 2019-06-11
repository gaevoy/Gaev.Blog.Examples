#!/bin/bash
ssh root@app.gaevoy.com 'bash -s' <<'ENDSSH'
printf "Stopping service...\n"
systemctl stop GaevKeybaseSignIn
printf "Service is "
systemctl is-active GaevKeybaseSignIn
mkdir -p /apps/GaevKeybaseSignIn
ENDSSH

printf "Uploading new version of service...\n"
rsync -v -a ./bin/Release/netcoreapp2.2/ubuntu.16.04-x64/publish/ root@app.gaevoy.com:/apps/GaevKeybaseSignIn/

ssh root@app.gaevoy.com 'bash -s' <<'ENDSSH'
chmod 777 /apps/GaevKeybaseSignIn/Gaev.Blog.Examples.KeybaseSignIn
if [[ ! -e /etc/systemd/system/GaevKeybaseSignIn.service ]]; then
    printf "Installing service...\n"
    cat > /etc/systemd/system/GaevKeybaseSignIn.service <<'EOF'
    [Unit]
    Description=GaevKeybaseSignIn
    After=network.target
    
    [Service]
    WorkingDirectory=/apps/GaevKeybaseSignIn
    ExecStart=/apps/GaevKeybaseSignIn/Gaev.Blog.Examples.KeybaseSignIn
    Restart=always
    KillSignal=SIGINT
    
    [Install]
    WantedBy=multi-user.target
EOF
    systemctl daemon-reload
    systemctl enable GaevKeybaseSignIn
fi
printf "Starting service...\n"
systemctl start GaevKeybaseSignIn
printf "Service is "
systemctl is-active GaevKeybaseSignIn
ENDSSH