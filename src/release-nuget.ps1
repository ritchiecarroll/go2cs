#Requires -Version 5.1
<#
.SYNOPSIS
    Publish a signed release of the go2cs NuGet packages, end to end, on ONE machine.

.DESCRIPTION
    Pack -> sign -> push -> record, as four phases of one ritual. It supersedes the
    offline-signing flow (`-OfflineSigning` keeps that path for a machine whose signing
    certificate lives elsewhere), and it exists because the certificate now lives HERE: the
    file-shuttle between machines was the only reason the release was ever two commands.

    THE ONLY HUMAN ACTION IS THE PIN. The Smart Card KSP caches a card PIN for the life of the
    process that unlocked it, so the whole package set signs inside ONE `dotnet nuget sign`
    invocation and the card is unlocked exactly once (measured: 6 packages, one prompt, 9 s).

    SIGNING IS MANDATORY, NOT OPTIONAL. The owner's code-signing certificate is REGISTERED with
    nuget.org (2026-08-24), and registration is an enforcement switch: nuget.org rejects any
    package pushed under that account which is not signed by a registered certificate. An
    unsigned push does not publish-with-a-warning -- it fails. That is why Phase 0 proves the
    certificate is reachable before anything is bumped, and why -OfflineSigning still signs
    rather than skipping.

    WHAT IS IRREVERSIBLE, AND WHERE. Phase 3 publishes to nuget.org, and a published version can
    be unlisted but never deleted. Everything before it is recoverable: -WhatIf stops after the
    census, a failed pack leaves only a bumped version.props (restore with
    `git checkout src/version.props` and delete the tag), and a failed sign leaves unsigned
    packages in the artifacts folder. The push is therefore gated on an explicit confirmation
    unless -Yes is passed, and the gate prints exactly what is about to become permanent.

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
    The full ritual: pack, sign (one PIN), confirm, push, then print the record-the-release
    commands.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch] $OfflineSigning,
    [switch] $Yes,
    [switch] $VerifyOnly,
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

$dirty = @(git -C $RepoRoot status --porcelain 2>$null)
if ($dirty.Count -gt 0) {
    Write-Host 'Working tree is not clean:' -ForegroundColor Red
    $dirty | Select-Object -First 8 | ForEach-Object { Write-Host "    $_" }
    Die 'A release commits version.props, a proof snapshot and retargeted badges together; unrelated changes must not ride along.'
}
Write-Host '  working tree     : clean'

$branch = (git -C $RepoRoot branch --show-current 2>$null)
Write-Host "  branch           : $branch"
if ($branch -ne 'master') { Write-Host '    (not master -- intentional?)' -ForegroundColor Yellow }

if (-not $env:NuGetCertFingerprint -and -not $OfflineSigning) {
    Die 'NuGetCertFingerprint is not set. Set it, or pass -OfflineSigning to sign elsewhere.'
}

if (-not $env:NUGET_API_KEY) {
    Die 'NUGET_API_KEY is not set -- Phase 3 would fail after the packages were already signed.'
}
Write-Host '  NUGET_API_KEY    : set'

# The certificate is proven reachable NOW rather than after a multi-minute pack, by running the
# signer's own census against whatever is currently in the artifacts folder (or an empty one --
# the certificate half of its checks runs regardless).
if (-not $OfflineSigning) {
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

Write-Phase 'Phase 1: bump version and pack Release packages'
Write-Host '  (the build number bumps UP FRONT so packed, signed and pushed packages carry ONE version)'

$pack = Invoke-Sibling -Script $push -Arguments @('-BumpBuild', '-OutDir', $OutDir) -Passthru
if ($pack.ExitCode -ne 0) {
    Write-Host ''
    Die "Phase 1 (pack) FAILED. version.props was already bumped -- either re-run (it advances again) or restore it: git checkout src/version.props, and delete the tag if one was minted."
}

$x = [xml](Get-Content -Raw (Join-Path $SrcRoot 'version.props'))
$p = $x.Project.PropertyGroup
$ver = "$($p.GoStdLibVersion).$($p.GoBuildNumber)".Trim()
Write-Host "  version          : $ver"

# ---- Phase 2: sign ---------------------------------------------------------------------------

if ($OfflineSigning) {
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

$packages = @(Get-ChildItem (Join-Path $OutDir '*.nupkg') -File)

Write-Phase 'Phase 3: publish to nuget.org'
Write-Host "  version          : $ver"
Write-Host "  packages         : $($packages.Count)"
Write-Host "  source           : https://api.nuget.org/v3/index.json"
Write-Host ''
Write-Host '  A published version can be UNLISTED but never DELETED.' -ForegroundColor Yellow

if (-not $Yes) {
    $answer = Read-Host "  Publish $($packages.Count) package(s) as $ver? (type 'publish' to proceed)"
    if ($answer -ne 'publish') { Die '  Not published. Nothing was sent; the signed packages remain in the artifacts folder.' }
}

dotnet nuget push (Join-Path $OutDir '*.nupkg') --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY --skip-duplicate
if ($LASTEXITCODE -ne 0) {
    Write-Host ''
    Die "Phase 3 (push) FAILED. The signed packages are still in $OutDir; re-run the push once resolved -- do NOT re-pack, the version is already minted and signed."
}

# ---- Phase 4: record --------------------------------------------------------------------------
# The published version, its frozen proof snapshot and the badge links pointing at it are ONE
# fact: commit them together or a published badge links a directory that is not in the
# repository. The tag already exists (Phase 1 minted it signed, before anything was packed), and
# every published README's C# Source badge links it -- until it reaches GitHub those links 404.

Write-Phase 'Phase 4: record the release'

# The converter's corpus-release constant (src/go2cs/corpus-release.txt) names the last PUBLISHED release,
# which is now $ver; it is regenerated here, after the irreversible push, and joins the record commit.
Push-Location (Join-Path $PSScriptRoot 'go2cs')
try { & go run ./internal/gencorpusrelease } finally { Pop-Location }
if ($LASTEXITCODE -ne 0) { Die "Phase 4: regenerating src/go2cs/corpus-release.txt FAILED. $ver IS published; run 'go run ./internal/gencorpusrelease' in src/go2cs before committing the record." }

Write-Host '  Commit the version, the frozen proof and the retargeted badges TOGETHER, then push'
Write-Host '  the commit and the tag Phase 1 already minted:'
Write-Host ''
Write-Host "    git add src/version.props docs/validation/$ver src/core src/go2cs/corpus-release.txt" -ForegroundColor DarkGray
Write-Host "    git commit -S -m `"release: go2cs converted stdlib $ver`"" -ForegroundColor DarkGray
Write-Host "    git push && git push origin nuget-$ver" -ForegroundColor DarkGray
Write-Host ''
Write-Host "  docs/validation/$ver is FROZEN from here -- it is the proof every $ver package's"
Write-Host '  green badge links and packs as VALIDATION.md.'
exit 0
