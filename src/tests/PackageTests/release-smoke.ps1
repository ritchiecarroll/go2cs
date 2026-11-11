#Requires -Version 7
<#
.SYNOPSIS
    Release smoke: consume a go.* package feed the way a user does, on THIS host, before the feed ships.

.DESCRIPTION
    Every arm restores the go.* packages from ONE local feed (a pack rehearsal's merged output, e.g.
    push-nuget.ps1 -VersionSuffix ci.N) into a FRESH package cache, so a green reading can only come from
    the packages under test. Nothing here packs, signs or pushes.

      A  RID COMPILE ASSET  src/tests/PackageTests/RidCompileAsset/test-rid-compile-asset.ps1 against the
                            feed: rid-less, -r <host rid>, publish, and the arm-can-fail control.
      B  SAMPLE             a small stdlib-only program (fmt, os, path/filepath, runtime, sort), generated
                            here, converted with go2cs -recurse=nuget, built against the feed, run, and its
                            stdout compared byte for byte with `go run`. It prints runtime.GOOS, so a
                            wrong-flavor load (lib/'s windows assembly on a Mac) is a visible mismatch.
      C  BEHAVIORAL         src/tests/Behavioral/StatLayoutTruth (os.Stat/Lstat/ReadDir/WalkDir over a real
                            tree), converted and compared the same way: the syscall struct layouts differ
                            per GOOS, so it fails if a wrong flavor loads.
      D  WALKTHROUGH        the README walkthrough (fatih/color v1.18.0 -> go-isatty -> golang.org/x/sys)
                            converted and compared the same way. It GATES only with -GateWalkthrough, which
                            the caller passes on the platforms the README says it works on; elsewhere it is
                            a MEASUREMENT that never moves the exit code, because this arm is what measures
                            a platform before the README may name it.
      G  PER-OS ASSETS      a consumer of runtime/coverage and runtime/debug, converted and compared the same
                            way, then the assembly NuGet hands this RID for go.internal.coverage.decodemeta and
                            go.runtime.debug (runtimes/<rid>/, else lib/): it must not name the other OS's syscall
                            buffer type, and a runtimes/<rid>/ twin must name this OS's (the 1.24.13.6 release
                            blocker). Naming neither is platform-neutral and passes.
      H  SOURCE COMPAT      the previous release's src/tests/CSharpConsumer, built UNCHANGED against the feed
                            (docs/PublicSurface.md: code written against the last release compiles against this
                            one). Its files come from -ConsumerRef, else from the newest nuget-<version> release
                            tag below the version under test. The modules it calls are converted with -Converter
                            at the versions that ref's run-csharp-consumer.ps1 pins, and the program must print
                            that runner's own "checks: <N> ok, 0 failed".

    Exit 0 when A, B, C, G and H pass (and D, with -GateWalkthrough); 1 otherwise. Every arm's evidence lands
    under -WorkRoot.

.PARAMETER Feed
    Folder holding the merged .nupkg files under test.

.PARAMETER Converter
    Path to a go2cs executable built from the same tree as the feed.

.PARAMETER WorkRoot
    Scratch root for the arms (created; never deleted by this script).

.PARAMETER Version
    The go.* package version to restore. Default: read off the feed's go.lib package.

.PARAMETER GateWalkthrough
    Make arm D gate. The release-smoke stage passes it on every leg; without it D is a measurement.

.PARAMETER ConsumerRef
    The git ref (a tag or a branch on origin) whose src/tests/CSharpConsumer arm H builds. Default: the newest
    nuget-<version> tag whose version is below -Version (for 1.26.10.4-ci.7, the newest tag below 1.26.10.4).
#>
param(
    [Parameter(Mandatory)] [string] $Feed,
    [Parameter(Mandatory)] [string] $Converter,
    [Parameter(Mandatory)] [string] $WorkRoot,
    [string] $Version,
    [switch] $GateWalkthrough,
    [string] $ConsumerRef
)

$ErrorActionPreference = 'Stop'

# A URL is a published feed (the post-publish smoke: -Feed https://api.nuget.org/v3/index.json); it carries
# every version ever shipped, so the version under test must be named.
if ($Feed -match '^https?://') { if (-not $Version) { throw "-Version is required when -Feed is a URL ($Feed)" } }
else { $Feed = (Resolve-Path $Feed).Path }
$Converter = (Resolve-Path $Converter).Path
New-Item -ItemType Directory -Force $WorkRoot | Out-Null
$WorkRoot = (Resolve-Path $WorkRoot).Path
$repoSrc = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$repoRoot = Split-Path $repoSrc -Parent

if (-not $Version) {
    $lib = @(Get-ChildItem $Feed -Filter 'go.lib.*.nupkg' | Where-Object { $_.Name -match '^go\.lib\.(\d.*)\.nupkg$' })
    if ($lib.Count -ne 1) { throw "Expected exactly one go.lib.<version>.nupkg in $Feed; found $($lib.Count)" }
    $null = $lib[0].Name -match '^go\.lib\.(\d.*)\.nupkg$'
    $Version = $Matches[1]
}

$rid = (dotnet --info | Select-String -Pattern '^\s*RID:\s*(\S+)' | Select-Object -First 1).Matches.Groups[1].Value
Write-Host "release smoke: go.* $Version from $Feed on $rid"

# The toolchain the converter was built with, not one a go/toolchain directive would fetch (README step 1).
$env:GOTOOLCHAIN = 'local'

function Write-FeedConfig([string]$Dir) {
    # go.* from the feed ONLY; everything else (the SDK's own packs) from nuget.org. The mapping is what
    # makes the feed authoritative for go.*: no other source is even consulted for those IDs.
    $nugetOrg = 'https://api.nuget.org/v3/index.json'
    if ($Feed.TrimEnd('/') -eq $nugetOrg) {
        # The post-publish smoke: the feed IS nuget.org. Listing that URL under two keys makes NuGet drop the
        # second, and the SDK's own packs (Microsoft.NET.ILLink.Tasks) then have no source: NU1100. One source.
        $sources = "    <add key=`"nuget.org`" value=`"$nugetOrg`" />"
        $mapping = '    <packageSource key="nuget.org"><package pattern="*" /></packageSource>'
    }
    else {
        $sources = "    <add key=`"feed`" value=`"$Feed`" />`n    <add key=`"nuget.org`" value=`"$nugetOrg`" />"
        $mapping = "    <packageSource key=`"feed`"><package pattern=`"go.*`" /></packageSource>`n    <packageSource key=`"nuget.org`"><package pattern=`"*`" /></packageSource>"
    }
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
$sources
  </packageSources>
  <packageSourceMapping>
$mapping
  </packageSourceMapping>
</configuration>
"@ | Set-Content -LiteralPath (Join-Path $Dir 'nuget.config') -Encoding utf8
}

# The first lines of a failing step's stderr, echoed to the console and folded into the verdict. A leg's
# artifacts are served from blob storage that a restricted-egress reader cannot reach; the console log and
# the annotation summary come back from the API itself, so the evidence has to be there too.
function Get-StderrHead([string]$Path, [int]$Lines = 12) {
    $head = @(Get-Content -LiteralPath $Path -ErrorAction SilentlyContinue | Where-Object { $_.Trim() } | Select-Object -First $Lines)
    $head | ForEach-Object { Write-Host "    stderr| $_" }
    if ($head.Count) { return " -- first stderr line: $($head[0].Trim())" }
    return ' -- stderr was empty'
}

function Get-FirstError([string]$Log) {
    $first = Get-Content -LiteralPath $Log -ErrorAction SilentlyContinue | Where-Object { $_ -match '(?i)\berror\b' } | Select-Object -First 1
    if ($first) { Write-Host "    error| $first"; return " -- first error: $($first.Trim())" }
    return ''
}

function Invoke-Logged([string]$Log, [scriptblock]$Command) {
    $ErrorActionPreference = 'Continue'
    & $Command *>&1 | ForEach-Object { "$_" } | Set-Content -LiteralPath $Log -Encoding utf8
    return $LASTEXITCODE
}

# Convert a Go module with -recurse=nuget, build it against the feed, run it, and compare its stdout with
# `go run`. Returns a verdict line.
function Invoke-ConvertArm([string]$Name, [string]$ModuleDir) {
    $arm = Join-Path $WorkRoot $Name
    $cache = Join-Path $arm 'nuget-cache'
    New-Item -ItemType Directory -Force $cache | Out-Null

    $module = ((Get-Content -LiteralPath (Join-Path $ModuleDir 'go.mod') | Where-Object { $_ -match '^module\s' } | Select-Object -First 1) -replace '^module\s+', '').Trim()
    if (-not $module) { return "FAIL ($Name): no module line in $ModuleDir/go.mod" }

    Push-Location $ModuleDir
    try {
        $goOut = Join-Path $arm 'go.stdout.txt'
        $env:NUGET_PACKAGES = $null
        $ErrorActionPreference = 'Continue'
        # `-tags safe,purego,noasm`: every arm converts with -recurse, which builds with those tags by default, so the baseline must too.
        & go run -tags safe,purego,noasm . 1> $goOut 2> (Join-Path $arm 'go.stderr.txt')
        $ErrorActionPreference = 'Stop'
        if ($LASTEXITCODE -ne 0) { return "FAIL ($Name): go run exited $LASTEXITCODE (the Go baseline itself)" }

        $code = Invoke-Logged (Join-Path $arm 'convert.log') { & $Converter -recurse=nuget . csharp }
        if ($code -ne 0) { return "FAIL ($Name): go2cs -recurse=nuget exited $code$(Get-FirstError (Join-Path $arm 'convert.log'))" }
    }
    finally { Pop-Location }

    $root = Join-Path $ModuleDir 'csharp'
    Write-FeedConfig $root
    $appDir = Join-Path $root ('src/' + $module)
    $slnx = @(Get-ChildItem -LiteralPath $appDir -Filter *.slnx -File -ErrorAction SilentlyContinue)
    $proj = @(Get-ChildItem -LiteralPath $appDir -Filter *.csproj -File -ErrorAction SilentlyContinue)
    if ($slnx.Count -ne 1 -or $proj.Count -ne 1) { return "FAIL ($Name): expected one .slnx and one .csproj under $appDir; found $($slnx.Count) and $($proj.Count)" }

    # The compile RID the conversion pinned for its target (moduleConverter.go): the flavor go.lib.targets
    # compiles against. A GOOS no go.* flavor ships for pins nothing, which this smoke treats as a failure.
    $props = [System.IO.File]::ReadAllText((Join-Path $root 'Directory.Build.props'))
    if ($props -notmatch '<GoCompileRuntimeIdentifier>([^<]+)</GoCompileRuntimeIdentifier>') { return "FAIL ($Name): the conversion pinned no GoCompileRuntimeIdentifier (no go.* flavor ships for its target)" }
    $compileRid = $Matches[1]

    $env:NUGET_PACKAGES = $cache
    try {
        $code = Invoke-Logged (Join-Path $arm 'build.log') { & dotnet build $slnx[0].FullName -c Debug "-p:GoStdLibVersion=$Version" --nologo }
        if ($code -ne 0) { return "FAIL ($Name): dotnet build exited $code$(Get-FirstError (Join-Path $arm 'build.log'))" }

        # Provenance: the restored go.os is THIS version, which only the feed carries, and it holds the twin
        # the conversion compiles against.
        $osPkg = Join-Path $cache "go.os/$($Version.ToLowerInvariant())"
        if (-not (Test-Path (Join-Path $osPkg "runtimes/$compileRid/lib"))) { return "FAIL ($Name): restored go.os $Version carries no runtimes/$compileRid/ twin ($osPkg)" }

        $csOut = Join-Path $arm 'cs.stdout.txt'
        $ErrorActionPreference = 'Continue'
        & dotnet run --project $proj[0].FullName -c Debug --no-build "-p:GoStdLibVersion=$Version" 1> $csOut 2> (Join-Path $arm 'cs.stderr.txt')
        $runCode = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        if ($runCode -ne 0) {
            # .NET prints "Stack overflow." before it aborts; say so in the verdict, so the crash never reads as a hang or a bare exit code.
            $overflow = if (Select-String -LiteralPath (Join-Path $arm 'cs.stderr.txt') -Pattern '^\s*Stack overflow' -Quiet -ErrorAction SilentlyContinue) { ' (a STACK OVERFLOW)' } else { '' }
            return "FAIL ($Name): the converted program exited $runCode$overflow$(Get-StderrHead (Join-Path $arm 'cs.stderr.txt'))"
        }
    }
    finally { $env:NUGET_PACKAGES = $null }

    $want = ([System.IO.File]::ReadAllText($goOut)) -replace "`r`n", "`n"
    $got = ([System.IO.File]::ReadAllText($csOut)) -replace "`r`n", "`n"
    if ($want -ne $got) {
        $wantLines = @($want -split "`n"); $gotLines = @($got -split "`n")
        $i = 0; while ($i -lt [Math]::Min($wantLines.Count, $gotLines.Count) -and $wantLines[$i] -eq $gotLines[$i]) { $i++ }
        $w = if ($i -lt $wantLines.Count) { $wantLines[$i] } else { '<end>' }
        $g = if ($i -lt $gotLines.Count) { $gotLines[$i] } else { '<end>' }
        return "FAIL ($Name): stdout differs from go run at line $($i + 1): go '$w' vs C# '$g'$(Get-StderrHead (Join-Path $arm 'cs.stderr.txt'))"
    }
    if (-not $want.Trim()) { return "FAIL ($Name): go run printed nothing, so the comparison proves nothing" }

    return "PASS ($Name): stdout identical to go run ($(@($want.TrimEnd("`n") -split "`n").Count) line(s)); go.* $Version from the feed, compiled against runtimes/$compileRid/"
}

$verdicts = @()

# ---- A: the RID-selected compile asset -----------------------------------------------------------
$code = Invoke-Logged (Join-Path $WorkRoot 'A-rid-compile-asset.log') {
    & (Join-Path $PSScriptRoot 'RidCompileAsset/test-rid-compile-asset.ps1') -Version $Version -Source $Feed
}
$verdicts += if ($code -eq 0) { 'PASS (A rid-compile-asset)' } else { "FAIL (A rid-compile-asset): exit $code (see A-rid-compile-asset.log)" }

# ---- B: a generated stdlib sample ----------------------------------------------------------------
$sample = Join-Path $WorkRoot 'B-sample-src'
New-Item -ItemType Directory -Force $sample | Out-Null
"module example.com/relsmoke`n`ngo 1.24`n" | Set-Content -LiteralPath (Join-Path $sample 'go.mod') -Encoding utf8 -NoNewline
@'
package main

import (
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"sort"
)

func main() {
	fmt.Println("GOOS =", runtime.GOOS)

	dir, err := os.MkdirTemp("", "relsmoke")
	if err != nil {
		fmt.Println("MkdirTemp error:", err)
		return
	}
	defer os.RemoveAll(dir)

	for _, name := range []string{"b.txt", "a.txt"} {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(name), 0o644); err != nil {
			fmt.Println("WriteFile error:", err)
			return
		}
	}

	entries, err := os.ReadDir(dir)
	if err != nil {
		fmt.Println("ReadDir error:", err)
		return
	}
	var names []string
	for _, e := range entries {
		names = append(names, e.Name())
	}
	sort.Strings(names)
	fmt.Println("ReadDir =", names)

	data, err := os.ReadFile(filepath.Join(dir, "a.txt"))
	fmt.Println("ReadFile =", string(data), err)

	// The sort forms whose converted bodies once called themselves (each xSlice.Sort() is `{ Sort(x) }`
	// in Go, which C# bound back to the method itself): every method form, and the bare call form.
	ints := sort.IntSlice{3, 1, 2}
	ints.Sort()
	floats := sort.Float64Slice{2.5, -1, 0.5}
	floats.Sort()
	strs := sort.StringSlice{"c", "a", "b"}
	strs.Sort()
	bare := []string{"z", "x", "y"}
	sort.Sort(sort.StringSlice(bare))
	fmt.Println("Sort =", ints, floats, strs, bare)

	// Which file of each tag's pair compiled: the converter's default tag set and the Go baseline must agree.
	fmt.Println("BuildTag =", buildModeSafe, buildModePurego, buildModeNoasm)
}
'@ | Set-Content -LiteralPath (Join-Path $sample 'main.go') -Encoding utf8
# The build-tag probe: for each default tag, exactly one of its two files compiles. A -recurse conversion builds with
# the tags safe,purego,noasm by default, so the baseline `go run` must too; if the two ever disagree, line 5 of arm B
# names the tag.
foreach ($tag in 'safe', 'purego', 'noasm') {
    $const = 'buildMode' + $tag.Substring(0, 1).ToUpperInvariant() + $tag.Substring(1)
    "//go:build $tag`n`npackage main`n`nconst $const = `"$tag`"`n" | Set-Content -LiteralPath (Join-Path $sample "mode_$tag.go") -Encoding utf8 -NoNewline
    "//go:build !$tag`n`npackage main`n`nconst $const = `"no-$tag`"`n" | Set-Content -LiteralPath (Join-Path $sample "mode_no$tag.go") -Encoding utf8 -NoNewline
}
$verdicts += Invoke-ConvertArm 'B-sample' $sample

# ---- C: one behavioral project -------------------------------------------------------------------
$behavioral = Join-Path $WorkRoot 'C-behavioral-src'
New-Item -ItemType Directory -Force $behavioral | Out-Null
Copy-Item (Join-Path $repoSrc 'tests/Behavioral/StatLayoutTruth/go.mod'), (Join-Path $repoSrc 'tests/Behavioral/StatLayoutTruth/main.go') $behavioral
$verdicts += Invoke-ConvertArm 'C-StatLayoutTruth' $behavioral

# ---- G: the per-OS assets of go.internal.coverage.decodemeta and go.runtime.debug ------------------
# An assembly whose metadata names a per-OS type ships one twin per RID. The 1.24.13.6 rehearsal shipped
# go.internal.coverage.decodemeta with its Windows build in lib/ only: its escape registry names
# go.syscall_package.WSABuf, which no unix go.syscall defines, so a consumer died at startup on Linux and macOS
# (TypeLoadException: Could not load type 'WSABuf' from assembly 'syscall', from the module initializer).
# runtime/coverage's cfile imports decodemeta, so this consumer loads it at init; the run is the behavior, and the
# restored assembly of each package for this leg's compile RID is the cause, read by name.
# The asset NuGet hands this RID is runtimes/<rid>/lib/ when the package carries one, else lib/. A runtimes/<rid>/
# twin exists because the assembly names a per-OS type, so it must name THIS OS's buffer type. A package with no
# twin for this RID loads lib/, which must not name the OTHER OS's type: that is the 1.24.13.6 blocker exactly
# (decodemeta's Windows build in lib/ only, loaded on Linux and macOS). Naming neither type is a platform-neutral
# assembly: 1.24.13.7 ships both packages that way, their syscall dependency gone, and that is a pass, not a defect.
function Get-GAssetVerdict([string]$PackageRoot, [string]$AssetRid, [string]$Dll, [string]$Want, [string]$Refuse) {
    $where = "runtimes/$AssetRid/"
    $asset = @(Get-ChildItem -LiteralPath (Join-Path $PackageRoot "runtimes/$AssetRid/lib") -Recurse -Filter $Dll -File -ErrorAction SilentlyContinue)
    $twin = $asset.Count -gt 0
    if (-not $twin) {
        $where = 'lib/'
        $asset = @(Get-ChildItem -LiteralPath (Join-Path $PackageRoot 'lib') -Recurse -Filter $Dll -File -ErrorAction SilentlyContinue)
    }
    if ($asset.Count -ne 1) { return @{ Fail = $true; Text = "carries $($asset.Count) $where assembly for $AssetRid, want 1 ($PackageRoot)" } }
    $text = [System.Text.Encoding]::Latin1.GetString([System.IO.File]::ReadAllBytes($asset[0].FullName))
    if ($text.Contains($Refuse)) { return @{ Fail = $true; Text = "$where assembly loaded on $AssetRid names $Refuse (this OS's syscall buffer type is $Want)" } }
    if ($twin -and -not $text.Contains($Want)) { return @{ Fail = $true; Text = "$where assembly does not name $Want (a per-OS twin must name this OS's syscall buffer type)" } }
    if ($text.Contains($Want)) { return @{ Fail = $false; Text = "$where names $Want, not $Refuse" } }
    return @{ Fail = $false; Text = "$where names neither $Want nor $Refuse (platform-neutral)" }
}

$cov = Join-Path $WorkRoot 'G-coverage-debug-src'
New-Item -ItemType Directory -Force $cov | Out-Null
"module example.com/relsmokecov`n`ngo 1.24`n" | Set-Content -LiteralPath (Join-Path $cov 'go.mod') -Encoding utf8 -NoNewline
@'
package main

import (
	"bytes"
	"fmt"
	"runtime/coverage"
	"runtime/debug"
)

func main() {
	// runtime/coverage's cfile imports internal/coverage/decodemeta, so this program loads that package at init.
	var meta bytes.Buffer
	fmt.Println("WriteMeta fails without -cover:", coverage.WriteMeta(&meta) != nil)
	fmt.Println("ClearCounters fails without -cover:", coverage.ClearCounters() != nil)

	old := debug.SetGCPercent(150)
	fmt.Println("SetGCPercent round trip:", debug.SetGCPercent(old))
}
'@ | Set-Content -LiteralPath (Join-Path $cov 'main.go') -Encoding utf8
$gVerdict = Invoke-ConvertArm 'G-coverage-debug' $cov
if ($gVerdict -like 'PASS*') {
    $props = [System.IO.File]::ReadAllText((Join-Path $cov 'csharp/Directory.Build.props'))
    $null = $props -match '<GoCompileRuntimeIdentifier>([^<]+)</GoCompileRuntimeIdentifier>'
    $assetRid = $Matches[1]
    $want, $refuse = if ($assetRid -like 'win-*') { 'WSABuf', 'Iovec' } else { 'Iovec', 'WSABuf' }
    $cache = Join-Path $WorkRoot 'G-coverage-debug/nuget-cache'
    $notes = @()
    foreach ($pkg in @(@{ Id = 'go.internal.coverage.decodemeta'; Dll = 'internal.coverage.decodemeta.dll' }, @{ Id = 'go.runtime.debug'; Dll = 'runtime.debug.dll' })) {
        $result = Get-GAssetVerdict -PackageRoot (Join-Path $cache "$($pkg.Id)/$($Version.ToLowerInvariant())") -AssetRid $assetRid -Dll $pkg.Dll -Want $want -Refuse $refuse
        if ($result.Fail) { $gVerdict = "FAIL (G-coverage-debug): $($pkg.Id) $Version $($result.Text)"; break }
        $notes += "$($pkg.Id) $($result.Text)"
    }
    if ($gVerdict -like 'PASS*') { $gVerdict += "; " + ($notes -join '; ') }
}
$verdicts += $gVerdict

# ---- H: source compatibility: the previous release's consumer, unchanged -----------------------------
# The newest nuget-<a.b.c.d> release tag below the version under test. A prerelease (1.26.10.4-ci.7) is below its own
# release, so it compares by its numeric part; a published version's own tag is not below it and is skipped.
function Resolve-ConsumerRef([string]$UnderTest) {
    $base = [version](($UnderTest -split '-', 2)[0])
    $ErrorActionPreference = 'Continue'
    $lines = @(& git -C $repoRoot ls-remote --tags origin 'refs/tags/nuget-*' 2>$null)
    $code = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    if ($code -ne 0) { return $null }
    $best = $null
    $bestVersion = $null
    foreach ($line in $lines) {
        if ($line -notmatch 'refs/tags/nuget-(\d+\.\d+\.\d+\.\d+)$') { continue }
        $tagVersion = [version]$Matches[1]
        if ($tagVersion -lt $base -and (-not $bestVersion -or $tagVersion -gt $bestVersion)) {
            $best = "nuget-$($Matches[1])"
            $bestVersion = $tagVersion
        }
    }
    return $best
}

function Invoke-SourceCompatArm {
    $name = 'H-source-compat'
    $arm = Join-Path $WorkRoot $name
    $cache = Join-Path $arm 'nuget-cache'
    New-Item -ItemType Directory -Force $cache | Out-Null

    $ref = $ConsumerRef
    $how = 'the -ConsumerRef override'
    if (-not $ref) {
        $ref = Resolve-ConsumerRef $Version
        $how = "the newest nuget-* release tag below $Version"
    }
    if (-not $ref) { return "FAIL ($name): found no nuget-<version> release tag below $Version on origin" }

    # Exactly that ref, shallow, so a shallow checkout has its tree too.
    $code = Invoke-Logged (Join-Path $arm 'fetch.log') { & git -C $repoRoot fetch --depth 1 --no-tags origin $ref }
    if ($code -ne 0) { return "FAIL ($name): git fetch origin $ref exited $code$(Get-FirstError (Join-Path $arm 'fetch.log'))" }
    $commit = (& git -C $repoRoot rev-parse FETCH_HEAD).Trim()
    Write-Host "release smoke: H builds the CSharpConsumer of $ref ($commit), chosen as $how"

    $tar = Join-Path $arm 'consumer.tar'
    $code = Invoke-Logged (Join-Path $arm 'archive.log') {
        & git -C $repoRoot archive --format=tar -o $tar $commit src/tests/CSharpConsumer
        if ($LASTEXITCODE -eq 0) { & tar -xf $tar -C $arm }
    }
    if ($code -ne 0) { return "FAIL ($name): $ref has no src/tests/CSharpConsumer to extract (exit $code)" }
    $consumer = Join-Path $arm 'src/tests/CSharpConsumer'

    # The ref's own runner says which modules its program calls, at which versions, and how many checks it runs.
    $runner = [System.IO.File]::ReadAllText((Join-Path $consumer 'run-csharp-consumer.ps1'))
    if ($runner -notmatch '\[int\]\s*\$ExpectedChecks\s*=\s*(\d+)') { return "FAIL ($name): $ref's run-csharp-consumer.ps1 names no ExpectedChecks default" }
    $expected = "checks: $($Matches[1]) ok, 0 failed"
    $modules = @([regex]::Matches($runner, "(?m)^\s*(\w+)\s*=\s*'([^'@\s]+@v[^'\s]+)'") | ForEach-Object { @{ Name = $_.Groups[1].Value; Module = $_.Groups[2].Value } })
    if ($modules.Count -eq 0) { return "FAIL ($name): $ref's run-csharp-consumer.ps1 pins no modules" }

    $converted = Join-Path $arm 'converted'
    foreach ($module in $modules) {
        $ErrorActionPreference = 'Continue'
        $download = (& go mod download -json $module.Module 2> (Join-Path $arm "download-$($module.Name).log")) -join "`n"
        $code = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        if ($code -ne 0) { return "FAIL ($name): go mod download $($module.Module) exited $code" }
        $source = ($download | ConvertFrom-Json).Dir

        $code = Invoke-Logged (Join-Path $arm "convert-$($module.Name).log") { & $Converter -recurse=nuget $source (Join-Path $converted $module.Name) }
        if ($code -ne 0) { return "FAIL ($name): go2cs -recurse=nuget $($module.Module) exited $code$(Get-FirstError (Join-Path $arm "convert-$($module.Name).log"))" }
    }

    # One nuget.config above the consumer and the converted modules both.
    Write-FeedConfig $arm
    $project = Join-Path $consumer 'CSharpConsumer.csproj'
    $env:NUGET_PACKAGES = $cache
    try {
        $code = Invoke-Logged (Join-Path $arm 'build.log') { & dotnet build $project -c Release "-p:ConsumerConvertedRoot=$converted" "-p:GoStdLibVersion=$Version" --nologo }
        if ($code -ne 0) { return "FAIL ($name): $ref's CSharpConsumer does not build unchanged against go.* $Version$(Get-FirstError (Join-Path $arm 'build.log'))" }

        $stdout = Join-Path $arm 'cs.stdout.txt'
        $ErrorActionPreference = 'Continue'
        & dotnet run --project $project -c Release --no-build "-p:ConsumerConvertedRoot=$converted" "-p:GoStdLibVersion=$Version" 1> $stdout 2> (Join-Path $arm 'cs.stderr.txt')
        $runCode = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
    }
    finally { $env:NUGET_PACKAGES = $null }

    $lines = @(Get-Content -LiteralPath $stdout -ErrorAction SilentlyContinue)
    $failedChecks = @($lines | Where-Object { $_ -match '\bFAIL\b' } | Select-Object -First 3)
    if ($runCode -ne 0) { return "FAIL ($name): $ref's CSharpConsumer exited $runCode$(if ($failedChecks) { ' -- ' + ($failedChecks -join ' | ') })$(Get-StderrHead (Join-Path $arm 'cs.stderr.txt'))" }
    if (@($lines | Where-Object { $_ -ceq $expected }).Count -ne 1) { return "FAIL ($name): $ref's CSharpConsumer did not print '$expected'$(if ($failedChecks) { ' -- ' + ($failedChecks -join ' | ') })" }

    return "PASS ($name): the CSharpConsumer of $ref ($($commit.Substring(0, 10))) built unchanged against go.* $Version and printed '$expected'"
}
$verdicts += Invoke-SourceCompatArm

# ---- D: the README walkthrough, measured, never gating -------------------------------------------
$walk = Join-Path $WorkRoot 'D-walkthrough-src'
New-Item -ItemType Directory -Force $walk | Out-Null
@'
package main

import "github.com/fatih/color"

func main() {
	color.New(color.FgGreen, color.Bold).Println("hello from fatih/color")
}
'@ | Set-Content -LiteralPath (Join-Path $walk 'main.go') -Encoding utf8
Push-Location $walk
try {
    $setup = Invoke-Logged (Join-Path $WorkRoot 'D-walkthrough-setup.log') {
        go mod init example.com/colordemo
        if ($LASTEXITCODE -eq 0) { go get github.com/fatih/color@v1.18.0 }
        if ($LASTEXITCODE -eq 0) { go mod tidy }
        if ($LASTEXITCODE -eq 0) { go build ./... }
    }
}
finally { Pop-Location }
$walkVerdict = if ($setup -ne 0) { "FAIL (D-walkthrough): the Go side did not set up (exit $setup)" } else { Invoke-ConvertArm 'D-walkthrough' $walk }

# ---- E: symbol files beside a single-file host, measured, never gating ------------------------------
# go.lib.symbols.targets keeps every referenced assembly's .pdb beside a single-file host across a SECOND unchanged
# publish (the SDK deletes them when the bundle step is skipped). PublishSymbols' guard reads that against this feed's
# go.lib, and its AOT control reads that the target leaves a Native AOT publish untouched. NOT GATING until it has read
# green on two trains (COORD, 2026-10-06): a go.lib without the target, every release up to 1.24.13.4, fails the guard.
#
# ---- F: the packages' OWN symbol files reach a consumer, measured, never gating ---------------------------------
# PackageSymbols restores go.sort from this feed and prints a std frame's Go file:line on `dotnet run`, on a first and
# a second unchanged single-file publish and on a framework-dependent publish; its off switch must print "none". Its
# Native AOT run is NOT here: under go.lib's TrimMode=partial that publish compiles the whole go.* closure (about 70 min
# on hosted linux), so it is os-matrix's own aot-smoke stage (COORD ruling 2026-10-06). The packages ship .pdb from the release that ships symbols
# (owner ruling, 2026-10-06); every earlier release fails RUN, PUBLISH and FDD. Its frame lines are carried in the detail.
$symbolVerdicts = @()
foreach ($check in @(
        @{ Name = 'E-publish-symbols'; Dir = 'PublishSymbols'; Script = 'test-publish-symbols.ps1'; Extra = @{} },
        @{ Name = 'E-publish-symbols-aot'; Dir = 'PublishSymbols'; Script = 'test-publish-symbols-aot.ps1'; Extra = @{} },
        @{ Name = 'F-package-symbols'; Dir = 'PackageSymbols'; Script = 'test-package-symbols.ps1'; Extra = @{}
           Detail = '^\s+(RUN|PUBLISH [12]|FDD|OFF|AOT): ' })) {
    $checkLog = Join-Path $WorkRoot "$($check.Name).log"
    $extra = $check.Extra
    $code = Invoke-Logged $checkLog { & (Join-Path (Join-Path $PSScriptRoot $check.Dir) $check.Script) -Version $Version -Source $Feed @extra }
    $pattern = if ($check.Detail) { "^(PASS|FAIL) |$($check.Detail)" } else { '^(PASS|FAIL) ' }
    $detail = @(Get-Content -LiteralPath $checkLog -ErrorAction SilentlyContinue | Where-Object { $_ -match $pattern } | ForEach-Object { $_.Trim() }) -join ' | '
    $symbolVerdicts += if ($code -eq 0) { "PASS ($($check.Name)): $detail" } else { "FAIL ($($check.Name)): exit $code -- $detail (see $($check.Name).log)" }
}

# ---- verdicts ------------------------------------------------------------------------------------
if ($GateWalkthrough) { $verdicts += $walkVerdict }
$verdicts | ForEach-Object { Write-Host $_ }
if (-not $GateWalkthrough) { Write-Host "MEASURED, NOT GATING: $walkVerdict" }
$symbolVerdicts | ForEach-Object { Write-Host "MEASURED, NOT GATING: $_" }

if ($verdicts | Where-Object { $_ -like 'FAIL*' }) { exit 1 }
exit 0
