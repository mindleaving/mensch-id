cd Mensch.Id.API
dotnet publish -c Release
Compress-Archive -Path bin/Release/net6.0/publish -DestinationPath ../Mensch.Id.API.zip -Force
cd ../mensch-id-frontend
npm run build
Compress-Archive -Path build -DestinationPath ../Mensch.Id.Frontend.zip -Force
cd ..

# scp -i ~/.ssh/nuc-webserver Mensch.Id.*.zip doctorstodo@192.168.178.73:~
# ssh -i ~/.ssh/nuc-webserver doctorstodo@192.168.178.73 "./deploy_mensch-id.sh"
