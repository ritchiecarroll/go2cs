<#
.SYNOPSIS
    Parse-and-run test of PIN1-SHEET.md's block 0 (the window guard) and block 3's gates (checksum, go.* release,
    package version, folder count), as the sheet writes them, against stand-in release candidates.

.DESCRIPTION
    Block 3 is taken from the sheet up to the line before the signer (nothing is signed), and run with a $Kits table
    and a $Pub folder built here. The stand-ins are the release candidates of the current kits, laid out as the wave
    kits will be (nupkg/release/<ID>.nupkg and a SHA256SUMS line "release/<ID>.nupkg"). Cases:
      1. x/sync and pflag (packed against 1.24.13.7), $Release = 1.24.13.7            -> passes
      2. the same two with ONE-LINE SHA256SUMS files (the live false red of 10-10)     -> passes
      3. hashset added (packed against 1.24.13.5), $Release = 1.24.13.7                -> "packed against another release"
      4. pflag's SHA256SUMS line with a wrong hash                                       -> "checksum mismatch"
      5. pflag declared at a version its package does not carry                         -> "the package version is not"
    Block 0 is run twice: outside any git worktree (refuses, printing the fix) and inside one (passes). Its 5.1
    refusal cannot be run here (no Windows PowerShell); its syntax is 5.1-only by construction.
    Run with PowerShell 7 from the go2cs clone: pwsh -NoProfile -File wave/Test-PIN1Gates.ps1 -Repo <go2cs clone>
    Exit code = the number of failed cases.
#>
param([Parameter(Mandatory)][string]$Repo)
$ErrorActionPreference = 'Stop'
$sheet = @((Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'PIN1-SHEET.md')) -split "`r?`n")

function Get-Block([string]$heading) {
    $h = [Array]::FindIndex([string[]]$sheet, [Predicate[string]] { param($l) $l -like "## $heading*" })
    $s = [Array]::FindIndex([string[]]$sheet, $h, [Predicate[string]] { param($l) $l -eq '```powershell' })
    $e = [Array]::FindIndex([string[]]$sheet, $s + 1, [Predicate[string]] { param($l) $l -eq '```' })
    return $sheet[($s + 1)..($e - 1)]
}

$failed = 0
function Check([string]$name, [scriptblock]$body, [string]$expect) {
    $threw = $null
    try { & $body | Out-Null } catch { $threw = $_.Exception.Message }
    $ok = if ($expect) { $threw -and $threw.Contains($expect) } else { -not $threw }
    if ($ok) { Write-Host "  ok    $name$(if ($threw) { " -- threw: $threw" })" }
    else { $script:failed++; Write-Host "  FAIL  $name -- $(if ($threw) { "threw: $threw" } else { 'did not throw' })" -ForegroundColor Red }
}

# Block 0, both ways.
$block0 = (Get-Block 'Block 0') -join "`n"
$tokens = $null; $errors = $null
[void][System.Management.Automation.Language.Parser]::ParseInput($block0, [ref]$tokens, [ref]$errors)
if ($errors.Count) { $failed++; Write-Host "  FAIL  block 0 does not parse: $($errors[0].Message)" }
$outside = Join-Path ([IO.Path]::GetTempPath()) "pin1-outside-$PID"
New-Item -ItemType Directory -Force $outside | Out-Null
Check 'block 0 outside a git worktree' { Push-Location $outside; try { . ([scriptblock]::Create($block0)) } finally { Pop-Location } } 'not inside the go2cs clone'
Check 'block 0 inside the go2cs clone' { Push-Location $Repo; try { . ([scriptblock]::Create($block0)); if (-not $GuardOK) { throw 'GuardOK not set' } } finally { Pop-Location } } $null
Remove-Item -Recurse -Force $outside

# Block 3's gates, up to the signer.
$block3 = Get-Block 'Block 3'
$cut = [Array]::FindIndex([string[]]$block3, [Predicate[string]] { param($l) $l -match 'NuGetCertFingerprint' })
$gates = (($block3[0..($cut - 1)]) + '}') -join "`n"
# The sheet is written for the owner's Windows box. .NET's zip reader does not translate '\' on Linux, so the test
# rewrites the sheet's Windows path separators, and nothing else, before it runs the gates here.
if (-not $IsWindows) {
    foreach ($pair in @(@('$($k.Dir)\nupkg\', '$($k.Dir)/nupkg/'), @('\release\', '/release/'), @('release\$', 'release/$'), @('"$SignDir\', '"$SignDir/'), @('"$Pub\sign"', '"$Pub/sign"'))) {
        $gates = $gates.Replace($pair[0], $pair[1])
    }
}
[void][System.Management.Automation.Language.Parser]::ParseInput($gates, [ref]$tokens, [ref]$errors)
if ($errors.Count) { $failed++; Write-Host "  FAIL  block 3's gates do not parse: $($errors[0].Message)" }

# The stand-ins are the PUBLISHED packages (the kit refs of x/sync and pflag are pruned once published). A signed
# package passes these gates exactly as an unsigned one: none of them reads the signature.
$standIns = @{
    xsync   = @{ Id = 'nugetgo.golang.org.x.sync';                 Version = '0.19.0';  Base = '0.19.0'; Rev = '0' }
    pflag   = @{ Id = 'nugetgo.github.com.spf13.pflag';            Version = '1.0.9';   Base = '1.0.9';  Rev = '0' }
    hashset = @{ Id = 'nugetgo.github.com.ritchiecarroll.hashset'; Version = $null;     Base = '1.0.0';  Rev = '2' }
}
$cache = Join-Path ([IO.Path]::GetTempPath()) "pin1-standins-$PID"
New-Item -ItemType Directory -Force $cache | Out-Null
foreach ($s in $standIns.Values) {
    if (-not $s.Version) { $s.Version = "$($s.Base).$($s.Rev)" }   # a rebuild's B3 version
    $s.File = Join-Path $cache "$($s.Id).$($s.Version).nupkg"
    Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/$($s.Id)/$($s.Version)/$($s.Id).$($s.Version).nupkg" -OutFile $s.File
}

function New-Pub([string[]]$names, [switch]$OneLine, [string]$wrongHash, [hashtable]$pvOverride = @{}) {
    $script:Pub = Join-Path ([IO.Path]::GetTempPath()) ("pin1-gates-$PID-" + [DateTime]::UtcNow.Ticks)
    New-Item -ItemType Directory $script:Pub | Out-Null
    $script:Kits = @()
    foreach ($n in $names) {
        $s = $standIns[$n]
        $k = [ordered]@{ Name = $n; Base = $s.Base; Rev = $s.Rev; Id = $s.Id; CsRepo = 'stand-in' }
        $k.PV = if ([int]$k.Rev) { "$($k.Base).$($k.Rev)" } else { $k.Base }
        if ($pvOverride.ContainsKey($n)) { $k.PV = $pvOverride[$n] }
        $k.Dir = Join-Path $script:Pub "kit-$n"
        New-Item -ItemType Directory -Force (Join-Path $k.Dir 'nupkg/release') | Out-Null
        $dest = Join-Path $k.Dir "nupkg/release/$($s.Id).nupkg"
        Copy-Item -LiteralPath $s.File -Destination $dest
        $hash = if ($wrongHash -eq $n) { '0' * 64 } else { (Get-FileHash -Algorithm SHA256 $dest).Hash.ToLowerInvariant() }
        $lines = @("$hash  release/$($s.Id).nupkg")
        if (-not $OneLine) { $lines = @(('f' * 64) + '  int.1/' + $s.Id + '.nupkg') + $lines }
        [IO.File]::WriteAllText((Join-Path $k.Dir 'nupkg/SHA256SUMS'), (($lines -join "`n") + "`n"))
        $script:Kits += $k
    }
}
function Invoke-Gates { $GuardOK = $true; . ([scriptblock]::Create($gates)) }

$Release = '1.24.13.7'
New-Pub @('xsync', 'pflag');                         Check 'x/sync + pflag at 1.24.13.7' { Invoke-Gates } $null; Remove-Item -Recurse -Force $Pub
New-Pub @('xsync', 'pflag') -OneLine;                Check 'one-line SHA256SUMS files' { Invoke-Gates } $null; Remove-Item -Recurse -Force $Pub
New-Pub @('xsync', 'pflag', 'hashset');              Check 'hashset packed against 1.24.13.5' { Invoke-Gates } 'packed against another release'; Remove-Item -Recurse -Force $Pub
New-Pub @('xsync', 'pflag') -wrongHash pflag;        Check 'a wrong hash' { Invoke-Gates } 'checksum mismatch'; Remove-Item -Recurse -Force $Pub
New-Pub @('xsync', 'pflag') -pvOverride @{ pflag = ('1.0.9', '1') -join '.' }; Check 'a version the package does not carry' { Invoke-Gates } 'the package version is not'; Remove-Item -Recurse -Force $Pub
Remove-Item -Recurse -Force $cache
Write-Host "failed $failed"
exit $failed
