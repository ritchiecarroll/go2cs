#Requires -Version 7
<#
.SYNOPSIS
    Gate for each converted package's own usings (build/<PackageId>.props, written at pack time by
    src/core/golib/buildTransitive/go.lib.pack.targets).

.DESCRIPTION
    Packs src/core/encoding/hex from this tree (--no-build: build the stdlib first), reads the
    build/go.encoding.hex.props out of the nupkg, then restores the PackageUsings fixture from ONE feed
    into a FRESH package cache and builds it against that props file:

      PACK            the nupkg carries build/go.encoding.hex.props, and it names the package's namespace
                      (go.encoding) and, opt-in, its alias (hex = go.encoding.hex_package).
      NAMESPACE       Program.cs, which declares no `using go.encoding;`, builds and runs: a Go method
                      of encoding/hex resolves through the props' namespace using.
      CONTROL         the arm can fail: without the props the same build fails with CS1929 (the Go
                      method is not in scope), as every package before the props did.
      ALIASES         with GoGlobalPackageAliases=true and no alias in the file, the `hex` alias comes
                      from the props and the fixture builds and runs.
      ALIAS CONTROL   the aliases are OFF by default: the same file without the property fails with
                      CS0246 (`hex` is not defined).
      ALIAS CLASH     why they are off by default: turned on for a file that declares the alias itself
                      (the documented form), the build fails with CS1537.

    Exit 0 only when every arm reads as expected. Nothing outside a temporary work directory is written.

.PARAMETER Version
    The published go.encoding.hex version the fixture restores (its GoPackageVersion). The packed props
    is imported explicitly, so a package published before the props existed is the right subject.

.PARAMETER Source
    The feed for the restore: a local folder of .nupkg files or a URL. Default nuget.org.

.PARAMETER Configuration
    The configuration of this tree's stdlib build to pack from. Default Debug.

.EXAMPLE
    pwsh ./test-package-usings.ps1 -Version 1.24.13.7
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $Configuration = 'Debug',
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$srcRoot = (Resolve-Path (Join-Path $fixture '../../..')).Path
$packProject = Join-Path $srcRoot 'core/encoding/hex/encoding.hex.csproj'
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("package-usings-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'PackageUsings.csproj'), (Join-Path $fixture 'Program.cs') $work

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$verdicts = @()

# --- PACK: the props the tree's pack writes -------------------------------------------------------
$packOut = Join-Path $work 'pack'
$props = Join-Path $work 'go.encoding.hex.props'
$packLog = & dotnet pack $packProject -c $Configuration --no-build -o $packOut -nologo -v:m 2>&1 | ForEach-Object { "$_" }
if ($LASTEXITCODE -ne 0) {
    $packLog | Select-Object -Last 10 | ForEach-Object { Write-Host "    $_" }
    Write-Host "FAIL (PACK): dotnet pack of $packProject exited $LASTEXITCODE (build the stdlib in $Configuration first)"
    if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
    exit 1
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$nupkg = @(Get-ChildItem $packOut -Filter 'go.encoding.hex.*.nupkg')
$propsText = $null
if ($nupkg.Count -eq 1) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg[0].FullName)
    try {
        $entry = $zip.GetEntry('build/go.encoding.hex.props')
        if ($entry) {
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $props, $true)
            $propsText = [System.IO.File]::ReadAllText($props)
        }
    }
    finally { $zip.Dispose() }
}

if ($null -eq $propsText) {
    $verdicts += "FAIL (PACK): the packed go.encoding.hex carries no build/go.encoding.hex.props"
}
elseif (-not $propsText.Contains('<Using Include="go.encoding" />') -or -not $propsText.Contains('<Using Include="go.encoding.hex_package" Alias="hex" />')) {
    $propsText -split "`n" | ForEach-Object { Write-Host "    $_" }
    $verdicts += "FAIL (PACK): build/go.encoding.hex.props does not name go.encoding and the hex alias"
}
else {
    $verdicts += "PASS (PACK): build/go.encoding.hex.props names go.encoding and, opt-in, hex = go.encoding.hex_package"
}

# --- The consumer arms ------------------------------------------------------------------------------
# A feed of ONE source and a cache nothing else has touched: a green read must come from the package
# under test, never from a machine-wide cache or a second feed.
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="under-test" value="$Source" />
  </packageSources>
</configuration>
"@ | Set-Content -Path (Join-Path $work 'nuget.config') -Encoding utf8

$env:NUGET_PACKAGES = Join-Path $work 'packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $work 'http-cache'

$project = Join-Path $work 'PackageUsings.csproj'
$withProps = "-p:GoPackageProps=$props"

function Invoke-Arm([string] $Name, [string[]] $Extra, [bool] $ExpectBuild, [string] $ExpectError) {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
    $log = & dotnet build $project -c Debug "-p:GoPackageVersion=$Version" -nologo -v:m @Extra 2>&1 | ForEach-Object { "$_" }
    $built = $LASTEXITCODE -eq 0

    if ($ExpectBuild) {
        if (-not $built) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return "FAIL ($Name): expected a green build" }
        $run = & dotnet run --project $project -c Debug --no-build 2>&1 | ForEach-Object { "$_" }
        $runCode = $LASTEXITCODE
        $run | ForEach-Object { Write-Host "    $_" }
        if ($runCode -ne 0 -or -not ($run -match 'PACKAGE-USINGS: ok')) { return "FAIL ($Name): the fixture did not run clean (exit $runCode)" }
        return "PASS ($Name)"
    }

    if ($built) { return "FAIL ($Name): expected the build to fail with $ExpectError, and it built" }
    if (-not ($log -match "error $ExpectError")) { $log | Select-String ' error ' | Select-Object -First 3 | ForEach-Object { Write-Host "    $_" }; return "FAIL ($Name): the build failed, but not with $ExpectError" }
    return "PASS ($Name): failed with $ExpectError as the control requires"
}

Write-Host "Package usings gate: go.encoding.hex $Version from $Source, props packed from $packProject"
if ($null -ne $propsText) {
    $verdicts += Invoke-Arm 'NAMESPACE' @($withProps) $true ''
    $verdicts += Invoke-Arm 'CONTROL no props' @() $false 'CS1929'
    $verdicts += Invoke-Arm 'ALIASES GoGlobalPackageAliases=true' @($withProps, '-p:GoGlobalPackageAliases=true', '-p:DefineConstants=GLOBAL_ALIASES') $true ''
    $verdicts += Invoke-Arm 'ALIAS CONTROL off by default' @($withProps, '-p:DefineConstants=GLOBAL_ALIASES') $false 'CS0246'
    $verdicts += Invoke-Arm 'ALIAS CLASH with the file''s own alias' @($withProps, '-p:GoGlobalPackageAliases=true') $false 'CS1537'
}

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
