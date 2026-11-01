@echo off
rem p6-drive.cmd <arms.tsv> <rounds> <out.tsv> -- the seat #53 drive.py under affinity 0xF (each bench inherits it),
rem GOMAXPROCS=4, with python -I from the kit folder's copy.
setlocal
set "GOMAXPROCS=4"
cd /d C:\go2cs-build\logs\pado\r1\p6
start "" /affinity 0xF /wait /b C:\Python312\python.exe -I -X utf8 C:\go2cs-build\logs\pado\r1\p6\kit\drive.py %1 %2 %3
exit /b %ERRORLEVEL%
