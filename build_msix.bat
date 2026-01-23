@echo off
echo Cleaning previous builds...
dotnet clean
if exist "bin\x64\Release\net8.0-windows10.0.19041.0\AppPackages" rmdir /s /q "bin\x64\Release\net8.0-windows10.0.19041.0\AppPackages"

echo Initializing Build Environment...
call "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat"

echo Building MSIX with MSBuild...
msbuild ComputerController.csproj /restore /t:Publish /p:Configuration=Release /p:Platform=x64 /p:GenerateAppxPackageOnBuild=true /p:AppxPackageSigningEnabled=true /p:PackageCertificateKeyFile=HanakKey.pfx /p:PackageCertificatePassword=123456 /p:AppxBundle=Always /p:UapAppxPackageBuildMode=SideloadOnly /p:AppxBundlePlatforms=x64
