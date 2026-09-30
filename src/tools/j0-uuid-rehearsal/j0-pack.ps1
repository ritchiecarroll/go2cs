<#
.SYNOPSIS
    J0 step 2: pack the rehearsal's local folder feed (docs/phase4/SIZING-j0-uuid.md).

.DESCRIPTION
    Two halves, each switchable, because they unblock at different times:

    -Closure  The converted standard library, go.lib and go.gen, through push-nuget.ps1's own pack path with
              -VersionSuffix, so every package is <GoStdLibVersion>.<GoBuildNumber>-<suffix>. push-nuget runs its
              release pre-flight first, exactly as a release would, and never pushes: -VersionSuffix is refused with
              -Push, -BumpBuild and -VerifyOnly.

    -Uuid     The converted google/uuid package. It is packed against the SAME version as the closure, so every
              go.* dependency it declares resolves to a closure package in the same feed. The package's OWN version
              is then set to the Go module's (<ModuleVersion>-<suffix>, e.g. 1.6.0-local.1), and the corpus release
              it was converted against is recorded in its release notes. That last step rewrites the .nuspec inside
              the .nupkg. A global -p:PackageVersion cannot do it, because MSBuild hands a global property to every
              referenced project too, and uuid's go.* dependencies would then claim the module's version.

    Nothing here publishes. The feed is a local folder, and every version carries the rehearsal suffix.
#>
#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Go2csPath,
    [Parameter(Mandatory)][string]$Feed,
    [Parameter(Mandatory)][string]$VersionSuffix,
    [string]$ProjectDir,
    [string]$ModuleVersion = '1.6.0',
    [string]$ModulePath = 'github.com/google/uuid',
    [switch]$Closure,
    [switch]$Uuid
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

if (-not $Closure -and -not $Uuid) { $Closure = $true; $Uuid = $true }

if (-not (Test-Path (Join-Path $Go2csPath 'core\golib\golib.csproj'))) {
    throw "REFUSED: -Go2csPath '$Go2csPath' is not a go2cs root (no core\golib\golib.csproj under it)."
}
if ($VersionSuffix -notmatch '^[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*$') {
    throw "-VersionSuffix '$VersionSuffix' is not a NuGet prerelease label."
}

$propsText = [System.IO.File]::ReadAllText((Join-Path $Go2csPath 'version.props'))
if ($propsText -notmatch '<GoStdLibVersion>([^<]+)</GoStdLibVersion>') { throw "GoStdLibVersion not found in version.props" }
$base = $Matches[1].Trim()
if ($propsText -notmatch '<GoBuildNumber>(\d+)</GoBuildNumber>') { throw "GoBuildNumber not found in version.props" }
$closureVersion = "$base.$($Matches[1])-$VersionSuffix"
$uuidVersion = "$ModuleVersion-$VersionSuffix"
$treeSha = (& git -C $Go2csPath rev-parse --short=10 HEAD 2>$null)

New-Item -ItemType Directory -Force $Feed | Out-Null
Write-Host "==> Feed $Feed   closure version $closureVersion   uuid version $uuidVersion   go2cs tree $treeSha"

if ($Closure) {
    $pushNuget = Join-Path $Go2csPath 'push-nuget.ps1'
    Write-Host "==> Closure: $pushNuget -VersionSuffix $VersionSuffix -OutDir $Feed"
    & $pushNuget -VersionSuffix $VersionSuffix -OutDir $Feed
    if ($LASTEXITCODE -ne 0) { throw "push-nuget.ps1 failed ($LASTEXITCODE)" }
}

if ($Uuid) {
    if (-not $ProjectDir) { throw "-Uuid needs -ProjectDir (the converted project's directory from j0-convert.ps1)" }
    $projectName = $ModulePath -replace '/', '.'
    $csproj = Join-Path $ProjectDir "$projectName.csproj"
    if (-not (Test-Path $csproj)) { throw "Converted project not found: $csproj" }

    $id = "go.$projectName"
    $stage = Join-Path ([System.IO.Path]::GetTempPath()) "j0-pack-$PID"
    New-Item -ItemType Directory -Force $stage | Out-Null

    try {
        Write-Host "==> Packing $id against the closure version $closureVersion"
        $go2csRoot = $Go2csPath.TrimEnd('\') + '\'
        & dotnet pack $csproj -c Release -o $stage "-p:go2csPath=$go2csRoot" "-p:PackageVersion=$closureVersion" -p:GeneratePackageOnBuild=false --nologo -v m
        if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed ($LASTEXITCODE)" }

        $packed = Join-Path $stage "$id.$closureVersion.nupkg"
        if (-not (Test-Path $packed)) { throw "Expected $packed" }

        # The package's own version becomes the module's; its dependencies keep the closure's.
        $notes = "LOCAL REHEARSAL, not a publication. Converted by go2cs from $ModulePath v$ModuleVersion against the " +
                 "converted Go standard library at corpus release $closureVersion (go2cs tree $treeSha)."
        $zip = [System.IO.Compression.ZipFile]::Open($packed, 'Update')
        try {
            $entry = @($zip.Entries | Where-Object { $_.FullName -notlike '*/*' -and $_.FullName -like '*.nuspec' })[0]
            $reader = New-Object System.IO.StreamReader($entry.Open())
            $nuspec = $reader.ReadToEnd()
            $reader.Dispose()

            $nuspec = [regex]::Replace($nuspec, '<version>[^<]*</version>', "<version>$uuidVersion</version>", 'IgnoreCase')
            if ($nuspec -match '<releaseNotes>') {
                $nuspec = [regex]::Replace($nuspec, '<releaseNotes>[\s\S]*?</releaseNotes>', "<releaseNotes>$notes</releaseNotes>")
            }
            else {
                $nuspec = $nuspec -replace '</description>', "</description>`n    <releaseNotes>$notes</releaseNotes>"
            }

            $entry.Delete()
            $newEntry = $zip.CreateEntry($entry.FullName)
            $writer = New-Object System.IO.StreamWriter($newEntry.Open(), (New-Object System.Text.UTF8Encoding($false)))
            $writer.Write($nuspec)
            $writer.Dispose()
        }
        finally { $zip.Dispose() }

        $final = Join-Path $Feed "$id.$uuidVersion.nupkg"
        Move-Item $packed $final -Force

        # Read it back: the id, its version, every go.* dependency at the closure version, and the licence.
        $zip = [System.IO.Compression.ZipFile]::OpenRead($final)
        try {
            $entry = @($zip.Entries | Where-Object { $_.FullName -notlike '*/*' -and $_.FullName -like '*.nuspec' })[0]
            $reader = New-Object System.IO.StreamReader($entry.Open())
            $text = $reader.ReadToEnd()
            $reader.Dispose()
            $hasLicense = @($zip.Entries | Where-Object { $_.FullName -eq 'LICENSE' }).Count -eq 1
        }
        finally { $zip.Dispose() }

        [xml]$doc = $text
        $meta = $doc.package.metadata
        $deps = @($doc.GetElementsByTagName('dependency'))
        $wrong = @($deps | Where-Object { $_.id -like 'go.*' -and $_.version -notmatch [regex]::Escape($closureVersion) })
        Write-Host "==> $($meta.id) $($meta.version): $($deps.Count) dependencies, licence $(if ($meta.license) { $meta.license.'#text' } else { 'none' }), LICENSE file packed: $hasLicense"
        Write-Host "    release notes: $($meta.releaseNotes)"
        if ($meta.version -ne $uuidVersion) { throw "The packed version is $($meta.version), not $uuidVersion" }
        if ($wrong.Count) { throw "go.* dependencies not at the closure version: $(($wrong | ForEach-Object { "$($_.id) $($_.version)" }) -join ', ')" }
        if (-not $hasLicense) { throw "The package carries no LICENSE file" }
        Write-Host "==> Wrote $final"
    }
    finally {
        Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
    }
}
