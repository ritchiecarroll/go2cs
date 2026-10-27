#Requires -Version 5.1
<#
.SYNOPSIS
    Pack the go2cs dotnet tool: one package per platform and the `go2cs` package that points at them.

.DESCRIPTION
    `dotnet tool install -g go2cs` installs the converter itself, a native Go binary, so the tool ships as seven
    packages (docs/PLAN-dotnet-tool-installer.md): `go2cs.<rid>` for each of the six platforms the release builds,
    holding that platform's `go2cs` executable, and `go2cs`, which names them. The .NET CLI reads the pointer, picks
    the package for the machine's RID and runs the executable directly (Runner="executable"), setting its execute bit
    on install. On first use the converter downloads the Go release it converts with (toolchainProvision.go).

    This script:
      1. cross-compiles the six binaries from src/go2cs as release-binaries.yml does (-trimpath, CGO_ENABLED=0,
         GOTOOLCHAIN=local), with the Go release go.mod names, since a go2cs built with any other Go provisions none;
      2. checks that the binary built for this machine reports the package version as its go2cs.release;
      3. Authenticode-signs the two Windows executables through src/_authenticode.ps1, the step the release archives
         use, in one signtool process (one PIN prompt);
      4. packs the seven packages into -OutDir, where sign-nupkgs.ps1 then signs them with the go.* packages.

    In the release ritual, release-nuget.ps1 runs it after Phase 1 has packed the go.* packages, so the tree it
    builds from is the release's record: version.props bumped, the proof frozen and corpus-release.txt regenerated.

.PARAMETER Version
    The packages' version. Defaults to the release src/version.props names. Without a prerelease suffix it must equal
    the go2cs.release the binaries report; a suffix (1.26.10.2-smoke) marks a test build of that release.

.PARAMETER OutDir
    Where the packages go: src/artifacts/nupkg unless given.

.PARAMETER Fingerprint
    The release certificate's SHA-256 fingerprint; $env:NuGetCertFingerprint unless given.

.PARAMETER CertificateFile
    With -KeyFile, sign with osslsigncode and that PEM certificate instead (the selftest's self-signed one).

.PARAMETER Unsigned
    Leave the Windows executables unsigned, for a smoke that never publishes. It refuses the default -OutDir, so
    unsigned executables cannot land in the folder the ritual signs and pushes.

.EXAMPLE
    .\pack-go2cs-tool.ps1
    In the release ritual: build, sign (one PIN prompt) and pack the release's tool packages.

.EXAMPLE
    .\pack-go2cs-tool.ps1 -Version 1.26.10.2-smoke -Unsigned -OutDir ./feed
    A local feed for an install smoke.
#>
[CmdletBinding()]
param(
    [string] $Version,
    [string] $OutDir,
    [string] $Fingerprint = $env:NuGetCertFingerprint,
    [string] $Timestamper = 'http://timestamp.digicert.com',
    [string] $CertificateFile,
    [string] $KeyFile,
    [switch] $NoTimestamp,
    [switch] $Unsigned
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '../../_paths.ps1')

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

function Die([string] $Message) { Write-Host "pack-go2cs-tool: $Message" -ForegroundColor Red; exit 1 }

# Native commands write progress to stderr; only their exit codes decide.
function Invoke-Native([string] $Exe, [string[]] $Arguments, [string] $Dir) {
    $ErrorActionPreference = 'Continue'
    if ($Dir) { Push-Location $Dir }
    try { $out = @(& $Exe @Arguments 2>&1 | ForEach-Object { "$_" }) } finally { if ($Dir) { Pop-Location } }
    if ($LASTEXITCODE -ne 0) { throw "$Exe $($Arguments -join ' ') exited $($LASTEXITCODE):`n$($out -join "`n")" }
    $out
}

# The six platforms the release builds, in release-binaries.yml's order: GOOS, GOARCH, .NET RID.
$platforms = @(
    @('windows', 'amd64', 'win-x64'), @('windows', 'arm64', 'win-arm64'),
    @('linux', 'amd64', 'linux-x64'), @('linux', 'arm64', 'linux-arm64'),
    @('darwin', 'amd64', 'osx-x64'), @('darwin', 'arm64', 'osx-arm64'))

$defaultOutDir = Join-Path $SrcRoot 'artifacts/nupkg'
if (-not $OutDir) {
    if ($Unsigned) { Die '-Unsigned needs its own -OutDir: the default is the folder the release signs and pushes.' }
    $OutDir = $defaultOutDir
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path
if ($Unsigned -and $OutDir -eq (Resolve-Path $defaultOutDir -ErrorAction SilentlyContinue).Path) {
    Die '-Unsigned refuses the release folder; unsigned executables must not reach the packages the release pushes.'
}

if (-not $Version) {
    $props = ([xml](Get-Content -Raw (Join-Path $SrcRoot 'version.props'))).Project.PropertyGroup
    $Version = "$($props.GoStdLibVersion).$($props.GoBuildNumber)".Trim()
}
$release = ($Version -split '-', 2)[0]

# A go2cs provisions Go only when it was built with the release go.mod names (newToolchainProvisioner).
$goLine = Select-String -Path (Join-Path $ConverterSrc 'go.mod') -Pattern '^go\s+(\S+)' | Select-Object -First 1
if (-not $goLine) { Die "no go line in $(Join-Path $ConverterSrc 'go.mod')" }
$wantGo = 'go' + $goLine.Matches[0].Groups[1].Value
$savedToolchain = $env:GOTOOLCHAIN
$env:GOTOOLCHAIN = 'local'
try { $haveGo = (Invoke-Native 'go' @('env', 'GOVERSION') $ConverterSrc | Select-Object -Last 1).Trim() }
finally { $env:GOTOOLCHAIN = $savedToolchain }
if ($haveGo -ne $wantGo) { Die "the go on PATH is $haveGo; go.mod names $wantGo, and a go2cs built with any other Go provisions none." }

$work = Join-Path ([System.IO.Path]::GetTempPath()) ('go2cs-tool-' + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null

try {
    # ---- 1. build ------------------------------------------------------------------------------------------------
    Write-Host "pack-go2cs-tool: building six go2cs binaries with $haveGo from $ConverterSrc"
    $saved = @{ GOOS = $env:GOOS; GOARCH = $env:GOARCH; CGO_ENABLED = $env:CGO_ENABLED; GOTOOLCHAIN = $env:GOTOOLCHAIN }
    try {
        $env:CGO_ENABLED = '0'; $env:GOTOOLCHAIN = 'local'
        foreach ($p in $platforms) {
            $exe = if ($p[0] -eq 'windows') { 'go2cs.exe' } else { 'go2cs' }
            $env:GOOS = $p[0]; $env:GOARCH = $p[1]
            [void](Invoke-Native 'go' @('build', '-trimpath', '-o', (Join-Path $work "$($p[2])/$exe"), '.') $ConverterSrc)
        }
    }
    finally { foreach ($name in $saved.Keys) { [System.Environment]::SetEnvironmentVariable($name, $saved[$name]) } }

    # ---- 2. the release the binaries report ----------------------------------------------------------------------
    $hostPlatform = $platforms | Where-Object { $_[0] -eq $HostGoos -and $_[1] -eq $HostGoarch } | Select-Object -First 1
    if (-not $hostPlatform) { Die "this machine ($HostGoos/$HostGoarch) is none of the six platforms, so no binary can report its release" }
    $hostExe = Join-Path $work "$($hostPlatform[2])/go2cs$ExeSuffix"
    if (-not $IsWindowsHost) { [void](Invoke-Native 'chmod' @('+x', $hostExe)) }
    $report = Invoke-Native $hostExe @('-version')
    $reported = ($report | Where-Object { $_ -like 'go2cs.release=*' } | Select-Object -First 1) -replace '^go2cs\.release=', ''
    if ($reported -ne $release) {
        Die ("the binaries report go2cs.release=$reported, not $release. In the ritual, regenerate it after Phase 1 " +
             "(go run ./internal/gencorpusrelease in src/go2cs); otherwise pass the -Version of the release the tree records.")
    }
    Write-Host "pack-go2cs-tool: go2cs.release=$reported"

    # ---- 3. sign the Windows executables -------------------------------------------------------------------------
    $windowsExes = @($platforms | Where-Object { $_[0] -eq 'windows' } | ForEach-Object { Join-Path $work "$($_[2])/go2cs.exe" })
    if ($Unsigned) {
        Write-Host 'pack-go2cs-tool: -Unsigned: the Windows executables are NOT signed; these packages are for a smoke only' -ForegroundColor Yellow
    }
    else {
        . (Join-Path $SrcRoot '_authenticode.ps1')
        $sign = @{ Files = $windowsExes; Timestamper = $Timestamper; NoTimestamp = $NoTimestamp }
        if ($CertificateFile -or $KeyFile) { $sign.CertificateFile = $CertificateFile; $sign.KeyFile = $KeyFile }
        else { $sign.Fingerprint = $Fingerprint }
        foreach ($signed in @(Invoke-GoAuthenticodeSign @sign)) {
            Write-Host "pack-go2cs-tool: signed $(Split-Path (Split-Path $signed.File -Parent) -Leaf)/go2cs.exe by $($signed.Signer)$(if ($signed.Timestamped) { ', timestamped' })"
        }
    }

    # ---- 4. pack -------------------------------------------------------------------------------------------------
    # Each package's files are staged in one folder, the nuspec's base path. pack.csproj is a stub that builds
    # nothing; dotnet pack only reads the nuspec. Its restore output stays in the work folder.
    $common = 'LICENSE', 'LICENSE-EXCEPTION', 'go2cs.png'
    $settings = '<?xml version="1.0" encoding="utf-8"?>' + "`n<DotNetCliTool Version=`"2`">`n  <Commands>`n"

    function Invoke-Pack([string] $Nuspec, [string] $Stage, [string] $Rid) {
        foreach ($file in $common) { Copy-Item (Join-Path $ConverterSrc $file) $Stage }
        Copy-Item (Join-Path $PSScriptRoot 'README.md') $Stage
        [void](Invoke-Native 'dotnet' @('pack', (Join-Path $work 'pack.csproj'), '--nologo', '-o', $OutDir,
            "-p:NuspecFile=$(Join-Path $PSScriptRoot $Nuspec)", "-p:NuspecBasePath=$Stage", "-p:ToolVersion=$Version", "-p:ToolRid=$Rid") $work)
    }

    Copy-Item (Join-Path $PSScriptRoot 'pack.csproj') $work

    $pointers = New-Object System.Collections.Generic.List[string]
    foreach ($p in $platforms) {
        $rid = $p[2]
        $exe = if ($p[0] -eq 'windows') { 'go2cs.exe' } else { 'go2cs' }
        $stage = Join-Path $work "stage/$rid"
        New-Item -ItemType Directory -Force -Path (Join-Path $stage 'tool') | Out-Null
        Move-Item (Join-Path $work "$rid/$exe") (Join-Path $stage 'tool')
        Set-Content -Encoding UTF8 -Path (Join-Path $stage 'tool/DotnetToolSettings.xml') -Value ($settings +
            "    <Command Name=`"go2cs`" EntryPoint=`"$exe`" Runner=`"executable`" />`n  </Commands>`n</DotNetCliTool>")
        Invoke-Pack 'go2cs.rid.nuspec' $stage $rid
        $pointers.Add("    <RuntimeIdentifierPackage RuntimeIdentifier=`"$rid`" Id=`"go2cs.$rid`" />")
    }

    $stage = Join-Path $work 'stage/any'
    New-Item -ItemType Directory -Force -Path (Join-Path $stage 'tool') | Out-Null
    Set-Content -Encoding UTF8 -Path (Join-Path $stage 'tool/DotnetToolSettings.xml') -Value ($settings +
        "    <Command Name=`"go2cs`" />`n  </Commands>`n  <RuntimeIdentifierPackages>`n" + ($pointers -join "`n") +
        "`n  </RuntimeIdentifierPackages>`n</DotNetCliTool>")
    Invoke-Pack 'go2cs.nuspec' $stage 'any'

    # ---- 5. read back what was packed ----------------------------------------------------------------------------
    # By the identity each nuspec declares, not the file name, which NuGet writes with the normalized version.
    . (Join-Path $SrcRoot '_nugetpush.ps1')
    $wanted = ConvertTo-GoNuGetNormalizedVersion $Version
    $packed = @{}
    foreach ($nupkg in Get-ChildItem (Join-Path $OutDir 'go2cs*.nupkg') -File) {
        $identity = Get-GoNuGetPackageIdentity $nupkg.FullName
        if ((ConvertTo-GoNuGetNormalizedVersion $identity.Version) -eq $wanted) { $packed[$identity.Id.ToLowerInvariant()] = $nupkg }
    }

    foreach ($id in @($platforms | ForEach-Object { "go2cs.$($_[2])" }) + 'go2cs') {
        $nupkg = $packed[$id]
        if (-not $nupkg) { Die "dotnet pack wrote no $id $Version into $OutDir" }
        $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg.FullName)
        try { $entries = @($zip.Entries | ForEach-Object { $_.FullName }) } finally { $zip.Dispose() }
        $rid = if ($id -eq 'go2cs') { 'any' } else { $id.Substring(6) }
        $want = @("tools/net10.0/$rid/DotnetToolSettings.xml", 'LICENSE', 'LICENSE-EXCEPTION', 'README.md', 'go2cs.png')
        if ($rid -ne 'any') { $want += "tools/net10.0/$rid/go2cs$(if ($rid -like 'win-*') { '.exe' })" }
        foreach ($entry in $want) { if ($entries -notcontains $entry) { Die "$($nupkg.Name) holds no $entry" } }
        Write-Host ('pack-go2cs-tool: {0,-40} {1,12:N0} bytes' -f $nupkg.Name, $nupkg.Length)
    }
}
finally {
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}

exit 0
