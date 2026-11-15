<#
.SYNOPSIS
    Build the cgo probe library on a hosted runner, with the OS's pinned default C toolchain, and hand its directory to
    the job's later steps.

.DESCRIPTION
    The one copy of the probe build that every os-matrix job which links or loads the probe runs: the run job's
    golib-tests, converter-cgo and cgo-e2e stages, and release-smoke's cgo job (release-smoke-cgo), so a pin or a
    load-name rule changes in one place. Moved here VERBATIM from the run job's step "GolibTests -- build the cgo probe
    library"; only the step's own label became the -Label parameter, and its default is that step's label, so those
    stages print what they printed before.

    The toolchain is .github/cgo-toolchain.ps1's pin for this runner (zig on linux and darwin, llvm-mingw on windows).
    It builds src/tests/GolibTests/cgo-probe/probe.c into the load-time name each OS's capture binds, writes
    GO2CS_CGO_PROBE_DIR and GO2CS_REQUIRE_CGO_PROBE=1 (and, on linux, LD_LIBRARY_PATH) to GITHUB_ENV, and writes the step
    summary. Reads GOOS_TARGET, RID and RUNNER_TEMP from the job's environment; run from the repository root.
#>
[CmdletBinding()]
param(
    # The step's name, for the toolchain's label, the refusal's message and the summary's title.
    [string] $Label = 'golib-tests cgo probe'
)

$ErrorActionPreference = 'Stop'

$goos = $env:GOOS_TARGET
$cc = if ($goos -eq 'windows') { 'llvm-mingw' } else { 'zig' }
$toolchain = & ./.github/cgo-toolchain.ps1 -Cc $cc -Rid $env:RID -Label $Label

# darwin's directory is fixed, since the golden binds the probe by its absolute install name there.
$dir = if ($goos -eq 'darwin') { '/tmp/go2cs-cgo-probe' } else { Join-Path $env:RUNNER_TEMP 'cgo-probe' }
New-Item -ItemType Directory -Force -Path $dir | Out-Null

# Each OS's load-time name, which the golden's probe fixture binds (src/go2cs's cgoProbeLibrary): the SONAME on
# linux, the absolute install name on darwin, the DLL's own name on windows.
$probe = @{
    windows = @{ name = 'go2csprobe.dll'; flags = @("-Wl,--out-implib,$dir/libgo2csprobe.dll.a") }
    darwin  = @{ name = 'libgo2csprobe.dylib'; flags = @("-Wl,-install_name,$dir/libgo2csprobe.dylib") }
    linux   = @{ name = 'libgo2csprobe.so'; flags = @('-fPIC', '-Wl,-soname,libgo2csprobe.so') }
}[$goos]
$name = $probe.name
$flags = $probe.flags
$library = Join-Path $dir $name
$source = Join-Path $PWD 'src/tests/GolibTests/cgo-probe/probe.c'

$clock = [Diagnostics.Stopwatch]::StartNew()
& $toolchain.Exe @($toolchain.Args) -shared -O2 @flags -o $library $source
$buildExit = $LASTEXITCODE
$buildSeconds = [math]::Round($clock.Elapsed.TotalSeconds, 1)
if ($buildExit -ne 0 -or -not (Test-Path -LiteralPath $library)) { throw "${Label}: $($toolchain.Version) could not build $name (exit $buildExit)." }

Add-Content -Path $env:GITHUB_ENV -Value "GO2CS_CGO_PROBE_DIR=$dir" -Encoding utf8
Add-Content -Path $env:GITHUB_ENV -Value 'GO2CS_REQUIRE_CGO_PROBE=1' -Encoding utf8

if ($goos -eq 'linux') {
    $path = if ([string]::IsNullOrEmpty($env:LD_LIBRARY_PATH)) { $dir } else { "${dir}:$($env:LD_LIBRARY_PATH)" }
    Add-Content -Path $env:GITHUB_ENV -Value "LD_LIBRARY_PATH=$path" -Encoding utf8
}

$summary = @(
    "## $Label -- $($toolchain.Version) on $($env:RID)"
    ''
    "READING built $name ($((Get-Item -LiteralPath $library).Length) bytes) in $buildSeconds s, into GO2CS_CGO_PROBE_DIR"
)
Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $summary
& ./.github/annotate-summary.ps1 -Title "$Label $($env:RID)" -Lines $summary
