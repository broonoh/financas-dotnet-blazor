#!/usr/bin/env bash
# Gera o APK de Release assinado, pronto para instalar fora da Play Store.
set -e
cd "$(dirname "$0")"
source ./env.sh

dotnet publish MinhasFinancas.Maui.csproj -f net9.0-android -c Release \
    -p:AndroidSdkDirectory="$ANDROID_SDK_ROOT" -p:JavaSdkDirectory="$JAVA_HOME" \
    -p:AndroidSigningKeyStore="$(pwd)/minhasfinancas-release.keystore" \
    -p:AndroidSigningKeyAlias=minhasfinancas \
    -p:AndroidSigningKeyPass='MinhasFinancas2026!' \
    -p:AndroidSigningStorePass='MinhasFinancas2026!' \
    -p:AndroidPackageFormat=apk

echo ""
echo "APK gerado em:"
find bin/Release/net9.0-android -iname "*-Signed.apk"
