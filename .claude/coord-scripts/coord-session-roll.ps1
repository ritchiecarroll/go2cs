<#
  coord-session-roll.ps1 -- the COORDINATOR handoff read.

  Run this FIRST in a fresh coordinator session. It is READ-ONLY: it fetches, measures and
  reports. It changes nothing, kills nothing, and merges nothing.

  Every figure is DERIVED, never carried. Where a number cannot be derived it says so rather
  than printing a stale constant -- the derivation-not-replacement rule.

  Traps designed out, each one paid for on 2026-09-06/07:
    * the toolchain pin ABORTS on mismatch; printing a pin is not checking it
    * the pin also asserts WHICH INSTALL -- a matching version string can come from an
      ambient toolchain when $GOROOT\bin holds no go.exe and PATH falls through
    * exit codes captured BEFORE any pipe ($LASTEXITCODE after a pipe is the PIPE's)
    * no `| head` on a question about EXISTENCE -- head is a silent WHERE clause
    * per-lane last-post EXCLUDES COORD posts (matching "COORD -> R" reports your own
      post as R's and makes a 5-hour-quiet lane look fresh)
    * process census never matches its own command line, and NEVER kills by name
      (Get-Process <name> | Stop-Process reaps sibling worktrees machine-wide)
    * unfiltered `git status --porcelain` -- a filtered status answers a different question
#>
[CmdletBinding()]
param(
    [string] $Repo    = 'C:\Projects\go2cs',
    [string] $Mailbox = 'origin/claude/mailbox',
    [switch] $NoFetch
)

$ErrorActionPreference = 'Continue'
function Line($t) { Write-Host ''; Write-Host "=== $t " -NoNewline; Write-Host ('=' * [Math]::Max(0, 62 - $t.Length)) }
function Item($k, $v) { Write-Host ("  {0,-26} {1}" -f $k, $v) }
function Warn($t) { Write-Host "  !! $t" }

# ---------------------------------------------------------------- 1. ENVIRONMENT
Line 'ENVIRONMENT (pin is ASSERTED, not printed)'
$env:DOTNET_ROOT = '$env:USERPROFILE\dotnet10'
$env:GOROOT      = '$env:USERPROFILE\sdk\go1.23.12'
$env:PATH        = "$env:GOROOT\bin;$env:DOTNET_ROOT;$env:PATH"
$env:MSBUILDDISABLENODEREUSE = '1'

$wantGo = 'go1.23.12'
$goVer  = (& go version) 2>&1 | Out-String
if ($goVer -notmatch [regex]::Escape($wantGo)) {
    Warn "TOOLCHAIN MISMATCH: wanted $wantGo, got $($goVer.Trim())"
    Warn 'ABORTING -- every measurement below would be taken against the wrong corpus.'
    exit 90
}
# The version string alone is not the pin: if $GOROOT\bin holds no go.exe, PATH falls THROUGH
# to an ambient toolchain and the version can still match while describing another install.
$resolved = (Get-Command go -ErrorAction SilentlyContinue).Source
$goRoot   = ((& go env GOROOT) 2>&1 | Out-String).Trim()
if ($resolved -notlike "$env:GOROOT*" -or $goRoot -ne $env:GOROOT) {
    Warn "go resolves to '$resolved' (GOROOT reports '$goRoot'), not the pinned $env:GOROOT"
    exit 91
}
Item 'go'     "$($goVer.Trim())  [resolved from the pinned root]"
Item 'dotnet' ((& dotnet --version) 2>&1)

# ---------------------------------------------------------------- 2. REFS
Line 'REFS'
Set-Location $Repo
if (-not $NoFetch) {
    & git fetch --prune --quiet origin 2>&1 | Out-Null
    $fetchRc = $LASTEXITCODE
    if ($fetchRc -ne 0) {
        Warn "git fetch exited $fetchRc -- refs below may be STALE."
        Warn 'A fetch that PRINTED AN ERROR may have left refs unmoved; verify before reading anything off them.'
    }
}
Item 'origin/master' (& git rev-parse --short origin/master 2>$null)
Item 'mailbox tip'   (& git rev-parse --short $Mailbox 2>$null)
Item 'local HEAD'    "$(& git rev-parse --abbrev-ref HEAD 2>$null) @ $(& git rev-parse --short HEAD 2>$null)"

# ---------------------------------------------------------------- 3. OBJECTIVE (derived)
Line 'OBJECTIVE (derived from the roster, never carried)'
# READ THE ROSTER FROM origin/master, NOT FROM THE WORKING TREE. This checkout can be many
# trains behind (its HEAD is printed above), and a figure read from a stale tree is a true
# statement about the wrong layer -- which is exactly the class this script exists to catch.
# Read as raw BYTES and decode UTF-8 explicitly: the header carries U+2212 MINUS and U+2014
# EM DASH, and PS 5.1's default read mangles both into multi-char ANSI sequences, which
# silently defeats any pattern written against the real characters.
$rosterBytes = & git show 'origin/master:docs/ValidatedTestPackages.md' 2>$null
$rtext = if ($rosterBytes) { ($rosterBytes -join "`n") } else { $null }
if ($rtext) {
    # the roster header is the authority and recomputes itself from its own table.
    # The dashes are matched as "one or more non-digits" rather than as literals, so the
    # pattern survives both the real glyphs and any transcoding of them.
    $m = [regex]::Match($rtext, 'implementable set \((?<tot>\d+)\D+?(?<exc>\d+) excluded = (?<impl>\d+)\):\s*(?<banked>\d+)\s*/\s*(?<den>\d+)\D+?(?<pct>[\d.]+)%')
    if ($m.Success) {
        Item 'banked / implementable' "$($m.Groups['banked'].Value) / $($m.Groups['den'].Value) = $($m.Groups['pct'].Value)%"
        Item 'testable total'         "$($m.Groups['tot'].Value)  (less $($m.Groups['exc'].Value) excluded)"
        Item 'REMAINING'              ([int]$m.Groups['den'].Value - [int]$m.Groups['banked'].Value)
    } else {
        Warn 'could not parse the roster header -- read it directly; do NOT quote a remembered figure'
    }
    # Both sides read at origin/master, for the same reason as the header above.
    $links   = @([regex]::Matches($rtext, 'validation/current/[a-z0-9._-]+\.md') | ForEach-Object { $_.Value } | Sort-Object -Unique)
    $tracked = @((& git ls-tree -r --name-only origin/master 'docs/validation/current/' 2>$null) |
                 ForEach-Object { $_ -replace '^docs/','' } | Sort-Object -Unique)
    $dead    = @($links | Where-Object { $tracked -notcontains $_ })
    Item 'proof links / pages' "$($links.Count) / $($tracked.Count)  (both at origin/master)"
    if ($dead.Count) { Warn "DEAD PROOF LINKS ($($dead.Count)): $($dead -join ', ')" }
} else {
    Warn 'could not read the roster from origin/master -- fetch first, then re-run'
}
$localHead = (& git rev-parse HEAD 2>$null)
$masterSha = (& git rev-parse origin/master 2>$null)
if ($localHead -ne $masterSha) {
    $behind = (& git rev-list --count "HEAD..origin/master" 2>$null)
    Warn "this checkout is $behind commits behind origin/master -- every figure above was read from the REF, not this tree"
}

# ---------------------------------------------------------------- 4. BRANCHES
Line 'BRANCHES AHEAD OF MASTER (seat candidates)'
$refs = & git for-each-ref --format='%(refname:short)' refs/remotes/origin/claude/ 2>$null
foreach ($r in $refs) {
    if ($r -eq $Mailbox) { continue }
    $n = (& git rev-list --count "origin/master..$r" 2>$null)
    if (-not $n -or [int]$n -eq 0) { continue }
    $files = @(& git diff --name-only "origin/master...$r" 2>$null)
    $docsOnly = ($files.Count -gt 0) -and -not ($files | Where-Object { $_ -notmatch '^docs/' -and $_ -notmatch '\.md$' })
    if ($docsOnly)                                                    { $kind = 'docs-only' }
    elseif ($files | Where-Object { $_ -like 'src/go2cs/*' })          { $kind = 'CONVERTER (CNR+two-seed)' }
    elseif ($files | Where-Object { $_ -like 'src/gen/*' })            { $kind = 'GEN (route #7)' }
    elseif ($files | Where-Object { $_ -like 'src/core/golib/*' })     { $kind = 'golib (slnx build)' }
    else                                                              { $kind = 'code' }
    $age = (& git log -1 --format='%cr' $r 2>$null)
    Write-Host ("  {0,-40} +{1,-3} {2,-28} {3,3} files  {4}" -f ($r -replace '^origin/claude/',''), $n, $kind, $files.Count, $age)
}

# ---------------------------------------------------------------- 5. LANES
Line 'LANE ACTIVITY (COORD posts EXCLUDED -- your own post reports a quiet lane as fresh)'
$log = & git log $Mailbox --format='%cr|%s' -120 2>$null
foreach ($lane in @('R','G','C1','C2','i9')) {
    $hit = $log | Where-Object { $_ -match "mailbox: $lane ->" } | Select-Object -First 1
    if ($hit) {
        $parts = $hit -split '\|', 2
        $subj  = $parts[1].Substring(0, [Math]::Min(76, $parts[1].Length))
        Write-Host ("  {0,-4} {1,-16} {2}" -f $lane, $parts[0], $subj)
    } else {
        Write-Host ("  {0,-4} {1}" -f $lane, '(no post in the last 120 mailbox commits)')
    }
}

# ---------------------------------------------------------------- 6. IN FLIGHT
Line 'IN FLIGHT (never kill by NAME -- that reaps sibling worktrees machine-wide)'
$me = $PID
$procs = @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
           Where-Object { $_.Name -match '^(go2cs|dotnet|testhost|MSBuild|powershell)\.exe$' -and $_.ProcessId -ne $me })
$batteries = @($procs | Where-Object { $_.CommandLine -match 'coord-t\d+-(union|canaries)|run-validated-sweep|check-no-regression|BehavioralRunner' })
if ($batteries.Count) {
    foreach ($b in $batteries) {
        $age = try { [int]((Get-Date) - $b.CreationDate).TotalMinutes } catch { '?' }
        $cmd = $b.CommandLine.Substring(0, [Math]::Min(66, $b.CommandLine.Length))
        Write-Host ("  PID {0,-7} {1,-14} {2,4} min   {3}" -f $b.ProcessId, $b.Name, $age, $cmd)
    }
    Warn 'A battery is RUNNING: the mid-battery SOURCE FREEZE binds its worktree. Do not merge there.'
} else {
    Item 'batteries' 'none running'
}
Item 'build children' @($procs | Where-Object { $_.Name -match '^(go2cs|dotnet|MSBuild)\.exe$' }).Count

# ---------------------------------------------------------------- 7. WORKTREES
Line 'WORKTREES (UNFILTERED status -- a filtered one answers a different question)'
foreach ($w in (& git worktree list 2>$null)) {
    $path = ($w -split '\s+')[0]
    if (-not (Test-Path $path)) { continue }
    $dirt = @(& git -C $path status --porcelain 2>$null)
    $head = (& git -C $path rev-parse --short HEAD 2>$null)
    $br   = (& git -C $path rev-parse --abbrev-ref HEAD 2>$null)
    $flag = if ($dirt.Count -gt 0) { "DIRTY $($dirt.Count)" } else { 'clean' }
    Write-Host ("  {0,-52} {1,-10} {2,-34} {3}" -f (Split-Path $path -Leaf), $head, $br, $flag)
}

# ---------------------------------------------------------------- 8. DISK
Line 'DISK'
$d = Get-PSDrive -Name C -ErrorAction SilentlyContinue
if ($d) {
    $freeGB = [math]::Round($d.Free / 1GB, 1)
    Item 'C: free' "$freeGB GB"
    if ($freeGB -lt 40) { Warn 'under 40 GB -- purge bin/obj before any battery (clean-bin.ps1, depth-UNLIMITED)' }
}

# ---------------------------------------------------------------- 9. HANDOFF
Line 'HANDOFF'
$mem = '$env:USERPROFILE\.claude\projects\C--Projects-go2cs\memory'
Item 'read FIRST' 'coordinator-handoff-state.md'
Item 'then'       'train14-seat-ledger.md, objective-remaining-rows-2026-09-02.md'
$acc = Join-Path $mem 'doctrine-batch3-accumulator.md'
if (Test-Path $acc) {
    Item 'doctrine items' ([regex]::Matches((Get-Content $acc -Raw), '(?m)^\d+\.\s')).Count
}
Write-Host ''
Write-Host '  RE-ARM THE MAILBOX MONITOR AND THE QUIET-WATCH BEFORE ANYTHING ELSE.'
Write-Host '  A task id that has EXITED is evidence of a PAST arming, not a live one.'
Write-Host ''
exit 0
