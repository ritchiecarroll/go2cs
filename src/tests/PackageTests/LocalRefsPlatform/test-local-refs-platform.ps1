#Requires -Version 7
<#
.SYNOPSIS
    Gate for the platform a `go2cs -recurse` conversion in local-reference mode builds, from a CLEAN tree, in both the
    separate-output-root shape and the in-place deployed shape.

.DESCRIPTION
    The output root's generated Directory.Build.props default GoTargetOS/GoTargetArch to the platform the tree was
    converted for and hand them to the referenced standard-library projects; its Directory.Build.targets hand the same
    two values to NuGet's restore walk, which ignores ProjectReference metadata. A tree that has built the target
    flavour before keeps every project.assets.json and hides a restore that skipped the flavour's own projects, so
    this gate builds against a runtime root extracted from git (tracked files only: no bin, no obj) every run:

      DEFAULT    the fixture converts with plain -recurse against that root; `dotnet build` and `dotnet run`, with
                 no flags, build and run it, and its output equals `go run`'s.
      OVERRIDE   an explicit -p:GoTargetArch=riscv64 still wins over the default and reaches the referenced runtime:
                 the standard library's layout check refuses "<host GOOS>/riscv64" by name.
      IN-PLACE   deploy-core -Target <deploy root>, then plain -recurse -go2cspath <deploy root> with no output root, so
                 the conversion lands in the deploy root itself. The converter keeps deploy-core's
                 Directory.Build.props and writes only its Directory.Build.targets, so that props carries no platform
                 default and the build names the host platform explicitly; it builds and runs, output equal to go run's.

    Before the restore half (release-smoke 45729437375, both linux legs) DEFAULT stopped on NETSDK1004 for each
    project the runtime references only on linux; before the platform default, a linux build compiled the windows
    runtime and died at start on kernel32.dll. Before the IN-PLACE guard the targets' generator-output path read two
    properties only the converter's own props defines, every project shared "gen/", and the build failed CS2021.

    Exit 0 only when every arm reads as expected. Nothing outside a temporary work directory is written.

.PARAMETER Converter
    The go2cs executable to convert with. Default: built from this clone's src/go2cs into the work directory.

.PARAMETER Repository
    The go2cs clone whose committed standard library (HEAD) is extracted as the runtime root. Default: this clone.

.EXAMPLE
    pwsh ./test-local-refs-platform.ps1
#>
param(
    [string] $Converter,
    [string] $Repository = (Join-Path $PSScriptRoot '../../../..'),
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$Repository = (Resolve-Path $Repository).Path
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("local-refs-platform-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:MSBUILDDISABLENODEREUSE = '1'
$hostOS = if ($IsWindows) { 'windows' } elseif ($IsMacOS) { 'darwin' } else { 'linux' }
$verdicts = @()

function Invoke-Logged([string] $Log, [scriptblock] $Command) {
    & $Command *> $Log
    return $LASTEXITCODE
}

function Get-FirstError([string] $Log) {
    $line = Select-String -Path $Log -Pattern ' error |error:' | Select-Object -First 1
    if ($line) { return ": $($line.Line.Trim())" }
    return ''
}

try {
    # The runtime root: the committed standard library, runtime and analyzer, as git holds them. No build output can
    # come along, which is the point: every project.assets.json the build needs must come from this build's restore.
    # (go2cs-gen embeds the converter's field-access collision table, so that one converter file comes along.)
    $runtime = Join-Path $work 'runtime'
    New-Item -ItemType Directory -Path $runtime | Out-Null
    $archive = Join-Path $work 'runtime.tar'
    & git -C $Repository archive --format=tar -o $archive HEAD src/core src/gen src/go2cs/fieldAccessCollisions.txt src/Directory.Build.props src/Directory.Build.targets src/version.props src/deploy-core.ps1 src/_paths.ps1
    if ($LASTEXITCODE -ne 0) { throw "git archive of $Repository exited $LASTEXITCODE" }
    & tar -xf $archive -C $runtime
    if ($LASTEXITCODE -ne 0) { throw "extracting the runtime root exited $LASTEXITCODE" }
    Remove-Item $archive
    $runtimeRoot = Join-Path $runtime 'src'

    if (-not $Converter) {
        $Converter = Join-Path $work ('go2cs' + $(if ($IsWindows) { '.exe' } else { '' }))
        Push-Location (Join-Path $Repository 'src/go2cs')
        try { & go build -o $Converter . } finally { Pop-Location }
        if ($LASTEXITCODE -ne 0) { throw "building the converter exited $LASTEXITCODE" }
    }

    $module = Join-Path $work 'module'
    New-Item -ItemType Directory -Path $module | Out-Null
    Copy-Item (Join-Path $PSScriptRoot 'go.mod'), (Join-Path $PSScriptRoot 'main.go') $module

    Push-Location $module
    try {
        $expected = (& go run -tags safe,purego,noasm . 2>&1 | ForEach-Object { "$_" }) -join "`n"
        if ($LASTEXITCODE -ne 0) { throw "go run of the fixture exited $LASTEXITCODE" }
        $converted = Join-Path $work 'converted'
        $code = Invoke-Logged (Join-Path $work 'convert.log') { & $Converter -recurse -go2cspath $runtimeRoot . $converted }
    } finally { Pop-Location }

    $app = Join-Path $converted 'src/example.com/localrefsplatform'
    if ($code -ne 0 -or -not (Test-Path $app)) {
        $verdicts += "FAIL (DEFAULT): go2cs -recurse exited $code$(Get-FirstError (Join-Path $work 'convert.log'))"
    } else {
        Push-Location $app
        try {
            # DEFAULT: no flags at all, the build and then the run.
            $code = Invoke-Logged (Join-Path $work 'build.log') { & dotnet build -c Debug -nologo -v:m }
            if ($code -ne 0) {
                $verdicts += "FAIL (DEFAULT): dotnet build exited $code$(Get-FirstError (Join-Path $work 'build.log'))"
            } else {
                $actual = (& dotnet run -c Debug --no-build 2>&1 | ForEach-Object { "$_" }) -join "`n"
                $runCode = $LASTEXITCODE
                if ($runCode -ne 0 -or $actual -ne $expected) {
                    $verdicts += "FAIL (DEFAULT): dotnet run exited $runCode, printed '$actual', want '$expected'"
                } else { $verdicts += "PASS (DEFAULT): no flags, built from a clean root and printed what go run prints ($hostOS)" }
            }

            # OVERRIDE: a global property beats the default and reaches the referenced runtime projects.
            $code = Invoke-Logged (Join-Path $work 'override.log') { & dotnet build -c Debug -nologo -v:m -p:GoTargetArch=riscv64 }
            $refused = Select-String -Path (Join-Path $work 'override.log') -SimpleMatch "$hostOS/riscv64 is not a target" -Quiet
            if ($code -ne 0 -and $refused) { $verdicts += "PASS (OVERRIDE): -p:GoTargetArch=riscv64 reached the runtime, refused as $hostOS/riscv64" }
            else { $verdicts += "FAIL (OVERRIDE): build exited $code; the layout check did not refuse $hostOS/riscv64" }
        } finally { Pop-Location }
    }

    # IN-PLACE: the deployed shape. deploy-core stages the same clean runtime root and writes its own props; the
    # conversion then runs with the deploy root as its only root.
    $deploy = Join-Path $work 'deploy'
    $code = Invoke-Logged (Join-Path $work 'deploy.log') { & pwsh -NoProfile -File (Join-Path $runtimeRoot 'deploy-core.ps1') -Target $deploy -NoBuild }
    if ($code -ne 0) {
        $verdicts += "FAIL (IN-PLACE): deploy-core exited $code$(Get-FirstError (Join-Path $work 'deploy.log'))"
    } else {
        Push-Location $module
        try { $code = Invoke-Logged (Join-Path $work 'convert-inplace.log') { & $Converter -recurse -go2cspath $deploy . } }
        finally { Pop-Location }
        $inPlaceApp = Join-Path $deploy 'src/example.com/localrefsplatform'
        if ($code -ne 0 -or -not (Test-Path $inPlaceApp)) {
            $verdicts += "FAIL (IN-PLACE): go2cs -recurse exited $code$(Get-FirstError (Join-Path $work 'convert-inplace.log'))"
        } else {
            Push-Location $inPlaceApp
            try {
                $hostArch = (& go env GOARCH).Trim()
                $platform = @("-p:GoTargetOS=$hostOS", "-p:GoTargetArch=$hostArch")
                $code = Invoke-Logged (Join-Path $work 'build-inplace.log') { & dotnet build -c Debug -nologo -v:m @platform }
                if ($code -ne 0) {
                    $verdicts += "FAIL (IN-PLACE): dotnet build exited $code$(Get-FirstError (Join-Path $work 'build-inplace.log'))"
                } else {
                    $actual = (& dotnet run -c Debug --no-build @platform 2>&1 | ForEach-Object { "$_" }) -join "`n"
                    $runCode = $LASTEXITCODE
                    if ($runCode -ne 0 -or $actual -ne $expected) {
                        $verdicts += "FAIL (IN-PLACE): dotnet run exited $runCode, printed '$actual', want '$expected'"
                    } else { $verdicts += "PASS (IN-PLACE): converted inside a deploy-core root, built and printed what go run prints ($hostOS/$hostArch)" }
                }
            } finally { Pop-Location }
        }
    }
} finally {
    $verdicts | ForEach-Object { Write-Host $_ }
    if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
    else { Write-Host "work directory kept: $work" }
}

if ($verdicts.Count -eq 0 -or @($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
