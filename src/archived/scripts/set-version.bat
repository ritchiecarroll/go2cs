@echo off
rem Launcher for set-version.ps1 -- stamps the version into winres, the csproj template and go2cs-gen.
pushd "%~dp0..\.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\set-version.ps1" %*
set "_ec=%ERRORLEVEL%"
popd
exit /b %_ec%
