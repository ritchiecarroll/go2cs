<#
.SYNOPSIS
    Red-first check for the H5c deletion pass (reconvert-deletions.ps1) on the GOOS-NAMED CHILD PACKAGE case:
    a package directory whose own Go source is gone at the target while a child package named for a GOOS
    remains (internal/runtime/syscall at go1.26.9). Builds a scratch fixture shaped like that tree, runs the
    instrument dry and with -Apply against real GOROOTs, and asserts the outcome. Exit 0 = every assertion
    held; exit 1 = at least one failed (each printed); exit 3 = could not run.

.DESCRIPTION
    Measured on the i9's Go 1.26 recon (docs/phase4/RECON-go1.26-hop.md, F1): at go1.24.13 internal/runtime/syscall
    is a linux-only package in the per-OS layout (core/internal/runtime/syscall/internal.runtime.syscall.csproj
    plus a linux/ folder). At go1.26.9 that package is gone and Go has two packages on its paths instead:
    internal/runtime/syscall/linux and internal/runtime/syscall/windows. The instrument deleted the old linux/
    files but never enumerated the PARENT directory's project file, README and icons, so the stale project
    survived H5c, stayed in go2cs-stdlib.slnx, and its $(GoTargetOS)/*.cs glob compiled the NEW windows
    package's files into the wrong assembly (CS0234 x12 at the 1.26.9 rung 1).

    ARM 1 (residue): the parent's project file, README and icons are residue, and its solution entry is
    removed, while the child package's project and solution entry are KEPT.
    ARM 2 (refusal): a per-OS-layout parent whose own project SURVIVES beside a child directory that is a
    package of its own (named for a GOOS, carrying its own .csproj) is refused by name: its per-OS glob would
    compile the child package. No Go release from 1.24 to 1.27 has that shape, so its fixture is synthetic.

    Needs both GOROOTs installed (the instrument refuses on a release mismatch). Writes only under -Scratch.

.PARAMETER SourceGoRoot
    The OUTGOING release's GOROOT (go1.24.13).
.PARAMETER GoRoot
    The TARGET release's GOROOT (go1.26.9 or later: the release that relocates internal/runtime/syscall).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $SourceGoRoot,
    [Parameter(Mandatory = $true)][string] $GoRoot,
    [string] $ExpectSourceGo = 'go1.24.13',
    [string] $ExpectGo = 'go1.26.9',
    [string] $Scratch = (Join-Path ([System.IO.Path]::GetTempPath()) ('h5c-goos-child-' + [guid]::NewGuid().ToString('N').Substring(0, 8))),
    # The shell the instrument runs under: the runbook's H5c commands use Windows PowerShell. (Not the
    # current process's path: under the pwsh global tool that is the dotnet host, not a shell.)
    [string] $Shell = 'powershell.exe'
)

$ErrorActionPreference = 'Stop'
$instrument = Join-Path $PSScriptRoot 'reconvert-deletions.ps1'
$psExe = $Shell
$failures = New-Object System.Collections.Generic.List[string]

function Check([string] $Name, [bool] $Ok, [string] $Detail = '') {
    if ($Ok) { Write-Host ("  PASS  {0}" -f $Name) }
    else { Write-Host ("  FAIL  {0}  {1}" -f $Name, $Detail) -ForegroundColor Red; $failures.Add($Name) }
}

foreach ($r in @($SourceGoRoot, $GoRoot)) {
    if (-not (Test-Path -LiteralPath (Join-Path $r 'src'))) { Write-Host "not a GOROOT: $r" -ForegroundColor Red; exit 3 }
}
if (-not (Test-Path -LiteralPath (Join-Path $GoRoot 'src/internal/runtime/syscall/windows'))) {
    Write-Host "the target GOROOT has no internal/runtime/syscall/windows: this check needs go1.26 or later" -ForegroundColor Red; exit 3
}

# One fixture tree: SEEDED files carry an old mtime, EMITTED ones a new one, and the sentinel sits between.
function New-Fixture([string] $Root, [switch] $LiveParentWithGoosChild) {
    $core = Join-Path $Root 'src/core'
    $seeded  = (Get-Date).AddHours(-2)
    $emitted = (Get-Date).AddMinutes(-5)
    $files = @()

    function Put([string] $Rel, [string] $Text, [datetime] $When) {
        $p = Join-Path $core $Rel
        New-Item -ItemType Directory -Force -Path (Split-Path $p) | Out-Null
        [System.IO.File]::WriteAllText($p, $Text)
        (Get-Item -LiteralPath $p).LastWriteTime = $When
    }

    if (-not $LiveParentWithGoosChild) {
        # The outgoing package (seeded), as the 1.24.13 corpus holds it.
        Put 'internal/runtime/syscall/internal.runtime.syscall.csproj' '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><Compile Include="$(GoTargetOS)/*.cs" /></ItemGroup></Project>' $seeded
        Put 'internal/runtime/syscall/README.md' 'internal/runtime/syscall' $seeded
        Put 'internal/runtime/syscall/go2cs.ico' 'ico' $seeded
        Put 'internal/runtime/syscall/go2cs.png' 'png' $seeded
        foreach ($f in 'defs_linux.cs', 'defs_linux_amd64.cs', 'syscall_linux.cs', 'package_info.cs') {
            Put ("internal/runtime/syscall/linux/" + $f) ('namespace go; // seeded ' + $f) $seeded
        }
        # The new 1.26 child package (emitted this run).
        Put 'internal/runtime/syscall/windows/internal.runtime.syscall.windows.csproj' '<Project Sdk="Microsoft.NET.Sdk" />' $emitted
        foreach ($f in 'defs_windows.cs', 'defs_windows_amd64.cs', 'syscall_windows.cs', 'package_info.cs') {
            Put ("internal/runtime/syscall/windows/" + $f) ('namespace go; // emitted ' + $f) $emitted
        }
        $slnx = @(
            '<Solution>',
            '  <Project Path="core/internal/runtime/syscall/internal.runtime.syscall.csproj" />',
            '  <Project Path="core/internal/runtime/syscall/windows/internal.runtime.syscall.windows.csproj" />',
            '</Solution>')
    }
    else {
        # ARM 2: internal/cpu is a LIVE package at every release; a child directory named for a GOOS that
        # carries its own project is the shape whose per-OS glob would compile another package.
        Put 'internal/cpu/internal.cpu.csproj' '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><Compile Include="$(GoTargetOS)/*.cs" /></ItemGroup></Project>' $emitted
        Put 'internal/cpu/cpu.cs' 'namespace go; // emitted cpu.cs' $emitted
        Put 'internal/cpu/package_info.cs' 'namespace go; // emitted' $emitted
        Put 'internal/cpu/windows/internal.cpu.windows.csproj' '<Project Sdk="Microsoft.NET.Sdk" />' $emitted
        Put 'internal/cpu/windows/child.cs' 'namespace go; // a different package' $emitted
        $slnx = @('<Solution>', '  <Project Path="core/internal/cpu/internal.cpu.csproj" />', '</Solution>')
    }

    [System.IO.File]::WriteAllText((Join-Path $Root 'src/go2cs-stdlib.slnx'), (($slnx -join "`r`n") + "`r`n"))
    $sentinel = Join-Path $Root 'run.stamp'
    [System.IO.File]::WriteAllText($sentinel, 'sentinel')
    (Get-Item -LiteralPath $sentinel).LastWriteTime = (Get-Date).AddHours(-1)
    return $sentinel
}

function Invoke-Instrument([string] $Root, [string] $Sentinel, [switch] $Apply) {
    $a = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $instrument,
        '-Root', (Join-Path $Root 'src'), '-GoRoot', $GoRoot, '-ExpectGo', $ExpectGo,
        '-SourceGoRoot', $SourceGoRoot, '-ExpectSourceGo', $ExpectSourceGo,
        '-Sentinel', $Sentinel, '-Goos', 'windows', '-Goarch', 'amd64')
    if ($Apply) { $a += '-Apply' }
    $out = & $psExe @a 2>&1 | ForEach-Object { "$_" }
    return [pscustomobject]@{ Exit = $LASTEXITCODE; Text = ($out -join "`n") }
}

New-Item -ItemType Directory -Force -Path $Scratch | Out-Null
Write-Host ("check-h5c-goos-child: scratch {0}" -f $Scratch)
$parent = 'internal/runtime/syscall'

# ARM 1, dry run
$r1 = Join-Path $Scratch 'arm1-dry'
$s1 = New-Fixture -Root $r1
$dry = Invoke-Instrument -Root $r1 -Sentinel $s1
[System.IO.File]::WriteAllText((Join-Path $Scratch 'arm1-dry.log'), $dry.Text)
Write-Host ("ARM 1 dry run: exit {0}" -f $dry.Exit)
Check 'arm1 dry: the instrument runs (exit 0 or 2)' (@(0, 2) -contains $dry.Exit) "exit $($dry.Exit)"
foreach ($f in 'internal.runtime.syscall.csproj', 'README.md', 'go2cs.ico', 'go2cs.png') {
    Check ("arm1 dry: residue names $parent/$f") ($dry.Text -match [regex]::Escape("$parent/$f")) 'not listed'
}
Check 'arm1 dry: the child project is NOT residue' (-not ($dry.Text -match [regex]::Escape("$parent/windows/internal.runtime.syscall.windows.csproj"))) 'listed'

# ARM 1, -Apply on a fresh copy
$r2 = Join-Path $Scratch 'arm1-apply'
$s2 = New-Fixture -Root $r2
$app = Invoke-Instrument -Root $r2 -Sentinel $s2 -Apply
[System.IO.File]::WriteAllText((Join-Path $Scratch 'arm1-apply.log'), $app.Text)
Write-Host ("ARM 1 apply: exit {0}" -f $app.Exit)
$c2 = Join-Path $r2 'src/core'
$slnxAfter = [System.IO.File]::ReadAllText((Join-Path $r2 'src/go2cs-stdlib.slnx'))
Check 'arm1 apply: exit 0' ($app.Exit -eq 0) "exit $($app.Exit)"
Check 'arm1 apply: the stale parent project is gone' (-not (Test-Path (Join-Path $c2 "$parent/internal.runtime.syscall.csproj")))
Check 'arm1 apply: the parent README and icons are gone' (-not (Test-Path (Join-Path $c2 "$parent/README.md")) -and -not (Test-Path (Join-Path $c2 "$parent/go2cs.ico")))
Check 'arm1 apply: the old linux/ folder is gone' (-not (Test-Path (Join-Path $c2 "$parent/linux")))
Check 'arm1 apply: the child project survives' (Test-Path (Join-Path $c2 "$parent/windows/internal.runtime.syscall.windows.csproj"))
Check 'arm1 apply: the child package files survive (4 .cs)' (@(Get-ChildItem (Join-Path $c2 "$parent/windows") -Filter *.cs).Count -eq 4)
Check 'arm1 apply: the parent solution entry is removed' (-not ($slnxAfter -match [regex]::Escape('core/internal/runtime/syscall/internal.runtime.syscall.csproj'))) 'still listed'
Check 'arm1 apply: the child solution entry is KEPT' ($slnxAfter -match [regex]::Escape('core/internal/runtime/syscall/windows/internal.runtime.syscall.windows.csproj')) 'removed'
Check 'arm1 apply: the solution keeps its CRLF terminators' ($slnxAfter.Contains("`r`n"))

# ARM 2, a live parent with a GOOS-named child package: refused by name, nothing removed
$r3 = Join-Path $Scratch 'arm2'
$s3 = New-Fixture -Root $r3 -LiveParentWithGoosChild
$ref = Invoke-Instrument -Root $r3 -Sentinel $s3 -Apply
[System.IO.File]::WriteAllText((Join-Path $Scratch 'arm2.log'), $ref.Text)
Write-Host ("ARM 2 apply: exit {0}" -f $ref.Exit)
Check 'arm2: refused (exit 3)' ($ref.Exit -eq 3) "exit $($ref.Exit)"
Check 'arm2: the refusal names internal/cpu/windows' ($ref.Text -match 'internal/cpu/windows') 'not named'
Check 'arm2: nothing was removed' ((Test-Path (Join-Path $r3 'src/core/internal/cpu/internal.cpu.csproj')) -and (Test-Path (Join-Path $r3 'src/core/internal/cpu/windows/child.cs')))

Write-Host ''
if ($failures.Count -gt 0) {
    Write-Host ("check-h5c-goos-child: {0} FAILED: {1}  (logs under {2})" -f $failures.Count, ($failures -join '; '), $Scratch) -ForegroundColor Red
    exit 1
}
Write-Host ("check-h5c-goos-child: all assertions hold  (logs under {0})" -f $Scratch)
exit 0
