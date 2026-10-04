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

    Exit 0 when A, B and C pass (and D, with -GateWalkthrough); 1 otherwise. Every arm's evidence lands
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
#>
param(
    [Parameter(Mandatory)] [string] $Feed,
    [Parameter(Mandatory)] [string] $Converter,
    [Parameter(Mandatory)] [string] $WorkRoot,
    [string] $Version,
    [switch] $GateWalkthrough
)

$ErrorActionPreference = 'Stop'

$Feed = (Resolve-Path $Feed).Path
$Converter = (Resolve-Path $Converter).Path
New-Item -ItemType Directory -Force $WorkRoot | Out-Null
$WorkRoot = (Resolve-Path $WorkRoot).Path
$repoSrc = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

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
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="feed" value="$Feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="feed"><package pattern="go.*" /></packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
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
        & go run . 1> $goOut 2> (Join-Path $arm 'go.stderr.txt')
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
}
'@ | Set-Content -LiteralPath (Join-Path $sample 'main.go') -Encoding utf8
$verdicts += Invoke-ConvertArm 'B-sample' $sample

# ---- C: one behavioral project -------------------------------------------------------------------
$behavioral = Join-Path $WorkRoot 'C-behavioral-src'
New-Item -ItemType Directory -Force $behavioral | Out-Null
Copy-Item (Join-Path $repoSrc 'tests/Behavioral/StatLayoutTruth/go.mod'), (Join-Path $repoSrc 'tests/Behavioral/StatLayoutTruth/main.go') $behavioral
$verdicts += Invoke-ConvertArm 'C-StatLayoutTruth' $behavioral

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

# ---- verdicts ------------------------------------------------------------------------------------
if ($GateWalkthrough) { $verdicts += $walkVerdict }
$verdicts | ForEach-Object { Write-Host $_ }
if (-not $GateWalkthrough) { Write-Host "MEASURED, NOT GATING: $walkVerdict" }

if ($verdicts | Where-Object { $_ -like 'FAIL*' }) { exit 1 }
exit 0
