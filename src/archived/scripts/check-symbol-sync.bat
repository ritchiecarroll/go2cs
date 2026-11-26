@echo off
rem Launcher for check-symbol-sync.ps1 -- verifies symbols.go / Symbols.cs match core/go2cs/symbols.json.
pushd "%~dp0..\.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\check-symbol-sync.ps1" %*
set "_ec=%ERRORLEVEL%"
popd
exit /b %_ec%
