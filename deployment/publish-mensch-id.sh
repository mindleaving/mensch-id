WORK_DIR=$(pwd)

if [ ! -f $WORK_DIR/Mensch.Id.API.sln ]
then
	echo "Script must be run from root solution directory"
	exit 1
fi

OUTPUT_DIR=$WORK_DIR/deployment
rm $OUTPUT_DIR/Mensch.Id.*.zip

# Update version
./update-version.sh

# Publish backend
cd $WORK_DIR/Mensch.Id.API
dotnet clean -c Release
dotnet publish -c Release -r linux-x64 --no-self-contained
cd bin/Release/net10.0/linux-x64
zip -q -r $OUTPUT_DIR/Mensch.Id.API.zip publish

# Publish frontend
cd $WORK_DIR/mensch-id-frontend
npm run build
zip -q -r $OUTPUT_DIR/Mensch.Id.Frontend.zip dist

# Copy to server
cd $WORK_DIR
#scp $OUTPUT_DIR/Mensch.Id.*.zip shehrd.eu:~
#ssh shehrd.eu
