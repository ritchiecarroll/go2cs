# coord-mailbox-fleetguard.ps1 -- run master's fleet-identifier guard against a mailbox clone's TREE.
#
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File coord-mailbox-fleetguard.ps1 [-ClonePath <dir>] [-Quiet]
# Exit 0 = no hits. Non-zero = hits, or the guard could not be run at all (it FAILS CLOSED: an
# instrument that cannot measure must never report success -- CLAUDE.md false-green route #6).
#
# WHY THIS EXISTS, and what it does NOT duplicate.
#
# coord-mailbox-post.ps1 already carries an armed, exit-gated IDENTIFIER CENSUS (exit 8, 2026-09-07).
# That census is DIFF-SCOPED: it reads the entry file and the commit subject -- the delta being pushed --
# which is the shape CLAUDE.md prescribes ("the pre-post census runs before EVERY push -- diff-scoped
# and EXIT-GATED"). It is genuinely armed; measured 2026-09-08 with four controls, it refuses a planted
# profile path, a planted UNC, and the account literal in the subject (arms c and d), and admits a clean
# entry at exit 0.
#
# What NOTHING covered until now is the mailbox branch's STANDING TREE. src/go2cs/fleetIdentifierCensus_test.go
# runs in the converter's own `go test`, so it guards MASTER's tree and only master's -- the file does not
# exist on claude/mailbox at all, and no gate of any kind read that branch's content. On 2026-09-08 master's
# guard, run against the mailbox tree, read SEVEN hits in docs/phase4/MAILBOX.md that had stood since before
# the post-tool census landed: pre-census residue that a delta-scoped guard can never see, because it was
# never in anyone's delta. This script closes exactly that gap and nothing else.
#
# So the two are complementary, not a duplicated census: the post tool's arms answer "is the NEW entry
# clean?", this one answers "is the TREE clean?". Doctrine's warning that "a second census duplicating a
# gate adds a way to be wrong and no way to be right" is about a second census with no additional reach;
# this one's reach is the 11,458 tracked files the other never opens.
#
# METHOD. Master's guard resolves its repository root structurally -- repoRootFromPackageDir walks up two
# directories from the working directory and requires a .git there -- and offers NO env var and NO flag to
# override it. So the only way to point it at another tree is to run it FROM that tree. It is placed at
# <clone>\src\go2csguard, a NEW untracked directory, rather than copied over the clone's own src\go2cs:
# the guard reads the WORKING-TREE content of every tracked path, so overwriting src\go2cs would silently
# substitute master's content for the branch's across 238 tracked files and change the very thing being
# measured. At an untracked path the scanned tree is untouched. The directory is removed on every exit
# path, including failure.
#
# The guard enumerates `git ls-files` and reads each path from the WORKING TREE, so an entry already
# appended to the tracked MAILBOX.md but not yet committed IS covered. That is why the call site in
# coord-mailbox-post.ps1 sits AFTER the append and BEFORE the commit and push.
param(
    [string] $ClonePath = 'C:\Projects\go2cs-mailbox-coord',
    [string] $MainCheckout = 'C:\Projects\go2cs',
    [switch] $Quiet
)
$ErrorActionPreference = 'Continue'   # native git/go write to stderr; a caller at 'Stop' would die on the first line

function fgFail([string]$why, [int]$code) { Write-Host "FLEET GUARD REFUSED: $why"; exit $code }

if (-not (Test-Path (Join-Path $ClonePath '.git')))          { fgFail "no git clone at $ClonePath -- it cannot know, so it does not pass" 11 }
if (-not (Test-Path (Join-Path $ClonePath 'docs\phase4\MAILBOX.md'))) { fgFail "no docs\phase4\MAILBOX.md under $ClonePath" 11 }
$guardSrc = Join-Path $MainCheckout 'src\go2cs'
if (-not (Test-Path (Join-Path $guardSrc 'internal/repoguard/fleetIdentifierCensus_test.go'))) { fgFail "master's guard is not at $guardSrc -- nothing to run" 11 }

# TOOLCHAIN PIN, ASSERTED not printed. CLAUDE.md: "an instrument that prints its pin and proceeds has no
# guard -- it ABORTS on mismatch, and the print is only evidence of what the abort compared." The root is
# DERIVED from the profile so this script never spells a profile path (the order it enforces applies to it).
$goRoot = Join-Path $env:USERPROFILE 'sdk\go1.24.13'
$goExe = Join-Path $goRoot 'bin\go.exe'
if (-not (Test-Path $goExe)) { fgFail "the pinned Go toolchain is not installed where expected" 12 }
$env:GOROOT = $goRoot
$env:GOTOOLCHAIN = 'local'
$env:PATH = (Join-Path $goRoot 'bin') + ';' + $env:PATH
$goVer = (& $goExe version) 2>&1
if ("$goVer" -notmatch 'go1\.24\.13') { fgFail "toolchain mismatch: the pinned release did not answer" 12 }

$stage = Join-Path $ClonePath 'src\go2csguard'
try {
    if (Test-Path $stage) { Remove-Item -Recurse -Force $stage -ErrorAction SilentlyContinue }
    Copy-Item -Recurse -Force $guardSrc $stage -ErrorAction Stop
    # Build output would be scanned as part of nothing (it is untracked) but slows the copy and the build.
    foreach ($junk in @('bin','obj')) {
        $j = Join-Path $stage $junk
        if (Test-Path $j) { Remove-Item -Recurse -Force $j -ErrorAction SilentlyContinue }
    }
    if (-not (Test-Path (Join-Path $stage 'go.mod'))) { fgFail "the staged guard package has no go.mod -- the copy is incomplete" 12 }

    Push-Location $stage
    # ONLY the tracked-files test. TestFleetIdentifierClearancesAreLive asserts master's clearance list
    # against the tree it runs in, and a mailbox clone legitimately lacks some master-only files it names --
    # a SCOPE artifact of running master's guard elsewhere, never a finding about this tree.
    $out = & $goExe test -count=1 -run 'TestNoFleetIdentifiersInTrackedFiles' ./internal/repoguard 2>&1
    $goRc = $LASTEXITCODE          # captured BEFORE anything else can touch $LASTEXITCODE or $?
    Pop-Location

    $text = ($out | ForEach-Object { "$_" }) -join "`n"
    # Findings are printed by the guard as "  <path>:<line> [<kind>]" and carry NO offending text by
    # design. Only those three fields are echoed here, for the same reason: a failing log is a surface too.
    $hits = @()
    foreach ($m in [regex]::Matches($text, '(?m)^\s+(\S+):(\d+)\s+\[([a-z-]+)\]\s*$')) {
        $hits += [pscustomobject]@{ Path = $m.Groups[1].Value; Line = [int]$m.Groups[2].Value; Kind = $m.Groups[3].Value }
    }

    if ($goRc -eq 0) {
        if (-not $Quiet) { Write-Host "FLEET GUARD: 0 hit(s) over the tracked tree at $ClonePath" }
        exit 0
    }
    # Non-zero with no parsed findings is NOT a pass: the guard refused to run (a broken enumeration, an
    # unreadable tree, a build failure). Report it as the infrastructure failure it is.
    if ($hits.Count -eq 0) {
        Write-Host "FLEET GUARD REFUSED: the guard exited $goRc without reporting findings -- it did not measure. Tail:"
        ($text -split "`n" | Select-Object -Last 12) | ForEach-Object { "  $_" }
        exit 13
    }
    Write-Host "FLEET GUARD: $($hits.Count) fleet-identifier hit(s) in the tracked tree at $ClonePath"
    Write-Host "The owner's 2026-09-01 security order keeps real machine names, non-public usernames,"
    Write-Host "profile paths and UNC/share names off every pushed surface. Substitute the identifier ALONE"
    Write-Host "(<user> for an account segment, the machine's fleet nickname or <host> for a host), leaving"
    Write-Host "the rest of the line intact. The identifier itself is NOT printed."
    foreach ($h in $hits) { Write-Host ("  " + $h.Path + ":" + $h.Line + " [" + $h.Kind + "]") }
    exit 10
}
finally {
    if (Test-Path $stage) { Remove-Item -Recurse -Force $stage -ErrorAction SilentlyContinue }
}
