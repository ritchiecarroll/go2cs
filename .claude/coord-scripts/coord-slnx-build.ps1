param([string] $Worktree = 'C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c')
$ErrorActionPreference = 'Continue'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'
function Stamp([string] $m) { Write-Output ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $m) }
$sln = Join-Path $Worktree 'src\go2cs.slnx'
if (-not (Test-Path $sln)) { Stamp "MISSING $sln"; exit 1 }
Stamp ("SLNX BUILD START head=" + ((& git -C $Worktree rev-parse HEAD) -join ''))
$t0 = Get-Date
$out = cmd /c "dotnet build `"$sln`" -c Debug -m -p:UseSharedCompilation=false 2>&1"
$code = $LASTEXITCODE
$errs = @($out | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+' | Select-Object -First 40)
$out | Select-Object -Last 8
Stamp ("SLNX BUILD END exit=$code strictErrors=" + $errs.Count + " elapsed=" + ([int]((Get-Date) - $t0).TotalSeconds) + "s")
$errs | ForEach-Object { Stamp ("  " + $_.Line.Trim()) }
exit $code
