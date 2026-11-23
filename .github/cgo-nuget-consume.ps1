<#
.SYNOPSIS
    cgo-nuget-e2e's J3 (cgo against go.* INC 4), one leg per shipped RID: the go-pointer sample consumes J2's -Cgo
    package with J1's go.* closure on this host, and is compared with `go run` on the same host. On osx-arm64 the build
    must refuse (R-b); on win-x64 a doctored copy must be refused at load (R-a).

.DESCRIPTION
    The consumer is src/tools/nugetgo/Invoke-NugetgoBehaviorRow.ps1, the nugetgo-behavior row, with -ClosureFeed,
    -ClosureVersion and -SampleDir src/tests/CgoNugetE2E/<module>@<version>/. go.* restores from J1's feed alone, the package from J2's folder, into a private NUGET_PACKAGES,
    and the program builds against the closure version. Go is the oracle, read on this RID.

    THE C TOOLCHAIN. go run needs one: go-pointer calls C.malloc and C.free. The converter needs one too: the
    sample's conversion loads go-pointer cgo-on, since it has no cgo-off files. The step provisions the OS's default
    pinned toolchain before this script runs: zig on linux and darwin, llvm-mingw on windows. The BUILT program needs
    none. Its C binding is the package's own assembly, which binds the C library by name at load.

    THE ISOLATION (P3's plan read, SHOULD 5). Before the row, a stale go.lib of the closure's version is planted in the
    user's global packages folder: a directory with an unloadable assembly. After the row, the go.lib the private
    restore holds must be J1's nupkg byte for byte (its .nupkg.sha512 against J1's file).

    R-b, osx-arm64. The package carries no osx-arm64 runtime, by name. The consumer's dotnet build must fail with
    NUGETGOCGO001.

    R-a, win-x64. A copy of the package whose runtimes/win-x64/ holds the linux flavor (J2). It must build, and its run
    must be refused by A6 at load, and the refusal must name both targets: "converted for linux/amd64" and "runs as
    windows/amd64". It reads A6 on a real pack, not on a patched string.

    linux-arm64 is read like every other RID. If its leg cannot run, the verdict says NOT MEASURED and is never green.
#>
[CmdletBinding()]
param(
    # J2's folder: the one go-pointer package.
    [Parameter(Mandatory)] [string] $Feed,
    # J2's doctored folder (R-a's input).
    [Parameter(Mandatory)] [string] $DoctoredFeed,
    # J1's go.* feed.
    [Parameter(Mandatory)] [string] $ClosureFeed,
    [Parameter(Mandatory)] [string] $Suffix,
    [Parameter(Mandatory)] [string] $Rid,
    [Parameter(Mandatory)] [string] $OutDir,
    [string] $ModulePath = 'github.com/mattn/go-pointer',
    [string] $GoVersion = 'v0.0.1'
)

$ErrorActionPreference = 'Continue'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$src = Join-Path $repo 'src'
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path
$readings = New-Object System.Collections.Generic.List[string]
$failures = New-Object System.Collections.Generic.List[string]
function Reading([string]$line) { $script:readings.Add($line); Write-Host "READING $line" }
function Failure([string]$line) { $script:failures.Add($line); Write-Host "::error::$line" }

Import-Module (Join-Path $src 'tools/nugetgo/NugetgoIdentity.psm1') -Force
$version = (Get-NugetgoVersion -GoVersion $GoVersion).Version + "-$Suffix"
$goLib = @(Get-ChildItem -LiteralPath $ClosureFeed -File -Filter 'go.lib.*.nupkg')
$closureVersion = if ($goLib.Count -eq 1) { $goLib[0].Name.Substring('go.lib.'.Length, $goLib[0].Name.Length - 'go.lib.'.Length - '.nupkg'.Length) } else { '' }
if (-not $closureVersion) { Failure "J1's feed holds $($goLib.Count) go.lib package(s), not one" }

$cc = "$(& go env CC)".Trim()
Reading "instrument: go env CC = $cc; dotnet $("$(& dotnet --version)".Trim()); package $version; closure $closureVersion"

# THE REHEARSAL CONVERTER. A -recurse=nuget conversion maps a module to its package only when the package's
# self-description names this converter's own corpus release (nugetPackage.go, qualifyNuGetPackage: the strict match).
# J2's package names the local closure's release, J1's prerelease of the next revision, as a release's packages name
# that release. So the consumer converts with this tree's converter stamped as that release, the way a release stamps
# its converter before it packs. The stamp file is rewritten in this job's checkout only, and restored after the build.
# Without it the module converts locally (run 45014928125: "built for go2cs 1.26.11.2-ci.45001845693, and this converter
# is 1.26.11.1"), and the row reads the module's local conversion, not the package.
$exeName = if ($IsWindows) { 'go2cs.exe' } else { 'go2cs' }
$converter = Join-Path $OutDir "bin/$exeName"
$stamp = Join-Path $src 'go2cs/corpus-release.txt'
$stamped = [System.IO.File]::ReadAllText($stamp)
Push-Location (Join-Path $src 'go2cs')
try {
    [System.IO.File]::WriteAllText($stamp, "$closureVersion`n")
    & go build -o $converter . ; if ($LASTEXITCODE -ne 0) { Failure "the converter did not build ($LASTEXITCODE)" }
}
finally { [System.IO.File]::WriteAllText($stamp, $stamped); Pop-Location }
Reading "converter: this tree's, stamped as the closure's release $closureVersion for this job only (it was $($stamped.Trim()))"

# ---- the planted stale go.lib ------------------------------------------------------------------------------------------
# Only on a hosted runner, whose cache is thrown away with it, and only for a prerelease closure version, which no
# published go.lib carries: a broken go.lib must never reach a fleet box's own cache (COORD's INC 4 review, fold 6).
if ($env:GITHUB_ACTIONS -eq 'true' -and $closureVersion.Contains('-')) {
    $global = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path ([Environment]::GetFolderPath('UserProfile')) '.nuget/packages' }
    $planted = Join-Path $global "go.lib/$($closureVersion.ToLowerInvariant())"
    New-Item -ItemType Directory -Force -Path (Join-Path $planted 'lib/net10.0') | Out-Null
    [System.IO.File]::WriteAllText((Join-Path $planted 'lib/net10.0/go.lib.dll'), 'a planted stale go.lib: not an assembly')
    [System.IO.File]::WriteAllText((Join-Path $planted "go.lib.$($closureVersion.ToLowerInvariant()).nupkg.sha512"), 'planted')
    Reading "planted: a stale go.lib $closureVersion in the user's global packages folder"
}
else {
    Reading "planted: nothing (not a hosted runner, or $closureVersion is no prerelease); the restored go.lib is still checked against J1's bytes"
}

function Invoke-Row([string]$label, [string]$feed) {
    $work = Join-Path $OutDir "work-$label"
    $log = Join-Path $OutDir "row-$label.log"
    & pwsh -NoProfile -File (Join-Path $src 'tools/nugetgo/Invoke-NugetgoBehaviorRow.ps1') -ModulePath $ModulePath -GoVersion $GoVersion `
        -PackageVersion $version -Feed $feed -Rid $Rid -Converter $converter -OutDir (Join-Path $OutDir "rows-$label") -Work $work `
        -ClosureFeed $ClosureFeed -ClosureVersion $closureVersion -SampleDir (Join-Path $src "tests/CgoNugetE2E/$ModulePath@$GoVersion") *> $log
    [pscustomobject]@{ Exit = $LASTEXITCODE; Work = $work; Log = $log; Text = [System.IO.File]::ReadAllText($log) }
}
function Read-Text([string]$path) { if (Test-Path -LiteralPath $path) { [System.IO.File]::ReadAllText($path) } else { '' } }

if ($Rid -ceq 'osx-arm64') {
    # ---- R-b ------------------------------------------------------------------------------------------------------------
    $row = Invoke-Row 'r-b' $Feed
    $build = Read-Text (Join-Path $row.Work 'build.log')
    if ($row.Exit -ne 0 -and $build.Contains('NUGETGOCGO001')) {
        $line = @($build -split "`n" | Where-Object { $_.Contains('NUGETGOCGO001') } | Select-Object -First 1)[0].Trim()
        Reading "R-b PASS: the osx-arm64 consumer build is refused: $line"
    }
    else { Failure "R-b: the osx-arm64 consumer was not refused with NUGETGOCGO001 (row exit $($row.Exit); see $($row.Log))" }
}
else {
    # ---- J3 -------------------------------------------------------------------------------------------------------------
    $row = Invoke-Row 'j3' $Feed
    $rowFile = Join-Path $OutDir "rows-j3/$Rid.md"
    if ($row.Exit -eq 0 -and (Test-Path -LiteralPath $rowFile) -and [System.IO.File]::ReadAllText($rowFile).Contains('outputs-equal: true')) {
        Reading "J3 PASS on ${Rid}: the converted program printed what go run printed"
        $readings.AddRange([string[]]@([System.IO.File]::ReadAllLines($rowFile) | Where-Object { $_ -match '^(rid|runtime-rid|goos|go-output|dotnet-output|outputs-equal|assembly):' } | ForEach-Object { "  $_" }))
    }
    else {
        $tail = @($row.Text -split "`n" | Where-Object { $_.Trim() } | Select-Object -Last 3) -join ' / '
        Failure "J3 on ${Rid}: the row did not read equal (exit $($row.Exit)): $tail"
    }

    # The private restore holds J1's go.lib, never the planted one.
    $restoredSha = Join-Path $row.Work "packages/go.lib/$($closureVersion.ToLowerInvariant())/go.lib.$($closureVersion.ToLowerInvariant()).nupkg.sha512"
    if (Test-Path -LiteralPath $restoredSha) {
        $sha512 = [System.Security.Cryptography.SHA512]::Create()
        try { $j1 = [System.Convert]::ToBase64String($sha512.ComputeHash([System.IO.File]::ReadAllBytes($goLib[0].FullName))) } finally { $sha512.Dispose() }
        if ([System.IO.File]::ReadAllText($restoredSha).Trim() -ceq $j1) { Reading "isolation PASS: the restored go.lib is J1's nupkg byte for byte, not the planted one" }
        else { Failure 'isolation: the restored go.lib is not J1''s nupkg' }
    }
    else { Failure "isolation: the row's private restore holds no go.lib $closureVersion" }

    if ($Rid -ceq 'win-x64') {
        # ---- R-a --------------------------------------------------------------------------------------------------------
        $doctored = Invoke-Row 'r-a' $DoctoredFeed
        $err = Read-Text (Join-Path $doctored.Work 'dotnet-run.err')
        $built = Test-Path -LiteralPath (Join-Path $doctored.Work 'dotnet-run.out')
        if ($doctored.Exit -ne 0 -and $built -and $err.Contains('converted for linux/amd64') -and $err.Contains('runs as windows/amd64')) {
            $line = @($err -split "`n" | Where-Object { $_.Contains('converted for linux/amd64') } | Select-Object -First 1)[0].Trim()
            Reading "R-a PASS: the doctored win-x64 runtime built and A6 refused it at load: $line"
        }
        else { Failure "R-a: the doctored package was not refused at load naming linux/amd64 and windows/amd64 (row exit $($doctored.Exit), built $built; see $($doctored.Log))" }
    }
}

# The restored packages and build output are large and say nothing the logs and rows do not.
Get-ChildItem -LiteralPath $OutDir -Directory -Filter 'work-*' | ForEach-Object {
    Remove-Item -Recurse -Force (Join-Path $_.FullName 'packages') -ErrorAction SilentlyContinue
    Get-ChildItem $_.FullName -Recurse -Directory -Include 'bin', 'obj' -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}
Remove-Item -Recurse -Force (Join-Path $OutDir 'bin') -ErrorAction SilentlyContinue

$verdict = if ($failures.Count) { "FAIL: $($failures -join ' | ')" } else { "PASS on ${Rid}" }
$report = @("## cgo-nuget-e2e J3 -- $ModulePath@$GoVersion on $Rid", '', $verdict, '', '```') + $readings + @('```')
[System.IO.File]::WriteAllLines((Join-Path $OutDir "cgo-nuget-consume-$Rid.md"), $report)
if ($env:GITHUB_STEP_SUMMARY) {
    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $report
    & (Join-Path $PSScriptRoot 'annotate-summary.ps1') -Title "cgo-nuget-e2e J3 $Rid" -Lines $report
}
if ($failures.Count) { exit 1 }
exit 0
