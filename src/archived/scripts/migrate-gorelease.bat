@echo off
rem Launcher for migrate-gorelease.ps1 -- censuses (and with -Apply performs) a Go corpus
rem migration's pin bump: version.props, the module go directive, and the docs that state the
rem release as present-tense fact. Census is the default; it changes nothing. Examples:
rem   migrate-gorelease                        census at the current pin
rem   migrate-gorelease -To 1.23.12            census showing what the hop would edit
rem   migrate-gorelease -To 1.23.12 -Apply     perform the edits, then verify zero sites remain
pushd "%~dp0..\.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\migrate-gorelease.ps1" %*
set "_ec=%ERRORLEVEL%"
popd
exit /b %_ec%
