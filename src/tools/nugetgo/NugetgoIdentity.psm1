<#
.SYNOPSIS
    The NuGet identity of a go2cs third-party conversion: package ID (owner ruling B2) and package version
    (owner ruling B3), docs/PLAN-nugetgo.md section 8, "OWNER RULINGS 2026-09-30".

.DESCRIPTION
    Get-NugetgoPackageId   module path -> 'nugetgo.' + the dotted module path, every segment kept, validated against
                           nuget.org's ID rule; a hash-shortened alternate when the natural ID fails the rule or
                           collides with an existing ID (case-insensitively, or as a/b.c against a.b/c).
    Get-NugetgoVersion     Go module version + revision -> the package version: the Go version without its v and
                           without +incompatible; a rebuild is X.Y.Z.N for a release and L.0.N for a prerelease or
                           pseudo-version; the refusals (an uppercase prerelease label, an Int32 overflow, more than
                           64 characters) and the @v/list guard before any L.0.N rebuild.

    Both return an object, never a bare string, so a refusal carries its reason instead of an empty value a caller
    could pack by mistake.
#>
Set-StrictMode -Version 3.0

# nuget.org's package ID rule: word characters separated by single '.', '-' or '_', at most 100 characters.
$script:IdPattern = '^\w+([_.-]\w+)*$'
$script:IdMaxLength = 100
$script:IdPrefix = 'nugetgo.'
$script:HashDigits = 8

function Get-NugetgoHash([string]$Text) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Text))
        return -join ($bytes[0..(($script:HashDigits / 2) - 1)] | ForEach-Object { $_.ToString('x2') })
    }
    finally { $sha.Dispose() }
}

# The dotted form of a module path, the key collisions compare: a/b.c and a.b/c both read a.b.c.
function ConvertTo-NugetgoDotted([string]$ModulePath) { $ModulePath.Replace('/', '.') }

function Test-NugetgoIdRule([string]$Id) {
    return $Id.Length -le $script:IdMaxLength -and [regex]::IsMatch($Id, $script:IdPattern)
}

<#
.SYNOPSIS
    The package ID for a Go module (B2). -ExistingIds names IDs already taken (the registry's rows, the stdlib
    IDs), compared case-insensitively.
#>
function Get-NugetgoPackageId {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ModulePath,
        [string[]]$ExistingIds = @()
    )

    if ($ModulePath -notmatch '^[A-Za-z0-9._~/-]+$' -or $ModulePath.StartsWith('/') -or $ModulePath.EndsWith('/') -or $ModulePath.Contains('//')) {
        return [pscustomobject]@{ ModulePath = $ModulePath; Id = $null; Display = $null; Natural = $null; Alternate = $false; Reason = "not a Go module path: '$ModulePath'" }
    }

    $natural = $script:IdPrefix + (ConvertTo-NugetgoDotted $ModulePath)
    $taken = @{}
    foreach ($e in $ExistingIds) { if ($e) { $taken[$e.ToLowerInvariant()] = $e } }

    $why = $null
    if (-not (Test-NugetgoIdRule $natural)) {
        $why = if ($natural.Length -gt $script:IdMaxLength) { "the natural ID is $($natural.Length) characters, over nuget.org's $($script:IdMaxLength)" } else { "the natural ID breaks nuget.org's ID rule ($($script:IdPattern))" }
    }
    elseif ($taken.ContainsKey($natural.ToLowerInvariant())) {
        $why = "the natural ID collides with '$($taken[$natural.ToLowerInvariant()])' (case-insensitive, or a/b.c against a.b/c)"
    }

    if (-not $why) {
        return [pscustomobject]@{ ModulePath = $ModulePath; Id = $natural; Display = $natural; Natural = $natural; Alternate = $false; Reason = $null }
    }

    # The hash-shortened alternate (PROPOSED shape; B2 names the triggers, not the format): the natural ID's valid
    # characters, truncated so that '.' + 8 hex digits of SHA-256(module path) fits in 100, the hash over the EXACT
    # module path so a/b.c and a.b/c get different alternates.
    $hash = Get-NugetgoHash $ModulePath
    $stem = [regex]::Replace($natural, '[^\w.-]', '_')
    $stem = [regex]::Replace($stem, '[_.-]{2,}', '.').Trim('.', '-', '_')
    $room = $script:IdMaxLength - ($script:HashDigits + 1)
    if ($stem.Length -gt $room) { $stem = $stem.Substring(0, $room).TrimEnd('.', '-', '_') }
    $alternate = "$stem.$hash"

    if (-not (Test-NugetgoIdRule $alternate) -or $taken.ContainsKey($alternate.ToLowerInvariant())) {
        return [pscustomobject]@{ ModulePath = $ModulePath; Id = $null; Display = $null; Natural = $natural; Alternate = $true; Reason = "$why, and the hash-shortened alternate '$alternate' is not usable either: refuse and escalate" }
    }
    return [pscustomobject]@{ ModulePath = $ModulePath; Id = $alternate; Display = $alternate; Natural = $natural; Alternate = $true; Reason = $why }
}

<#
.SYNOPSIS
    The package version for a Go module version (B3). -Revision 0 is the first publish; N >= 1 a rebuild. -VList is
    the module's @v/list (every published Go version), required for a prerelease rebuild's guard.
#>
function Get-NugetgoVersion {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$GoVersion,
        [int]$Revision = 0,
        [string[]]$VList
    )

    $refuse = { param($why) [pscustomobject]@{ GoVersion = $GoVersion; Revision = $Revision; Version = $null; Refused = $true; Reason = $why } }

    if ($Revision -lt 0) { return & $refuse "a revision is 0 (the first publish) or a positive rebuild number, not $Revision" }
    if ($GoVersion -notmatch '^v(?<core>(?<maj>\d+)\.(?<min>\d+)\.(?<pat>\d+))(?:-(?<pre>[0-9A-Za-z.-]+))?(?<inc>\+incompatible)?$') {
        return & $refuse "not a Go module version (vX.Y.Z[-prerelease][+incompatible]): '$GoVersion'"
    }
    $core = $Matches['core']
    $pre = $Matches['pre']
    foreach ($part in 'maj', 'min', 'pat') {
        if ([decimal]$Matches[$part] -gt [int]::MaxValue) { return & $refuse "version component $($Matches[$part]) overflows Int32" }
    }
    if ($pre -and $pre -cmatch '[A-Z]') { return & $refuse "the prerelease label '$pre' has uppercase letters" }

    $L = if ($pre) { "$core-$pre" } else { $core }
    $version = $L
    if ($Revision -gt 0) {
        if ($pre) {
            # L.0.N, never a fourth number on a prerelease. The guard: a Go tag whose prerelease starts with L's own
            # prerelease + '.0' could equal this rebuild, so refuse (escalate) if @v/list has one.
            if ($null -eq $VList) { return & $refuse "a prerelease rebuild needs the module's @v/list for the L.0 guard" }
            $guard = "v$core-$pre.0"
            $clash = @($VList | Where-Object { $_ -eq $guard -or $_.StartsWith("$guard.") })
            if ($clash.Count) { return & $refuse "the @v/list has $($clash -join ', '), which an L.0.N rebuild could equal: escalate" }
            $version = "$L.0.$Revision"
        }
        else {
            $version = "$core.$Revision"
        }
    }
    if ($version.Length -gt 64) { return & $refuse "the package version '$version' is $($version.Length) characters, over 64" }
    return [pscustomobject]@{ GoVersion = $GoVersion; Revision = $Revision; Version = $version; Refused = $false; Reason = $null }
}

Export-ModuleMember -Function Get-NugetgoPackageId, Get-NugetgoVersion
