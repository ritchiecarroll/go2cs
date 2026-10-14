<#
.SYNOPSIS
    Maps one hand-owned file to the upstream Go source it replaces, in each of two releases.
    Dot-sourced by handown-census.ps1 and by src/tests/handown-census-selftest.ps1.
.DESCRIPTION
    src/core/<pkg-path>/[<goos>/]<name>[_impl].cs  ->  <GOROOT>/src/<pkg-path>/<name>.go

    Under layout L3 the GOOS folder is routing, so pkg/linux/x_impl.cs maps to pkg/x.go. But a release
    can hold a REAL package named after a GOOS: Go 1.26 splits internal/runtime/syscall into
    syscall/linux (package linux) and syscall/windows. So each release is resolved on its own:
      the folded path (pkg/x.go) when that release has it,
      else the unfolded path (pkg/linux/x.go) when that release has it,
      else the folded path, absent.
    A row whose two releases resolve to different paths is RELOCATED. A true deletion stays a deletion,
    because the unfolded path is taken only where it exists.

    Resolve-HandOwnUpstream returns an object with:
      From, To      the upstream path (relative to <GOROOT>/src) used for each release
      FromExists, ToExists
      Relocated     From and To differ
#>

function Resolve-HandOwnUpstream([string] $Rel, [string] $FromGoRoot, [string] $ToGoRoot) {
    $goosDirs = @('windows', 'linux', 'darwin')
    $parts = $Rel -split '[\\/]'
    $file = $parts[-1]
    # DROP THE LEAF via Select-Object, never `$x[0..($x.Count - 2)]`. On a SINGLE-element array that
    # range is 0..-1, which PowerShell counts DOWN to @(0, -1) -- and -1 is the LAST index, so the
    # slice returns the one element TWICE instead of nothing. This script sets no StrictMode, so the
    # result is not an error: it is a silently DOUBLED path segment, and the row it mismaps lands in
    # `no-upstream-counterpart`, the largest bucket, looking like an ordinary finding.
    $dirParts = @($parts | Select-Object -SkipLast 1)
    if ($dirParts.Count -gt 0 -and $goosDirs -contains $dirParts[-1]) {
        # layout L3: per-GOOS folder is routing, not package path. `-gt 0` is the right guard HERE
        # only because -SkipLast 1 yields @() on a one-element array; the old slice yielded @(x, x).
        $dirParts = @($dirParts | Select-Object -SkipLast 1)
    }
    $goName = ([System.IO.Path]::GetFileNameWithoutExtension($file) -replace '_impl$', '') + '.go'
    $folded = (@($dirParts) + $goName) -join '/'
    $unfolded = (@($parts | Select-Object -SkipLast 1) + $goName) -join '/'

    $pick = {
        param([string] $GoRoot)
        $src = Join-Path $GoRoot 'src'
        if (Test-Path (Join-Path $src $folded)) { return @($folded, $true) }
        if ($unfolded -ne $folded -and (Test-Path (Join-Path $src $unfolded))) { return @($unfolded, $true) }
        return @($folded, $false)
    }
    $f = & $pick $FromGoRoot
    $t = & $pick $ToGoRoot

    return [pscustomobject]@{
        From       = $f[0]
        To         = $t[0]
        FromExists = $f[1]
        ToExists   = $t[1]
        Relocated  = ($f[0] -ne $t[0])
    }
}
