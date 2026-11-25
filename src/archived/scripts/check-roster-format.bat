@echo off
rem Launcher for check-roster-format.ps1 -- verifies docs\ValidatedTestPackages.md parses and adds up.
pushd "%~dp0..\.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\check-roster-format.ps1" %*
set "_ec=%ERRORLEVEL%"
popd
exit /b %_ec%
