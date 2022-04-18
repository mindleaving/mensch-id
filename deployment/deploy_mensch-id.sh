#!/bin/bash

unzip -q Mensch.Id.API.zip
sudo cp /var/www/Mensch.Id.API/appsettings.json /var/www/Mensch.Id.API/appsettings.json.bak
sudo cp -R publish/* /var/www/Mensch.Id.API/
sudo cp /var/www/Mensch.Id.API/appsettings.json.bak /var/www/Mensch.Id.API/appsettings.json
sudo rm -rf publish

unzip -q Mensch.Id.Frontend.zip
sudo cp -R build/* /var/www/Mensch.Id.Frontend/
sudo rm -rf build
rm -rf Mensch.Id*.zip
sudo chmod 777 -R /var/www/Mensch.Id*
sudo systemctl restart mensch-id-api.service


