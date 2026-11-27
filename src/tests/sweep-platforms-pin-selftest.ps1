<#
.SYNOPSIS
    Guards the sweep's row target: Resolve-SweepPlatforms and the darwin/arm64 interim pin.

.DESCRIPTION
    run-validated-sweep.ps1 converts and builds every row at the target Resolve-SweepPlatforms
    (src/_roster.ps1) answers: an explicit -Platforms, or the interim pin for this host
    ($SweepInterimPlatformPins), or nothing. The pin is what keeps an Apple-silicon sweep on the
    darwin/amd64 flavour, whose counts every darwin row is banked on, from the seat that adds
    darwin/arm64 to the corpus universe (D2) until the bank re-reads the osx-arm64 cells (D7)
    (docs/phase4/DESIGN-darwin-arm64-flavour.md section 6, ruling R5). Getting it wrong in one
    direction makes every row of a hosted macos-15 sweep read a regression; in the other it lets a
    sweep convert a row at a target no corpus flavour serves. Neither needs a Mac to exercise, so
    it is exercised HERE, over the function and the table, with the host passed in.

    The table assertion is the reminder: D7 removes the pin, and this file changes with it.

        ./sweep-platforms-pin-selftest.ps1

.NOTES
    Requires PowerShell 5.1 (Windows) or PowerShell 7+ (any platform). Exit 0 clean, 1 on any
    violation. No build, no converter, no corpus. No non-ASCII literal appears in this file.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '../_roster.ps1')

$failures = 0
function Check([string] $name, [bool] $ok, [string] $detail) {
    if ($ok) { Write-Host "  ok   $name" }
    else { Write-Host "  FAIL $name -- $detail"; $script:failures++ }
}

function Resolve([hashtable] $a) {
    try { return [PSCustomObject]@{ Value = (Resolve-SweepPlatforms @a); Error = '' } }
    catch { return [PSCustomObject]@{ Value = $null; Error = $_.Exception.Message } }
}

$four = @('windows/amd64', 'linux/amd64', 'darwin/amd64', 'linux/arm64')
$five = $four + @('darwin/arm64')

Write-Host 'the interim pin table'
Check 'one entry, darwin/arm64 -> darwin/amd64 (D7 removes it, and this assertion with it)' `
    ($SweepInterimPlatformPins.Count -eq 1 -and $SweepInterimPlatformPins['darwin/arm64'] -ceq 'darwin/amd64') `
    "table: $(($SweepInterimPlatformPins.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ', ')"

Write-Host 'no -Platforms'
foreach ($u in @(@{ Name = 'four'; List = $four }, @{ Name = 'five'; List = $five })) {
    $r = Resolve @{ Explicit = ''; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $u.List }
    Check "an Apple-silicon host takes the pin at the $($u.Name)-target universe" ($r.Value -ceq 'darwin/amd64') "got '$($r.Value)' $($r.Error)"
}
foreach ($h in @(@('darwin', 'amd64'), @('linux', 'amd64'), @('linux', 'arm64'), @('windows', 'amd64'))) {
    $r = Resolve @{ Explicit = ''; HostGoos = $h[0]; HostGoarch = $h[1]; Universe = $five }
    Check "a $($h[0])/$($h[1]) host passes nothing (near-miss of the pin)" ($r.Value -ceq '' -and -not $r.Error) "got '$($r.Value)' $($r.Error)"
}
$r = Resolve @{ Explicit = ''; HostGoos = ''; HostGoarch = ''; Universe = $five }
Check 'a host this harness cannot name passes nothing' ($r.Value -ceq '' -and -not $r.Error) "got '$($r.Value)' $($r.Error)"

Write-Host 'an explicit -Platforms'
$r = Resolve @{ Explicit = 'darwin/arm64'; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $five }
Check 'wins over the pin once darwin/arm64 is in the universe (D7''s re-read)' ($r.Value -ceq 'darwin/arm64') "got '$($r.Value)' $($r.Error)"
$r = Resolve @{ Explicit = ' darwin/amd64 '; HostGoos = 'darwin'; HostGoarch = 'amd64'; Universe = $four }
Check 'is trimmed' ($r.Value -ceq 'darwin/amd64') "got '$($r.Value)' $($r.Error)"
$r = Resolve @{ Explicit = 'darwin/arm64'; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $four }
Check 'is refused when no corpus flavour serves it' ($r.Error -match 'not a target of the corpus universe') "got '$($r.Value)' '$($r.Error)'"
$r = Resolve @{ Explicit = 'linux/amd64'; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $five }
Check 'is refused on another GOOS' ($r.Error -match "host's GOOS is darwin") "got '$($r.Value)' '$($r.Error)'"
foreach ($bad in @('darwin', 'darwin/amd64,darwin/arm64', 'Darwin/AMD64', 'darwin/amd64/x')) {
    $r = Resolve @{ Explicit = $bad; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $five }
    Check "is refused as malformed: '$bad'" ($r.Error -match 'ONE target written os/arch') "got '$($r.Value)' '$($r.Error)'"
}

Write-Host 'the guards on the function''s own inputs'
$r = Resolve @{ Explicit = ''; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = @() }
Check 'refuses with no universe to check against' ($r.Error -match 'no corpus universe') "got '$($r.Value)' '$($r.Error)'"
$r = Resolve @{ Explicit = ''; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $five; Pins = @{ 'darwin/arm64' = 'darwin/386' } }
Check 'refuses a pin outside the universe' ($r.Error -match 'interim pin for a darwin/arm64 host names darwin/386') "got '$($r.Value)' '$($r.Error)'"

Write-Host 'the sweep threads it to every row'
$sweep = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../run-validated-sweep.ps1') -Raw
Check 'resolves -Platforms once, after _paths.ps1 and _roster.ps1' `
    ($sweep -match '(?s)_paths\.ps1.*_roster\.ps1.*\$Platforms = Resolve-SweepPlatforms -Explicit \$Platforms -HostGoos \$HostGoos -HostGoarch \$HostGoarch') 'no resolution line in that order'
Check 'adds -platforms to each row''s converter arguments' ($sweep -match "\`$execArgs \+= @\('-platforms', \`$Platforms\)") 'no -platforms argument line'

Write-Host ''
if ($failures) { Write-Host "sweep-platforms-pin-selftest: $failures FAILED"; exit 1 }
Write-Host 'sweep-platforms-pin-selftest: all checks passed'
exit 0
