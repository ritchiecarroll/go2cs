@echo off
rem Launcher for push-nuget.ps1 -- pack (and optionally push) the go2cs stdlib NuGet packages.
pushd "%~dp0..\.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\push-nuget.ps1" %*
set "_ec=%ERRORLEVEL%"
popd
exit /b %_ec%
