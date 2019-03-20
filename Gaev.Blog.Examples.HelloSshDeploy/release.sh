#!/bin/bash
ssh root@192.168.2.4 'bash -s' <<'ENDSSH'
systemctl stop HelloSshDeploy
systemctl is-active HelloSshDeploy
mkdir -p /apps/HelloSshDeploy
ENDSSH

rsync -v -a ./bin/Release/netcoreapp2.2/ubuntu.16.04-x64/publish/ root@192.168.2.4:/apps/HelloSshDeploy/

ssh root@192.168.2.4 'bash -s' <<'ENDSSH'
chmod 777 /apps/HelloSshDeploy/Gaev.Blog.Examples.HelloSshDeploy
if [[ ! -e /etc/systemd/system/HelloSshDeploy.service ]]; then
    # Create SystemD service file
    cat > /etc/systemd/system/HelloSshDeploy.service <<'EOF'
    [Unit]
    Description=HelloSshDeploy
    After=network.target
    
    [Service]
    WorkingDirectory=/apps/HelloSshDeploy
    ExecStart=/apps/HelloSshDeploy/Gaev.Blog.Examples.HelloSshDeploy
    Restart=always
    KillSignal=SIGINT
    
    [Install]
    WantedBy=multi-user.target
EOF
    systemctl daemon-reload
    systemctl enable HelloSshDeploy
fi
systemctl start HelloSshDeploy
systemctl is-active HelloSshDeploy
ENDSSH