#!/bin/bash
set -e

APP_ROOT=/var/www/mymbackup
PUBLISH_DIR=./publish-server

echo "Publishing BackupServerApi..."
dotnet publish ./BackupServerApi/BackupServerApi.csproj -c Release -o "$PUBLISH_DIR"

echo "Creating app directory $APP_ROOT..."
sudo mkdir -p "$APP_ROOT"
sudo cp -r "$PUBLISH_DIR"/* "$APP_ROOT"

echo "Copying systemd service..."
sudo cp ./BackupServerApi/systemd/mymbackup.service /etc/systemd/system/mymbackup.service

echo "Reloading systemd..."
sudo systemctl daemon-reload
sudo systemctl enable mymbackup
sudo systemctl restart mymbackup

echo "Copying nginx config..."
sudo cp ./BackupServerApi/nginx/backup.mymsoftware.com.conf /etc/nginx/sites-available/backup.mymsoftware.com.conf
sudo ln -sf /etc/nginx/sites-available/backup.mymsoftware.com.conf /etc/nginx/sites-enabled/backup.mymsoftware.com.conf

echo "Testing nginx config..."
sudo nginx -t

echo "Reloading nginx..."
sudo systemctl reload nginx

echo "Done. API should be available at http://backup.mymsoftware.com and /swagger and /backups"
