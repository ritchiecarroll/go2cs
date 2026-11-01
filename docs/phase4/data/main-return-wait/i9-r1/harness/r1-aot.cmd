@echo off
rem r1-aot.cmd <project dir> <name> <outdir> <go2csPath> -- Native AOT publish of one behavioral test for main-return-wait
rem R1/R2: r0-aot.cmd's recipe (seat #53 P1 kit aot.targets, win-x64, Release, affinity 0xF, the env repair) with the
rem tree's go2csPath as an argument.
setlocal
set "DOTNET_ROOT="
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
set "PATH=C:\Program Files (x86)\Microsoft Visual Studio\Installer;%PATH%"
set "UseSharedCompilation=false"
cd /d "%~1"
start "" /affinity 0xF /wait /b "C:\Program Files\dotnet\dotnet.exe" publish "%~2.csproj" -c Release -r win-x64 -m:4 -nr:false -p:UseSharedCompilation=false -p:GoAot=true "-p:CustomAfterMicrosoftCommonTargets=C:\go2cs-build\logs\pado\r1\aot.targets" "-p:go2csPath=%~4" -o "%~3" -nologo -v:m
exit /b %ERRORLEVEL%
