<#
.SYNOPSIS
    The NuGet identity of a go2cs third-party conversion: package ID (owner ruling B2), package version (owner ruling
    B3) and description (B6), docs/PLAN-nugetgo.md section 8, "OWNER RULINGS 2026-09-30" and its 2026-10-02 amendment.

.DESCRIPTION
    Get-NugetgoPackageId   module path -> 'nugetgo.' + the dotted module path, every segment kept, validated against
                           nuget.org's ID rule; a hash-shortened alternate when the natural ID fails the rule or
                           collides with an existing ID (case-insensitively, or as a/b.c against a.b/c).
    Get-NugetgoVersion     Go module version + revision -> the package version: the Go version without its v and
                           without +incompatible; a rebuild is X.Y.Z.N for a release and L.0.N for a prerelease or
                           pseudo-version; the refusals (an uppercase prerelease label, an Int32 overflow, more than
                           64 characters) and the @v/list guard before any L.0.N rebuild.
    Get-NugetgoDescription the package description: the third-party sentence (B6) or the author's form, decided
                           by -UpstreamPublishes cross-checked against the module's and -RepositoryUrl's host/org.

    All three return an object, never a bare string, so a refusal carries its reason instead of an empty value a caller
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

    # The hash-shortened alternate (COORD ruling within B2, 2026-09-30):
    # - STEM: the natural ID with every character outside [A-Za-z0-9_.-] mapped to '-', runs of separators collapsed
    #   to their first, leading and trailing separators trimmed;
    # - HASH: the first 8 lowercase hex digits of SHA-256 over the EXACT module path as UTF-8 (case-sensitive, /vN
    #   included), so a/b.c and a.b/c, or Foo/x and foo/x, take different alternates;
    # - TRUNCATE: the stem is cut at a SEPARATOR BOUNDARY so stem + '.' + hash fits in 100, never leaving a dangling
    #   separator. The module published first keeps the natural ID; the caller passes it in -ExistingIds.
    $hash = Get-NugetgoHash $ModulePath
    $stem = [regex]::Replace($natural, '[^A-Za-z0-9_.-]', '-')
    $stem = [regex]::Replace($stem, '([_.-])[_.-]+', '$1').Trim('.', '-', '_')
    $room = $script:IdMaxLength - ($script:HashDigits + 1)
    if ($stem.Length -gt $room) {
        $cut = $stem.Substring(0, $room + 1)           # one past the room, so a separator AT the boundary counts
        $at = $cut.LastIndexOfAny([char[]]'._-')
        $stem = if ($at -gt 0) { $cut.Substring(0, $at) } else { $stem.Substring(0, $room) }
        $stem = $stem.TrimEnd('.', '-', '_')
    }
    $alternate = "$stem.$hash"

    if (-not (Test-NugetgoIdRule $alternate) -or $taken.ContainsKey($alternate.ToLowerInvariant())) {
        $whyAlt = if ($taken.ContainsKey($alternate.ToLowerInvariant())) { "collides with '$($taken[$alternate.ToLowerInvariant()])'" } else { "breaks nuget.org's ID rule" }
        return [pscustomobject]@{ ModulePath = $ModulePath; Id = $null; Display = $null; Natural = $natural; Alternate = $true; Reason = "$why, and the hash-shortened alternate '$alternate' ${whyAlt}: refused by name, the hash is never extended" }
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

# The hosts whose module paths read host/ORG/repo, so the ORG is the account that owns the repository -- the shape
# PLAN-nugetgo section 2's canonical rule reads. A path on any other host (gopkg.in, a vanity domain) names no org the
# pack can compare; resolving its go-import record would, and is not built (the registry's canonical rule has the same gap).
$script:OrgHosts = @('github.com', 'gitlab.com', 'bitbucket.org')

function Get-NugetgoHostOrg([string]$Path) {
    $parts = @($Path.Trim('/') -split '/')
    if ($parts.Count -lt 2 -or $script:OrgHosts -notcontains $parts[0].ToLowerInvariant() -or -not $parts[1]) { return $null }
    return "$($parts[0].ToLowerInvariant())/$($parts[1].ToLowerInvariant())"
}

function Get-NugetgoDescription {
    <#
    .SYNOPSIS
        The package description: the third-party sentence (B6), or the AUTHOR's form when the module's own
        author publishes (owner ruling, 2026-10-02). Neither carries a "PROOF:" prefix (owner review of the hashset revision-1
        preview, 2026-10-10: unexplained to a nuget.org reader; the registry tier and the Tests badge carry the proof). The fact that decides it is -UpstreamPublishes, CROSS-CHECKED against
        the module path's host/org and -RepositoryUrl's, the URL the registry's canonical rule reads: the switch with the
        same org gives the author's form; neither gives the third-party form; the switch with another org, the same org
        without the switch, or the switch on a path whose host/org the pack cannot compare, is REFUSED by name.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ModulePath,
        [Parameter(Mandatory)][string]$GoVersion,
        [Parameter(Mandatory)][string]$GoRelease,
        [Parameter(Mandatory)][string]$Upstream,
        [Parameter(Mandatory)][string]$RepositoryUrl,
        [switch]$UpstreamPublishes
    )
    $refuse = { param($why) [pscustomobject]@{ Description = $null; Author = $false; Refused = $true; Reason = $why } }
    $moduleOrg = Get-NugetgoHostOrg $ModulePath
    $repositoryOrg = if ($RepositoryUrl -match '^https://(?<rest>.+)$') { Get-NugetgoHostOrg $Matches['rest'] } else { $null }
    $security = "Security: that standard library carries no Go security fixes issued after Go $GoRelease; review before any production use."

    if ($UpstreamPublishes) {
        if (-not $moduleOrg) {
            return & $refuse "-UpstreamPublishes: the pack cannot corroborate authorship for $ModulePath -- its path is not <host>/<org>/... on $($script:OrgHosts -join ', '), so there is no org to compare -RepositoryUrl against (resolving the path's go-import record would; not built)"
        }
        if ($repositoryOrg -ne $moduleOrg) {
            return & $refuse "-UpstreamPublishes: -RepositoryUrl $RepositoryUrl is not under $moduleOrg, the module's own org; the author's conversion-source repository lives there (the registry's canonical rule reads the same URL)"
        }
        return [pscustomobject]@{ Author = $true; Refused = $false; Reason = $null
            Description = "go2cs C# conversion of $ModulePath $GoVersion, published by its author, built on the Go $GoRelease standard library; " +
                "not affiliated with or endorsed by the Go project. $security" }
    }
    if ($moduleOrg -and $repositoryOrg -eq $moduleOrg) {
        return & $refuse "-RepositoryUrl $RepositoryUrl is under $moduleOrg, the module's own org, which is the author's form: pass -UpstreamPublishes, or publish from a repository outside that org"
    }
    return [pscustomobject]@{ Author = $false; Refused = $false; Reason = $null
        Description = "unofficial go2cs C# conversion of $ModulePath $GoVersion, built on the Go $GoRelease standard library; " +
            "not affiliated with or endorsed by $Upstream or the Go project. $security" }
}

Export-ModuleMember -Function Get-NugetgoPackageId, Get-NugetgoVersion, Get-NugetgoDescription
