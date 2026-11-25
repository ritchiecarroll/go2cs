<#
.SYNOPSIS
    A converter change's corpus footprint on one target: two seeded -stdlib reconverts, one by the converter at -Base
    and one by the converter at -Changed, diffed against each other.

.DESCRIPTION
    The os-matrix reconvert-footprint stage's instrument (COORD's ruling, 2026-11-23). A footprint is measured by TWO
    seeded reconverts diffed against EACH OTHER, never by a reconvert diffed against the committed tree, which carries
    unbanked drift (.claude/skills/corpus-reconvert, section 2). This script does that for ONE target, the -Goos/-Goarch
    flavour, so a lane whose box cannot hold two corpus roots still gets a measured footprint.

    -Goarch defaults to amd64, the arch every flavour but linux/arm64 is converted at. The pair must be one the corpus
    is laid out for, and the script reads that set from -Base's own src/core/Directory.Build.props (the
    GoCheckLayoutTarget conditions, which TestCorpusBuildFileCarriesTheUniverseCheck holds to the converter's
    platformUniverse) rather than keeping a list of its own. Any other pair is refused by name before anything is built.

    Both arms are seeded from the SAME tree, -Base's: its src/core, src/version.props, docs/validation and
    docs/ValidatedTestPackages.md, laid out as the repository lays them out. The seeds are therefore identical and only
    the converter differs, which is the axis a footprint measures. Seeding is not optional: an unseeded root gives the
    [module: GoManualConversion] detector nothing to detect, and every hand-owned file is clobbered by an emission that
    compiles. The seeded .cs count is asserted against git's own count for -Base, so a partial seed fails by name.

    Each arm's converter is built from that commit's src/go2cs alone. The two conversions run one after the other,
    never at once (a raced conversion corrupts a file with unresolved lift markers), each into its own root, with
    GOROOT set exactly as `go env GOROOT` prints it.

    The step FAILS when: a conversion exits non-zero; either arm holds an unresolved lift marker
    (DYNTYPE, DYNBASE, DYNNEW, ADAPTER); either arm rewrote a [module: GoManualConversion] file (the hand-own gate,
    path-precise); or the diff has a line this script cannot attribute. A non-empty footprint is NOT a failure: it is
    the reading. The controls the stage was seated with are base == changed (0 files) and TC-13, afb01dfb6c against
    14fe938a7a (2 files, -2/+2, per target).

    Written to -OutDir: footprint.diff (the whole diff), numstat.tsv (status, added, removed, path), packages.tsv
    (directory, files, added, removed), convert-base.log and convert-changed.log, and summary.md. The last line of
    summary.md is one machine-readable FOOTPRINT line.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Base,
    [Parameter(Mandatory)] [string] $Changed,
    [Parameter(Mandatory)] [ValidateSet('windows', 'linux', 'darwin')] [string] $Goos,
    # The flavour's arch. With -Goos it must name a target of -Base's layout universe (see the description).
    [ValidatePattern('^[a-z0-9]+$')] [string] $Goarch = 'amd64',
    # An empty or absent directory outside the repository: the two arms, the two converters and the archives go here.
    [Parameter(Mandatory)] [string] $Work,
    [Parameter(Mandatory)] [string] $OutDir
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

function Deny([string] $message) { throw "reconvert-footprint: $message" }

foreach ($pair in @(@('-Base', $Base), @('-Changed', $Changed))) {
    if ($pair[1] -cnotmatch '^[0-9a-f]{40}$') { Deny "$($pair[0]) must be a full 40-digit lowercase commit SHA; got '$($pair[1])'." }
}

$repo = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) { Deny 'run this from inside the repository checkout.' }
$Work = [System.IO.Path]::GetFullPath($Work)
$OutDir = [System.IO.Path]::GetFullPath($OutDir)
$repoFull = [System.IO.Path]::GetFullPath($repo)
if ($Work.StartsWith($repoFull, [System.StringComparison]::OrdinalIgnoreCase)) { Deny "-Work ($Work) is inside the repository; the arms must not land in a tracked tree." }
if ((Test-Path -LiteralPath $Work) -and @(Get-ChildItem -LiteralPath $Work -Force).Count -gt 0) { Deny "-Work ($Work) is not empty: a second conversion into one root is the raced-conversion hazard." }
New-Item -ItemType Directory -Force -Path $Work, $OutDir | Out-Null

# Both commits, fetched by name when the checkout does not hold them (a dispatch checks out one ref, shallow).
foreach ($sha in @($Base, $Changed | Select-Object -Unique)) {
    & git -C $repo cat-file -e "$sha^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0) {
        & git -C $repo fetch --no-tags --quiet --depth=1 origin $sha
        if ($LASTEXITCODE -ne 0) { Deny "could not fetch $sha from origin." }
        & git -C $repo cat-file -e "$sha^{commit}"
        if ($LASTEXITCODE -ne 0) { Deny "fetched $sha, and it is still not a commit here." }
    }
}

# The target must be one the corpus is laid out for, read from -Base's own build file: src/core/Directory.Build.props
# carries the converter's platformUniverse as GoCheckLayoutTarget's conditions. A pair outside it (windows/arm64) would
# convert a flavour the corpus never carries and read as a footprint.
$target = "$Goos/$Goarch"
$layoutProps = (& git -C $repo show "${Base}:src/core/Directory.Build.props" 2>$null) -join "`n"
$universe = @([regex]::Matches($layoutProps, "'\$\(_GoLayoutTarget\)'!='(?<t>[a-z0-9]+/[a-z0-9]+)'") | ForEach-Object { $_.Groups['t'].Value })
if (-not $universe.Count) { Deny "$Base's src/core/Directory.Build.props declares no layout targets (GoCheckLayoutTarget), so -Goos/-Goarch cannot be checked against it." }
if ($target -cnotin $universe) { Deny "$target is not a target the converted standard library is laid out for at $Base ($($universe -join ', ')); pass -Goos and -Goarch as one of them." }

# GOROOT exactly as go prints it: a respelled GOROOT misroutes the whole emission and still exits 0 (safety floor 6).
$goroot = (& go env GOROOT).Trim()
if ($LASTEXITCODE -ne 0 -or -not $goroot) { Deny 'go env GOROOT failed.' }
$env:GOROOT = $goroot
$goVersion = (& go env GOVERSION).Trim()

# Extract paths of a commit into a directory, through a zip so no host tar is involved (a GNU tar on a Windows PATH
# reads C: as a remote host).
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Expand-Commit([string] $sha, [string[]] $paths, [string] $destination) {
    $zip = Join-Path $Work ("{0}-{1}.zip" -f $sha.Substring(0, 12), [guid]::NewGuid().ToString('N').Substring(0, 8))
    & git -C $repo archive --format=zip -o $zip $sha -- @paths
    if ($LASTEXITCODE -ne 0) { Deny "git archive of $($paths -join ', ') at $sha failed." }
    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $destination)
    Remove-Item -LiteralPath $zip
}

# The seed set, as -Base holds it. version.props and docs/ feed the README badges; without them every README moves.
$seedPaths = @('src/core', 'src/version.props', 'docs/validation', 'docs/ValidatedTestPackages.md') | Where-Object {
    & git -C $repo cat-file -e "${Base}:$_" 2>$null
    $LASTEXITCODE -eq 0
}
if ('src/core' -notin $seedPaths) { Deny "$Base has no src/core." }
# core.quotepath=false: git otherwise QUOTES a non-ASCII path (golib's ж.*.cs files), and a quoted name ends in `.cs"`,
# so the count read 21 short and refused a correct seed (run 45104579032).
$expectedCs = @(& git -C $repo -c core.quotepath=false ls-tree -r --name-only $Base -- src/core | Where-Object { $_.EndsWith('.cs') }).Count

$arms = [ordered]@{ base = $Base; changed = $Changed }
$seedStamp = [DateTime]::new(2000, 1, 1, 0, 0, 0, [DateTimeKind]::Utc)
$exeSuffix = if ($IsWindows) { '.exe' } else { '' }
$report = [ordered]@{}

foreach ($arm in $arms.Keys) {
    $sha = $arms[$arm]

    # The converter at this arm's commit, from its own src/go2cs and nothing else.
    $convSrc = Join-Path $Work "converter-$arm"
    Expand-Commit $sha @('src/go2cs') $convSrc
    $exe = Join-Path $Work "go2cs-$arm$exeSuffix"
    Push-Location (Join-Path $convSrc 'src/go2cs')
    try {
        & go build -buildvcs=false -o $exe .
        if ($LASTEXITCODE -ne 0) { Deny "go build of the converter at $sha failed." }
    }
    finally { Pop-Location }

    # The seed: -Base's tree, for both arms.
    $root = Join-Path $Work $arm
    Expand-Commit $Base $seedPaths $root
    $core = Join-Path $root 'src/core'
    # Every seeded file is stamped to one fixed instant, so "written by the conversion" is a time test that cannot
    # depend on how an archive's timestamps were read back.
    Get-ChildItem -LiteralPath $root -Recurse -File | ForEach-Object { $_.LastWriteTimeUtc = $seedStamp }
    $seededCs = @(Get-ChildItem -LiteralPath $core -Recurse -File -Filter '*.cs').Count
    if ($seededCs -ne $expectedCs) { Deny "the $arm seed holds $seededCs .cs files where $Base tracks $expectedCs." }
    $stray = @(Get-ChildItem -LiteralPath $core -Recurse -Directory | Where-Object { $_.Name -in 'bin', 'obj', 'Generated' })
    if ($stray.Count) { Deny "the $arm seed holds build output ($($stray[0].FullName)); the seed must be tracked sources only." }
    # A hand-own carries the attribute on a line of its own. A generated file can MENTION it (a placeholder's
    # comment names the *_impl.cs that holds the body), so a substring match read runtime/alg.cs as a hand-own
    # (run 45104975959).
    $handOwns = @(Get-ChildItem -LiteralPath $core -Recurse -File -Filter '*.cs' | Where-Object {
        [regex]::IsMatch([System.IO.File]::ReadAllText($_.FullName), '(?m)^\s*\[module:\s*GoManualConversion\]')
    })

    # The conversion. Under 'Stop' a stderr warning would end this script and leave the converter running.
    $log = Join-Path $OutDir "convert-$arm.log"
    $started = [DateTime]::UtcNow
    Write-Host "== $arm ($sha): go2cs -stdlib -comments -platforms $target -go2cspath $(Join-Path $root 'src')"
    $ErrorActionPreference = 'Continue'
    & $exe -stdlib -comments -platforms $target -go2cspath (Join-Path $root 'src') *>&1 | Out-File -LiteralPath $log -Encoding utf8
    $convertExit = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    $seconds = [int]([DateTime]::UtcNow - $started).TotalSeconds

    # What it wrote: a file whose bytes it did not change keeps its seed time (the converter skips identical writes).
    $written = @(Get-ChildItem -LiteralPath $root -Recurse -File | Where-Object { $_.LastWriteTimeUtc -gt $seedStamp })
    $markers = @(Get-ChildItem -LiteralPath $core -Recurse -File -Filter '*.cs' | Where-Object {
        [System.IO.File]::ReadAllText($_.FullName) -match '«(DYNTYPE|DYNBASE|DYNNEW|ADAPTER):'
    } | ForEach-Object { [System.IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/') })
    $clobbered = @($handOwns | Where-Object { (Get-Item -LiteralPath $_.FullName).LastWriteTimeUtc -gt $seedStamp } |
        ForEach-Object { [System.IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/') })

    # Files the conversion writes beside the corpus (its progress text and HTML report) carry timings, so they differ
    # between any two runs; they are listed, never diffed.
    $coreFull = [System.IO.Path]::GetFullPath($core) + [System.IO.Path]::DirectorySeparatorChar
    $outside = @($written | Where-Object { -not $_.FullName.StartsWith($coreFull) } |
        ForEach-Object { [System.IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/') } | Sort-Object)

    $report[$arm] = [pscustomobject]@{
        Sha = $sha; Exit = $convertExit; Seconds = $seconds; Seeded = $seededCs; HandOwns = $handOwns.Count
        Written = $written.Count - $outside.Count; Outside = $outside; Markers = $markers; Clobbered = $clobbered
    }
    Write-Host ("   exit {0}, {1} s, {2} files written, {3} marker file(s), {4} hand-own(s) rewritten" -f $convertExit, $seconds, $written.Count, $markers.Count, $clobbered.Count)
}

# The footprint: the two corpora against each other.
$diffFile = Join-Path $OutDir 'footprint.diff'
& git -C $Work -c core.autocrlf=false -c core.quotepath=false diff --no-index --no-renames --no-color --no-ext-diff "--output=$diffFile" -- base/src/core changed/src/core
if ($LASTEXITCODE -gt 1) { Deny "git diff --no-index failed with exit $LASTEXITCODE." }
$numstatLines = @(& git -C $Work -c core.autocrlf=false -c core.quotepath=false diff --no-index --no-renames --numstat -- base/src/core changed/src/core)
if ($LASTEXITCODE -gt 1) { Deny "git diff --no-index --numstat failed with exit $LASTEXITCODE." }

$rows = @(foreach ($line in $numstatLines) {
    if ($line -notmatch '^(?<a>\d+|-)\t(?<r>\d+|-)\t(?<p>.+)$') { Deny "numstat line not understood: '$line'." }
    $p = $Matches['p']; $a = $Matches['a']; $r = $Matches['r']
    $status, $path = switch -Regex ($p) {
        '^\{base => changed\}/(?<x>.+)$' { 'M', $Matches['x']; break }
        '^base/(?<x>.+) => /dev/null$'   { 'D', $Matches['x']; break }
        '^/dev/null => changed/(?<x>.+)$' { 'A', $Matches['x']; break }
        default { Deny "numstat path not understood: '$p'." }
    }
    [pscustomobject]@{
        Status = $status; Added = if ($a -eq '-') { 0 } else { [int]$a }; Removed = if ($r -eq '-') { 0 } else { [int]$r }
        Binary = ($a -eq '-'); Path = $path
    }
})
$rows = @($rows | Sort-Object Path)
$rows | ForEach-Object { "{0}`t{1}`t{2}`t{3}" -f $_.Status, $_.Added, $_.Removed, $_.Path } |
    Set-Content -LiteralPath (Join-Path $OutDir 'numstat.tsv') -Encoding utf8

$packages = @($rows | Group-Object { [System.IO.Path]::GetDirectoryName($_.Path).Replace('\', '/') } | ForEach-Object {
    [pscustomobject]@{
        Directory = $_.Name; Files = $_.Count
        Added = ($_.Group | Measure-Object Added -Sum).Sum; Removed = ($_.Group | Measure-Object Removed -Sum).Sum
    }
} | Sort-Object Directory)
$packages | ForEach-Object { "{0}`t{1}`t{2}`t{3}" -f $_.Directory, $_.Files, $_.Added, $_.Removed } |
    Set-Content -LiteralPath (Join-Path $OutDir 'packages.tsv') -Encoding utf8

# GoPositionMap lines in the delta: an additive change predicts none unless a mapped file gains or loses a line.
$posAdded = 0; $posRemoved = 0
if (Test-Path -LiteralPath $diffFile) {
    foreach ($line in [System.IO.File]::ReadLines($diffFile)) {
        if ($line.StartsWith('+++') -or $line.StartsWith('---') -or -not $line.Contains('GoPositionMap')) { continue }
        if ($line.StartsWith('+')) { $posAdded++ } elseif ($line.StartsWith('-')) { $posRemoved++ }
    }
}

$files = $rows.Count
# A loop, not Measure-Object: over zero rows (the base == changed control) Measure-Object returns no Sum, which
# strict mode refuses (run 45107295889).
$added = 0; $removed = 0
foreach ($row in $rows) { $added += $row.Added; $removed += $row.Removed }
$b = $report['base']; $c = $report['changed']

$failures = @()
foreach ($arm in $report.Keys) {
    $r = $report[$arm]
    if ($r.Exit -ne 0) { $failures += "the $arm conversion exited $($r.Exit) (convert-$arm.log)" }
    if ($r.Markers.Count) { $failures += "the $arm arm holds unresolved lift markers in $($r.Markers.Count) file(s): $($r.Markers -join ', ')" }
    if ($r.Clobbered.Count) { $failures += "the $arm arm REWROTE $($r.Clobbered.Count) hand-owned file(s): $($r.Clobbered -join ', ')" }
}

$summary = @(
    "## Reconvert footprint -- $target"
    ''
    "base ``$Base`` -> changed ``$Changed``; both seeded from base ($($b.Seeded) .cs, $($b.HandOwns) hand-owned files); $goVersion, GOROOT ``$goroot``"
    ''
    '| arm | converter exit | seconds | corpus files written | lift-marker files | hand-owns rewritten |'
    '|---|---|---|---|---|---|'
    "| base | $($b.Exit) | $($b.Seconds) | $($b.Written) | $($b.Markers.Count) | $($b.Clobbered.Count) |"
    "| changed | $($c.Exit) | $($c.Seconds) | $($c.Written) | $($c.Markers.Count) | $($c.Clobbered.Count) |"
    ''
    "written outside src/core, not diffed: $(@($b.Outside + $c.Outside | Sort-Object -Unique) -join ', ')"
    ''
    "**$files file(s), -$removed/+$added**; GoPositionMap lines -$posRemoved/+$posAdded"
    ''
)
if ($files) {
    $summary += @('| directory | files | removed | added |', '|---|---|---|---|')
    $summary += @($packages | ForEach-Object { "| ``$($_.Directory)`` | $($_.Files) | $($_.Removed) | $($_.Added) |" })
    $summary += ''
    $summary += @('```') + @($rows | Select-Object -First 200 | ForEach-Object { "{0} -{1}/+{2} {3}" -f $_.Status, $_.Removed, $_.Added, $_.Path })
    if ($rows.Count -gt 200) { $summary += "... $($rows.Count - 200) more in numstat.tsv" }
    $summary += @('```', '')
}
if ($failures) { $summary += @('**FAILED:**', '') + @($failures | ForEach-Object { "- $_" }) + '' }
$summary += "FOOTPRINT goos=$Goos goarch=$Goarch base=$Base changed=$Changed files=$files removed=$removed added=$added posmap=-$posRemoved/+$posAdded markers=$($b.Markers.Count)/$($c.Markers.Count) handown-rewritten=$($b.Clobbered.Count)/$($c.Clobbered.Count) written=$($b.Written)/$($c.Written) exit=$($b.Exit)/$($c.Exit)"

$summary | Set-Content -LiteralPath (Join-Path $OutDir 'summary.md') -Encoding utf8
$summary | ForEach-Object { Write-Host $_ }

if ($failures) { exit 1 }
exit 0
