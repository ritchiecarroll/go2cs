<#
.SYNOPSIS
    Publish a fixed set of behavioral projects the way a converted application is published, run the
    PUBLISHED executables, and compare each one's stdout and exit code with the Go program's.

.DESCRIPTION
    Every other behavioral gate BUILDS a project and runs the build output. A converted application is
    shipped by `dotnet publish`, and its project file turns trimming on for a publish, so the published
    program is a different binary from the built one: the linker has removed whatever it could not see
    a reference to. golib and the generated code reach a great deal by reflection, which the linker
    cannot see. Measured 2026-10-02 at the TRAIN M union: with the linker left to trim every assembly,
    two of ten behavioral projects printed WRONG output once published while the same build ran
    correctly (the generated interface wrapper classes had lost every instance field; reflect's method
    sets and type names had lost members). The -tests hosts never trim, so no validated row can see it.

    This leg is the standing gate for that class. It publishes with NO extra properties beyond the
    tree's own $(go2csPath) -- whatever the project file implies is what is measured -- so it goes red
    the day a published program stops matching Go, whatever the cause.

    The set is fixed and small on purpose: a publish costs tens of seconds per project. The first two
    are the projects that were wrong; the others are controls that were right and must stay right.

.PARAMETER Projects
    Behavioral project names to publish. Defaults to the fixed set.

.PARAMETER KeepOutput
    Leave the published directories in place (under the system temp directory) instead of deleting them.

.PARAMETER PublishArgs
    Extra arguments handed to `dotnet publish`, for measuring an alternative by hand
    (e.g. -PublishArgs '-p:TrimMode=full'). The gate itself passes none.

.EXAMPLE
    ./check-published-output.ps1
    ./check-published-output.ps1 -Projects DeepEqual -KeepOutput

.NOTES
    Exit code: 0 when every project's published output and exit code equal Go's; 1 otherwise, each
    failing project named with the first differing line. A publish or `go run` that fails is reported
    as NOT MEASURED and fails the leg: an unmeasured project must not read as a pass.
#>
[CmdletBinding()]
param(
    [string[]] $Projects = @(
        'InterfaceAssertionMapKey',   # was wrong published: a map keyed by an interface missed every lookup
        'ReflectFieldMetadata',       # was wrong published: field indices, a promoted method, type names
        'DeepEqual',                  # control
        'ZeroSizeFieldLayout'         # control
    ),
    [switch] $KeepOutput,
    [string[]] $PublishArgs = @()
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '_paths.ps1')

$architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
$rid = if ($IsWindowsHost) { "win-$architecture" } elseif ($IsMacOS) { "osx-$architecture" } else { "linux-$architecture" }
$go2csPath = ($SrcRoot -replace '\\', '/').TrimEnd('/') + '/'
$workRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("go2cs-published-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))

function Read-Lines([string] $text) {
    return @(($text -replace "`r", '').TrimEnd("`n") -split "`n")
}

$failures = [System.Collections.Generic.List[string]]::new()
$measured = 0

Write-Host "published-output gate: $($Projects.Count) project(s), runtime $rid, go2csPath $go2csPath"

foreach ($name in $Projects) {
    $projectDir = Join-Path $BehavioralRoot $name
    $project = Join-Path $projectDir "$name.csproj"

    if (-not (Test-Path $project)) {
        $failures.Add("$name NOT MEASURED: no project at $project")
        continue
    }

    Push-Location $projectDir
    try {
        $goOutput = (& go run . 2>$null | Out-String)
        $goExit = $LASTEXITCODE
    }
    finally { Pop-Location }

    $out = Join-Path $workRoot $name
    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    $publishLog = (& dotnet publish $project -c Release -r $rid "-p:go2csPath=$go2csPath" @PublishArgs -o $out 2>&1 | Out-String)
    $publishExit = $LASTEXITCODE
    $watch.Stop()

    if ($publishExit -ne 0) {
        $firstError = (Read-Lines $publishLog | Where-Object { $_ -match ': error ' } | Select-Object -First 1)
        $failures.Add("$name NOT MEASURED: dotnet publish exited $publishExit ($firstError)")
        continue
    }

    $exe = Join-Path $out "$name$ExeSuffix"

    if (-not (Test-Path $exe)) {
        $failures.Add("$name NOT MEASURED: the publish produced no $name$ExeSuffix")
        continue
    }

    $publishedOutput = (& $exe 2>$null | Out-String)
    $publishedExit = $LASTEXITCODE
    $measured++

    $goLines = Read-Lines $goOutput
    $publishedLines = Read-Lines $publishedOutput
    $same = ($goExit -eq $publishedExit) -and ($goLines.Count -eq $publishedLines.Count)
    $firstDiff = $null

    for ($i = 0; $i -lt [Math]::Max($goLines.Count, $publishedLines.Count); $i++) {
        $want = if ($i -lt $goLines.Count) { $goLines[$i] } else { '<no line>' }
        $got = if ($i -lt $publishedLines.Count) { $publishedLines[$i] } else { '<no line>' }

        if ($want -cne $got) {
            $same = $false
            $firstDiff = "line $($i + 1): go `"$want`" published `"$got`""
            break
        }
    }

    $size = [math]::Round(((Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum) / 1MB, 1)
    $verdict = if ($same) { 'same as go' } else { 'DIFFERS' }
    Write-Host ('  {0,-28} {1,-11} publish {2,5:N0}s  {3,6:N1} MB  exit go {4} published {5}' -f $name, $verdict, $watch.Elapsed.TotalSeconds, $size, $goExit, $publishedExit)

    if (-not $same) {
        $detail = if ($firstDiff) { $firstDiff } else { "exit code go $goExit published $publishedExit" }
        $failures.Add("$name DIFFERS once published: $detail")
    }
}

if (-not $KeepOutput -and (Test-Path $workRoot)) {
    Remove-Item $workRoot -Recurse -Force -ErrorAction SilentlyContinue
}
elseif ($KeepOutput) {
    Write-Host "published output kept under $workRoot"
}

Write-Host ''

if ($failures.Count -gt 0) {
    Write-Host "published-output gate: FAILED -- $($failures.Count) of $($Projects.Count) project(s), $measured measured" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

Write-Host "published-output gate: $measured of $($Projects.Count) published programs match go"
exit 0
