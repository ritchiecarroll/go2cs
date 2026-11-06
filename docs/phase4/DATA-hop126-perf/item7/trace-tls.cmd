@echo off
rem trace-tls.cmd -- item 7 (TlsHandshake AOT 2.4x its JIT), the profile step. RUN FROM AN ELEVATED PROMPT ON G (the owner's
rem hand: wpr's CPU sampling needs SeSystemProfilePrivilege). It records two CPU-sampling traces, one per binary, and nothing else:
rem   tls-aot.etl  the symbol-carrying AOT image from the i9 (C:\go2cs\scratch\p1262\trace\aot\PerfTlsHandshake.exe + .pdb)
rem   tls-tc0.etl  the .2 JIT binary under DOTNET_TieredCompilation=0, with .NET rundown so JIT frames resolve
rem wpr runs in its default memory mode (no -filemode), so it writes no temporary files; each -stop writes one .etl here.
rem BEFORE anything runs, every file either run loads is hash-checked, and any mismatch stops the script (COORD's review):
rem the AOT exe and pdb against their sha256 inline; the JIT folder against jit-manifest.txt (all 435 files), whose own
rem sha256 is inline here, so neither a binary nor the manifest can be swapped.
rem Each trace wraps exactly one run of the program. G reads them afterwards, without elevation.
setlocal
set T=C:\go2cs\scratch\p1262\trace
set JD=C:\go2cs\scratch\p1262\i7\jit
set J=%JD%\PerfTlsHandshake.exe
set M=%T%\jit-manifest.txt
if not exist "%T%\aot\PerfTlsHandshake.exe" (echo MISSING the AOT image & exit /b 2)
if not exist "%T%\aot\PerfTlsHandshake.pdb" (echo MISSING the AOT pdb & exit /b 2)
if not exist "%J%" (echo MISSING the JIT binary & exit /b 2)
if not exist "%M%" (echo MISSING the JIT manifest & exit /b 2)
certutil -hashfile "%T%\aot\PerfTlsHandshake.exe" SHA256 | findstr /i /c:"7f2534b73ec9db82189bca03eca2d0e1cb794c9cd690fc759f72c7459a96b1ac" >nul || (echo HASH MISMATCH aot exe & exit /b 3)
certutil -hashfile "%T%\aot\PerfTlsHandshake.pdb" SHA256 | findstr /i /c:"6fa856d5e92f674c15e05f961d9af6d05c4e2c55ea706ac380e3217d551c84f1" >nul || (echo HASH MISMATCH aot pdb & exit /b 3)
certutil -hashfile "%M%" SHA256 | findstr /i /c:"feb8ed56dce55a8be89a546a2cf2dfcec602a2f2f6f3b68e0fd6ff1a939a6582" >nul || (echo HASH MISMATCH jit manifest & exit /b 3)
for /f "usebackq tokens=1,*" %%A in ("%M%") do (
  certutil -hashfile "%JD%\%%B" SHA256 | findstr /i /c:"%%A" >nul || (echo HASH MISMATCH jit %%B & exit /b 3)
)
echo HASHES OK: the aot exe and pdb, and the 435 jit files
if "%1"=="--check-only" exit /b 0
set DOTNET_ROOT=C:\Users\Admin\dotnet10
cd /d %T%
wpr.exe -cancel >nul 2>&1
wpr.exe -start CPU || goto :fail
"%T%\aot\PerfTlsHandshake.exe" > "%T%\tls-aot.out"
wpr.exe -stop "%T%\tls-aot.etl" "item7 aot" || goto :fail
wpr.exe -start CPU -start DotNET || goto :fail
set DOTNET_TieredCompilation=0
"%J%" > "%T%\tls-tc0.out"
set DOTNET_TieredCompilation=
wpr.exe -stop "%T%\tls-tc0.etl" "item7 jit tc0" || goto :fail
echo done > "%T%\trace.done"
echo TRACES WRITTEN: %T%\tls-aot.etl and %T%\tls-tc0.etl
exit /b 0
:fail
echo WPR FAILED (rc %errorlevel%) > "%T%\trace.failed"
wpr.exe -cancel >nul 2>&1
exit /b 1
