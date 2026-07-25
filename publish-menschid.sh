WORK_DIR=$(pwd)
rm $WORK_DIR/Mensch.Id.*.zip

# Publish backend
cd $WORK_DIR/Mensch.Id.API
dotnet clean -c Release
dotnet publish -c Release -r linux-x64 --no-self-contained
cd bin/Release/net10.0/linux-x64
zip -q -r $WORK_DIR/Mensch.Id.API.zip publish

# Publish frontend
cd $WORK_DIR/mensch-id-frontend
npm run build
zip -q -r $WORK_DIR/Mensch.Id.Frontend.zip dist

# Copy to server
cd $WORK_DIR
#scp Mensch.Id.*.zip shehrd.eu:~
#ssh shehrd.eu
