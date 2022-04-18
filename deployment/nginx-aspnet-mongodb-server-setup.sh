#!/bin/bash


# Before running the script:
# Copy mongod.conf, nginx/sites-available-conf, systemctl-conf, environment-files, letsencrypt

sudo ufw allow ssh
sudo ufw allow http
sudo ufw allow https
sudo ufw enable

sudo apt update
sudo apt upgrade

wget -qO - https://www.mongodb.org/static/pgp/server-5.0.asc | sudo apt-key add -
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu focal/mongodb-org/5.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-5.0.list
sudo apt update
sudo apt-get install -y mongodb-org
sudo mkdir /data
sudo mkdir /data/db
sudo mkdir /data/log
sudo chown mongodb:mongodb -R /data/*
sudo systemctl start mongod

# In mongosh run:
# > rs.initiate()
# > disableTelemetry()

sudo apt install nginx unzip

wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-5.0

sudo mkdir /var/www/Mensch.Id.API
sudo mkdir /var/www/Mensch.Id.Frontend
sudo chown www-data:www-data -R /var/www/Mensch.Id*

sudo ln -s /etc/nginx/sites-available/mensch-id.conf /etc/nginx/sites-enabled/mensch-id.conf
sudo systemctl restart nginx
