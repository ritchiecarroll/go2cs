<#
.SYNOPSIS
    Prepares the IDE-mode spike samples for the desktop checks in CHECKLIST.md.

.DESCRIPTION
    From a clone of the spike branch, this script:
      1. builds go2cs from src/go2cs into a temporary root;
      2. converts samples/stepping and samples/caller, each into its OWN output root;
      3. writes #line-mapped copies of the converted files with tools/linedirect.py
         (--mode hidden --skip-comment-records), the design the spike chose (option b);
      4. makes the generated projects compile those copies in place of the converted files, for
         every build (command line, VS Code, Visual Studio, Rider), and builds them in Debug;
      5. writes .vscode/launch.json, tasks.json and settings.json beside each sample;
      6. prints the paths to open in VS Code, Visual Studio and Rider.

    Runs under PowerShell 7 and Windows PowerShell 5.1. Needs Go 1.24, the .NET 10 SDK and Python 3,
    and nuget.org access for the first restore.

.PARAMETER Sample
    stepping, caller, or all (the default).

.PARAMETER Root
    Where the converter and the converted output go. Defaults to <temp>/go2cs-ide-spike.

.EXAMPLE
    pwsh -NoProfile -File docs/phase4/spike-ide-mode/desktop/prepare.ps1
#>
param(
    [ValidateSet('all', 'stepping', 'caller')]
    [string] $Sample = 'all',
    [string] $Root = ''
)

$ErrorActionPreference = 'Stop'

function Fail([string] $message) {
    Write-Host ''
    Write-Host "prepare.ps1: $message" -ForegroundColor Red
    exit 1
}

function Step([string] $message) {
    Write-Host "==> $message" -ForegroundColor Cyan
}

function Write-Utf8([string] $path, [string] $text) {
    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($path, $text, $encoding)
}

function Invoke-Checked([string] $what, [scriptblock] $block) {
    & $block
    if ($LASTEXITCODE -ne 0) { Fail "$what failed (exit code $LASTEXITCODE). The output above says why." }
}

$onWindows = ($PSVersionTable.PSEdition -eq 'Desktop') -or ($IsWindows -eq $true)
$exe = ''
if ($onWindows) { $exe = '.exe' }

# --- Locations -------------------------------------------------------------------------------------
$kit = Split-Path -Parent $PSScriptRoot                                  # docs/phase4/spike-ide-mode
$repo = (Resolve-Path (Join-Path $kit '../../..')).Path                   # repository root
$tools = Join-Path $kit 'tools'
$samples = Join-Path $kit 'samples'
if (-not (Test-Path (Join-Path $repo 'src/go2cs/go.mod'))) { Fail "cannot find src/go2cs under $repo. Run this script from a clone of the spike branch." }
if ($Root -eq '') { $Root = Join-Path ([System.IO.Path]::GetTempPath()) 'go2cs-ide-spike' }
New-Item -ItemType Directory -Force -Path $Root | Out-Null
$Root = (Resolve-Path $Root).Path

# --- Prerequisites ---------------------------------------------------------------------------------
Step 'checking prerequisites'
$go = Get-Command go -ErrorAction SilentlyContinue
if ($null -eq $go) { Fail 'Go was not found on PATH. Install Go 1.24 from https://go.dev/dl/ and open a new terminal.' }
$goVersion = (& go env GOVERSION).Trim()
if ($goVersion -notmatch '^go1\.24(\.|$)') { Fail "Go $goVersion was found, but the converted standard library is Go 1.24. Install Go 1.24 (or set GOTOOLCHAIN=go1.24.13) and run again." }

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) { Fail 'dotnet was not found on PATH. Install the .NET 10 SDK from https://dotnet.microsoft.com/download.' }
$sdks = (& dotnet --list-sdks) -join "`n"
if ($sdks -notmatch '(?m)^10\.') { Fail ".NET 10 SDK not found. 'dotnet --list-sdks' shows:`n$sdks" }

$python = $null
foreach ($candidate in @('python3', 'python', 'py')) {
    if ($null -ne (Get-Command $candidate -ErrorAction SilentlyContinue)) {
        $probe = ''
        try { $probe = (& $candidate --version 2>&1) -join ' ' } catch { $probe = '' }
        if ($probe -match 'Python 3\.') { $python = $candidate; break }
    }
}
if ($null -eq $python) { Fail 'Python 3 was not found (tried python3, python and py). Install it from https://www.python.org/downloads/ and open a new terminal.' }
Write-Host "    $goVersion; .NET SDK 10; $((& $python --version 2>&1) -join ' ')"

# GOROOT exactly as Go prints it: a respelled GOROOT misroutes the whole conversion.
$env:GOROOT = (& go env GOROOT).Trim()
$env:GOTOOLCHAIN = 'local'
$env:CGO_ENABLED = '0'
$goos = (& go env GOOS).Trim()
$goarch = (& go env GOARCH).Trim()

# --- 1. Build the converter ------------------------------------------------------------------------
Step "building go2cs into $Root"
$converter = Join-Path $Root ('go2cs' + $exe)
Push-Location (Join-Path $repo 'src/go2cs')
try { Invoke-Checked 'go build' { & go build -o $converter . } } finally { Pop-Location }

# --- 2-5. Each sample ------------------------------------------------------------------------------
$names = @('stepping', 'caller')
if ($Sample -ne 'all') { $names = @($Sample) }
$opened = @()

foreach ($name in $names) {
    $source = Join-Path $samples $name
    $out = Join-Path $Root ('out-' + $name)
    $lines = Join-Path $Root ('lines-' + $name)

    # Each sample converts into its own fresh output root; nothing else writes there.
    Step "converting samples/$name into $out"
    foreach ($dir in @($out, $lines)) {
        if (Test-Path $dir) { Remove-Item -Recurse -Force -LiteralPath $dir }
    }
    New-Item -ItemType Directory -Force -Path $lines | Out-Null
    Push-Location $source
    try { Invoke-Checked 'go2cs' { & $converter -recurse=nuget -platforms "$goos/$goarch" . $out } } finally { Pop-Location }

    # The converted main package: the project whose package_info.cs carries position maps.
    $info = Get-ChildItem -Path (Join-Path $out 'src') -Recurse -Filter 'package_info.cs' |
        Where-Object { (Get-Content -Raw -LiteralPath $_.FullName) -match 'GoPositionMap\(' } | Select-Object -First 1
    if ($null -eq $info) { Fail "no converted package with a position map under $out" }
    $projectDir = $info.DirectoryName
    $project = Get-ChildItem -Path $projectDir -Filter '*.csproj' | Select-Object -First 1
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project.Name)

    Step "writing #line copies into $lines"
    $mapped = Get-ChildItem -Path $projectDir -Filter '*.cs' | Where-Object {
        $_.Name -ne 'package_info.cs' -and ((Get-Content -Raw -LiteralPath $info.FullName) -match ('GoPositionMap\("[^"]*", "' + [regex]::Escape($_.Name) + '"'))
    }
    foreach ($file in $mapped) {
        Invoke-Checked 'linedirect.py' {
            & $python (Join-Path $tools 'linedirect.py') $info.FullName $file.FullName (Join-Path $lines $file.Name) --mode hidden --skip-comment-records
        }
    }

    # Every build of this output root compiles the copies: the converter's Directory.Build.targets
    # imports the swap, so Visual Studio and Rider builds keep the mapping too.
    $targets = Join-Path $out 'Directory.Build.targets'
    $swap = Join-Path $tools 'swap.targets'
    $hook = @"
  <!-- IDE-mode spike: compile the #line copies (prepare.ps1). -->
  <PropertyGroup>
    <GoLineProject>$projectName</GoLineProject>
    <GoLineDir>$lines$([System.IO.Path]::DirectorySeparatorChar)</GoLineDir>
  </PropertyGroup>
  <Import Project="$swap" />
</Project>
"@
    $text = Get-Content -Raw -LiteralPath $targets
    $at = $text.LastIndexOf('</Project>')
    if ($at -lt 0) { Fail "unexpected $targets (no closing </Project>)" }
    Write-Utf8 $targets ($text.Substring(0, $at) + $hook)

    Step "building $projectName (Debug)"
    Invoke-Checked 'dotnet build' { & dotnet build $project.FullName -c Debug -nologo -v:minimal }
    $targetPath = ((& dotnet msbuild $project.FullName -nologo -getProperty:TargetPath -p:Configuration=Debug) -join '').Trim()
    if (-not (Test-Path -LiteralPath $targetPath)) { Fail "the build reported TargetPath '$targetPath', which does not exist" }

    # VS Code: debug configuration and a rebuild task beside the sample.
    $vscode = Join-Path $source '.vscode'
    New-Item -ItemType Directory -Force -Path $vscode | Out-Null
    $launch = [ordered]@{
        version        = '0.2.0'
        configurations = @([ordered]@{
                name               = "go2cs: $name (Debug, #line)"
                type               = 'coreclr'
                request            = 'launch'
                program            = $targetPath
                args               = @()
                cwd                = $source
                console            = 'internalConsole'
                stopAtEntry        = $false
                justMyCode         = $true
                requireExactSource = $true
            })
    }
    Write-Utf8 (Join-Path $vscode 'launch.json') ($launch | ConvertTo-Json -Depth 6)
    $hostExe = (Get-Process -Id $PID).Path
    $tasks = [ordered]@{
        version = '2.0.0'
        tasks   = @([ordered]@{
                label          = "go2cs: reconvert and rebuild $name"
                type           = 'process'
                command        = $hostExe
                args           = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $PSCommandPath, '-Sample', $name, '-Root', $Root)
                problemMatcher = @()
            })
    }
    Write-Utf8 (Join-Path $vscode 'tasks.json') ($tasks | ConvertTo-Json -Depth 6)
    # Without a Go extension, VS Code allows breakpoints only in languages it has a debugger for.
    Write-Utf8 (Join-Path $vscode 'settings.json') ((@{ 'debug.allowBreakpointsEverywhere' = $true }) | ConvertTo-Json)

    $opened += [pscustomobject]@{
        Name     = $name
        Folder   = $source
        GoFile   = (Join-Path $source 'main.go')
        Solution = (Get-ChildItem -Path $projectDir -Filter '*.slnx' | Select-Object -First 1).FullName
        Program  = $targetPath
    }
}

# --- 6. What to open -------------------------------------------------------------------------------
Write-Host ''
Write-Host 'Ready. Open these (CHECKLIST.md says what to do in each):' -ForegroundColor Green
foreach ($item in $opened) {
    Write-Host ''
    Write-Host "  [$($item.Name)]"
    Write-Host "    VS Code, open folder:         $($item.Folder)"
    Write-Host "    Visual Studio / Rider, open:  $($item.Solution)"
    Write-Host "    then open the Go file:        $($item.GoFile)"
    Write-Host "    built program:                $($item.Program)"
}
Write-Host ''
Write-Host "  [E8 checks] put the converter's folder on PATH first:  $Root"
exit 0
