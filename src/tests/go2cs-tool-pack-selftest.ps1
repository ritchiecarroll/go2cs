#Requires -Version 5.1
<#
.SYNOPSIS
    Guards the go2cs dotnet tool's packing step (src/tools/go2cs-tool/pack-go2cs-tool.ps1) with a self-signed test
    certificate, so it never needs the release card.

.DESCRIPTION
    Runs the packing step the release ritual's Phase 1b runs, from this tree, into a scratch folder:

      signed      -CertificateFile/-KeyFile (a throwaway self-signed code-signing certificate) -> exit 0; seven
                  packages: go2cs.<rid> for win-x64, win-arm64, linux-x64, linux-arm64, osx-x64 and osx-arm64, each
                  a DotnetToolRidPackage holding tools/net10.0/<rid>/go2cs[.exe] and a DotnetToolSettings.xml that
                  runs it as an executable, and the go2cs DotnetTool package whose settings name all six; each
                  package carries LICENSE, LICENSE-EXCEPTION, README.md and the icon; both Windows executables
                  verify as signed by the test certificate
      release     a -Version whose release is not the go2cs.release the binaries report -> refused by name, nothing
                  packed
      unsigned    -Unsigned with no -OutDir -> refused: unsigned executables never go to the release's folder

    Needs go (the release src/go2cs/go.mod names), dotnet, openssl and osslsigncode on PATH. Exit 0 clean, 1 on any
    violation. Writes only under the host's temp directory.
#>
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'

$srcRoot = Split-Path $PSScriptRoot -Parent
$pack = Join-Path $srcRoot 'tools/go2cs-tool/pack-go2cs-tool.ps1'
$pwshPath = (Get-Process -Id $PID).Path

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$failures = New-Object System.Collections.Generic.List[string]
function Check([bool] $Condition, [string] $Message) { if (-not $Condition) { $failures.Add($Message) } }

foreach ($tool in 'go', 'dotnet', 'openssl', 'osslsigncode') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { Write-Host "go2cs-tool pack selftest: $tool is not on PATH"; exit 1 }
}

# Native commands write progress to stderr; only their exit codes decide.
function Invoke-Native([string] $Exe, [string[]] $Arguments) {
    $ErrorActionPreference = 'Continue'
    $out = @(& $Exe @Arguments 2>&1 | ForEach-Object { "$_" })
    [pscustomobject]@{ Code = $LASTEXITCODE; Text = ($out -join "`n") }
}

function Invoke-Pack([string[]] $Arguments) {
    Invoke-Native $pwshPath (@('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $pack) + $Arguments)
}

# The entries and the text of one entry of a package.
function Read-Package([string] $Path, [string] $Entry) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $names = @($zip.Entries | ForEach-Object { $_.FullName })
        $text = $null
        $found = $zip.GetEntry($Entry)
        if ($found) {
            $reader = New-Object System.IO.StreamReader($found.Open())
            try { $text = $reader.ReadToEnd() } finally { $reader.Dispose() }
        }
    }
    finally { $zip.Dispose() }
    [pscustomobject]@{ Names = $names; Text = $text }
}

$work = Join-Path ([System.IO.Path]::GetTempPath()) ('gtps-' + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null

try {
    $release = (Get-Content -Raw (Join-Path $srcRoot 'go2cs/corpus-release.txt')).Trim()
    $version = "$release-selftest"

    $certFile = Join-Path $work 'cert.pem'
    $keyFile = Join-Path $work 'key.pem'
    $made = Invoke-Native 'openssl' @('req', '-x509', '-newkey', 'rsa:2048', '-nodes', '-keyout', $keyFile, '-out', $certFile, '-days', '2',
        '-subj', '/CN=go2cs selftest code signing', '-addext', 'extendedKeyUsage=codeSigning')
    if ($made.Code -ne 0) { throw "openssl could not make the test certificate:`n$($made.Text)" }

    # ---- signed ---------------------------------------------------------------------------------------------------
    $out = Join-Path $work 'signed'
    $run = Invoke-Pack @('-Version', $version, '-OutDir', $out, '-CertificateFile', $certFile, '-KeyFile', $keyFile, '-NoTimestamp')
    Check ($run.Code -eq 0) "signed: exit $($run.Code); want 0:`n$($run.Text)"

    $rids = 'win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64'
    foreach ($rid in $rids) {
        $exe = if ($rid -like 'win-*') { 'go2cs.exe' } else { 'go2cs' }
        $nupkg = Join-Path $out "go2cs.$rid.$version.nupkg"
        if (-not (Test-Path $nupkg)) { $failures.Add("signed: no go2cs.$rid package"); continue }

        $package = Read-Package $nupkg "tools/net10.0/$rid/DotnetToolSettings.xml"
        foreach ($entry in "tools/net10.0/$rid/$exe", 'LICENSE', 'LICENSE-EXCEPTION', 'README.md', 'go2cs.png', "go2cs.$rid.nuspec") {
            Check ($package.Names -contains $entry) "signed: go2cs.$rid holds no $entry"
        }
        Check ($package.Text -match "<Command Name=`"go2cs`" EntryPoint=`"$([regex]::Escape($exe))`" Runner=`"executable`" />") "signed: go2cs.$rid's DotnetToolSettings.xml does not run $exe as an executable:`n$($package.Text)"
        Check ((Read-Package $nupkg "go2cs.$rid.nuspec").Text -match '<packageType name="DotnetToolRidPackage" />') "signed: go2cs.$rid is not a DotnetToolRidPackage"

        if ($rid -like 'win-*') {
            $extracted = Join-Path $work "$rid.exe"
            $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg)
            try { [System.IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry("tools/net10.0/$rid/go2cs.exe"), $extracted) } finally { $zip.Dispose() }
            $verify = Invoke-Native 'osslsigncode' @('verify', '-CAfile', $certFile, '-in', $extracted)
            Check ($verify.Code -eq 0) "signed: go2cs.exe in go2cs.$rid does not verify against the test certificate:`n$($verify.Text)"
        }
    }

    $pointer = Join-Path $out "go2cs.$version.nupkg"
    if (Test-Path $pointer) {
        $package = Read-Package $pointer 'tools/net10.0/any/DotnetToolSettings.xml'
        Check (-not ($package.Names -match '/go2cs(\.exe)?$')) "signed: the go2cs package holds an executable: $($package.Names -join ', ')"
        foreach ($rid in $rids) {
            Check ($package.Text -match "<RuntimeIdentifierPackage RuntimeIdentifier=`"$rid`" Id=`"go2cs\.$rid`" />") "signed: the go2cs package does not name go2cs.$rid"
        }
        Check ((Read-Package $pointer 'go2cs.nuspec').Text -match '<packageType name="DotnetTool" />') 'signed: the go2cs package is not a DotnetTool'
    }
    else { $failures.Add('signed: no go2cs package') }

    # ---- release --------------------------------------------------------------------------------------------------
    $out = Join-Path $work 'release'
    $run = Invoke-Pack @('-Version', '0.0.1-foreign', '-OutDir', $out, '-CertificateFile', $certFile, '-KeyFile', $keyFile, '-NoTimestamp')
    Check ($run.Code -eq 1) "release: exit $($run.Code); want 1:`n$($run.Text)"
    Check ($run.Text -match "go2cs\.release=$([regex]::Escape($release)), not 0\.0\.1") "release: the refusal does not name both releases:`n$($run.Text)"
    Check (@(Get-ChildItem $out -Filter '*.nupkg' -ErrorAction SilentlyContinue).Count -eq 0) 'release: packages were written although the release was refused'

    # ---- unsigned -------------------------------------------------------------------------------------------------
    $run = Invoke-Pack @('-Version', $version, '-Unsigned')
    Check ($run.Code -eq 1) "unsigned: exit $($run.Code); want 1:`n$($run.Text)"
    Check ($run.Text -match '-Unsigned needs its own -OutDir') "unsigned: the refusal does not say why:`n$($run.Text)"
}
finally {
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}

if ($failures.Count -gt 0) { $failures | ForEach-Object { Write-Host "FAIL: $_" }; exit 1 }
Write-Host 'go2cs-tool pack selftest: PASS (seven packages of the right types; both Windows executables signed by the test certificate; a foreign release and an unsigned pack to the release folder refused)'
exit 0
