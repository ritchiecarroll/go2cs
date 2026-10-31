@echo off
rem aff.cmd <exe> -- run <exe> under the owner's 4-CPU affinity (0xF); exit with its code.
start "" /affinity 0xF /wait /b %*
exit /b %ERRORLEVEL%
