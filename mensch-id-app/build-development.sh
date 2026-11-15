#!/bin/bash

export ANDROID_HOME=/home/jan/Android/Sdk
export EXPO_PUBLIC_API_URL=https://mensch.id
export EXPO_PUBLIC_OIDC_REDIRECT_MODE=customScheme

npx expo run:android
