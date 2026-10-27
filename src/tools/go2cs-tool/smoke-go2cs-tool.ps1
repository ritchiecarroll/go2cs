#Requires -Version 7
<#
.SYNOPSIS
    Install the go2cs dotnet tool from a feed and prove that a conversion on a machine with no Go matches one made
    with Go installed, byte for byte.

.DESCRIPTION
    The tool's promise is that it needs no Go: the first conversion downloads the Go release go2cs converts with.
    A broken download would rarely fail outright. The stack-append harvest compiles the package being converted,
    and when that compile fails the conversion goes on without its hints (docs/PLAN-dotnet-tool-installer.md, section 8),
    so the C# changes and go2cs still exits 0. This smoke therefore compares emission, not exit codes:

      1. installs `go2cs` at -Version from -Feed into a tool path;
      2. converts smoke/ with the Go on PATH, which must be the release go.mod names (the reference);
      3. hides every Go (PATH, GOROOT, GO2CS_GOROOT) and points the user cache at an empty folder, converts again,
         and requires the download message and a byte-identical conversion;
      4. converts a third time and requires no download and the same bytes;
      5. requires `go2cs toolchain env` to name the downloaded Go.

    release-binaries.yml runs it on the six platforms' runners against packages pack-go2cs-tool.ps1 made with
    -Unsigned. Exit 0 clean, 1 on any failure.

.EXAMPLE
    ./smoke-go2cs-tool.ps1 -Feed ./feed -Version 1.26.10.2-smoke
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Feed,
    [Parameter(Mandatory)] [string] $Version,
    [string] $Work = (Join-Path ([System.IO.Path]::GetTempPath()) "go2cs-tool-smoke-$PID")
)

$ErrorActionPreference = 'Stop'

function Die([string] $Message) { Write-Host "smoke-go2cs-tool: FAIL: $Message" -ForegroundColor Red; exit 1 }

# Runs a native command and returns its exit code and output lines, stdout and stderr together.
function Invoke-Native([string] $Exe, [string[]] $Arguments) {
    $ErrorActionPreference = 'Continue'
    $out = @(& $Exe @Arguments 2>&1 | ForEach-Object { "$_" })
    [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $out }
}

# Every file under a conversion's output, by relative path, with its SHA-256.
function Get-Emission([string] $Root) {
    $table = [ordered]@{}
    foreach ($file in Get-ChildItem -Recurse -File $Root | Sort-Object FullName) {
        $table[[System.IO.Path]::GetRelativePath($Root, $file.FullName).Replace('\', '/')] = (Get-FileHash $file.FullName -Algorithm SHA256).Hash
    }
    $table
}

function Assert-SameEmission([string] $Reference, [string] $Candidate, [string] $Label) {
    $want = Get-Emission $Reference
    $got = Get-Emission $Candidate
    if ($want.Count -eq 0) { Die "the reference conversion wrote no files" }
    $differ = @(@($want.Keys) + @($got.Keys) | Sort-Object -Unique | Where-Object { $want[$_] -ne $got[$_] })
    if ($differ.Count -gt 0) {
        $differ | Select-Object -First 20 | ForEach-Object { Write-Host "    differs: $_" }
        Die "$Label emitted $($differ.Count) file(s) unlike the conversion made with Go installed"
    }
    Write-Host "smoke-go2cs-tool: $Label is byte-identical to the conversion with Go installed ($($want.Count) files)"
}

$exe = if ($IsWindows) { '.exe' } else { '' }
$smoke = Join-Path $PSScriptRoot 'smoke'
$goMod = Join-Path $PSScriptRoot '../../go2cs/go.mod'
$wantGo = 'go' + ((Select-String -Path $goMod -Pattern '^go\s+(\S+)' | Select-Object -First 1).Matches[0].Groups[1].Value)

New-Item -ItemType Directory -Force -Path $Work | Out-Null
$Work = (Resolve-Path $Work).Path
$tool = Join-Path $Work 'tool'
$go2cs = Join-Path $tool "go2cs$exe"

# ---- 1. install ----------------------------------------------------------------------------------------------------
$install = Invoke-Native 'dotnet' @('tool', 'install', 'go2cs', '--version', $Version, '--add-source', (Resolve-Path $Feed).Path, '--tool-path', $tool)
$install.Output | ForEach-Object { Write-Host "    $_" }
if ($install.ExitCode -ne 0) { Die "dotnet tool install exited $($install.ExitCode)" }

# ---- 2. the reference: Go installed --------------------------------------------------------------------------------
$env:GOTOOLCHAIN = 'local'
$haveGo = (Invoke-Native 'go' @('env', 'GOVERSION')).Output | Select-Object -Last 1
if ($haveGo -ne $wantGo) { Die "the reference needs $wantGo on PATH; found '$haveGo'" }
Remove-Item env:GOTOOLCHAIN

$reference = Invoke-Native $go2cs @($smoke, (Join-Path $Work 'with-go'))
if ($reference.ExitCode -ne 0) { $reference.Output | ForEach-Object { Write-Host "    $_" }; Die "go2cs with Go installed exited $($reference.ExitCode)" }
if ($reference.Output -match 'go2cs: downloading') { Die 'go2cs downloaded a Go although the pinned one was on PATH' }

# ---- 3. no Go anywhere, an empty cache -----------------------------------------------------------------------------
$sep = [System.IO.Path]::PathSeparator
$env:PATH = (($env:PATH -split $sep) | Where-Object { $_ -and -not (Test-Path (Join-Path $_ "go$exe")) }) -join $sep
foreach ($name in 'GOROOT', 'GO2CS_GOROOT', 'GOTOOLCHAIN') { Remove-Item "env:$name" -ErrorAction SilentlyContinue }
if (Get-Command go -ErrorAction SilentlyContinue) { Die 'a go is still on PATH after hiding them' }

# os.UserCacheDir: %LocalAppData% on Windows, $HOME/Library/Caches on macOS, $XDG_CACHE_HOME elsewhere.
$home2 = Join-Path $Work 'home'
New-Item -ItemType Directory -Force -Path $home2 | Out-Null
if ($IsWindows) { $env:LOCALAPPDATA = $home2 } elseif ($IsMacOS) { $env:HOME = $home2 } else { $env:XDG_CACHE_HOME = $home2 }

$first = Invoke-Native $go2cs @($smoke, (Join-Path $Work 'no-go'))
$first.Output | Where-Object { $_ -match 'go2cs: downloading' } | ForEach-Object { Write-Host "    $_" }
if ($first.ExitCode -ne 0) { $first.Output | ForEach-Object { Write-Host "    $_" }; Die "go2cs with no Go exited $($first.ExitCode)" }
if (-not ($first.Output -match 'go2cs: downloading')) { Die 'the first conversion with no Go and an empty cache downloaded nothing' }
Assert-SameEmission (Join-Path $Work 'with-go') (Join-Path $Work 'no-go') 'the first conversion with no Go'

# ---- 4. again, from the cache --------------------------------------------------------------------------------------
$second = Invoke-Native $go2cs @($smoke, (Join-Path $Work 'no-go-again'))
if ($second.ExitCode -ne 0) { $second.Output | ForEach-Object { Write-Host "    $_" }; Die "the second conversion with no Go exited $($second.ExitCode)" }
if ($second.Output -match 'go2cs: downloading') { Die 'the second conversion downloaded again' }
Assert-SameEmission (Join-Path $Work 'with-go') (Join-Path $Work 'no-go-again') 'the second conversion with no Go'

# ---- 5. toolchain env ----------------------------------------------------------------------------------------------
$envReport = Invoke-Native $go2cs @('toolchain', 'env')
$envReport.Output | ForEach-Object { Write-Host "    $_" }
$root = ($envReport.Output | Where-Object { $_ -like 'GOROOT=*' } | Select-Object -First 1) -replace '^GOROOT=', ''
if ($envReport.ExitCode -ne 0 -or -not $root.StartsWith($home2)) { Die "go2cs toolchain env did not name the downloaded Go under $home2" }
if (-not ($envReport.Output -contains "GOVERSION=$wantGo")) { Die "go2cs toolchain env did not name $wantGo" }

Write-Host "smoke-go2cs-tool: PASS: go2cs $Version converts with no Go installed, identically to a conversion with $wantGo"
if ($env:GITHUB_STEP_SUMMARY) {
    Add-Content $env:GITHUB_STEP_SUMMARY "go2cs dotnet tool ``$Version``: installed, converted with no Go (downloaded $wantGo), byte-identical to the conversion with Go installed."
}
exit 0
