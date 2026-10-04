<#
.SYNOPSIS
    Convert two real Go modules and prove the C# that calls them still compiles and behaves as
    docs/ConsumingGoFromCSharp.md says.

.DESCRIPTION
    The page documents the C# surface of converted Go: @string, slice<T>, tuples and error values, the
    pointer box, channel<T>, the goroutine launcher, PanicException. Those are emission and golib shapes,
    and they move. This runner keeps the page honest:

      1. builds the converter from this tree (skip with -SkipBuild);
      2. downloads github.com/ritchiecarroll/hashset and github.com/google/uuid at their pinned versions
         (the Go module proxy, checked against the checksum database);
      3. converts each with `go2cs -recurse` into its own folder under -WorkRoot, which must be outside
         the repository;
      4. builds CSharpConsumer against the converted projects and runs it;
      5. passes only when the program exits 0 AND prints exactly "checks: <ExpectedChecks> ok, 0 failed".

    Pinning the count means a check that stops running fails the run as surely as one that fails.
    Adding a check to Program.cs means raising -ExpectedChecks's default in the same change.

.PARAMETER WorkRoot
    Where the conversions go. Created if missing; refused if it is inside the repository or not empty.

.PARAMETER ExpectedChecks
    The number of checks Program.cs runs.

.PARAMETER SkipBuild
    Use the converter already at src/go2cs/bin instead of building it.

.EXAMPLE
    ./run-csharp-consumer.ps1 -WorkRoot $env:TEMP/go2cs-consumer
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $WorkRoot,
    [int] $ExpectedChecks = 21,
    [switch] $SkipBuild
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../../_paths.ps1')

$modules = [ordered]@{
    hashset = 'github.com/ritchiecarroll/hashset@v1.0.0'
    uuid    = 'github.com/google/uuid@v1.6.0'
}

function Fail([string] $message) {
    Write-Host "FAIL: $message" -ForegroundColor Red
    exit 1
}

# One release, no silent toolchain switch, and GOROOT exactly as the toolchain spells it. An inherited
# GOROOT that names a different install is cleared first, so `go env` reports the root of the go on PATH
# rather than echoing the variable back (a mismatch fails every compile with "version ... does not match").
$env:GOTOOLCHAIN = 'local'
$env:GOFLAGS = ''
Remove-Item Env:GOROOT -ErrorAction SilentlyContinue
$goroot = (& go env GOROOT)
if ($LASTEXITCODE -ne 0 -or -not $goroot) { Fail 'go env GOROOT failed: is Go on PATH?' }
$env:GOROOT = $goroot

$WorkRoot = [IO.Path]::GetFullPath($WorkRoot)
$repo = [IO.Path]::GetFullPath($RepoRoot).TrimEnd('\', '/')
if ($WorkRoot.StartsWith($repo + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or $WorkRoot -ieq $repo) {
    Fail "-WorkRoot must be outside the repository: $WorkRoot"
}
if ((Test-Path $WorkRoot) -and @(Get-ChildItem -Force $WorkRoot).Count -gt 0) {
    Fail "-WorkRoot is not empty; give a fresh folder so no earlier conversion is reused: $WorkRoot"
}
New-Item -ItemType Directory -Force $WorkRoot | Out-Null

if (-not $SkipBuild) {
    Write-Host '==> building the converter'
    Push-Location $ConverterSrc
    try {
        & go build -o $Go2csExe .
        if ($LASTEXITCODE -ne 0) { Fail "go build exited $LASTEXITCODE" }
    }
    finally { Pop-Location }
}
if (-not (Test-Path $Go2csExe)) { Fail "no converter at $Go2csExe" }

$modCache = (& go env GOMODCACHE)
foreach ($name in $modules.Keys) {
    $module = $modules[$name]
    Write-Host "==> $name ($module)"

    & go mod download $module
    if ($LASTEXITCODE -ne 0) { Fail "go mod download $module exited $LASTEXITCODE" }

    $path, $version = $module -split '@'
    $source = Join-Path $modCache "$path@$version"
    $output = Join-Path $WorkRoot $name

    & $Go2csExe -recurse -go2cspath $SrcRoot $source $output
    if ($LASTEXITCODE -ne 0) { Fail "go2cs -recurse $module exited $LASTEXITCODE" }
}

Write-Host '==> building CSharpConsumer'
# No -o and no intermediate-path override: a global property flows into every referenced converted
# project and would merge their outputs. The converted projects keep their own artifact roots.
$project = Join-Path $PSScriptRoot 'CSharpConsumer.csproj'
& dotnet build $project -c Release "-p:ConsumerConvertedRoot=$WorkRoot" "-p:go2csPath=$SrcRoot/"
if ($LASTEXITCODE -ne 0) { Fail "dotnet build exited $LASTEXITCODE" }

Write-Host '==> running CSharpConsumer'
$output = & dotnet run --project $project -c Release --no-build
$exit = $LASTEXITCODE
$output | ForEach-Object { Write-Host $_ }

$expected = "checks: $ExpectedChecks ok, 0 failed"
if ($exit -ne 0) { Fail "CSharpConsumer exited $exit" }
if (@($output | Where-Object { $_ -ceq $expected }).Count -ne 1) { Fail "expected the line '$expected'" }

Write-Host "PASS: $expected" -ForegroundColor Green
