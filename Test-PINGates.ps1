<#
.SYNOPSIS
    Parse-and-run test of PIN-SHEET.md's block 0 (the window guard) and block 3's gates (checksum, go.* release,
    package version, folder count), as the sheet writes them, against stand-in release candidates.

.DESCRIPTION
    Block 3 is taken from the sheet up to the line before the signer (nothing is signed), and run with a $Kits table
    and a $Pub folder built here. The stand-ins are the S1 release candidates under -KitRoot
    (<name>/nupkg/release/<ID>.nupkg, as each kit ref carries them), the ones -Names lists. Cases:
      1. the kits, $Release = 1.26.10.1                                                  -> passes
      2. the same with ONE-LINE SHA256SUMS files (the live false red of 10-10)           -> passes
      3. $Release = 1.24.13.7                                                            -> "packed against another release"
      4. pflag's SHA256SUMS line with a wrong hash                                       -> "checksum mismatch"
      5. pflag declared at a version its package does not carry                         -> "the package version is not"
      6. pflag's README without its release line (the package rewritten, its hash too)   -> "does not name release"
      7. pflag declared to require hashset, which it does not                             -> "does not require"
    Block 0 is run twice: outside any git worktree (refuses, printing the fix) and inside one (passes). Its 5.1
    refusal cannot be run here (no Windows PowerShell); its syntax is 5.1-only by construction.
    Run with PowerShell 7: pwsh -NoProfile -File Test-PINGates.ps1 -Repo <go2cs clone> -KitRoot <folder of run-<name>>
    Exit code = the number of failed cases.
#>
param([Parameter(Mandatory)][string]$Repo, [Parameter(Mandatory)][string]$KitRoot,
    [string[]]$Names = @('hashset', 'uuid', 'jwt', 'xsync', 'pflag', 'mousetrap'))
$ErrorActionPreference = 'Stop'
$sheet = @((Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'PIN-SHEET.md')) -split "`r?`n")

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
$outside = Join-Path ([IO.Path]::GetTempPath()) "pin-outside-$PID"
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

$standIns = [ordered]@{
    hashset = @{ Id = 'nugetgo.github.com.ritchiecarroll.hashset'; Base = '1.0.0';  Rev = '3' }
    uuid    = @{ Id = 'nugetgo.github.com.google.uuid';            Base = '1.6.0';  Rev = '1' }
    jwt     = @{ Id = 'nugetgo.github.com.golang-jwt.jwt.v5';      Base = '5.3.1';  Rev = '1' }
    xsync   = @{ Id = 'nugetgo.golang.org.x.sync';                 Base = '0.19.0'; Rev = '1' }
    pflag     = @{ Id = 'nugetgo.github.com.spf13.pflag';              Base = '1.0.9';  Rev = '1' }
    mousetrap = @{ Id = 'nugetgo.github.com.inconshreveable.mousetrap'; Base = '1.1.0';  Rev = '0' }
}
foreach ($n in $standIns.Keys) { $standIns[$n].File = Join-Path $KitRoot "$n/nupkg/release/$($standIns[$n].Id).nupkg" }

function New-Pub([string[]]$names, [switch]$OneLine, [string]$wrongHash, [hashtable]$pvOverride = @{}, [string]$noReleaseLine, [hashtable]$requires = @{}) {
    $script:Pub = Join-Path ([IO.Path]::GetTempPath()) ("pin-gates-$PID-" + [DateTime]::UtcNow.Ticks)
    New-Item -ItemType Directory $script:Pub | Out-Null
    $script:Kits = @()
    foreach ($n in $names) {
        $s = $standIns[$n]
        $k = [ordered]@{ Name = $n; Base = $s.Base; Rev = $s.Rev; Id = $s.Id; CsRepo = 'stand-in'; Requires = @() }
        if ($requires.ContainsKey($n)) { $k.Requires = $requires[$n] }
        $k.PV = if ([int]$k.Rev) { "$($k.Base).$($k.Rev)" } else { $k.Base }
        if ($pvOverride.ContainsKey($n)) { $k.PV = $pvOverride[$n] }
        $k.Dir = Join-Path $script:Pub "kit-$n"
        New-Item -ItemType Directory -Force (Join-Path $k.Dir 'nupkg/release') | Out-Null
        $dest = Join-Path $k.Dir "nupkg/release/$($s.Id).nupkg"
        Copy-Item -LiteralPath $s.File -Destination $dest
        if ($noReleaseLine -eq $n) {
            Add-Type -AssemblyName System.IO.Compression
            $zip = [IO.Compression.ZipFile]::Open($dest, 'Update')
            try {
                $e = $zip.GetEntry('README.md')
                $r = [IO.StreamReader]::new($e.Open()); try { $text = $r.ReadToEnd() } finally { $r.Dispose() }
                $text = (@($text -split "`n") | Where-Object { $_ -notlike 'This package is compiled against *' }) -join "`n"
                $e.Delete()
                $w = [IO.StreamWriter]::new($zip.CreateEntry('README.md').Open()); try { $w.Write($text) } finally { $w.Dispose() }
            } finally { $zip.Dispose() }
        }
        $hash = if ($wrongHash -eq $n) { '0' * 64 } else { (Get-FileHash -Algorithm SHA256 $dest).Hash.ToLowerInvariant() }
        $lines = @("$hash  release/$($s.Id).nupkg")
        if (-not $OneLine) { $lines = @(('f' * 64) + '  int.1/' + $s.Id + '.nupkg') + $lines }
        [IO.File]::WriteAllText((Join-Path $k.Dir 'nupkg/SHA256SUMS'), (($lines -join "`n") + "`n"))
        $script:Kits += $k
    }
}
function Invoke-Gates { $GuardOK = $true; . ([scriptblock]::Create($gates)) }

$all = @($Names | ForEach-Object { $_ -split "," } | Where-Object { $_ })   # pwsh -File passes a comma list as one string
$Release = '1.26.10.1'
New-Pub $all;                                  Check 'the kits at 1.26.10.1' { Invoke-Gates } $null; Remove-Item -Recurse -Force $Pub
New-Pub $all -OneLine;                         Check 'one-line SHA256SUMS files' { Invoke-Gates } $null; Remove-Item -Recurse -Force $Pub
$Release = '1.24.13.7'
New-Pub $all;                                  Check 'a release the kits were not packed against' { Invoke-Gates } 'packed against another release'; Remove-Item -Recurse -Force $Pub
$Release = '1.26.10.1'
New-Pub $all -wrongHash pflag;                 Check 'a wrong hash' { Invoke-Gates } 'checksum mismatch'; Remove-Item -Recurse -Force $Pub
New-Pub $all -pvOverride @{ pflag = '1.0.9' }; Check 'a version the package does not carry' { Invoke-Gates } 'the package version is not'; Remove-Item -Recurse -Force $Pub
New-Pub $all -noReleaseLine pflag;             Check 'a README without its release line' { Invoke-Gates } 'does not name release'; Remove-Item -Recurse -Force $Pub
New-Pub $all -requires @{ pflag = @('hashset') }; Check 'a dependency the nuspec does not name' { Invoke-Gates } 'does not require nugetgo.github.com.ritchiecarroll.hashset 1.0.0.3'; Remove-Item -Recurse -Force $Pub
Write-Host "failed $failed"
exit $failed
