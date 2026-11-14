#!/bin/bash

VERSION=$(date +%Y.%m.%d)
echo $VERSION > version

# shEHRd
sed -i -r 's/"version":\s*"[^"]+"/"version": "'"$VERSION"'"/g' ./mensch-id-frontend/package.json
sed -i -r 's/meta name="version" content="[^"]+"/meta name="version" content="'"$VERSION"'"/g' ./mensch-id-frontend/index.html
sed -i -r 's=<Version>[^<]+</Version>=<Version>'"$VERSION"'</Version>=g' ./Mensch.Id.API/Mensch.Id.API.csproj
