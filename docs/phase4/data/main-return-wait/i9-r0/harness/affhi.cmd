@echo off
rem affhi.cmd <exe> [args] -- run <exe> at ABOVE-NORMAL priority under the owner's 4-CPU affinity (0xF).
start "" /abovenormal /affinity 0xF /wait /b %*
exit /b %ERRORLEVEL%
