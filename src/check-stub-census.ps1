<#
.SYNOPSIS
    The GENERATED-STUB CENSUS gate (go1.26 hop, H5; COORD 2026-10-14): every member go2cs-gen's
    PartialStubGenerator fills with a throwing body, per GOOS flavour, diffed against a baseline.
.DESCRIPTION
    A bodyless partial method that nothing implements COMPILES GREEN: PartialStubGenerator gives it
    `throw new NotImplementedException(...)` and marks it [go.GoExternalStub]. The failure then waits for
    the first call at run time. A release hop can strand a member that way with no compile red at all
    (H5 pre-seat 1: go1.26 moved Syscall6 into a new package, and every Linux syscall would have thrown).
    This gate makes that population visible per flavour, and fails on any member that APPEARS without a
    prediction.

    The predicate is the generator's own output, nothing ported: for each flavour it builds
    src/go2cs-stdlib.slnx exactly as the baseline was built, from a clean Generated/, and reads
    src/core/<project>/Generated/go2cs-gen/go2cs.PartialStubGenerator/*.stub.g.cs.

        dotnet build src/go2cs-stdlib.slnx -c Release -p:GoTargetOS=<goos> --no-incremental -m:4 -p:UseSharedCompilation=false

    Each list line is `project<TAB>file name` (project relative to src, ordinal sort, LC_ALL=C). Lists are
    compared on `project + namespace.class.member`, with the trailing `.<n>` index stripped: that index is
    a per-compilation counter, so one added member renumbers every stub after it in the same project.
.PARAMETER Flavors
    The GOOS flavours to census. Default: windows, linux, darwin.
.PARAMETER Baseline
    The directory holding stubs-<goos>.txt baselines. Default: docs/phase4/data/stub-census-h0.
.PARAMETER Predicted
    Optional TSV of PREDICTED appearances: `goos<TAB>project<TAB>namespace.class.member<TAB>owner and
    disposition`. A predicted appearance is reported as PREDICTED, not as a finding. `*` in the goos
    column matches every flavour.
.PARAMETER OutDir
    Where this run writes its stubs-<goos>.txt lists, build logs and report. Default: a fresh directory
    under the system temp folder.
.PARAMETER SkipBuild
    Read the Generated/ trees already on disk instead of building (re-reading a run's output, or a
    planted control). Never use it for a gate reading: a stale Generated/ is exactly what the gate cannot
    tell from a fresh one.
.NOTES
    Exit codes: 0 every appearance predicted; 1 at least one UNPREDICTED appearance (a named finding);
    2 refused (vacuous list, missing positive control, build error, or a tracked file under Generated/).
#>
param(
    [string[]] $Flavors = @("windows", "linux", "darwin"),
    [string] $Baseline = "",
    [string] $Predicted = "",
    [string] $OutDir = "",
    [switch] $SkipBuild
)

$ErrorActionPreference = "Stop"
$src = $PSScriptRoot
$repo = Split-Path $src -Parent

if ($Baseline -eq "") { $Baseline = Join-Path $repo "docs/phase4/data/stub-census-h0" }
if ($OutDir -eq "") { $OutDir = Join-Path ([System.IO.Path]::GetTempPath()) ("stub-census-" + (Get-Date -Format "yyyyMMdd-HHmmss")) }

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$stubDirName = "go2cs.PartialStubGenerator"
$core = Join-Path $src "core"

function Refuse([string] $why) {
    Write-Host "STUB CENSUS REFUSED: $why" -ForegroundColor Red
    exit 2
}

# The key the comparison runs on: project + namespace.class.member, the per-compilation `.<n>` index stripped.
function StubKey([string] $line) {
    $parts = $line -split "`t", 2
    if ($parts.Count -ne 2) { return $null }
    $member = $parts[1] -replace '\.\d+\.stub\.g\.cs$', ''
    return "$($parts[0])`t$member"
}

# LF-terminated, on every host: the committed baselines are LF, so a list this script writes compares with them
# byte for byte (WriteAllLines would write CRLF on Windows and differ by line endings alone).
function WriteLf([string] $path, [string[]] $lines) {
    $text = if ($lines.Count -gt 0) { ($lines -join "`n") + "`n" } else { "" }
    [System.IO.File]::WriteAllText($path, $text, [System.Text.UTF8Encoding]::new($false))
}

# Generated/ is build output. A TRACKED file under it would be deleted by the clean below.
$tracked = @(& git -C $repo ls-files -- "src/core/*/Generated/*" 2>$null)
if ($LASTEXITCODE -ne 0) { Refuse "git ls-files failed; cannot prove Generated/ holds no tracked file" }
if ($tracked.Count -gt 0) { Refuse "$($tracked.Count) tracked file(s) under src/core/**/Generated/, first: $($tracked[0])" }

$predictions = @{}

if ($Predicted -ne "") {
    foreach ($row in Get-Content -LiteralPath $Predicted) {
        if ($row -match '^\s*(#|$)') { continue }
        $cols = $row -split "`t"
        if ($cols.Count -lt 4) { Refuse "predicted row has fewer than 4 columns: $row" }
        $predictions["$($cols[0])`t$($cols[1])`t$($cols[2])"] = $cols[3]
    }
}

$report = New-Object System.Collections.Generic.List[string]
$findings = 0

foreach ($goos in $Flavors) {
    $listPath = Join-Path $OutDir "stubs-$goos.txt"

    if (-not $SkipBuild) {
        # From a clean Generated/, as the baseline was taken: a stale stub file would read as a member.
        Get-ChildItem -LiteralPath $core -Recurse -Directory -Filter "Generated" -ErrorAction SilentlyContinue |
            ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force }

        $log = Join-Path $OutDir "build-$goos.log"
        & dotnet build (Join-Path $src "go2cs-stdlib.slnx") -c Release "-p:GoTargetOS=$goos" --no-incremental -m:4 -p:UseSharedCompilation=false -nologo -v:m -tl:off *> $log
        $buildExit = $LASTEXITCODE
        $errors = @(Select-String -LiteralPath $log -Pattern ' error [A-Z]+[0-9]+' | ForEach-Object { $_.Line } | Sort-Object -Unique).Count

        if ($buildExit -ne 0 -or $errors -gt 0) { Refuse "$goos build exit $buildExit with $errors distinct error(s); see $log" }
    }

    # project<TAB>file name, project relative to src, ordinal sort.
    $lines = @(Get-ChildItem -LiteralPath $core -Recurse -File -Filter "*.stub.g.cs" -ErrorAction SilentlyContinue |
        Where-Object { $_.Directory.Name -eq $stubDirName } |
        ForEach-Object {
            $projectDir = $_.Directory.Parent.Parent.Parent.FullName  # <project>/Generated/go2cs-gen/<generator>
            $project = [System.IO.Path]::GetRelativePath($src, $projectDir).Replace('\', '/')
            "$project`t$($_.Name)"
        })
    [Array]::Sort($lines, [StringComparer]::Ordinal)
    WriteLf $listPath ([string[]]$lines)

    if ($lines.Count -eq 0) { Refuse "$goos lists 0 stubs: a census that finds nothing is not measuring" }
    if (-not ($lines | Where-Object { $_ -match "^core/internal/cpu`t.*\.cpuid\.\d+\.stub\.g\.cs$" })) {
        Refuse "$goos positive control missing: internal/cpu cpuid (bodyless in every flavour) is not in the list"
    }

    $basePath = Join-Path $Baseline "stubs-$goos.txt"
    if (-not (Test-Path -LiteralPath $basePath)) { Refuse "no baseline for $goos at $basePath" }

    $baseKeys = @{}
    foreach ($line in Get-Content -LiteralPath $basePath) { $k = StubKey $line; if ($k) { $baseKeys[$k] = $true } }
    $runKeys = @{}
    foreach ($line in $lines) { $k = StubKey $line; if ($k) { $runKeys[$k] = $true } }

    $appeared = @($runKeys.Keys | Where-Object { -not $baseKeys.ContainsKey($_) } | Sort-Object)
    $gone = @($baseKeys.Keys | Where-Object { -not $runKeys.ContainsKey($_) } | Sort-Object)

    $report.Add("== $goos : $($lines.Count) stub files ($($runKeys.Count) members); baseline $($baseKeys.Count) members; appeared $($appeared.Count), gone $($gone.Count)")

    foreach ($key in $appeared) {
        $p = $key -split "`t"
        $hit = $predictions["$goos`t$($p[0])`t$($p[1])"]
        if (-not $hit) { $hit = $predictions["*`t$($p[0])`t$($p[1])"] }

        if ($hit) {
            $report.Add("   PREDICTED  $($p[0])  $($p[1])  -- $hit")
        } else {
            $report.Add("   FINDING    $($p[0])  $($p[1])  -- appeared, no prediction: name its owner")
            $findings++
        }
    }

    foreach ($key in $gone) {
        $p = $key -split "`t"
        $report.Add("   GONE       $($p[0])  $($p[1])  (gained a body, or its package/member left)")
    }
}

$reportPath = Join-Path $OutDir "report.txt"
WriteLf $reportPath ([string[]]$report)
$report | ForEach-Object { Write-Host $_ }
Write-Host "lists and report: $OutDir"

if ($findings -gt 0) {
    Write-Host "STUB CENSUS: $findings UNPREDICTED appearance(s). Each is a named H5 finding." -ForegroundColor Red
    exit 1
}

Write-Host "STUB CENSUS: every appearance predicted." -ForegroundColor Green
exit 0
