<#
.SYNOPSIS
    The go2cs release a module pack is built against must be a PUBLISHED one (COORD's ruling on the nugetgo rehearsal,
    2026-10-09). The rehearsal packed hashset at a tree whose golib had moved past the last published release: against the
    published go.gen the pack could not compile, and the packed assembly, compiled against the unpublished golib, could not
    load against the go.lib its own dependency range named. So every go.* package the pack restored must be exactly
    -ClosureVersion, and nuget.org must list that version.
#>

<#
.SYNOPSIS
    The go.* packages a restore placed in -PackagesFolder (NuGet's <id>/<version> layout), as id -> versions.
#>
function Get-NugetgoRestoredClosure([string]$PackagesFolder) {
    $restored = @{}
    if (-not (Test-Path -LiteralPath $PackagesFolder)) { return $restored }
    foreach ($package in @(Get-ChildItem -LiteralPath $PackagesFolder -Directory | Where-Object { $_.Name -like 'go.*' })) {
        $restored[$package.Name.ToLowerInvariant()] = @(Get-ChildItem -LiteralPath $package.FullName -Directory | ForEach-Object { $_.Name })
    }
    return $restored
}

<#
.SYNOPSIS
    nuget.org's published versions of a package (its flat container), or an empty list when it publishes none.
#>
function Get-NugetgoPublishedVersions([string]$Id) {
    try {
        $index = Invoke-RestMethod -Uri "https://api.nuget.org/v3-flatcontainer/$($Id.ToLowerInvariant())/index.json" -TimeoutSec 60
        return @($index.versions)
    }
    catch {
        if ($_.Exception.Response -and [int]$_.Exception.Response.StatusCode -eq 404) { return @() }
        throw "could not read nuget.org's published versions of ${Id}: $($_.Exception.Message)"
    }
}

<#
.SYNOPSIS
    The reasons a restored closure is not the published release -ClosureVersion; empty when it is. -PublishedVersions
    answers an id's published versions (Get-NugetgoPublishedVersions in the pack, a table in the tests).
#>
function Test-NugetgoPublishedClosure([hashtable]$Restored, [string]$ClosureVersion, [scriptblock]$PublishedVersions) {
    $reasons = New-Object System.Collections.Generic.List[string]
    foreach ($required in @('go.lib', 'go.gen')) {
        if (-not $Restored.ContainsKey($required) -or @($Restored[$required]).Count -eq 0) { $reasons.Add("no $required was restored") }
    }
    foreach ($id in @($Restored.Keys | Sort-Object)) {
        $published = @(& $PublishedVersions $id)
        foreach ($version in @($Restored[$id] | Sort-Object)) {
            if (-not @($published | Where-Object { $_ -ieq $version }).Count) {
                $reasons.Add("$id $version is not a published release (nuget.org lists $(if ($published.Count) { @($published)[-1] + ' last' } else { 'no version' }))")
            }
            elseif ($version -ine $ClosureVersion) {
                $reasons.Add("$id $version is not -ClosureVersion $ClosureVersion")
            }
        }
    }
    return $reasons.ToArray()
}

# ---- a LOCAL go.* closure (nugetgo-pack -LocalClosure; cgo against go.* INC 4, COORD's ruling (1), 2026-11-22) ---------
# Before .5 no published go.lib declares A6, so no -Cgo pack can be made against a published release. A rehearsal may be
# packed against go.* packages built from the tree, in a local folder feed. It skips the published-closure check above and
# instead requires every restored go.* package to be -ClosureVersion and byte-equal to that folder's; its self-description
# records "local-closure: ", which src/_nugetpush.ps1 refuses.

<#
.SYNOPSIS
    The nuget.config a pack restores with: -Feed alone, or with -LocalClosure, that folder too, and package source mapping
    that sends every go.* package to it and nothing else. Without -LocalClosure it is the same text the pack wrote as a
    here-string before, joined with LF where the here-string took the script's line endings.
#>
function Get-NugetgoRestoreConfig([Parameter(Mandatory)][string]$Feed, [string]$LocalClosure) {
    $esc = { param($s) [System.Security.SecurityElement]::Escape($s) }
    $lines = @('<?xml version="1.0" encoding="utf-8"?>', '<configuration>', '  <packageSources>', '    <clear />')
    if ($LocalClosure) { $lines += "    <add key=`"nugetgo-local-closure`" value=`"$(& $esc $LocalClosure)`" />" }
    $lines += @("    <add key=`"nugetgo-closure`" value=`"$(& $esc $Feed)`" />", '  </packageSources>')
    if ($LocalClosure) {
        $lines += @('  <packageSourceMapping>',
            '    <packageSource key="nugetgo-local-closure">', '      <package pattern="go.*" />', '    </packageSource>',
            '    <packageSource key="nugetgo-closure">', '      <package pattern="*" />', '    </packageSource>',
            '  </packageSourceMapping>')
    }
    ($lines + '</configuration>') -join "`n"
}

<#
.SYNOPSIS
    The reasons a restored closure is not -LocalClosure's packages at -ClosureVersion; empty when it is. Each restored go.*
    package must be -ClosureVersion, and the restore's own <id>.<version>.nupkg.sha512 must equal the SHA-512 of the
    same-named nupkg in -LocalClosure (searched recursively, so a flat or a hierarchical folder feed both read).
#>
function Test-NugetgoLocalClosure([hashtable]$Restored, [string]$ClosureVersion, [string]$PackagesFolder, [string]$LocalClosure) {
    $reasons = New-Object System.Collections.Generic.List[string]
    foreach ($required in @('go.lib', 'go.gen')) {
        if (-not $Restored.ContainsKey($required) -or @($Restored[$required]).Count -eq 0) { $reasons.Add("no $required was restored") }
    }
    $folder = @{}
    foreach ($file in @(Get-ChildItem -LiteralPath $LocalClosure -Recurse -Filter '*.nupkg' -File -ErrorAction SilentlyContinue)) { $folder[$file.Name.ToLowerInvariant()] = $file.FullName }
    $sha512 = [System.Security.Cryptography.SHA512]::Create()
    try {
        foreach ($id in @($Restored.Keys | Sort-Object)) {
            foreach ($version in @($Restored[$id] | Sort-Object)) {
                if ($version -ine $ClosureVersion) { $reasons.Add("$id $version is not -ClosureVersion $ClosureVersion"); continue }
                $name = "$id.$version.nupkg".ToLowerInvariant()
                if (-not $folder.ContainsKey($name)) { $reasons.Add("$id $version is not in the local closure"); continue }
                $hashFile = Join-Path (Join-Path (Join-Path $PackagesFolder $id) $version) "$name.sha512"
                if (-not (Test-Path -LiteralPath $hashFile -PathType Leaf)) { $reasons.Add("$id $version left no $name.sha512 to compare with the local closure's"); continue }
                $local = [System.Convert]::ToBase64String($sha512.ComputeHash([System.IO.File]::ReadAllBytes($folder[$name])))
                if ([System.IO.File]::ReadAllText($hashFile).Trim() -cne $local) { $reasons.Add("$id $version was restored from another source: its bytes are not the local closure's $name") }
            }
        }
    }
    finally { $sha512.Dispose() }
    return $reasons.ToArray()
}

# The self-description's record that the package was packed against a local closure; src/_nugetpush.ps1 refuses its
# "local-closure: " prefix. It names no folder: a host path never reaches a packed byte.
function Get-NugetgoLocalClosureNote([Parameter(Mandatory)][string]$Version) {
    "local-closure: packed against go.* $Version from a local folder feed, not a published release: a rehearsal, never pushed"
}

Export-ModuleMember -Function Get-NugetgoRestoredClosure, Get-NugetgoPublishedVersions, Test-NugetgoPublishedClosure, Get-NugetgoRestoreConfig, Test-NugetgoLocalClosure, Get-NugetgoLocalClosureNote
