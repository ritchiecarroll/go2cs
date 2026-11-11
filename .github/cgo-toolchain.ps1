<#
.SYNOPSIS
    Provision one pinned cgo C toolchain on a hosted runner: download its archive, refuse it unless its sha256 is the
    pin's, unpack it, and return the compiler command.

.DESCRIPTION
    The one pin table for every os-matrix step that needs a C compiler (PLAN-cgo-interop §5.1): converter-cgo's ;cc=
    option, and golib-tests' cgo probe library (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.10, increment 4d), which
    builds with the OS's DEFAULT toolchain -- zig on linux and darwin, llvm-mingw on windows (the owner's ruling,
    PLAN §5.1, batch 284). A pin is bumped HERE, once.

    One archive per toolchain and runner, with the sha256 its publisher states: zig's from ziglang.org's download index
    (whose tarballs minisign signs), llvm-mingw's from its GitHub release's asset digest. Both releases were at least
    two weeks old when pinned. llvm-mingw's compilers target windows only, so it has a windows pin only.

    A refusal writes the step summary, deletes the archive and throws, so no later step runs an unverified compiler.
    Only a verified archive is unpacked. zig keeps a cache of the C runtime it builds: the script points it beside the
    toolchain, inside the job, for this process and, with -ExportEnv, for the job's later steps (which also get CC).

.OUTPUTS
    An object with Version, Exe (the compiler's path), Args (its leading arguments, "cc" for zig) and Cc (the two joined,
    as go reads CC).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateSet('zig', 'llvm-mingw')] [string] $Cc,
    [Parameter(Mandatory)] [string] $Rid,
    # Replaces the pin's sha256 (converter-cgo's ;sha= option).
    [string] $Sha,
    # The step's name, for the summary's title and the refusal's message.
    [Parameter(Mandatory)] [string] $Label,
    # Also hand the job's later steps CC and zig's cache (converter-cgo, whose tests step runs go with CC).
    [switch] $ExportEnv
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$zig = 'https://ziglang.org/download/0.17.0'
$mingw = 'https://github.com/mstorsjo/llvm-mingw/releases/download/20261020'
$pins = @{
    'zig|win-x64'        = @{ version = 'zig 0.17.0'; url = "$zig/zig-x86_64-windows-0.17.0.zip"; sha256 = 'b5663f69581dcf391293fbf16c06cb80d81d806545ce618b4d0bab7f0eb8c428'; cc = @('zig.exe', 'cc') }
    'zig|linux-x64'      = @{ version = 'zig 0.17.0'; url = "$zig/zig-x86_64-linux-0.17.0.tar.xz"; sha256 = '1cbe9df9f27e6b78d14ccbca43b6703a404ef79ef1c463de901d7f088d4e2026'; cc = @('zig', 'cc') }
    'zig|osx-x64'        = @{ version = 'zig 0.17.0'; url = "$zig/zig-x86_64-macos-0.17.0.tar.xz"; sha256 = '4f9a1c5269aa17ebda5e6d3c2b89d6cbf36f7d2b22a0306e9ab98f25f95529c6'; cc = @('zig', 'cc') }
    'zig|osx-arm64'      = @{ version = 'zig 0.17.0'; url = "$zig/zig-aarch64-macos-0.17.0.tar.xz"; sha256 = 'b607e9b9234790a008116ae5bdb71c6243b84b9fb42a53a9e70fde41c06c536a'; cc = @('zig', 'cc') }
    'llvm-mingw|win-x64' = @{ version = 'llvm-mingw 20261020 (ucrt)'; url = "$mingw/llvm-mingw-20261020-ucrt-x86_64.zip"; sha256 = 'fd5462b7f5d93b2ff981fcd3f9c7e427e47f3203c64ea91c27bcc444eebd74cd'; cc = @('bin/x86_64-w64-mingw32-clang.exe') }
}

$key = "$Cc|$Rid"
if (-not $pins.ContainsKey($key)) { throw "${Label}: no $Cc pin for runner $Rid." }
$pin = $pins[$key]
$expected = if ($Sha) { $Sha } else { $pin.sha256 }
$hashSource = if ($Sha) { 'REPLACED by the plan''s sha= option' } else { 'the pin' }

$root = Join-Path $env:RUNNER_TEMP "cgo-cc-$Cc"
New-Item -ItemType Directory -Force -Path $root | Out-Null
$archive = Join-Path $root ([IO.Path]::GetFileName($pin.url))

$clock = [Diagnostics.Stopwatch]::StartNew()
Invoke-WebRequest -Uri $pin.url -OutFile $archive
$downloadSeconds = [math]::Round($clock.Elapsed.TotalSeconds, 1)
$bytes = (Get-Item -LiteralPath $archive).Length
$actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $archive).Hash.ToLowerInvariant()
$title = "$Label toolchain $Rid"

if ($actual -ne $expected) {
    Remove-Item -LiteralPath $archive -Force
    $summary = @(
        "## $Label toolchain -- $Cc REFUSED on $Rid"
        ''
        "$($pin.version): $($pin.url)"
        "$bytes bytes in $downloadSeconds s; sha256 $actual, want $expected ($hashSource)."
        'Nothing was unpacked and no compiler was handed on.'
    )
    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $summary
    & (Join-Path $PSScriptRoot 'annotate-summary.ps1') -Title $title -Lines $summary
    throw "${Label}: REFUSED $($pin.url): sha256 $actual, want $expected ($hashSource)."
}

# Unpacked only now. Each archive holds one top-level directory.
$clock.Restart()
if ($archive.EndsWith('.zip')) { & tar -xf $archive -C $root } else { & tar -xJf $archive -C $root }
if ($LASTEXITCODE -ne 0) { throw "${Label}: could not unpack $archive (tar exit $LASTEXITCODE)." }
$unpackSeconds = [math]::Round($clock.Elapsed.TotalSeconds, 1)
$unpacked = @(Get-ChildItem -LiteralPath $root -Directory | Where-Object Name -ne 'zig-cache')
if ($unpacked.Count -ne 1) { throw "${Label}: $archive unpacked to $($unpacked.Count) directories, want one." }

$exe = Join-Path $unpacked[0].FullName $pin.cc[0]
if (-not (Test-Path -LiteralPath $exe)) { throw "${Label}: $exe is not in the unpacked $($pin.version)." }
$leading = @($pin.cc | Select-Object -Skip 1)
$command = (@($exe) + $leading) -join ' '

$zigCache = Join-Path $root 'zig-cache'
$env:ZIG_GLOBAL_CACHE_DIR = $zigCache
$env:ZIG_LOCAL_CACHE_DIR = $zigCache

if ($ExportEnv) {
    Add-Content -Path $env:GITHUB_ENV -Value "CC=$command" -Encoding utf8
    Add-Content -Path $env:GITHUB_ENV -Value "ZIG_GLOBAL_CACHE_DIR=$zigCache" -Encoding utf8
    Add-Content -Path $env:GITHUB_ENV -Value "ZIG_LOCAL_CACHE_DIR=$zigCache" -Encoding utf8
}

$summary = @(
    "## $Label toolchain -- $Cc on $Rid"
    ''
    "$($pin.version): $($pin.url)"
    "READING download $bytes bytes in $downloadSeconds s; sha256 $actual = $hashSource; unpacked in $unpackSeconds s"
    "CC = $command"
)
Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $summary
& (Join-Path $PSScriptRoot 'annotate-summary.ps1') -Title $title -Lines $summary

[pscustomobject]@{ Version = $pin.version; Exe = $exe; Args = $leading; Cc = $command }
