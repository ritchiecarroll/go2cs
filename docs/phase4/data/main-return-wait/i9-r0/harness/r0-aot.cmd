@echo off
rem r0-aot.cmd <project dir> <name> <outdir> -- Native AOT publish of one behavioral test for the main-return-wait R0
rem arm: the seat #53 P1 kit's recipe (GoAot=true through aot.targets, so only the Exe is AOT), win-x64, Release, under
rem the owner's 4-CPU affinity (0xF), set at process creation and inherited by MSBuild and ILC. The env repair is
rem logs/h8-go126/fd/aot-publish.cmd's (the x86 roots and the VS Installer dir, for vswhere).
setlocal
set "DOTNET_ROOT="
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
set "PATH=C:\Program Files (x86)\Microsoft Visual Studio\Installer;%PATH%"
set "UseSharedCompilation=false"
cd /d "%~1"
start "" /affinity 0xF /wait /b "C:\Program Files\dotnet\dotnet.exe" publish "%~2.csproj" -c Release -r win-x64 -m:4 -nr:false -p:UseSharedCompilation=false -p:GoAot=true "-p:CustomAfterMicrosoftCommonTargets=C:\go2cs-build\logs\pado\r0\aot.targets" "-p:go2csPath=D:/go2cs-build/i9-mrw/src/" -o "%~3" -nologo -v:m
exit /b %ERRORLEVEL%
