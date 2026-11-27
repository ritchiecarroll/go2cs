<#
.SYNOPSIS
    Guards the sweep's row target: Resolve-SweepPlatforms and its interim-pin table, EMPTY since D7.

.DESCRIPTION
    run-validated-sweep.ps1 converts and builds every row at the target Resolve-SweepPlatforms
    (src/_roster.ps1) answers: an explicit -Platforms, or the interim pin for this host
    ($SweepInterimPlatformPins), or nothing. From the seat that added darwin/arm64 to the corpus
    universe (D2) to D7, the table held one pin, darwin/arm64 -> darwin/amd64, which kept an
    Apple-silicon sweep on the flavour every darwin row was banked on
    (docs/phase4/DESIGN-darwin-arm64-flavour.md section 6, ruling R5). D7 re-banked the osx-arm64
    cells on darwin/arm64 and emptied the table in the same seat (section 7), so an Apple-silicon
    host now sweeps its own flavour. The table assertion holds it empty: a pin added later is a
    ruling, and this file changes with it. The pin branch of the function is still exercised, with
    a table passed in, so a future pin finds it working. None of it needs a Mac, so it is exercised
    HERE, over the function and the table, with the host passed in.

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
Check 'empty since D7 (the darwin/arm64 -> darwin/amd64 pin came off with the osx-arm64 re-bank)' `
    ($SweepInterimPlatformPins -is [hashtable] -and $SweepInterimPlatformPins.Count -eq 0) `
    "table: $(($SweepInterimPlatformPins.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ', ')"

Write-Host 'no -Platforms, the table as it stands'
foreach ($h in @(@('darwin', 'arm64'), @('darwin', 'amd64'), @('linux', 'amd64'), @('linux', 'arm64'), @('windows', 'amd64'))) {
    $r = Resolve @{ Explicit = ''; HostGoos = $h[0]; HostGoarch = $h[1]; Universe = $five }
    Check "a $($h[0])/$($h[1]) host passes nothing" ($r.Value -ceq '' -and -not $r.Error) "got '$($r.Value)' $($r.Error)"
}

Write-Host 'no -Platforms, a pin passed in (the branch a future pin takes)'
$pin = @{ 'darwin/arm64' = 'darwin/amd64' }
foreach ($u in @(@{ Name = 'four'; List = $four }, @{ Name = 'five'; List = $five })) {
    $r = Resolve @{ Explicit = ''; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $u.List; Pins = $pin }
    Check "a pinned host takes its pin at the $($u.Name)-target universe" ($r.Value -ceq 'darwin/amd64') "got '$($r.Value)' $($r.Error)"
}
foreach ($h in @(@('darwin', 'amd64'), @('linux', 'arm64'))) {
    $r = Resolve @{ Explicit = ''; HostGoos = $h[0]; HostGoarch = $h[1]; Universe = $five; Pins = $pin }
    Check "a $($h[0])/$($h[1]) host passes nothing (near-miss of the pin)" ($r.Value -ceq '' -and -not $r.Error) "got '$($r.Value)' $($r.Error)"
}
$r = Resolve @{ Explicit = 'darwin/arm64'; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $five; Pins = $pin }
Check 'an explicit -Platforms wins over a pin' ($r.Value -ceq 'darwin/arm64') "got '$($r.Value)' $($r.Error)"
$r = Resolve @{ Explicit = ''; HostGoos = ''; HostGoarch = ''; Universe = $five }
Check 'a host this harness cannot name passes nothing' ($r.Value -ceq '' -and -not $r.Error) "got '$($r.Value)' $($r.Error)"

Write-Host 'an explicit -Platforms'
$r = Resolve @{ Explicit = 'darwin/arm64'; HostGoos = 'darwin'; HostGoarch = 'arm64'; Universe = $five }
Check 'names darwin/arm64 on an Apple-silicon host' ($r.Value -ceq 'darwin/arm64') "got '$($r.Value)' $($r.Error)"
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
