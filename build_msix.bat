@echo off
call "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat"
dotnet publish -f net8.0-windows10.0.19041.0 -c Release -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=true -p:PackageCertificateKeyFile=HanakKey.pfx -p:PackageCertificatePassword=123456 -p:AppxBundle=Always -p:UapAppxPackageBuildMode=SideloadOnly -p:AppxBundlePlatforms=x64
