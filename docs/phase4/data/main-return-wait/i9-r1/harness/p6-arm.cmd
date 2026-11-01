@echo off
rem p6-arm.cmd <convert-and-build.sh args...> -- one P6 arm through the seat #53 P1 kit's convert-and-build.sh, under the
rem owner's 4-CPU affinity (0xF, inherited by the converter, MSBuild and ILC), with the AOT env repair
rem (logs/h8-go126/fd/aot-publish.cmd's: the x86 roots and the VS Installer dir, for vswhere).
setlocal
set "DOTNET_ROOT="
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
set "PATH=C:\Program Files (x86)\Microsoft Visual Studio\Installer;%PATH%"
set "UseSharedCompilation=false"
set "GOFLAGS=-p=4"
set "GOMAXPROCS=4"
set "CGO_ENABLED=0"
start "" /affinity 0xF /wait /b "C:\Program Files\Git\bin\bash.exe" C:/go2cs-build/logs/pado/r1/p6/kit/convert-and-build.sh %*
exit /b %ERRORLEVEL%
