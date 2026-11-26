<#
.SYNOPSIS
    Deploys the go2cs core libraries to the standard root %GOPATH%\src\go2cs so that
    converted projects (and, later, recursively converted end-user apps that target the
    same root) can lean on relative $(go2csPath) project references.

.DESCRIPTION
    Stages src\core -- the converted Go standard library, the shared golib runtime and the
    hand-owned packages -- at <root>\core\<pkg>, and the source-generator/analyzer at
    <root>\gen\go2cs-gen, then writes a Directory.Build.props at <root> pinning $(go2csPath) to
    that root. Because every generated .csproj references its dependencies as
    $(go2csPath)core\<pkg> / $(go2csPath)gen\go2cs-gen, that single props file makes the whole
    deployed tree -- and anything a recursive conversion later drops under the same root --
    resolve without any per-build -p:go2csPath flag.

    The deployed layout is IDENTICAL to the repository layout: since the converted standard
    library moved home to src\core (2026-08-01) there is one tree and one path scheme, so this
    script no longer has stub/stdlib modes and no longer rewrites any project reference.

.PARAMETER Target
    Deploy root. Defaults to <GOPATH>\src\go2cs (GOPATH from `go env GOPATH`).

.PARAMETER Configuration
    Build configuration used by the verify build. Defaults to Debug.

.PARAMETER NoBuild
    Copy + wire up only; skip the verify build.

.PARAMETER WhatIf
    Dry run: report exactly what would be cleaned, copied and written, and change nothing. The
    deploy root defaults to %GOPATH%\src\go2cs, which is MACHINE-GLOBAL and shared with every other
    checkout on the box -- so a change to this script has to be provable without deploying. Pair it
    with -Target <scratch dir> for an end-to-end test that touches nothing anyone else can see.

.EXAMPLE
    .\deploy-core.ps1
.EXAMPLE
    .\deploy-core.ps1 -NoBuild
.EXAMPLE
    .\deploy-core.ps1 -WhatIf
.EXAMPLE
    .\deploy-core.ps1 -Target D:\scratch\deploy-probe -NoBuild
#>
#Requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$Target,
    [string]$Configuration = 'Debug',
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'

# Repo src\ directory (this script's home) -- all sources are resolved from here, so the
# script works regardless of the caller's current directory. The shared platform primitives come
# from src\_paths.ps1 so this script agrees with every other instrument about the roots.
. (Join-Path $PSScriptRoot '_paths.ps1')
$srcRoot = $PSScriptRoot

# Portable replacement for the robocopy call this script used to make. robocopy is Windows-only and
# was this repository's single hard external-tool dependency (F7, docs/PLAN-linux-operation.md);
# rsync-on-Linux behind the same signature would be a SECOND implementation of the exclusion rules,
# which are load-bearing. One .NET-level implementation instead, identical on every host.
#
# Reproduces exactly the robocopy invocation it replaces:
#   /E                      recurse INCLUDING empty directories
#   /XD bin obj Generated .vs   exclude those directory NAMES at any depth
#   /XF <names>             exclude those file names/wildcards at any depth
# and nothing else -- no mirroring, no deletion. The destination is cleaned by the caller first,
# exactly as before, so /MIR semantics were never in play.
function Copy-SourceTree {
    param(
        [Parameter(Mandatory)] [string]$From,
        [Parameter(Mandatory)] [string]$To,
        [string[]]$ExcludeFiles = @()
    )

    $excludeDirs = @('bin', 'obj', 'Generated', '.vs')
    $from = [System.IO.Path]::GetFullPath($From).TrimEnd('\', '/')

    # Directories first (so /E's empty-directory behavior is preserved), then files. Both walks
    # apply the same directory exclusion, tested SEGMENT-WISE rather than by substring: a substring
    # test would also drop a legitimate path that merely contains "bin" (e.g. a package named
    # "binary" -- src\core\encoding\binary is exactly that, and matching it would silently deploy an
    # incomplete standard library).
    $isExcluded = {
        param([string]$fullPath)

        $relative = $fullPath.Substring($from.Length).Trim('\', '/')

        if (-not $relative) { return $false }

        foreach ($segment in $relative -split '[\\/]') {
            if ($excludeDirs -contains $segment) { return $true }
        }

        return $false
    }

    $dirs = @(Get-ChildItem -LiteralPath $from -Directory -Recurse -Force |
        Where-Object { -not (& $isExcluded $_.FullName) })

    $files = @(Get-ChildItem -LiteralPath $from -File -Recurse -Force | Where-Object {
        if (& $isExcluded $_.DirectoryName) { return $false }

        foreach ($pattern in $ExcludeFiles) {
            if ($_.Name -like $pattern) { return $false }
        }

        return $true
    })

    if (-not $PSCmdlet.ShouldProcess($To, "copy $($files.Count) file(s) in $($dirs.Count + 1) directory(ies) from $from")) {
        Write-Host "    [WhatIf] would copy $($files.Count) file(s), $($dirs.Count + 1) directory(ies)" -ForegroundColor DarkGray
        return
    }

    [System.IO.Directory]::CreateDirectory($To) | Out-Null

    foreach ($dir in $dirs) {
        [System.IO.Directory]::CreateDirectory((Join-Path $To $dir.FullName.Substring($from.Length).Trim('\', '/'))) | Out-Null
    }

    foreach ($file in $files) {
        # Copy-Item, not [System.IO.File]::Copy: the latter resolves a relative destination against
        # the PROCESS working directory, which PowerShell's own location does not track.
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $To $file.FullName.Substring($from.Length).Trim('\', '/')) -Force
    }

    Write-Host "    copied $($files.Count) file(s), $($dirs.Count + 1) directory(ies)" -ForegroundColor DarkGray
}

# ---- Resolve the deploy root ------------------------------------------------------------
if (-not $Target) {
    $goPath = (& go env GOPATH 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($goPath)) {
        throw "Could not resolve GOPATH (is Go on PATH?). Pass -Target explicitly."
    }
    $Target = Join-Path $goPath.Trim() 'src/go2cs'
}

# Normalize to a full path with no trailing separator (used for relative-path math below). Both
# separators are trimmed: on a non-Windows host the trailing one is '/'.
if ($PSCmdlet.ShouldProcess($Target, 'create deploy root')) {
    [System.IO.Directory]::CreateDirectory($Target) | Out-Null
}
$Target = [System.IO.Path]::GetFullPath($Target).TrimEnd('\', '/')

$coreDst = Join-Path $Target 'core'
$genRoot = Join-Path $Target 'gen'
$genDst  = Join-Path $genRoot 'go2cs-gen'

Write-Host "deploy-core: $(Join-Path $srcRoot 'core') -> $Target" -ForegroundColor Cyan

# ---- Clean the pieces this script owns (leave sibling app dirs untouched) ----------------
foreach ($dir in @($coreDst, $genRoot)) {
    if (Test-Path $dir) {
        Write-Host "  cleaning $dir" -ForegroundColor DarkGray
        Remove-Item -Recurse -Force $dir
    }
}

# ---- Deploy the analyzer -----------------------------------------------------------------
Write-Host "  deploying analyzer  -> gen/go2cs-gen" -ForegroundColor Yellow
Copy-SourceTree (Join-Path $srcRoot 'gen/go2cs-gen') $genDst

# ---- Deploy the standard library ----------------------------------------------------------
# A straight copy: src\core already IS the deployed layout. Two exclusions:
#   * core's own Directory.Build.props -- the root props written below is the authoritative
#     one; a nested copy would shadow it for everything under core\.
#   * the committed Phase-4 test projects -- a `<pkg>.tests.csproj` builds against the
#     hand-owned go.testing host and belongs to go2cs's own test infrastructure, not to the
#     deployable standard library; staging it makes the verify build fail.
Write-Host "  deploying standard library -> core" -ForegroundColor Yellow
Copy-SourceTree (Join-Path $srcRoot 'core') $coreDst -ExcludeFiles @('Directory.Build.props', '*.tests.csproj')

# ---- Stage the shared version.props at the deploy root -----------------------------------
# go2cs-gen.csproj does <Import ..\..\version.props /> (the single-source NuGet version stamp,
# src\version.props; golib gets it via core's Directory.Build.props instead, which this deploy
# excludes). Deployed to <root>\gen\go2cs-gen, that relative import resolves to
# <root>\version.props, so the file must sit at the deploy root or every project transitively
# referencing the analyzer fails with MSB4019.
Copy-Item (Join-Path $srcRoot 'version.props') -Destination (Join-Path $Target 'version.props') -Force

# ---- Stage the field-access collision list where go2cs-gen embeds it from ----------------
# go2cs-gen.csproj embeds ..\..\go2cs\fieldAccessCollisions.txt (since 810b9bb208): the ONE list the converter
# reads by go:embed (src\go2cs\fieldAccess.go) and go2cs-gen reads as a resource. go:embed cannot reach outside
# the converter's module, so the list stays in src\go2cs and is staged here at <root>\go2cs\, where the
# deployed analyzer's relative include resolves. Without it a deployed go2cs-gen fails CS1566 and nothing in the
# deploy root builds. repoguard TestDeployCoreStagesEveryGenInput holds every such ..\..\ input to this script.
$collisionsDst = Join-Path $Target 'go2cs'
if ($PSCmdlet.ShouldProcess($collisionsDst, 'stage go2cs/fieldAccessCollisions.txt')) {
    [System.IO.Directory]::CreateDirectory($collisionsDst) | Out-Null
    Copy-Item (Join-Path $srcRoot 'go2cs/fieldAccessCollisions.txt') -Destination (Join-Path $collisionsDst 'fieldAccessCollisions.txt') -Force
}

# ---- Write the root Directory.Build.props ------------------------------------------------
# Pins $(go2csPath) to this deploy root for every project beneath it, so all
# $(go2csPath)core\... / $(go2csPath)gen\... references resolve with no per-build flag.
#
# It also defaults GoTargetOS/GoTargetArch to the corpus flavor of the host that deploys. A program
# converted in place here (go2cs -recurse -go2cspath <this root>) keeps THIS props -- the converter
# never overwrites a props it did not write -- so without these defaults every L3 project built its
# csproj default, the windows flavor, and a program built on Linux or macOS died at start on
# kernel32.dll unless each build named -p:GoTargetOS. Both stay condition-guarded on empty, so an
# explicit -p: global or an environment variable still wins. A host whose GOOS the corpus does not
# carry ($HostGoos is $null) gets no default rather than a guess, as in _paths.ps1. The arch is
# Get-HostFlavorArch's, the one every other instrument on this host builds: an Apple-silicon host keeps
# darwin/amd64 under _paths.ps1's interim pin until D5, so a deploy there does not default a flavor
# the host's own instruments do not build.
$hostFlavorArch = Get-HostFlavorArch -Goos $HostGoos -Goarch $HostGoarch
$platformLines = ''
if ($HostGoos -and $hostFlavorArch) {
    $platformLines = @"

  <!-- The corpus flavor of the host that ran deploy-core.ps1. Override with -p:GoTargetOS and
       -p:GoTargetArch (or the environment variables of the same names) to build another flavor. -->
  <PropertyGroup Condition="'`$(GoTargetOS)' == ''">
    <GoTargetOS>$HostGoos</GoTargetOS>
  </PropertyGroup>

  <PropertyGroup Condition="'`$(GoTargetArch)' == ''">
    <GoTargetArch>$hostFlavorArch</GoTargetArch>
  </PropertyGroup>
"@
}

$propsContent = @'
<Project>

  <!-- Written by deploy-core.ps1. Pins $(go2csPath) to this deploy root so every project
       below it (the converted standard library, golib, the go2cs-gen analyzer, and any
       recursively converted end-user app placed under this root) resolves its
       $(go2csPath)core\... and $(go2csPath)gen\... ProjectReferences without needing a
       -p:go2csPath flag on the build. MSBuildThisFileDirectory ends with a separator. -->
  <PropertyGroup>
    <go2csPath>$(MSBuildThisFileDirectory)</go2csPath>
  </PropertyGroup>
'@ + $platformLines + @'

</Project>
'@
# [System.IO.File]::WriteAllText is not a cmdlet and knows nothing about -WhatIf, so the three
# writes below are gated explicitly. Losing that gate is how a "dry run" silently writes into a
# machine-global deploy root.
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$propsPath = Join-Path $Target 'Directory.Build.props'

if ($PSCmdlet.ShouldProcess($propsPath, 'write root Directory.Build.props')) {
    [System.IO.File]::WriteAllText($propsPath, $propsContent, $utf8NoBom)
}

# ---- Generate a solution over everything deployed ----------------------------------------
# Skip any `<pkg>.tests.csproj` (excluded from staging above; also filtered here so a stale one
# left in the deploy root from a prior deploy can't slip into the verify solution).
#
# Enumerated from the SOURCE under -WhatIf, because nothing has been copied to the target then and
# a target walk would report zero projects -- a dry run that says "deployed 0 project(s)" is worse
# than no dry run at all. The two roots are the same layout, so the projected relative paths are
# the real ones.
if ($WhatIfPreference) {
    $projectLines = @('core', 'gen') | ForEach-Object { Join-Path $srcRoot $_ } |
        Where-Object { Test-Path $_ } |
        ForEach-Object { Get-ChildItem -Path $_ -Recurse -Filter *.csproj } |
        Where-Object { $_.Name -notlike '*.tests.csproj' } | ForEach-Object {
        $rel = $_.FullName.Substring($srcRoot.Length).TrimStart('\', '/').Replace('\', '/')
        "  <Project Path=`"$rel`" />"
    }
}
else {
    $projectLines = Get-ChildItem -Path $Target -Recurse -Filter *.csproj | Where-Object { $_.Name -notlike '*.tests.csproj' } | ForEach-Object {
        $rel = $_.FullName.Substring($Target.Length).TrimStart('\', '/').Replace('\', '/')
        "  <Project Path=`"$rel`" />"
    }
}
$slnx = @("<Solution>",
    "  <Configurations>",
    "    <Platform Name=`"Any CPU`" />",
    "    <Platform Name=`"x64`" />",
    "    <Platform Name=`"x86`" />",
    "  </Configurations>") + $projectLines + @("</Solution>")
$slnxText = ($slnx -join "`r`n") + "`r`n"
$slnxPath = Join-Path $Target 'go2cs-core.slnx'

if ($PSCmdlet.ShouldProcess($slnxPath, "write solution over $($projectLines.Count) project(s)")) {
    [System.IO.File]::WriteAllText($slnxPath, $slnxText, $utf8NoBom)
}

$deployVerb = if ($WhatIfPreference) { 'would deploy' } else { 'deployed' }
Write-Host "  $deployVerb $($projectLines.Count) project(s); solution: $slnxPath" -ForegroundColor Green

# ---- Verify build ------------------------------------------------------------------------
if ($WhatIfPreference) {
    Write-Host "deploy-core: dry run complete (-WhatIf); nothing was written." -ForegroundColor Cyan
    exit 0
}

if ($NoBuild) {
    Write-Host "deploy-core: copy complete (-NoBuild); skipped verify build." -ForegroundColor Cyan
    exit 0
}

Write-Host "Building $slnxPath ($Configuration)..." -ForegroundColor Cyan
# csprojs default GeneratePackageOnBuild off; the explicit false is a guard so a future
# regression cannot emit hundreds of .nupkg files just to confirm the tree compiles.
& dotnet build $slnxPath -c $Configuration -p:GeneratePackageOnBuild=false -v m
if ($LASTEXITCODE -ne 0) {
    throw "Verify build FAILED ($LASTEXITCODE)."
}

Write-Host "deploy-core: deployment verified at $Target" -ForegroundColor Green
exit 0
