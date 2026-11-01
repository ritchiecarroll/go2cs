@echo off
rem behav.cmd <tree> <runner args...> -- run-behavioral.ps1 on <tree> with the given args, 0xF, go1.26.10.
setlocal
set "GOROOT=%USERPROFILE%\sdk\go1.26.10"
set "GOTOOLCHAIN=local"
set "GOFLAGS=-p=4"
set "GOMAXPROCS=4"
set "CGO_ENABLED=0"
set "UseSharedCompilation=false"
set "DOTNET_ROOT="
set "PATH=%USERPROFILE%\sdk\go1.26.10\bin;%PATH%"
cd /d "%~1\src\tests\Behavioral"
shift
start "" /affinity 0xF /wait /b "%USERPROFILE%\.dotnet\tools\pwsh.exe" -NoProfile -ExecutionPolicy Bypass -File run-behavioral.ps1 %1 %2 %3 %4 %5 %6
exit /b %ERRORLEVEL%
