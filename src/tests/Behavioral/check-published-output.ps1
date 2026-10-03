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

    THE SINGLE-FILE ARM. Every program the converter creates also gets publish profiles
    (Properties/PublishProfiles/<rid>.pubxml) that turn on a self-contained single-file publish. Measured
    2026-10-02: with partial trimming AND ReadyToRun, such a single-file program intermittently never
    finished -- no output, no CPU -- in 5 of 10 runs (10 of 10 on a repeat), while the same program
    published without single-file, or single-file without ReadyToRun, never hung. So one project is also
    published through the converter's own single-file profile, read from src/go2cs/profiles (the files
    the converter writes, independent of whether this tree has transpiled the project), and run
    -SingleFileRuns times. A run that passes -RunTimeoutSeconds FAILS the leg as HUNG.

.PARAMETER Projects
    Behavioral project names to publish. Defaults to the fixed set.

.PARAMETER SingleFileProject
    The project published through the converter's single-file profile. Empty skips the arm.

.PARAMETER SingleFileRuns
    How many times the single-file program is run. The hang is intermittent; one run proves little.

.PARAMETER RunTimeoutSeconds
    Per-run limit for every published program. A run past it is killed and reported HUNG.

.PARAMETER KeepOutput
    Leave the published directories in place (under the system temp directory) instead of deleting them.

.PARAMETER PublishArgs
    Extra arguments handed to `dotnet publish`, for measuring an alternative by hand
    (e.g. -PublishArgs '-p:TrimMode=full'). The gate itself passes none.

.EXAMPLE
    ./check-published-output.ps1
    ./check-published-output.ps1 -Projects DeepEqual -SingleFileProject '' -KeepOutput

.NOTES
    Exit code: 0 when every published program's output and exit code equal Go's and no run hangs; 1
    otherwise, each failure named. A publish or `go run` that fails is reported as NOT MEASURED and fails
    the leg: an unmeasured project must not read as a pass. A run past the limit is HUNG, never NOT
    MEASURED.
#>
[CmdletBinding()]
param(
    [string[]] $Projects = @(
        'InterfaceAssertionMapKey',   # was wrong published: a map keyed by an interface missed every lookup
        'ReflectFieldMetadata',       # was wrong published: field indices, a promoted method, type names
        'DeepEqual',                  # control
        'ZeroSizeFieldLayout'         # control
    ),
    [string] $SingleFileProject = 'InterfaceAssertionMapKey',
    [int] $SingleFileRuns = 10,
    [string] $OneCpuProject = 'InterfaceAssertionMapKey',
    [int] $OneCpuRuns = 20,
    [int] $RunTimeoutSeconds = 60,
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

# Runs a program with a time limit: its stdout, its exit code, and whether it had to be killed. With
# -OneCpu the program runs on a single CPU: on windows it inherits this process's affinity, set to CPU 0
# just for the start; elsewhere it starts under `taskset -c 0`.
function Invoke-Limited([string] $exe, [int] $seconds, [switch] $OneCpu) {
    $info = if ($OneCpu -and -not $IsWindowsHost) { [System.Diagnostics.ProcessStartInfo]::new('taskset', "-c 0 `"$exe`"") } else { [System.Diagnostics.ProcessStartInfo]::new($exe) }
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.UseShellExecute = $false
    $self = [System.Diagnostics.Process]::GetCurrentProcess()
    $savedAffinity = $self.ProcessorAffinity

    if ($OneCpu -and $IsWindowsHost) {
        $self.ProcessorAffinity = [IntPtr]1
    }

    try {
        $process = [System.Diagnostics.Process]::Start($info)
    }
    finally {
        if ($OneCpu -and $IsWindowsHost) {
            $self.ProcessorAffinity = $savedAffinity
        }
    }
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $null = $process.StandardError.ReadToEndAsync()

    if (-not $process.WaitForExit($seconds * 1000)) {
        $process.Kill($true)
        $process.WaitForExit()
        return [pscustomobject]@{ Output = ''; ExitCode = -1; Hung = $true }
    }

    return [pscustomobject]@{ Output = $stdout.Result; ExitCode = $process.ExitCode; Hung = $false }
}

# The first line where two outputs differ, or $null when they are the same.
function Compare-Output([string] $goOutput, [string] $publishedOutput) {
    $goLines = Read-Lines $goOutput
    $publishedLines = Read-Lines $publishedOutput

    for ($i = 0; $i -lt [Math]::Max($goLines.Count, $publishedLines.Count); $i++) {
        $want = if ($i -lt $goLines.Count) { $goLines[$i] } else { '<no line>' }
        $got = if ($i -lt $publishedLines.Count) { $publishedLines[$i] } else { '<no line>' }

        if ($want -cne $got) {
            return "line $($i + 1): go `"$want`" published `"$got`""
        }
    }

    return $null
}

# Removes a project's Release build output before a publish, so the publish measures what the project
# implies and not an earlier publish's state. Measured 2026-10-02: an ordinary publish over intermediate
# output left by a single-file publish of the same project produced a 55 MB program that crashed at
# start (exit 0xC0000602) where a clean publish produced the 67.5 MB program that runs.
function Clear-PublishState([string] $projectDir) {
    foreach ($folder in @('bin/Release', 'obj/Release')) {
        $path = Join-Path $projectDir $folder

        if (Test-Path $path) {
            Remove-Item $path -Recurse -Force
        }
    }
}

function Get-GoOutput([string] $projectDir) {
    Push-Location $projectDir
    try {
        $output = (& go run . 2>$null | Out-String)
        return [pscustomobject]@{ Output = $output; ExitCode = $LASTEXITCODE }
    }
    finally { Pop-Location }
}

$failures = [System.Collections.Generic.List[string]]::new()
$measured = 0
$expected = $Projects.Count + $(if ($SingleFileProject) { 1 } else { 0 }) + $(if ($OneCpuProject) { 1 } else { 0 })

Write-Host "published-output gate: $($Projects.Count) project(s) + single-file arm '$SingleFileProject', runtime $rid, go2csPath $go2csPath, run limit ${RunTimeoutSeconds}s"

foreach ($name in $Projects) {
    $projectDir = Join-Path $BehavioralRoot $name
    $project = Join-Path $projectDir "$name.csproj"

    if (-not (Test-Path $project)) {
        $failures.Add("$name NOT MEASURED: no project at $project")
        continue
    }

    $go = Get-GoOutput $projectDir

    Clear-PublishState $projectDir
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

    $run = Invoke-Limited $exe $RunTimeoutSeconds
    $measured++

    $size = [math]::Round(((Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum) / 1MB, 1)

    if ($run.Hung) {
        Write-Host ('  {0,-28} {1,-11} publish {2,5:N0}s  {3,6:N1} MB' -f $name, 'HUNG', $watch.Elapsed.TotalSeconds, $size)
        $failures.Add("$name HUNG once published: no exit within ${RunTimeoutSeconds}s")
        continue
    }

    $firstDiff = Compare-Output $go.Output $run.Output
    $same = ($null -eq $firstDiff) -and ($go.ExitCode -eq $run.ExitCode)
    $verdict = if ($same) { 'same as go' } else { 'DIFFERS' }
    Write-Host ('  {0,-28} {1,-11} publish {2,5:N0}s  {3,6:N1} MB  exit go {4} published {5}' -f $name, $verdict, $watch.Elapsed.TotalSeconds, $size, $go.ExitCode, $run.ExitCode)

    if (-not $same) {
        $detail = if ($firstDiff) { $firstDiff } else { "exit code go $($go.ExitCode) published $($run.ExitCode)" }
        $failures.Add("$name DIFFERS once published: $detail")
    }
}

# THE ONE-CPU ARM. golib's extension-method scan and its AssemblyLoad handler once took one lock in
# opposite order with the runtime's type-load lock; a plain ReadyToRun publish pinned to one CPU hung in 20
# of 23 runs (rooted 2026-10-03), while several CPUs almost never showed it. So one project is published
# plainly (ReadyToRun, partial trim, not single-file) and run -OneCpuRuns times on a single CPU; a run past
# the limit FAILS the leg as HUNG.
if ($OneCpuProject) {
    $name = $OneCpuProject
    $projectDir = Join-Path $BehavioralRoot $name
    $project = Join-Path $projectDir "$name.csproj"

    if (-not (Test-Path $project)) {
        $failures.Add("$name (one CPU) NOT MEASURED: no project at $project")
    }
    else {
        $go = Get-GoOutput $projectDir
        Clear-PublishState $projectDir
        $out = Join-Path $workRoot "$name-one-cpu"
        $watch = [System.Diagnostics.Stopwatch]::StartNew()
        $publishLog = (& dotnet publish $project -c Release -r $rid "-p:go2csPath=$go2csPath" @PublishArgs -o $out 2>&1 | Out-String)
        $publishExit = $LASTEXITCODE
        $watch.Stop()
        $exe = Join-Path $out "$name$ExeSuffix"

        if ($publishExit -ne 0 -or -not (Test-Path $exe)) {
            $firstError = (Read-Lines $publishLog | Where-Object { $_ -match ': error ' } | Select-Object -First 1)
            $failures.Add("$name (one CPU) NOT MEASURED: dotnet publish exited $publishExit ($firstError)")
        }
        else {
            $measured++
            $hung = 0
            $differing = $null

            for ($i = 1; $i -le $OneCpuRuns; $i++) {
                $run = Invoke-Limited $exe $RunTimeoutSeconds -OneCpu

                if ($run.Hung) {
                    $hung++
                    continue
                }

                $firstDiff = Compare-Output $go.Output $run.Output

                if (($null -ne $firstDiff -or $go.ExitCode -ne $run.ExitCode) -and $null -eq $differing) {
                    $differing = "run ${i}: " + $(if ($firstDiff) { $firstDiff } else { "exit code go $($go.ExitCode) published $($run.ExitCode)" })
                }
            }

            $size = [math]::Round(((Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum) / 1MB, 1)
            $verdict = if ($hung -gt 0) { 'HUNG' } elseif ($differing) { 'DIFFERS' } else { 'same as go' }
            Write-Host ('  {0,-28} {1,-11} publish {2,5:N0}s  {3,6:N1} MB  one CPU, {4} runs, {5} hung' -f $name, $verdict, $watch.Elapsed.TotalSeconds, $size, $OneCpuRuns, $hung)

            if ($hung -gt 0) {
                $failures.Add("$name (one CPU) HUNG: $hung of $OneCpuRuns runs did not exit within ${RunTimeoutSeconds}s")
            }

            if ($differing) {
                $failures.Add("$name (one CPU) DIFFERS once published: $differing")
            }
        }
    }
}

if ($SingleFileProject) {
    $name = $SingleFileProject
    $projectDir = Join-Path $BehavioralRoot $name
    $project = Join-Path $projectDir "$name.csproj"
    $profile = Join-Path $ConverterSrc "profiles/$rid.pubxml"

    if (-not (Test-Path $project) -or -not (Test-Path $profile)) {
        $failures.Add("$name (single-file) NOT MEASURED: no project at $project or no converter profile at $profile")
    }
    else {
        $go = Get-GoOutput $projectDir
        Clear-PublishState $projectDir
        $out = Join-Path $workRoot "$name-single-file"
        $watch = [System.Diagnostics.Stopwatch]::StartNew()
        $publishLog = (& dotnet publish $project "-p:PublishProfileFullPath=$profile" "-p:go2csPath=$go2csPath" @PublishArgs -o $out 2>&1 | Out-String)
        $publishExit = $LASTEXITCODE
        $watch.Stop()
        $exe = Join-Path $out "$name$ExeSuffix"

        if ($publishExit -ne 0 -or -not (Test-Path $exe)) {
            $firstError = (Read-Lines $publishLog | Where-Object { $_ -match ': error ' } | Select-Object -First 1)
            $failures.Add("$name (single-file) NOT MEASURED: dotnet publish exited $publishExit ($firstError)")
        }
        else {
            $measured++
            $hung = 0
            $differing = $null

            for ($i = 1; $i -le $SingleFileRuns; $i++) {
                $run = Invoke-Limited $exe $RunTimeoutSeconds

                if ($run.Hung) {
                    $hung++
                    continue
                }

                $firstDiff = Compare-Output $go.Output $run.Output

                if (($null -ne $firstDiff -or $go.ExitCode -ne $run.ExitCode) -and $null -eq $differing) {
                    $differing = "run ${i}: " + $(if ($firstDiff) { $firstDiff } else { "exit code go $($go.ExitCode) published $($run.ExitCode)" })
                }
            }

            $size = [math]::Round(((Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum) / 1MB, 1)
            $verdict = if ($hung -gt 0) { 'HUNG' } elseif ($differing) { 'DIFFERS' } else { 'same as go' }
            Write-Host ('  {0,-28} {1,-11} publish {2,5:N0}s  {3,6:N1} MB  single-file, {4} runs, {5} hung' -f $name, $verdict, $watch.Elapsed.TotalSeconds, $size, $SingleFileRuns, $hung)

            if ($hung -gt 0) {
                $failures.Add("$name (single-file) HUNG: $hung of $SingleFileRuns runs did not exit within ${RunTimeoutSeconds}s")
            }

            if ($differing) {
                $failures.Add("$name (single-file) DIFFERS once published: $differing")
            }
        }
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
    Write-Host "published-output gate: FAILED -- $($failures.Count) failure(s), $measured of $expected measured" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

Write-Host "published-output gate: $measured of $expected published programs match go, none hung"
exit 0
