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

Export-ModuleMember -Function Get-NugetgoRestoredClosure, Get-NugetgoPublishedVersions, Test-NugetgoPublishedClosure
