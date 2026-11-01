@echo off
rem p6-conv.cmd <src/go2cs dir> <out exe> -- build the converter from a checkout, go1.26.10, under affinity 0xF.
setlocal
set "GOROOT=%USERPROFILE%\sdk\go1.26.10"
set "GOTOOLCHAIN=local"
set "GOFLAGS=-p=4"
set "GOMAXPROCS=4"
set "CGO_ENABLED=0"
set "PATH=%USERPROFILE%\sdk\go1.26.10\bin;%PATH%"
cd /d "%~1"
start "" /affinity 0xF /wait /b go build -o "%~2" .
exit /b %ERRORLEVEL%
