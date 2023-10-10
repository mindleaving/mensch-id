cd Mensch.Id.API
dotnet publish -c Release -r linux-x64 --no-self-contained
Compress-Archive -Path bin/Release/net7.0/linux-x64/publish -DestinationPath ../Mensch.Id.API.zip -Force
cd ../mensch-id-frontend
npm run build
Compress-Archive -Path build -DestinationPath ../Mensch.Id.Frontend.zip -Force
cd ..

# scp -i ~/.ssh/nuc-webserver Mensch.Id.*.zip doctorstodo@192.168.178.73:~
# ssh -i ~/.ssh/nuc-webserver doctorstodo@192.168.178.73 "./deploy_mensch-id.sh"
