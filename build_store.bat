@echo off
echo Initializing Visual Studio Build Environment...
call "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat"

echo Building Store Package (x64)...
msbuild ComputerController.csproj /restore /t:Publish /p:Configuration=Release /p:Platform=x64 /p:GenerateAppxPackageOnBuild=true /p:AppxBundle=Always /p:UapAppxPackageBuildMode=StoreUpload /p:AppxPackageSigningEnabled=false

echo.
echo Build complete. Check the output for the .msixupload file.
pause
