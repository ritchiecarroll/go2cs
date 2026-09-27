<#
.SYNOPSIS
    The TFM-hop instrument (DotNetMigration.md section 5.1): census by default, -Apply to act, -WhatIf
    honored. Encodes CENSUS-tfm-inventory.md's classes so a TFM bump is a dispatch, not a discovery.

.DESCRIPTION
    Commissioned by user directive (mailbox, 2026-08-24) as a first-class runbook instrument. A bare
    run REPORTS every site class and changes nothing. -Apply edits exactly the SOURCE set (Class B's
    property line AND the seventeen project files no emitter reaches, the converter's two csproj
    templates, the nine embedded publish profiles, the CI SDK channel, and the present-tense Class-E
    doc lines) and then SELF-VERIFIES: a re-census must find zero remaining apply-set sites, and that
    property is the gate.

    WHAT THIS SCRIPT NEVER DOES, by design (the census's own rules):
      - It never edits GENERATED files. The Class-A csproj are emission output; after -Apply
        this script NAMES the regens the operator owes (the three-target -platforms merge,
        UpdateTestTargets --createTargetFiles, `go generate .` in src\go2cs) and the section 8 gates. A
        script that edits generated files is the fourth trap wearing a helpful face.
        The test of Class A is REACHABILITY, not location: seventeen project files sit in those same
        trees (or beside them) that no emitter emits, so no regen can level them. Those are Class B,
        they are edited here, and they are subtracted from the Class-A count so it reads true.
      - It never touches Class C (must-not-change): go2cs-gen stays netstandard2.0 (Roslyn analyzer
        contract), push-nuget's matching path stays, and dated measurement provenance keeps saying
        the version it measured. These are ENFORCED refusals with reasons, not omissions.
      - It does not run the regen, the gate ladder, or the judgement. section 8's checklist stays the
        operator's; this script is steps 3-4 (and the CI/doc lines of steps 9-10) made mechanical.

.PARAMETER To
    The target TFM. Defaults to net10.0 (the current hop); hop N+1 passes its own.

.PARAMETER From
    The TFM being retired. Defaults to net9.0.

.PARAMETER Apply
    Perform the edits. Without it, census-and-report only.

.NOTES
    File I/O per the repo's documented trap: [System.IO.File]::ReadAllText/WriteAllText with UTF-8
    no-BOM, line endings preserved -- never PS 5.1 Get-Content/Out-File (the mojibake class).
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$To = 'net10.0',
    [string]$From = 'net9.0',
    [switch]$Apply
)

$ErrorActionPreference = 'Stop'

# Paths from the shared primitives, never re-derived (repo rule).
. (Join-Path $PSScriptRoot '_paths.ps1')
$src = $PSScriptRoot
$repo = Split-Path $src -Parent

function Read-Text([string]$Path) { [System.IO.File]::ReadAllText($Path) }
# Does the file carry a UTF-8 BOM? Some tracked csproj do (golib, GenTests, UpdateTestTargets) and
# rewriting them without one is a content change BEYOND the TFM line -- caught in execution
# 2026-08-24, when the shape check showed '<Project Sdk=...>' as a changed line. The no-BOM rule
# exists to dodge the PS 5.1 read-as-ANSI mojibake trap, and PRESERVING the file's own BOM state
# satisfies it just as well: we never re-encode, we round-trip. (Both branches of the 2026-08-24
# collision found this defect independently and fixed it the same way; this is the single surviving
# implementation of that fix.)
function Test-Bom([string]$Path) {
    $b = New-Object byte[] 3
    $fs = [System.IO.File]::OpenRead($Path)
    try { $n = $fs.Read($b, 0, 3) } finally { $fs.Dispose() }
    return ($n -eq 3 -and $b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF)
}
function Write-Text([string]$Path, [string]$Text) {
    $bom = Test-Bom $Path
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($bom)))
}

# --- The APPLY SET: every source-of-truth site, with the exact old/new text per site --------------
# Exactness is the safety: a site that no longer matches is REPORTED as moved, never fuzzily edited.
# Doc lines edit the TFM token only, inside the quoted exact context the census recorded.
$applySites = @(
    # THE EDIT (census section 8 step 4): the one property everything conditioned hangs off.
    @{ File = "$src\Directory.Build.props";
       Old  = "<TargetFramework Condition=`"'`$(TargetFramework)'==''`">$From</TargetFramework>";
       Why  = 'Class B: the property of record' },

    # D': the converter's two embedded csproj templates (their conditioned FALLBACKS).
    #     (Class B's project files are appended below, after this literal array.)
    @{ File = "$src\go2cs\csproj-template.xml";
       Old  = "<TargetFramework Condition=`"'`$(TargetFramework)'==''`">$From</TargetFramework>";
       Why  = "D': emission fallback -- editing it self-invalidates go2cs.exe via route #5's predicate" },
    @{ File = "$src\go2cs\test-csproj-template.xml";
       Old  = "<TargetFramework Condition=`"'`$(TargetFramework)'==''`">$From</TargetFramework>";
       Why  = "D': test-host emission fallback" },

    # CI: the SDK channel default + its description (census section 8 step 9; the PROBE path already
    # derives since Class D, so only the channel selection moves).
    @{ File = "$repo\.github\workflows\os-matrix.yml";
       Old  = 'default: 9.0.x';
       New  = 'default: 10.0.x';
       Why  = 'step 9: SDK 9 cannot build the new TFM' }
)

# D'': the nine embedded publish profiles, TWO sites each (PublishDir and TargetFramework -- both,
# or the publish lands in a folder named for the old TFM).
foreach ($pubxml in Get-ChildItem "$src\go2cs\profiles" -Filter *.pubxml -File) {
    $applySites += @{ File = $pubxml.FullName;
                      Old  = "bin/Release/$From/publish/";
                      Why  = "D'': PublishDir names the TFM folder" }
    $applySites += @{ File = $pubxml.FullName;
                      Old  = "<TargetFramework>$From</TargetFramework>";
                      Why  = "D'': the profile's own TFM" }
}

# Class E: present-tense doc lines (census section 6), each edited inside its exact recorded context.
# DotNetMigration.md and CIMatrix.md are deliberately REPORT-ONLY: most of their mentions are
# scouting history (Class-C-shaped), and CIMatrix:67 should describe Class D's DERIVED probe
# rather than gain a new TFM literal -- both are prose judgements, not token swaps.
$applySites += @(
    @{ File = "$repo\docs\README.md";        Old = "cd bin/Debug/$From";  Why = 'Class E: visitor-facing command' },
    @{ File = "$repo\CLAUDE.md";             Old = "(target **$From**, C# latest)"; Why = 'Class E: orientation fact' },
    @{ File = "$repo\CLAUDE.md";             Old = "its ``bin/Debug/$From``"; Why = 'Class E: operational instruction' },
    @{ File = "$repo\docs\Glossary.md";      Old = "bin/Debug/$From/<AssemblyName>.dll"; Why = 'Class E: definition' },
    @{ File = "$repo\docs\Glossary.md";      Old = "<TargetFramework>$From</TargetFramework>"; Why = 'Class E: definition example' },
    @{ File = "$repo\docs\ConversionStrategies-Reference\package-conversion\project-files.md";
       Old = "<TargetFramework Condition=`"'`$(TargetFramework)'==''`">$From</TargetFramework>";
       Why = 'Class E: the emitted form as documented' }
)

# --- The HAND-OWNED csproj class (found in execution, 2026-08-24) --------------------------------
# 17 project files NO emitter rewrites, so NO regen can ever level them. MEASURED, not theorized:
# after a full three-target regen every one still read the old TFM in the staging root, because the
# driver either skips the package (skip-listed hand-owns; unmarkedFileCount == 0 makes it 'continue'
# before writeProjectFile) or the file is hand-written and was never emitted at all.
#
# IN-REPO THEY ARE INERT and this is NOT a build break: they carry the same CONDITIONED form the
# generated corpus carries, the root props sets TargetFramework before the project body is read, and
# MSBuild evaluates all of them at the NEW TFM (verified: dotnet msbuild <proj>
# -getProperty:TargetFramework). A census that greps the FILE finds them either way -- ask MSBuild.
#
# THE EXPOSURE IS PROPS-LESS CONTEXTS: a deploy-core GOPATH tree (its generated props pins
# $(go2csPath) ONLY, no TFM), a -recurse output root, a single-package conversion. There the
# fallbacks govern, the tree goes mixed, and a LOWER-TFM project referencing a higher one is NU1201
# (core/testing -> core/time is the live pair). So this class is CONSISTENCY, not an emergency.
$handOwnedCsproj = @(
    @{ Path = 'src\core\golib\golib.csproj'; Why = 'the hand-written runtime; never generated' },
    @{ Path = 'src\core\testing\testing.csproj'; Why = 'skip-listed hand-own (isNonConvertedStdLibPackage)' },
    @{ Path = 'src\core\unsafe\unsafe.csproj'; Why = 'skip-listed hand-own (isNonConvertedStdLibPackage)' },
    @{ Path = 'src\core\internal\concurrent\internal.concurrent.csproj'; Why = 'hand-owned BY CONSEQUENCE: unmarkedFileCount == 0 continues before writeProjectFile' },
    @{ Path = 'src\core\internal\godebug\internal.godebug.csproj'; Why = 'hand-owned BY CONSEQUENCE: same driver path' },
    @{ Path = 'src\core\internal\weak\internal.weak.csproj'; Why = 'hand-owned BY CONSEQUENCE: same driver path' },
    @{ Path = 'src\core\internal\runtime\syscall\internal.runtime.syscall.csproj'; Why = 'platform-remainder: the merge kept the seeded file, never re-emitted it' },
    @{ Path = 'src\core\crypto\x509\internal\macos\crypto.x509.internal.macos.csproj'; Why = 'platform-exclusive remainder: seeded, never re-emitted' },
    @{ Path = 'src\core\vendor\golang.org\x\net\route\vendor.golang.org.x.net.route.csproj'; Why = 'platform-remainder: seeded, never re-emitted' },
    @{ Path = 'src\tests\Behavioral\BehavioralRunner\BehavioralRunner.csproj'; Why = 'LOAD-BEARING: derives its TFM from its own bin tail (Class D) -- a stale value makes it miss every assembly' },
    @{ Path = 'src\tests\Behavioral\BehavioralTests\BehavioralTests.csproj'; Why = 'LOAD-BEARING: the MSTest harness, same derivation exposure' },
    @{ Path = 'src\tests\ChannelTests\ChannelTests.csproj'; Why = 'hand-written test project' },
    @{ Path = 'src\tests\GenTests\GenTests.csproj'; Why = 'hand-written test project' },
    @{ Path = 'src\tests\GenericTests\GenericTests.csproj'; Why = 'hand-written test project' },
    @{ Path = 'src\tests\GolibTests\GolibTests.csproj'; Why = 'hand-written test project (a Stage gate instrument)' },
    @{ Path = 'src\tests\Performance\PerformanceRunner\PerformanceRunner.csproj'; Why = 'LOAD-BEARING: the perf harness, same bin-tail derivation as its Behavioral siblings -- and it hides in the Perf* tree, so a census excluding tests\Performance as generated misses it' },
    @{ Path = 'src\utilities\UpdateTestTargets\UpdateTestTargets.csproj'; Why = 'hand-written utility: regenerates goldens' }
)
foreach ($h in $handOwnedCsproj) {
    $applySites += @{ File = "$repo\$($h.Path)";
                      Old  = "<TargetFramework Condition=`"'`$(TargetFramework)'==''`">$From</TargetFramework>";
                      Why  = "hand-owned csproj (no regen reaches it): $($h.Why)" }
}

# golib's NoWarn rationale states the live TFM as present-tense FACT, not as dated provenance -- it
# explains which warnings cannot fire on the framework the project TARGETS, so it moves with it. This
# is the one prose site inside a hand-owned csproj: Class C keeps a mention that RECORDS what was
# measured on a given date, and this sentence does not. Carrying it here is what stops a naive
# grep-based census from reading one site higher than the truth at the NEXT hop.
$applySites += @{ File = "$repo\src\core\golib\golib.csproj";
                  Old  = "cannot fire on $From";
                  Why  = 'hand-owned csproj prose stating the live TFM as present-tense fact' }

# --- MUST NOT CHANGE (Class C), enforced with reasons --------------------------------------------
$mustNotChange = @(
    @{ File = "$src\gen\go2cs-gen\go2cs-gen.csproj"; Text = '<TargetFramework>netstandard2.0</TargetFramework>';
       Why  = 'Roslyn analyzers load in the compiler; netstandard2.0 is the contract, not a default' },
    @{ File = "$src\push-nuget.ps1"; Text = 'netstandard2.0';
       Why  = 'pairs with the analyzer TFM above; correct as-is' },
    @{ File = "$src\core\runtime\managed_impl.cs"; Text = "288 B on net9.0";
       Why  = 'dated measurement provenance in a hand-owned file' },
    @{ File = "$src\core\testing\testing.cs"; Text = "net9.0/9.0.18";
       Why  = 'dated measurement provenance' }
)

# --- Census ---------------------------------------------------------------------------------------
Write-Host "migrate-tfm: $From -> $To  ($(if ($Apply) { 'APPLY' } else { 'census only' }))"
Write-Host ''

$pending = @()
$moved = @()

foreach ($site in $applySites) {
    if (-not (Test-Path $site.File)) { $moved += "$($site.File) -- file missing"; continue }
    $text = Read-Text $site.File
    $new = if ($site.ContainsKey('New')) { $site.New } else { ($site.Old -replace [regex]::Escape($From), $To) }
    if ($text.Contains($site.Old)) { $pending += @{ Site = $site; New = $new } }
    elseif (-not $text.Contains($new)) { $moved += "$($site.File) -- expected text not found: $($site.Old)" }
}

Write-Host "apply-set sites still at ${From}: $($pending.Count)"
foreach ($p in $pending) { Write-Host ("  {0}`n      [{1}]" -f $p.Site.File.Substring($repo.Length + 1), $p.Site.Why) }

if ($moved.Count -gt 0) {
    Write-Host ''
    Write-Warning "sites that match NEITHER ${From} nor ${To} -- the census is stale against the tree; fix the census, then this script:"
    foreach ($m in $moved) { Write-Warning "  $m" }
}

# Generated corpus (Class A): counted, never edited. The hand-owned project files above live INSIDE
# these same directories, so they must be subtracted -- counted as Class A they read as "a regen
# levels them" when no regen can reach them, which is how they stayed at $From through a full
# three-target merge and a behavioral re-transpile. The subtraction is REPORTED rather than silent:
# a whole-directory rule is what hid PerformanceRunner in the first census, so the count of files
# this enumeration steps over is stated, not assumed.
$classARoots = @("$src\core", "$src\tests\Behavioral", "$src\tests\Performance")
$classA = @(Get-ChildItem $classARoots -Recurse -Filter *.csproj -File -ErrorAction SilentlyContinue |
    Where-Object { (Read-Text $_.FullName).Contains(">$From<") -and
                   $handOwnedCsproj.Path -notcontains $_.FullName.Substring($repo.Length + 1) })
$shadowed = @($handOwnedCsproj | Where-Object { $p = "$repo\$($_.Path)"; @($classARoots | Where-Object { $p.StartsWith("$_\") }).Count -gt 0 })
Write-Host ''
Write-Host "Class A (generated; regen levels them, this script never touches them): $($classA.Count) csproj at $From"
Write-Host "  (excludes $($shadowed.Count) Class-B project files sitting inside those trees that no emitter reaches)"

# Class C verification: the protected values must still be present (their ABSENCE is the alarm).
Write-Host ''
foreach ($c in $mustNotChange) {
    $state = if ((Test-Path $c.File) -and (Read-Text $c.File).Contains($c.Text)) { 'intact' } else { 'MISSING -- investigate' }
    Write-Host ("Class C {0}: {1}  [{2}]" -f $state, $c.File.Substring($repo.Length + 1), $c.Why)
}

if (-not $Apply) {
    Write-Host ''
    Write-Host 'Census only. Re-run with -Apply to edit the apply set (never the generated corpus).'
    exit 0
}

# --- Apply ----------------------------------------------------------------------------------------
Write-Host ''
foreach ($p in $pending) {
    if ($PSCmdlet.ShouldProcess($p.Site.File, "replace '$($p.Site.Old)'")) {
        $text = Read-Text $p.Site.File
        Write-Text $p.Site.File ($text.Replace($p.Site.Old, $p.New))
        Write-Host "edited: $($p.Site.File.Substring($repo.Length + 1))"
    }
}

# --- Self-verify: idempotence is the gate (only for a run that actually edited) -------------------
if ($WhatIfPreference) {
    Write-Host ''
    Write-Host "WhatIf: $($pending.Count) site(s) would be edited; nothing was. Self-verify skipped by design."
    exit 0
}
$remaining = 0
foreach ($site in $applySites) {
    if ((Test-Path $site.File) -and (Read-Text $site.File).Contains($site.Old)) { $remaining++ }
}
Write-Host ''
if ($remaining -ne 0) {
    Write-Error "self-verify FAILED: $remaining apply-set site(s) still at $From after -Apply."
    exit 1
}
Write-Host "self-verify: zero apply-set sites remain at $From."
Write-Host ''
Write-Host 'WHAT THIS SCRIPT DID NOT DO -- the operator owes, in CENSUS-tfm-inventory.md section 8 order:'
Write-Host '  1. go test -count=1 ./...  (src\go2cs -- template guards; -count=1 required, section 5.2)'
Write-Host '  2. The regens: seeded three-target -platforms merge; UpdateTestTargets --createTargetFiles;'
Write-Host '     go generate .  (Class A levels here, not by hand)'
Write-Host '  3. The section 4 gate ladder (purging bin/obj/Generated between flavor switches), CNR + accounting'
Write-Host '  4. Purge stale emitted publish profiles (**/Properties/PublishProfiles/ -- gitignored, never overwritten)'
Write-Host "  5. The commit-message staleness statement: generated csproj still read $From; inert, level on regen"
