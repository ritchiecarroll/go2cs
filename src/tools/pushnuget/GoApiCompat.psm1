<#
.SYNOPSIS
    The binary-compatibility guard: is a package's public surface what its last published revision promised, except
    for the breaks its suppression file lists?

.DESCRIPTION
    A NuGet range only governs restore. The go.* assemblies are not strong-named, so the loader takes whatever version
    restore picked, and a member a compiled consumer binds that no longer exists fails at run time
    (MissingMethodException, TypeLoadException). PLAN-nugetgo B4 gives every converted module's stdlib dependency the
    range [build, next Go minor), which is honest only while each revision keeps the surface the earlier ones
    published. B3 (batch 231) is the first break known to matter; nothing in the release flow saw it.
    docs/phase4/DESIGN-binary-compat-guard.md sizes this guard and measures every claim below.

    THE ENGINE is Microsoft.DotNet.ApiCompat.Tool, the engine the SDK's package validation runs, pinned in the
    repository's local tool manifest (.config/dotnet-tools.json) and restored by Initialize-GoApiCompatTool.

    IT IS CALLED DIRECTLY, ONE PROCESS PER ASSET, IN ASSEMBLY MODE. Three measured reasons:
    - The SDK's package validation is INCREMENTAL. It writes a semaphore in obj/, and a pack whose inputs did not
      change skips it and exits 0. Measured: with the suppression file REMOVED, the second pack read green ("Skipping
      target RunPackageValidation"). A direct call has no state to skip on.
    - push-nuget.ps1 packs once per GoTargetOS and merges the flavors, so only the MERGED package is the one published.
      Validation inside `dotnet pack` sees one unmerged flavor and would compare, say, the linux build with the
      published Windows lib/.
    - Package mode also compares each package's compile asset with its runtime assets: 8,523 diagnostics in syscall,
      runtime and os, which are Go's per-GOOS API and unchanged across revisions. Assembly mode, baseline asset against
      the current build of the same asset, compares only revisions.

    IT PROVES IT RAN. Every process must print exactly one verdict line ("APICompat ran successfully ..." or "API
    breaking changes found ..."), and the guard counts compared assets against the baseline's asset count. A run that
    compared nothing cannot read green.

    SUPPRESSIONS. A package with an announced break carries CompatibilitySuppressions.xml beside its project. Each entry
    names its asset in Left and Right (package-relative: lib/net10.0/golib.dll). Before each asset's run, the guard
    writes just that asset's entries to a scratch file, so an entry is judged only by the comparison it belongs to.
    - An entry that matches nothing is RED: the tool exits 0 for one and only prints "Unnecessary suppressions found",
      so the guard reads that line itself. When a release becomes the baseline, its entries match nothing and must be
      deleted, so a suppression cannot outlive the release that announced it.
    - An entry naming an asset the comparison does not have is RED.
#>
#Requires -Version 5.1
Set-StrictMode -Version 2

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

# The scratch directories each side is extracted into. The transformation pattern strips everything up to them, so a
# diagnostic and a suppression name the package-relative asset (lib/net10.0/golib.dll) whatever the scratch root is.
$script:LeftDir = 'apicompat-left'
$script:RightDir = 'apicompat-right'
$script:Transform = '^.*[\\/]apicompat-(?:left|right)[\\/](.*)$;$1'

# A package asset this guard compares: an assembly under lib/<tfm>/ or runtimes/<rid>/lib/<tfm>/.
$script:AssetPattern = '^(lib/[^/]+|runtimes/[^/]+/lib/[^/]+)/[^/]+\.dll$'

$script:VerdictClean = 'APICompat ran successfully without finding any breaking changes.'
$script:VerdictBreaks = 'API breaking changes found.'
$script:UnnecessaryLine = 'Unnecessary suppressions found.'

# Restores the pinned tool from the repository's local manifest and returns the version it answers. Throws when the
# restore fails or the tool answers another version than the manifest pins.
function Initialize-GoApiCompatTool([Parameter(Mandatory)][string]$RepoRoot) {
    $manifest = Join-Path $RepoRoot '.config/dotnet-tools.json'
    if (-not (Test-Path -LiteralPath $manifest)) { throw "No tool manifest at ${manifest}: the ApiCompat pin is missing" }

    $pinned = "$((Get-Content -Raw -LiteralPath $manifest | ConvertFrom-Json).tools.'microsoft.dotnet.apicompat.tool'.version)"
    if (-not $pinned) { throw "${manifest} pins no microsoft.dotnet.apicompat.tool" }

    # A native tool's stderr is an error record under Windows PowerShell 5.1, and a terminating one under a caller's
    # Stop: the exit code is what this reads, so the preference is pinned here.
    $ErrorActionPreference = 'Continue'
    Push-Location $RepoRoot
    try {
        $restore = (& dotnet tool restore --tool-manifest $manifest 2>&1 | Out-String)
        if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed ($LASTEXITCODE): $restore" }

        $answer = "$(& dotnet tool run apicompat --version 2>&1 | Select-Object -Last 1)".Trim()
        if ($LASTEXITCODE -ne 0) { throw "apicompat --version failed ($LASTEXITCODE): $answer" }
    }
    finally { Pop-Location }

    # The tool answers <version>+<commit>.
    if (($answer -split '\+', 2)[0] -cne $pinned) { throw "apicompat answers '$answer', not the pinned $pinned" }
    $pinned
}

# Every lib/ and runtimes/ assembly a .nupkg carries, as package-relative paths, sorted.
function Get-GoApiCompatAssets([Parameter(Mandatory)][string]$Package) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try { @($zip.Entries | ForEach-Object { $_.FullName -replace '\\', '/' } | Where-Object { $_ -match $script:AssetPattern } | Sort-Object) }
    finally { $zip.Dispose() }
}

# Extracts the named assets of a .nupkg under $Dir, keeping their package-relative paths.
function Expand-GoApiCompatAssets([string]$Package, [string[]]$Assets, [string]$Dir) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        foreach ($entry in $zip.Entries) {
            $name = $entry.FullName -replace '\\', '/'
            if ($Assets -notcontains $name) { continue }
            $out = Join-Path $Dir $name
            New-Item -ItemType Directory -Force (Split-Path -Parent $out) | Out-Null
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $out, $true)
        }
    }
    finally { $zip.Dispose() }
}

<#
    Every CompatibilitySuppressions.xml under the stdlib tree, keyed by the package id push-nuget.ps1 gives that
    directory: its path below $CoreDir with each separator made a dot, after 'go.' (golib is go.lib). The map is built
    from the directories, never by turning an id back into a path: an id cannot say which of its dots were separators.
    go.vendor.golang.org.x.crypto.cryptobyte lives in vendor/golang.org/x/crypto/cryptobyte, and the reverse mapping
    looked in vendor/golang/org/... and found nothing (the whole-surface census, 2026-11-07).
#>
function Get-GoApiCompatSuppressionMap([Parameter(Mandatory)][string]$CoreDir) {
    $root = [System.IO.Path]::GetFullPath($CoreDir).TrimEnd('\', '/')
    $map = @{}
    foreach ($file in @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter 'CompatibilitySuppressions.xml')) {
        $rel = $file.Directory.FullName.Substring($root.Length).TrimStart('\', '/') -replace '[\\/]', '.'
        $id = if ($rel -ceq 'golib') { 'go.lib' } else { "go.$rel" }
        $map[$id] = $file.FullName
    }
    $map
}

# The entries of a suppression file, each with the XML node it came from. Refuses a file whose entry names no asset,
# or names different assets on its two sides: this guard compares an asset only with its own baseline.
function Read-GoApiCompatSuppressions([Parameter(Mandatory)][string]$Path) {
    # XmlDocument.Load, never Get-Content: Windows PowerShell 5.1 reads a BOM-less UTF-8 file as the ANSI code page, and
    # release-nuget.ps1 runs push-nuget.ps1 under 5.1. Every Target naming zhe (U+0436) came back as two cp1252
    # characters (U+00D0 U+00B6) and matched nothing, so the guard printed each announced break beside an "unnecessary"
    # entry for the same member (release-smoke 42533214613, 2026-11-08). Load honors the BOM and the file's own encoding
    # declaration under either PowerShell.
    $xml = New-Object System.Xml.XmlDocument
    $xml.Load((Resolve-Path -LiteralPath $Path).ProviderPath)
    if (-not $xml.Suppressions) { throw "${Path} is not an ApiCompat suppression file (no <Suppressions> root)" }

    # Under strict mode a missing child element is an error to read as a property, so each is read by name.
    function Get-Child($element, [string]$name) { $child = $element[$name]; if ($child) { $child.InnerText } else { '' } }

    foreach ($node in @($xml.Suppressions.ChildNodes | Where-Object { $_.NodeType -eq 'Element' })) {
        if ($node.LocalName -cne 'Suppression') { throw "${Path} carries a <$($node.LocalName)> where only <Suppression> belongs" }
        $left = Get-Child $node 'Left'; $right = Get-Child $node 'Right'
        $diagnostic = Get-Child $node 'DiagnosticId'; $target = Get-Child $node 'Target'
        if ($left -notmatch $script:AssetPattern) { throw "${Path}: a $diagnostic suppression of $target names '$left', not a package asset" }
        if ($left -cne $right) { throw "${Path}: a $diagnostic suppression of $target names '$left' and '$right'; both sides must name the same asset" }
        if ((Get-Child $node 'IsBaselineSuppression') -ceq 'true') { throw "${Path}: a $diagnostic suppression of $target is a package-mode baseline entry; this guard runs in assembly mode (Left and Right name the asset, no IsBaselineSuppression)" }
        [pscustomobject]@{ DiagnosticId = $diagnostic; Target = $target; Asset = $left; Node = $node }
    }
}

# Writes the given suppression entries to $Path as a suppression file of their own.
function Write-GoApiCompatSuppressions([object[]]$Entries, [string]$Path) {
    $doc = New-Object System.Xml.XmlDocument
    $doc.AppendChild($doc.CreateXmlDeclaration('1.0', 'utf-8', $null)) | Out-Null
    $root = $doc.AppendChild($doc.CreateElement('Suppressions'))
    foreach ($entry in $Entries) { $root.AppendChild($doc.ImportNode($entry.Node, $true)) | Out-Null }
    $doc.Save($Path)
}

# One comparison: the baseline asset against the current one, in its own process. Returns the verdict, the
# diagnostics and the unnecessary suppressions, or a Problem when the tool did not report exactly one comparison.
function Invoke-GoApiCompatAsset([string]$RepoRoot, [string]$Left, [string]$Right, [string]$SuppressionFile) {
    $arguments = @('tool', 'run', 'apicompat', '--',
        '-l', ($Left -replace '\\', '/'), '-r', ($Right -replace '\\', '/'),
        '--left-assemblies-transformation-pattern', $script:Transform,
        '--right-assemblies-transformation-pattern', $script:Transform)
    if ($SuppressionFile) { $arguments += @('--suppression-file', ($SuppressionFile -replace '\\', '/')) }

    $ErrorActionPreference = 'Continue'   # see Initialize-GoApiCompatTool: the verdict lines and the exit code decide
    Push-Location $RepoRoot
    try { $lines = @(& dotnet @arguments 2>&1 | ForEach-Object { "$_" }); $exit = $LASTEXITCODE }
    finally { Pop-Location }

    $clean = @($lines | Where-Object { $_.Trim() -ceq $script:VerdictClean }).Count
    $breaks = @($lines | Where-Object { $_.Trim().StartsWith($script:VerdictBreaks, [StringComparison]::Ordinal) }).Count
    $diagnostics = @($lines | Where-Object { $_ -match '^CP\d{4}: ' })

    # The unnecessary entries are listed after their header line, one "CPnnnn (Target: ...)" per line.
    $unnecessary = @()
    $at = [Array]::FindIndex([string[]]$lines, [Predicate[string]] { param($l) $l.Trim().StartsWith($script:UnnecessaryLine, [StringComparison]::Ordinal) })
    if ($at -ge 0 -and $at -lt $lines.Count - 1) { $unnecessary = @($lines[($at + 1)..($lines.Count - 1)] | Where-Object { $_ -match '^CP\d{4} \(Target: ' }) }

    $problem = $null
    if ($clean + $breaks -ne 1) { $problem = "the tool reported $($clean + $breaks) verdict(s) for one comparison (exit $exit): $(($lines | Select-Object -Last 5) -join ' | ')" }
    elseif ($breaks -eq 1 -and $diagnostics.Count -eq 0) { $problem = "the tool reported breaking changes and listed none (exit $exit)" }
    elseif ($clean -eq 1 -and $exit -ne 0) { $problem = "the tool reported no breaking change and exited $exit" }
    elseif ($at -ge 0 -and $unnecessary.Count -eq 0) { $problem = 'the tool reported unnecessary suppressions and listed none' }

    [pscustomobject]@{ Diagnostics = $diagnostics; Unnecessary = $unnecessary; Problem = $problem }
}

<#
    Compares one package with its baseline, asset by asset. Returns:
      Compared   the number of assets the tool reported a verdict for
      Assets     the baseline's assets
      Failures   one line per reason the package is red, each naming the package and its asset
    Every baseline asset must exist in the current package (a missing one is a break the tool cannot see). An asset only
    the current package has is new surface, which is compatible, and is not compared. THE NEW-RID RULE: a RID the
    baseline does not ship (the first linux-arm64, S6) is admitted with no prior baseline, and so is every runtimes/
    folder of a package newly RID-split; each is compared from the next release on, against this one. A package with no lib/ or
    runtimes/ asset (go.gen, an analyzer) compares nothing and reads green with Compared 0 of 0.
#>
function Test-GoApiCompatPackage {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$Baseline,
        [Parameter(Mandatory)][string]$Current,
        [string]$SuppressionFile,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$WorkDir
    )

    $failures = New-Object System.Collections.Generic.List[string]
    $assets = @(Get-GoApiCompatAssets $Baseline)
    $currentAssets = @(Get-GoApiCompatAssets $Current)

    $entries = @()
    if ($SuppressionFile) { $entries = @(Read-GoApiCompatSuppressions $SuppressionFile) }
    foreach ($entry in $entries) {
        if ($assets -notcontains $entry.Asset) { $failures.Add("${Id}: the suppression of $($entry.DiagnosticId) $($entry.Target) names $($entry.Asset), which the baseline does not carry") }
    }

    $work = Join-Path $WorkDir $Id
    if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force }
    # Created here, not by the first extraction: a package with no asset extracts nothing, and the removal below must
    # still find it (the first whole-surface census stopped on go.gen without it).
    New-Item -ItemType Directory -Force $work | Out-Null
    $leftRoot = Join-Path $work $script:LeftDir
    $rightRoot = Join-Path $work $script:RightDir
    Expand-GoApiCompatAssets $Baseline $assets $leftRoot
    Expand-GoApiCompatAssets $Current $assets $rightRoot

    $compared = 0
    foreach ($asset in $assets) {
        if ($currentAssets -notcontains $asset) { $failures.Add("${Id}: $asset is in the baseline and missing from the current package"); continue }

        $mine = @($entries | Where-Object { $_.Asset -ceq $asset })
        $suppressions = $null
        if ($mine.Count) {
            $suppressions = Join-Path $work ("suppressions-" + ($asset -replace '[\\/]', '_') + '.xml')
            Write-GoApiCompatSuppressions $mine $suppressions
        }

        $result = Invoke-GoApiCompatAsset $RepoRoot (Join-Path $leftRoot $asset) (Join-Path $rightRoot $asset) $suppressions
        if ($result.Problem) { $failures.Add("${Id}: ${asset}: $($result.Problem)"); continue }

        $compared++
        foreach ($line in $result.Diagnostics) { $failures.Add("${Id}: $line") }
        foreach ($line in $result.Unnecessary) { $failures.Add("${Id}: an unnecessary suppression (it matches nothing; delete it): $line") }
    }

    Remove-Item -LiteralPath $work -Recurse -Force
    [pscustomobject]@{ Id = $Id; Compared = $compared; Assets = $assets; Failures = @($failures) }
}

# The baseline for a package version: the highest published version on the same line (the same first three numbers),
# below the current one and not a prerelease. $null when the line has no published version yet.
function Get-GoApiCompatBaselineVersion([string[]]$Published, [Parameter(Mandatory)][string]$Current) {
    $mine = [version](($Current -split '[-+]', 2)[0])
    $candidates = foreach ($v in $Published) {
        if ($v -match '[-+]') { continue }
        $parsed = $null
        if (-not [version]::TryParse($v, [ref]$parsed)) { continue }
        if ($parsed.Major -ne $mine.Major -or $parsed.Minor -ne $mine.Minor -or $parsed.Build -ne $mine.Build) { continue }
        if ($parsed -ge $mine) { continue }
        [pscustomobject]@{ Text = $v; Version = $parsed }
    }
    $best = @($candidates | Sort-Object Version -Descending | Select-Object -First 1)
    if ($best.Count) { $best[0].Text } else { $null }
}

# The versions the flat container lists for an id; an empty list when the id was never published (404).
function Get-GoApiCompatPublishedVersions([System.Net.Http.HttpClient]$Client, [string]$FlatContainer, [string]$Id) {
    $url = "$FlatContainer$($Id.ToLowerInvariant())/index.json"
    $response = $Client.GetAsync($url).GetAwaiter().GetResult()
    try {
        $status = [int]$response.StatusCode
        if ($status -eq 404) { return @() }
        if ($status -ne 200) { throw "Cannot list the published versions of ${Id}: $url answered $status" }
        @(($response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json).versions)
    }
    finally { $response.Dispose() }
}

# Downloads one published .nupkg to $Dir and returns its path.
function Save-GoApiCompatBaseline([System.Net.Http.HttpClient]$Client, [string]$FlatContainer, [string]$Id, [string]$Version, [string]$Dir) {
    $id = $Id.ToLowerInvariant(); $v = $Version.ToLowerInvariant()
    $path = Join-Path $Dir "$id.$v.nupkg"
    if (Test-Path -LiteralPath $path) { return $path }
    $url = "$FlatContainer$id/$v/$id.$v.nupkg"
    $response = $Client.GetAsync($url).GetAwaiter().GetResult()
    try {
        if ([int]$response.StatusCode -ne 200) { throw "Cannot download the baseline $Id ${Version}: $url answered $([int]$response.StatusCode)" }
        New-Item -ItemType Directory -Force $Dir | Out-Null
        $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
        [System.IO.File]::WriteAllBytes($path, $bytes)
    }
    finally { $response.Dispose() }
    $path
}

<#
    The guard over a set of packed packages. For each package it finds the baseline on the feed's flat container, and
    compares the two with the package's suppression file ($SuppressionFor maps an id to a path or $null). Returns:
      Packages    the packages compared with a baseline
      Compared    the assets compared, over all packages
      NoBaseline  the ids whose line has no published version: new packages, or the first release of a Go minor.
                  They are reported, not gated (DESIGN-binary-compat-guard.md section 6).
      NoSurface   the ids whose baseline has no lib/ or runtimes/ asset (go.gen, an analyzer): nothing to compare.
      Baselines   id -> { Version, Path } of each baseline downloaded, under $WorkDir (read it before $WorkDir goes)
      Failures    every reason the guard is red
    A guard that was asked about at least one package with a baseline and compared no asset is itself a failure.

    $ReleaseVersion is the version the run stands for, when that is not the version it packed: the baseline is then
    the revision before the release, not before the pack. push-nuget.ps1 -VersionSuffix packed at version.props' build,
    which is the LAST-PUBLISHED one, so the 1.26.10.3 rehearsal packed 1.26.10.2-ci.<run>. By its own version that
    compared with .1 and read .2's own changes as breaks the release could never meet (release-smoke 42533214613,
    2026-11-08). A rehearsal now packs a prerelease of the would-be release (the same root failed the consumer's break
    floor in release-smoke 43000314806), so its own version gives the same baseline; an unsuffixed pack-only run still
    packs at the last-published build, and push-nuget passes the would-be version for both. Without it, each package's
    own version decides, as nugetgo-pack.ps1's real revisions want.
#>
function Invoke-GoApiCompatGuard {
    param(
        [Parameter(Mandatory)][System.IO.FileInfo[]]$Packages,
        [Parameter(Mandatory)][scriptblock]$Identity,
        [scriptblock]$SuppressionFor = { param($id) $null },
        [Parameter(Mandatory)][string]$FlatContainer,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$WorkDir,
        [string]$ReleaseVersion
    )

    Add-Type -AssemblyName System.Net.Http
    $client = New-Object System.Net.Http.HttpClient
    $client.Timeout = [TimeSpan]::FromMinutes(5)
    $flat = $FlatContainer.TrimEnd('/') + '/'

    $failures = New-Object System.Collections.Generic.List[string]
    $noBaseline = New-Object System.Collections.Generic.List[string]
    $noSurface = New-Object System.Collections.Generic.List[string]
    $baselines = @{}
    $packagesCompared = 0
    $compared = 0
    $expected = 0

    try {
        foreach ($package in $Packages) {
            $id = & $Identity $package.FullName
            $published = @(Get-GoApiCompatPublishedVersions $client $flat $id.Id)
            $baselineVersion = Get-GoApiCompatBaselineVersion -Published $published -Current $(if ($ReleaseVersion) { $ReleaseVersion } else { $id.Version })
            if (-not $baselineVersion) { $noBaseline.Add("$($id.Id) $($id.Version)"); continue }

            $baseline = Save-GoApiCompatBaseline $client $flat $id.Id $baselineVersion (Join-Path $WorkDir '_baselines')
            $baselines[$id.Id] = [pscustomobject]@{ Version = $baselineVersion; Path = $baseline }
            $suppression = & $SuppressionFor $id.Id
            $result = Test-GoApiCompatPackage -Id $id.Id -Baseline $baseline -Current $package.FullName -SuppressionFile $suppression -RepoRoot $RepoRoot -WorkDir $WorkDir

            $packagesCompared++
            $compared += $result.Compared
            $expected += $result.Assets.Count
            if ($result.Assets.Count -eq 0) { $noSurface.Add($id.Id) }
            foreach ($line in $result.Failures) { $failures.Add("$line (baseline $baselineVersion)") }
        }
    }
    finally { $client.Dispose() }

    if ($packagesCompared -gt 0 -and $compared -eq 0) { $failures.Add("the guard compared no asset of $packagesCompared package(s) with a baseline: it did not run") }

    [pscustomobject]@{
        Packages   = $packagesCompared
        Compared   = $compared
        Expected   = $expected
        NoBaseline = @($noBaseline)
        NoSurface  = @($noSurface)
        Failures   = @($failures)
        Baselines  = $baselines
    }
}

<#
    THE BREAK FLOOR (docs/phase4/DESIGN-go-break-floor.md). go.lib's buildTransitive/go.lib.targets states
    <GoBreakFloor>, the newest go.* release that changed members compiled code binds, and fails a consumer's build for
    any package built below it (GO2CS3002). Nothing moves the floor but its one line in that file; push-nuget refuses a
    release whose floor disagrees with what the guard says the release does. Once the guard is green, every suppression
    entry left is a break against the baseline (an entry that matches nothing is red), so:
      a reachable break             the floor is this release
      no reachable break            the floor is the baseline go.lib's floor
        ... and the baseline has none (the first release with the floor): a published release on this Go line
      no baseline on this Go line   the floor is this release (the first release of a Go minor)
    Returns one line per failure; none when the floor is right.
#>
function Test-GoBreakFloor {
    param(
        [Parameter(Mandatory)][string]$ReleaseVersion,
        [AllowEmptyString()][string]$Floor,
        [AllowEmptyString()][string]$BaselineVersion,
        [AllowEmptyString()][string]$BaselineFloor,
        [AllowEmptyCollection()][string[]]$ReachableBreaks = @()
    )

    $release = ($ReleaseVersion -split '[-+]', 2)[0]
    $file = 'src/core/golib/buildTransitive/go.lib.targets'
    $shape = @(Test-GoBreakFloorShape -ReleaseVersion $ReleaseVersion -Floor $Floor)
    if ($shape.Count -gt 0) { return $shape }
    $parsed = [version]$Floor
    $breaks = @($ReachableBreaks | Where-Object { $_ })

    if ($breaks.Count -gt 0) {
        if ($Floor -cne $release) {
            return @("break floor: release $release breaks members a consumer binds ($($breaks -join ', ') carr$(if ($breaks.Count -eq 1) { 'ies' } else { 'y' }) suppression entries), so <GoBreakFloor> in $file must be $release, not $Floor")
        }
        return @()
    }

    if (-not $BaselineVersion) {
        if ($Floor -cne $release) {
            return @("break floor: release $release is the first on its Go line, so <GoBreakFloor> in $file must be $release, not $Floor (everything built on an earlier minor is below it)")
        }
        return @()
    }

    if ($BaselineFloor) {
        if ($Floor -cne $BaselineFloor) {
            return @("break floor: release $release breaks nothing a consumer binds, so <GoBreakFloor> in $file must stay at the baseline $BaselineVersion's $BaselineFloor, not $Floor (a move forces a rebuild wave for no break)")
        }
        return @()
    }

    $baseline = [version](($BaselineVersion -split '[-+]', 2)[0])
    if ($parsed.Major -ne $baseline.Major -or $parsed.Minor -ne $baseline.Minor -or $parsed -gt $baseline) {
        return @("break floor: the baseline go.lib $BaselineVersion states no floor, so <GoBreakFloor> in $file must name a release published on its Go line, at or below $BaselineVersion, not $Floor")
    }
    @()
}

# What holds in all four of Test-GoBreakFloor's cases, and needs no baseline: the floor parses as
# major.minor.patch.revision, sits on the release's Go line (major.minor) and is at most the release. -VerifyOnly packs
# nothing and compares with nothing, so this is the part of the check it runs. Returns failure strings.
function Test-GoBreakFloorShape {
    param(
        [Parameter(Mandatory)][string]$ReleaseVersion,
        [AllowEmptyString()][string]$Floor
    )

    $release = ($ReleaseVersion -split '[-+]', 2)[0]
    $file = 'src/core/golib/buildTransitive/go.lib.targets'
    $parsed = $null
    if (-not $Floor -or -not [version]::TryParse($Floor, [ref]$parsed) -or $parsed.Revision -lt 0) {
        return @("break floor: $file states no <GoBreakFloor> major.minor.patch.revision (read '$Floor'); release $release cannot ship without one")
    }
    $releaseParsed = [version]$release
    if ($parsed.Major -ne $releaseParsed.Major -or $parsed.Minor -ne $releaseParsed.Minor) {
        return @("break floor: <GoBreakFloor> $Floor in $file is not on release ${release}'s Go line ($($releaseParsed.Major).$($releaseParsed.Minor)); every case puts the floor on it")
    }
    if ($parsed -gt $releaseParsed) {
        return @("break floor: <GoBreakFloor> $Floor in $file is above release $release; a floor names this release or one already published")
    }
    @()
}

<#
.SYNOPSIS
    The break floor read from the consumer's side: the reasons a pack's go.* packages fail the floor of the go.lib it ships.
.DESCRIPTION
    A pack is consumed with its own go.lib, whose targets fail the build, as GO2CS3002, for each go.* package whose
    version is below <GoBreakFloor>, a prerelease counting as its release. So a pack with any go.* package below that
    floor cannot be consumed at all. Release-smoke 43000314806 packed 1.26.10.3-ci.<run> beside a floor of 1.26.10.4:
    push-nuget's own checks were green, because they judge the release the run stands for, and every consumer build on
    every RID failed. A -VersionSuffix pack is therefore a prerelease of the release it rehearses (push-nuget.ps1).
    This applies go.lib.targets' rule as written: the version less everything from its first '-', compared with
    MSBuild's VersionLessThan; a version that does not parse counts as below. -Packages are identities with Id and
    Version; IDs that do not start with go. are not read, as go.lib.targets does not read them as go.* packages.
    Returns one failure per version below the floor, naming its packages; none when no package is below it.
#>
function Test-GoPackFloor {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Packages,
        [AllowEmptyString()][string]$Floor
    )

    $floorParsed = $null
    if (-not $Floor -or -not [version]::TryParse($Floor, [ref]$floorParsed)) {
        return @("pack floor: the packed go.lib states no <GoBreakFloor> that parses (read '$Floor'), so no consumer build of this pack can be judged")
    }
    $failures = [System.Collections.Generic.List[string]]::new()
    foreach ($group in @($Packages | Where-Object { $_.Id -like 'go.*' } | Group-Object -Property Version | Sort-Object -Property Name)) {
        $release = ($group.Name -split '-', 2)[0]
        $parsed = $null
        if ([version]::TryParse($release, [ref]$parsed) -and $parsed -ge $floorParsed) { continue }
        $ids = @($group.Group | ForEach-Object { $_.Id } | Sort-Object)
        $named = ($ids | Select-Object -First 3) -join ', '
        if ($ids.Count -gt 3) { $named += " and $($ids.Count - 3) more" }
        $failures.Add("pack floor: $named $(if ($ids.Count -eq 1) { 'is' } else { 'are' }) packed at $($group.Name), which go.lib.targets reads as $release, below the <GoBreakFloor> $Floor of the go.lib this pack ships: every build that consumes the pack fails with GO2CS3002 (a prerelease pack is versioned as the release it rehearses)")
    }
    $failures.ToArray()
}

# A second export line, so this function's export sits beside it rather than in the list at the end. The two accumulate.
Export-ModuleMember -Function Test-GoPackFloor

# Whether a converted module can bind a package: Go's import rule. A path with an `internal` element, or under
# `vendor/`, is importable only by the standard library. golib (go.lib) is reachable.
function Test-GoBreakFloorReachable([Parameter(Mandatory)][string]$Id) {
    if ($Id -ceq 'go.lib') { return $true }
    $segments = @(($Id -replace '^go\.', '') -split '\.')
    if ($segments[0] -ceq 'vendor') { return $false }
    -not ($segments -ccontains 'internal')
}

# The floor a go.lib package states, read from its buildTransitive/go.lib.targets; $null when it states none.
function Get-GoBreakFloorFromPackage([Parameter(Mandatory)][string]$Package) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        $entry = $zip.GetEntry('buildTransitive/go.lib.targets')
        if (-not $entry) { return $null }
        $reader = New-Object System.IO.StreamReader($entry.Open())
        try { Get-GoBreakFloor $reader.ReadToEnd() } finally { $reader.Dispose() }
    }
    finally { $zip.Dispose() }
}

# The floor a go.lib.targets text states; $null when it states none.
function Get-GoBreakFloor([AllowEmptyString()][string]$TargetsText) {
    if ($TargetsText -match '<GoBreakFloor>\s*([^<\s]+)\s*</GoBreakFloor>') { return $Matches[1] }
    $null
}

Export-ModuleMember -Function Initialize-GoApiCompatTool, Get-GoApiCompatAssets, Get-GoApiCompatSuppressionMap,
    Read-GoApiCompatSuppressions, Test-GoApiCompatPackage, Get-GoApiCompatBaselineVersion, Get-GoApiCompatPublishedVersions,
    Save-GoApiCompatBaseline, Invoke-GoApiCompatGuard,
    Test-GoBreakFloor, Test-GoBreakFloorShape, Test-GoBreakFloorReachable, Get-GoBreakFloor, Get-GoBreakFloorFromPackage
