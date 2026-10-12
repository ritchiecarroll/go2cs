# NugetgoFlavors.psm1 -- RED STUB. Today's nugetgo-pack.ps1 packs every module from one -platforms target into lib/
# alone: no flavor is measured, so a module whose exported API differs by GOOS ships one flavor to every RID. The
# functions below model exactly that, so Test-NugetgoFlavors.ps1 fails by name on each arm the next commit must meet.

function Get-NugetgoRidFlavors { [ordered]@{} }

function Get-NugetgoReferenceGoos { '' }

function Get-NugetgoFlavorDecision {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$ProofGoos)
    [pscustomobject]@{ RidSpecific = $false; Reasons = @() }
}

function Set-NugetgoRidAssets {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Package, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$TargetFramework)
}

function Test-NugetgoRidLayout {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Package, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$TargetFramework)
    @()
}

Export-ModuleMember -Function Get-NugetgoRidFlavors, Get-NugetgoReferenceGoos, Get-NugetgoFlavorDecision, Set-NugetgoRidAssets, Test-NugetgoRidLayout
