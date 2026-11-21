#Requires -Version 5.1
<#
.SYNOPSIS
    Publish a signed release of the go2cs NuGet packages, end to end, on ONE machine.

.DESCRIPTION
    Pack -> commit the record -> sign -> push, as one ritual. It supersedes the
    offline-signing flow (`-OfflineSigning` keeps that path for a machine whose signing
    certificate lives elsewhere), and it exists because the certificate now lives HERE: the
    file-shuttle between machines was the only reason the release was ever two commands.

    THE ONLY HUMAN ACTION IS THE PIN. The Smart Card KSP caches a card PIN for the life of the
    process that unlocked it, so the whole package set signs inside ONE `dotnet nuget sign`
    invocation and one prompt unlocks it (measured: 6 packages, one prompt, 9 s). Phase 1b asks
    once before that, for the go2cs dotnet tool: its Windows executables are Authenticode-signed
    by one signtool process before they are packed.

    THE RECORD IS COMMITTED BEFORE THE TOOL IS BUILT (2026-11-20). Phase 1b commits the release's
    record (version.props, the frozen proof, the retargeted badges, corpus-release.txt) LOCALLY,
    signed, and builds the go2cs dotnet tool from that clean commit, so each binary's -version reads
    vcs.revision=<the record commit> and vcs.modified=false. Nothing is pushed before Phase 4, and the
    tag Phase 1 minted stays where it was. (1.26.11.1's tool, built from the tag commit over the
    uncommitted record, read the tag's revision and vcs.modified=true.)

    SIGNING IS MANDATORY, NOT OPTIONAL. The owner's code-signing certificate is REGISTERED with
    nuget.org (2026-08-24), and registration is an enforcement switch: nuget.org rejects any
    package pushed under that account which is not signed by a registered certificate. An
    unsigned push does not publish-with-a-warning -- it fails. That is why Phase 0 proves the
    certificate is reachable before anything is bumped, and why -OfflineSigning still signs
    rather than skipping.

    WHAT IS IRREVERSIBLE, AND WHERE. Phase 3 publishes to nuget.org, and a published version can
    be unlisted but never deleted. Everything before it is recoverable: -WhatIf stops after the
    census, a failed pack leaves only a bumped version.props (restore with
    `git checkout src/version.props` and delete the tag), the record commit is local until Phase 4
    (undo it with `git reset --soft HEAD~1`), and a failed sign leaves unsigned packages in the
    artifacts folder. The push is therefore gated on an explicit confirmation
    unless -Yes is passed, and the gate prints exactly what is about to become permanent.

    PHASE 3 IS RESUMABLE (2026-10-26). nuget.org caps pushes per API key per window: the 1.26.10.1
    Phase 3 died with 403 Quota Exceeded (Retry-After 2489 s) on push 351 of 362. The push now goes
    one package at a time (src\_nugetpush.ps1), skips every version the feed's flat container already
    has, and waits out a refusal that carries a Retry-After, printing when it resumes. A push that
    stops anyway is finished with -ResumePush, which re-enters Phase 3 without bumping, packing or
    signing again.

.PARAMETER ResumePush
    Finish a Phase 3 that stopped: push the signed packages already in the artifacts folder at the
    version src\version.props names, skipping every one the feed already has. Nothing is bumped,
    packed, signed or tagged, and the signing certificate is not needed. HEAD must be the release's
    local record commit (Phase 1b), not yet pushed: it is checked before the confirmation, and nothing is
    pushed otherwise. Every package in the folder must carry that version, or nothing is pushed. Refused
    with -OfflineSigning.

.PARAMETER Source
    The feed Phase 3 pushes to: nuget.org's v3 service index unless a rehearsal names another
    (src\tests\nuget-push-selftest.ps1 runs -ResumePush against a mock feed on the loopback).

.PARAMETER OfflineSigning
    Stop after packing and print the copy-to-signing-machine instructions, then wait -- the
    pre-2026-08-24 flow, for a machine without the signing certificate.

.PARAMETER Yes
    Skip the pre-push confirmation. For an operator who has read the census and wants the
    ritual to run through the PIN prompt and out the other side without a second keystroke.

.PARAMETER VerifyOnly
    Forward to push-nuget.ps1 -VerifyOnly and exit with its code: verify every green badge's
    arithmetic against its proof page, every package README's C# Source badge, and the four-number
    release census. Nothing is bumped, tagged, frozen, packed, signed or pushed.

    It runs BEFORE this script's own Phase 0 gates and deliberately skips them. Those gates guard a
    PUBLICATION -- a clean tree, an API key, a card in the reader -- and none of them bears on
    whether the tree's badges agree with its proof pages. Requiring them would make the check
    unrunnable on any machine but release morning's, which is the opposite of the point: this is
    what to run after the last roster-moving sweep, days ahead, so the release never discovers a
    stale badge with a signed tag already minted.

.PARAMETER WhatIf
    Census only: verify the certificate is reachable and report what would be packed. Nothing
    is bumped, packed, signed, tagged or pushed. NOTE it stops at the end of Phase 0 and is NOT
    forwarded to the children, so it does not exercise push-nuget.ps1 at all -- -VerifyOnly is the
    switch that checks the tree.

.EXAMPLE
    .\release-nuget.ps1 -WhatIf
    What would happen, including whether the signing certificate is reachable.

.EXAMPLE
    .\release-nuget.ps1 -VerifyOnly
    Is the tree releasable? Seconds, no card, no network, nothing written.

.EXAMPLE
    .\release-nuget.ps1
    The full ritual: pack, commit the record, sign (one PIN), confirm, push, then print the
    commands that push the record commit and the tag.

.EXAMPLE
    .\release-nuget.ps1 -ResumePush
    Finish a publish whose Phase 3 stopped: push what the feed does not have yet, then print the
    commands that push the record commit and the tag.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch] $OfflineSigning,
    [switch] $Yes,
    [switch] $VerifyOnly,
    [switch] $ResumePush,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $OutDir
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '_paths.ps1')

if (-not $OutDir) { $OutDir = Join-Path $SrcRoot 'artifacts/nupkg' }

$push = Join-Path $PSScriptRoot 'push-nuget.ps1'
$sign = Join-Path $PSScriptRoot 'sign-nupkgs.ps1'

function Write-Phase([string] $Message) {
    Write-Host ''
    Write-Host "=== $Message ===" -ForegroundColor Cyan
}
function Die([string] $Message) { Write-Host $Message -ForegroundColor Red; exit 1 }

# The sibling instruments end with `exit`, and an `exit` inside a script invoked with `&` in the
# SAME runspace terminates the HOST -- so this orchestrator died mid-preflight rather than read
# the signer's verdict, the first time Phase 0 ran. Every sibling call therefore goes through a
# CHILD PROCESS, whose exit code is a value rather than a fate. (The card PIN cache is
# unaffected: it lives in the `dotnet nuget sign` process, not in PowerShell.)
#
# THE CHILD'S EXIT CODE DECIDES, NEVER A STDERR LINE (2026-09-24, ledger 620ba7a2b8). This ran the
# child with `2>&1` under the script's $ErrorActionPreference = 'Stop', and under 'Stop' Windows
# PowerShell turns the FIRST stderr line of a redirected native command into a terminating
# NativeCommandError. A child that failed by throwing -- which is how push-nuget.ps1 reports every
# red -- therefore killed THIS script at that line: none of the child's output printed (a red
# -VerifyOnly showed a truncated throw and no census line, no named problem), and the caller's own
# failure message never ran (Phase 1's "version.props was already bumped ... delete the tag" recovery
# text, which is the one line an operator needs after a bump). Measured on the i7 at bccf8d977b.
#
# Now: 'Continue' in THIS function's scope only, so a stderr line is an ordinary record; every line,
# stdout and stderr, is streamed to the console AS IT ARRIVES when -Passthru is given (a Phase-1 pack
# runs for tens of minutes, and a buffered console looks hung) and collected as text for callers that
# read it; and the verdict is $LASTEXITCODE alone, which is the child's exit code.
function Invoke-Sibling {
    param([string] $Script, [string[]] $Arguments, [switch] $Passthru)
    $ErrorActionPreference = 'Continue'
    $lines = New-Object System.Collections.Generic.List[string]
    & powershell -NoProfile -ExecutionPolicy Bypass -File $Script @Arguments 2>&1 | ForEach-Object {
        $line = if ($_ -is [System.Management.Automation.ErrorRecord]) { $_.ToString() } else { "$_" }
        $lines.Add($line)
        if ($Passthru) { Write-Host $line }
    }
    $code = $LASTEXITCODE
    [pscustomobject]@{ Output = $lines.ToArray(); ExitCode = $code }
}

# ---- -VerifyOnly: the tree check, ahead of every publication gate ----------------------------
# Placed BEFORE Phase 0 on purpose. Phase 0's gates -- a clean tree, NUGET_API_KEY, a signing card
# in the reader -- are preconditions of PUBLISHING, and none of them bears on whether the badges
# agree with the proof pages. Gating this behind them would confine the cheapest check in the
# release to the one machine and the one morning that need it least.
if ($VerifyOnly) {
    Write-Phase 'Verify only: badges, proof pages and the release census'
    $verify = Invoke-Sibling -Script $push -Arguments @('-VerifyOnly') -Passthru
    if ($verify.ExitCode -ne 0) { Die "Verification FAILED ($($verify.ExitCode)). Nothing was bumped, tagged, frozen, packed or pushed." }
    Write-Host ''
    Write-Host '  Tree is releasable.' -ForegroundColor Green
    exit 0
}

# ---- Phase 0: the preconditions, all of them, before anything moves --------------------------
# Every check here is one a later phase would have hit anyway -- the point is to hit them while
# nothing has been bumped, tagged, packed or published. A release that discovers a missing API
# key AFTER minting a signed tag has made work for someone.

Write-Phase 'Phase 0: preflight'

if ($ResumePush -and $OfflineSigning) { Die '-ResumePush pushes packages that are already signed; it takes no -OfflineSigning.' }

# A resumed push follows a Phase 1b that committed the record locally, so its tree is normally clean; it is
# not checked, because a resume moves nothing but the push.
$dirty = @(git -C $RepoRoot status --porcelain 2>$null)
if ($ResumePush) {
    Write-Host "  working tree     : not checked (-ResumePush; $($dirty.Count) uncommitted path(s); HEAD $(git -C $RepoRoot log -1 --format='%h %s' 2>$null))"
}
elseif ($dirty.Count -gt 0) {
    Write-Host 'Working tree is not clean:' -ForegroundColor Red
    $dirty | Select-Object -First 8 | ForEach-Object { Write-Host "    $_" }
    Die 'A release commits version.props, a proof snapshot and retargeted badges together; unrelated changes must not ride along.'
}
else {
    Write-Host '  working tree     : clean'
}

# A release is cut on master, or on a maintenance line's release/go1.N branch (docs/PLAN-version-lines.md, T4);
# any other branch is worth a second look, so it is flagged, never refused.
function Test-GoReleaseBranch {
    param([AllowEmptyString()][string] $Branch)

    return ($Branch -ceq 'master' -or $Branch -cmatch '^release/go[0-9]+\.[0-9]+$')
}

$branch = (git -C $RepoRoot branch --show-current 2>$null)
Write-Host "  branch           : $branch"
if (-not (Test-GoReleaseBranch $branch)) { Write-Host '    (not master or a release/go1.N line -- intentional?)' -ForegroundColor Yellow }

# THE SDK IS READ HERE, NOT DISCOVERED BY THE FIRST BUILD (2026-10-06, the 1.24.13.4 run on the i7).
# That run's shell resolved a machine dotnet with no SDK satisfying global.json, and the first build
# to notice was Phase 1's -- AFTER version.props was bumped, the tag minted, the snapshot frozen and
# the READMEs retargeted. `dotnet --version` run from the repository root applies global.json's own
# resolution (version and rollForward) and exits non-zero when nothing satisfies it, so its exit code
# is the whole test. Every child process inherits this PATH, so the SDK read here is the one Phase 1
# packs with. 'Continue' in this function only: under 'Stop', Windows PowerShell turns the muxer's
# first stderr line into a terminating error and the remedy below would never print (see
# Invoke-Sibling). A dotnet absent from PATH is caught as the same refusal.
function Read-DotnetSdk {
    $ErrorActionPreference = 'Continue'
    Push-Location $RepoRoot
    try {
        $out = @(& dotnet --version 2>&1 | ForEach-Object { "$_" })
        [pscustomobject]@{ Output = $out; ExitCode = $LASTEXITCODE }
    } catch {
        [pscustomobject]@{ Output = @("$_"); ExitCode = -1 }
    } finally {
        Pop-Location
    }
}
$sdkWanted = (Get-Content -Raw (Join-Path $RepoRoot 'global.json') | ConvertFrom-Json).sdk
$sdk = Read-DotnetSdk
if ($sdk.ExitCode -ne 0) {
    $sdk.Output | Where-Object { $_.Trim() } | Select-Object -First 8 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
    Die ("No .NET SDK satisfying global.json ($($sdkWanted.version), rollForward $($sdkWanted.rollForward)) resolves from the repository root (dotnet --version exit $($sdk.ExitCode)). " +
         "Put that SDK's root first on PATH in this shell, confirm with 'dotnet --version' from $RepoRoot, and re-run. Nothing was bumped, tagged, frozen or packed.")
}
$dotnetPath = (Get-Command dotnet).Source
Write-Host "  .NET SDK         : $($sdk.Output[-1]) ($dotnetPath; global.json $($sdkWanted.version), rollForward $($sdkWanted.rollForward))"

# A resolution that SUCCEEDS can still be the wrong SDK (2026-11-20, the latestMajor pin): global.json takes the
# highest installed SDK at or above its floor, newer majors included, and a machine install puts an 11.x SDK in the
# same root as 10.0.4xx. A release packs with the SDK of its own framework's major, read from the hoisted
# <TargetFramework> in src/Directory.Build.props (net10.0 -> 10), so a hop moves this check with the framework.
$tfmMatch = [regex]::Match((Get-Content -Raw (Join-Path $SrcRoot 'Directory.Build.props')), '<TargetFramework\b[^>]*>net(\d+)\.\d+</TargetFramework>')
if (-not $tfmMatch.Success) {
    Die 'src/Directory.Build.props names no netN.M <TargetFramework>, so Phase 0 cannot tell which SDK major this release packs with. Nothing was bumped, tagged, frozen or packed.'
}
$releaseMajor = $tfmMatch.Groups[1].Value
$sdkVersion = "$($sdk.Output[-1])".Trim()
if ($sdkVersion.Split('.')[0] -ne $releaseMajor) {
    Die ("The .NET SDK that resolves from the repository root is $sdkVersion ($dotnetPath), not a $releaseMajor.x SDK: the release packs net$releaseMajor.0 " +
         "with the SDK of that major. global.json's latestMajor takes the highest SDK installed, so put a root holding only the $releaseMajor.x SDK first on PATH, " +
         "confirm with 'dotnet --version' from $RepoRoot, and re-run. Nothing was bumped, tagged, frozen or packed.")
}

# Phase 1b cross-compiles the go2cs dotnet tool with the go on PATH, and a go2cs built with any Go other than the
# release src/go2cs/go.mod names provisions none, so that Go is read here, before anything is bumped. GOTOOLCHAIN
# is local for the read as for the build: under auto an older go would fetch the named release and answer for it.
if (-not $ResumePush -and -not $OfflineSigning) {
    $goWanted = 'go' + (Select-String -Path (Join-Path $SrcRoot 'go2cs/go.mod') -Pattern '^go\s+(\S+)' | Select-Object -First 1).Matches[0].Groups[1].Value
    $goHave = & {
        $ErrorActionPreference = 'Continue'
        $savedToolchain = $env:GOTOOLCHAIN
        $env:GOTOOLCHAIN = 'local'
        try { "$(& go env GOVERSION 2>&1 | Select-Object -Last 1)".Trim() } catch { "$_" } finally { $env:GOTOOLCHAIN = $savedToolchain }
    }
    if ($goHave -ne $goWanted) {
        Die "The go on PATH answers '$goHave'; Phase 1b builds the go2cs dotnet tool with $goWanted, the release src/go2cs/go.mod names. Put that Go first on PATH and re-run. Nothing was bumped, tagged, frozen or packed."
    }
    Write-Host "  Go (go2cs tool)  : $goHave"
}

if (-not $env:NuGetCertFingerprint -and -not $OfflineSigning -and -not $ResumePush) {
    Die 'NuGetCertFingerprint is not set. Set it, or pass -OfflineSigning to sign elsewhere.'
}

if (-not $env:NUGET_API_KEY) {
    Die 'NUGET_API_KEY is not set -- Phase 3 would fail after the packages were already signed.'
}
Write-Host '  NUGET_API_KEY    : set'

# The certificate is proven reachable NOW rather than after a multi-minute pack, by running the
# signer's own census against whatever is currently in the artifacts folder (or an empty one --
# the certificate half of its checks runs regardless). A resumed push signs nothing.
if (-not $OfflineSigning -and -not $ResumePush) {
    $probe = Invoke-Sibling -Script $sign -Arguments @('-PackageDir', $OutDir)
    $certLine = $probe.Output | Where-Object { $_ -match 'Certificate:' } | Select-Object -First 1
    if (-not $certLine) {
        $probe.Output | Select-Object -First 6 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
        Die 'The signing certificate is not reachable. Is the card inserted?'
    }
    Write-Host "  signing cert     : $($certLine -replace '.*Certificate: ','')"
}

if ($WhatIfPreference) {
    Write-Phase 'CENSUS ONLY (-WhatIf)'
    Write-Host '  Preconditions pass. Nothing was bumped, packed, signed or pushed.'
    exit 0
}

# ---- Phase 1: bump, pack, freeze the proof, mint the tag -------------------------------------

if ($ResumePush) {
    Write-Phase 'Phases 1 and 2: skipped (-ResumePush: the packages are already packed and signed)'
}
else {
    Write-Phase 'Phase 1: bump version and pack Release packages'
    Write-Host '  (the build number bumps UP FRONT so packed, signed and pushed packages carry ONE version)'

    $pack = Invoke-Sibling -Script $push -Arguments @('-BumpBuild', '-OutDir', $OutDir) -Passthru
    if ($pack.ExitCode -ne 0) {
        Write-Host ''
        Die "Phase 1 (pack) FAILED. version.props was already bumped -- either re-run (it advances again) or restore it: git checkout src/version.props, and delete the tag if one was minted."
    }
}

# ReadAllText, not Get-Content: Windows PowerShell 5.1 reads BOM-less UTF-8 (version.props' comments carry em dashes)
# as the ANSI code page (the binary-compatibility guard's suppression files, 2026-11-08).
$x = [xml][System.IO.File]::ReadAllText((Join-Path $SrcRoot 'version.props'))
$p = $x.Project.PropertyGroup
$ver = "$($p.GoStdLibVersion).$($p.GoBuildNumber)".Trim()
Write-Host "  version          : $ver"

# ---- Phase 1b: commit the record, then the go2cs dotnet tool -------------------------------------
# Seven more packages for Phase 2 to sign and Phase 3 to push: go2cs.<rid> for the six platforms and the go2cs
# package that names them (docs/PLAN-dotnet-tool-installer.md, OQ-3). Their binaries are cross-compiled HERE, from
# the release's RECORD COMMIT, made first: corpus-release.txt is regenerated (it names this release from here on),
# and it is committed with version.props, the frozen proof and the retargeted badges, LOCALLY and signed. A go build
# stamps the commit and git status into each binary, so a tool built from the tag commit over the uncommitted
# record reported the tag's revision and vcs.modified=true (1.26.11.1, 2026-11-20); built from the record commit it
# reports that commit, clean, and pack-go2cs-tool.ps1 refuses a signed pack that reports otherwise. Nothing is
# pushed until Phase 4, and the tag Phase 1 minted stays on the commit before the record. The Windows executables
# are Authenticode-signed before packing, by one signtool process: a PIN prompt of its own, ahead of Phase 2's.

$recordPaths = @('src/version.props', "docs/validation/$ver", 'src/core', 'src/go2cs/corpus-release.txt')
$recordSubject = "release: go2cs converted stdlib $ver"
$recordHashFile = Join-Path $OutDir 'go2cs-record-commit.txt'

function Save-GoReleaseRecord {
    Push-Location (Join-Path $PSScriptRoot 'go2cs')
    try { & go run ./internal/gencorpusrelease } finally { Pop-Location }
    if ($LASTEXITCODE -ne 0) { Die "Phase 1b: regenerating src/go2cs/corpus-release.txt FAILED. Nothing was committed, signed or published. version.props was already bumped -- either re-run (it advances again) or restore it: git checkout src/version.props, and delete the tag if one was minted." }

    & git -C $RepoRoot add -- @recordPaths
    if ($LASTEXITCODE -ne 0) { Die "Phase 1b: git add of the record FAILED. Nothing was committed, signed or published." }

    # Checked BEFORE the commit, so a refusal leaves nothing committed: once the record paths are staged, every
    # porcelain line must be a staged change (worktree column blank) under one of them. Anything else -- an untracked
    # file, an unstaged edit, a staged path outside the record -- would make the tool report vcs.modified=true.
    # core.quotePath=false: git otherwise quotes a path with non-ASCII bytes (a quoted "src/core/..." name), which then matches
    # no record path and stops the release with the tag minted; this tree's file names carry non-ASCII letters.
    $stray = @(git -C $RepoRoot -c core.quotePath=false status --porcelain 2>$null | Where-Object {
        $line = "$_"
        if ($line.Length -lt 4 -or $line[1] -ne ' ' -or $line[0] -eq '?') { return $true }
        $path = $line.Substring(3)
        -not ($recordPaths | Where-Object { $path -eq $_ -or $path.StartsWith("$_/") })
    })
    if ($stray.Count -gt 0) {
        $stray | Select-Object -First 8 | ForEach-Object { Write-Host "    $_" }
        Die ("Phase 1b: the tree holds changes outside the record ($($recordPaths -join ', ')), so the go2cs tool would report vcs.modified=true. NOTHING was committed, signed or published; the record is staged. " +
             "Do NOT re-run this script: Phase 0 refuses a dirty tree, and a clean re-run bumps the build number again under the tag already minted. " +
             "Either move the paths above out of the tree and go on by hand: git commit -S -m `"$recordSubject`", then the three commands of the Phase 1b failure text below (pack, sign, -ResumePush); " +
             "or abandon: git reset; git checkout src/version.props src/go2cs/corpus-release.txt, and delete the tag nuget-$ver.")
    }

    & git -C $RepoRoot commit -S -m $recordSubject
    if ($LASTEXITCODE -ne 0) { Die "Phase 1b: the record commit FAILED (is the commit-signing key available?). Nothing was signed or published; the record is staged, not committed. To abandon the release: git reset; git checkout src/version.props src/go2cs/corpus-release.txt, and delete the tag if one was minted." }

    $left = @(git -C $RepoRoot status --porcelain 2>$null)
    if ($left.Count -gt 0) {
        $left | Select-Object -First 8 | ForEach-Object { Write-Host "    $_" }
        Die "Phase 1b: the tree is not clean after the record commit (the check before the commit passed, so something changed while committing, such as a hook), and the go2cs tool would report vcs.modified=true. Nothing was signed or published. The record commit is local. Move the paths above out of the tree and go on by hand with the three commands of the Phase 1b failure text (pack, sign, -ResumePush); do NOT re-run this script, which would bump the build number again."
    }
    # The record commit's HASH goes beside the packages (the artifacts folder is ignored), so the checks before the
    # publish and in Phase 4 refuse a record amended after the tool was built from it: same subject, other hash.
    $hash = "$(git -C $RepoRoot rev-parse HEAD 2>$null)".Trim()
    if ($hash -notmatch '^[0-9a-f]{40,64}$') { Die "Phase 1b: could not read the record commit's hash. Nothing was signed or published; the record commit is local." }
    New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
    Set-Content -LiteralPath $recordHashFile -Value $hash -Encoding ascii
    Write-Host "  record commit    : $(git -C $RepoRoot log -1 --format='%h %s') (local, not pushed; hash in $recordHashFile)"
}

# HEAD must be the record commit Phase 1b made, and regenerating corpus-release.txt must change nothing. Read before
# the publish confirmation, on every path (a -ResumePush included: Phase 0 does not check the tree on a resume), so a
# record commit undone or replaced after Phase 1b refuses with NOTHING published; read again in Phase 4. The subject
# is compared whole and case-sensitively: a revert of the record, or the record of 1.26.11.10 when this is 1.26.11.1,
# contains the subject and is not it. Returns "<short hash> <subject>".
function Assert-GoReleaseRecordHead([string] $Phase, [string] $Consequence) {
    # Out-Host: the generator's output is shown, not returned with the head.
    Push-Location (Join-Path $PSScriptRoot 'go2cs')
    try { & go run ./internal/gencorpusrelease | Out-Host } finally { Pop-Location }
    if ($LASTEXITCODE -ne 0) { Die "${Phase}: regenerating src/go2cs/corpus-release.txt FAILED. $Consequence" }
    # Strings, never git's raw output: a failed git writes nothing to stdout, and an empty result compared with an
    # operator yields an empty array, which reads as false and would let this check pass without a commit at all.
    $subject = "$(git -C $RepoRoot log -1 --format='%s' 2>$null)"
    $head = "$(git -C $RepoRoot log -1 --format='%h %s' 2>$null)"
    $moved = @(git -C $RepoRoot status --porcelain -- src/go2cs/corpus-release.txt 2>$null)
    $hash = "$(git -C $RepoRoot rev-parse HEAD 2>$null)".Trim()
    $recorded = if (Test-Path -LiteralPath $recordHashFile) { "$(Get-Content -LiteralPath $recordHashFile -Raw)".Trim() } else { '' }
    if (-not $recorded) { Die "${Phase}: $recordHashFile is missing or empty, so the record commit the packages were built from is unknown. $Consequence" }
    if ($moved.Count -gt 0 -or $subject -cne $recordSubject -or $hash -cne $recorded) {
        Write-Host "  HEAD is $head ($hash)" -ForegroundColor Yellow
        Write-Host "  want  : $recordSubject at $recorded (Phase 1b's record commit), with src/go2cs/corpus-release.txt unchanged after regenerating it" -ForegroundColor Yellow
        Die "${Phase}: HEAD is not the record commit for $ver that the packages were built from (amended, replaced or undone), or corpus-release.txt moved after it. $Consequence"
    }
    return $head
}

$toolPack = Join-Path $PSScriptRoot 'tools/go2cs-tool/pack-go2cs-tool.ps1'
if ($ResumePush) {
    # Packed and signed by the run that stopped.
}
elseif ($OfflineSigning) {
    Write-Phase 'Phase 1b: commit the record; the go2cs dotnet tool is NOT packed (-OfflineSigning)'
    Save-GoReleaseRecord
    Write-Host '  Its Windows executables are signed before they are packed, so it is packed where the certificate is, and a'
    Write-Host '  signed pack refuses unless HEAD is the record commit, unmodified. That commit is local and unpushed, and a'
    Write-Host '  commit re-made on the signing machine has another hash, which is not the one Phase 4 pushes. Carry THIS'
    Write-Host '  commit there in a bundle (its parent must already be on the signing machine, e.g. from a fetch of master):'
    Write-Host ''
    Write-Host "    here:     git bundle create go2cs-record-$ver.bundle nuget-$ver..HEAD" -ForegroundColor DarkGray
    Write-Host "    there:    git fetch <path>\go2cs-record-$ver.bundle HEAD; git checkout --detach FETCH_HEAD" -ForegroundColor DarkGray
    Write-Host "              git log -1 --format=%H    (must equal $(git -C $RepoRoot rev-parse HEAD 2>$null))" -ForegroundColor DarkGray
    Write-Host "              src\tools\go2cs-tool\pack-go2cs-tool.ps1 -Version $ver -OutDir <the copied package folder>" -ForegroundColor DarkGray
    Write-Host ''
    Write-Host '  then sign the whole folder as Phase 2 says. Or leave the tool out and publish it with a later release.'
    Write-Host "  Leave go2cs-record-commit.txt in $OutDir; Phase 3 refuses without it."
}
else {
    Write-Phase 'Phase 1b: commit the record, then pack the go2cs dotnet tool (one PIN prompt for its Windows executables)'
    Save-GoReleaseRecord

    $tool = Invoke-Sibling -Script $toolPack -Arguments @('-Version', $ver, '-OutDir', $OutDir) -Passthru
    if ($tool.ExitCode -ne 0) {
        Write-Host ''
        Die ("Phase 1b (go2cs tool) FAILED. Nothing was signed or published. The record commit is local. Either fix the cause and go on by hand, from src, with HEAD still the record commit: " +
             ".\tools\go2cs-tool\pack-go2cs-tool.ps1 -Version $ver -OutDir `"$OutDir`"; .\sign-nupkgs.ps1 -PackageDir `"$OutDir`" -Apply; .\release-nuget.ps1 -ResumePush -OutDir `"$OutDir`". " +
             "Or abandon: git reset --soft HEAD~1; git reset; git checkout src/version.props src/go2cs/corpus-release.txt, and delete the tag nuget-$ver.")
    }
}

# ---- Phase 2: sign ---------------------------------------------------------------------------

if ($ResumePush) {
    # Nothing to sign. What is checked instead is that the folder holds THIS release and nothing else.
}
elseif ($OfflineSigning) {
    Write-Phase 'Phase 2: OFFLINE signing (this machine does not sign)'
    Write-Host "  Packages are in: $OutDir"
    Write-Host ''
    Write-Host '    1. Copy *.nupkg to the signing machine'
    Write-Host '    2. Run sign-nupkgs.bat -Apply there (with NuGetCertFingerprint set)'
    Write-Host '    3. Copy the SIGNED *.nupkg back, overwriting the originals'
    Write-Host ''
    Read-Host 'Press Enter when the signed packages are back in place (Ctrl+C to stop)'
}
else {
    Write-Phase 'Phase 2: sign (ONE PIN prompt for the whole set)'
    $signRun = Invoke-Sibling -Script $sign -Arguments @('-PackageDir', $OutDir, '-Apply') -Passthru
    if ($signRun.ExitCode -ne 0) {
        Write-Host ''
        Die "Phase 2 (sign) FAILED. Nothing was published. The packages in $OutDir are unsigned or partly signed; fix the card session and re-run the signer, or restore version.props to abandon this release."
    }
}

# ---- Phase 3: push -- THE IRREVERSIBLE ONE ---------------------------------------------------

. (Join-Path $PSScriptRoot '_nugetpush.ps1')

$packages = @(Get-GoNuGetPushOrder @(Get-ChildItem (Join-Path $OutDir '*.nupkg') -File))
if ($packages.Count -eq 0) { Die "Phase 3: no .nupkg in $OutDir. Nothing was published." }

if ($ResumePush) {
    $wanted = ConvertTo-GoNuGetNormalizedVersion $ver
    $foreign = @($packages | Where-Object { (ConvertTo-GoNuGetNormalizedVersion (Get-GoNuGetPackageIdentity $_.FullName).Version) -ne $wanted })
    if ($foreign.Count -gt 0) {
        $foreign | Select-Object -First 8 | ForEach-Object { Write-Host "    $($_.Name)" -ForegroundColor Red }
        Die "Phase 3: $($foreign.Count) package(s) in $OutDir are not version $ver (src/version.props). -ResumePush pushes one release; nothing was published."
    }
}

$recordHead = Assert-GoReleaseRecordHead 'Phase 3' 'NOTHING was published. Put HEAD back on the record commit (git reflog names it), or abandon the release.'

Write-Phase "Phase 3: publish to $(([uri]$Source).Host)"
Write-Host "  record commit    : $recordHead"
Write-Host "  version          : $ver"
Write-Host "  packages         : $($packages.Count)"
Write-Host "  source           : $Source"
Write-Host ''
Write-Host '  A published version can be UNLISTED but never DELETED.' -ForegroundColor Yellow

if (-not $Yes) {
    $answer = Read-Host "  Publish $($packages.Count) package(s) as ${ver}? (type 'publish' to proceed)"
    if ($answer -ne 'publish') { Die '  Not published. Nothing was sent; the signed packages remain in the artifacts folder.' }
}

try {
    $result = Invoke-GoNuGetPush -Packages $packages -Source $Source -ApiKey $env:NUGET_API_KEY
}
catch {
    Write-Host ''
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    Die "Phase 3 (push) STOPPED. The signed packages are still in $OutDir; once resolved, finish with .\release-nuget.ps1 -ResumePush -- do NOT re-pack, the version is already minted and signed."
}
Write-Host "  pushed           : $($result.Pushed); already published: $($result.AlreadyPublished); rate-limit waits: $($result.Waits)"

# ---- Phase 4: record --------------------------------------------------------------------------
# The published version, its frozen proof snapshot and the badge links pointing at it are ONE
# fact, committed together in Phase 1b (the record commit) so a published badge never links a
# directory that is not in the repository. The tag already exists (Phase 1 minted it signed,
# before anything was packed), and every published README's C# Source badge links it -- until it
# reaches GitHub those links 404.

Write-Phase 'Phase 4: record the release'

# The converter's corpus-release constant (src/go2cs/corpus-release.txt) names the last PUBLISHED release, which
# is now $ver. Phase 1b regenerated and committed it; regenerating it again must change nothing. The same check ran
# before the publish confirmation; this is its second read, after the push.
$head = Assert-GoReleaseRecordHead 'Phase 4' "$ver IS published; amend nothing -- make a commit on top that records it, then push."

Write-Host "  The record commit ($head) was made in Phase 1b. Push it and the tag"
Write-Host '  Phase 1 already minted:'
Write-Host ''
Write-Host "    git push && git push origin nuget-$ver" -ForegroundColor DarkGray
Write-Host ''
Write-Host "  docs/validation/$ver is FROZEN from here -- it is the proof every $ver package's"
Write-Host '  green badge links and packs as VALIDATION.md.'
Write-Host ''
Write-Host '  The tag push starts release-binaries.yml, which drafts the GitHub Release. When the draft is up, sign the'
Write-Host '  Windows executables and SHA256SUMS here (one PIN prompt), then attest what the draft holds:'
Write-Host ''
Write-Host "    .\sign-release-binaries.ps1 -Tag nuget-$ver -Apply" -ForegroundColor DarkGray
Write-Host "    gh workflow run attest-release.yml -f tag=nuget-$ver" -ForegroundColor DarkGray
Write-Host ''
Write-Host '  Publish the draft only after that workflow succeeds.'
exit 0
