<#
.SYNOPSIS
    What a nugetgo module pack's self-description lists (docs/PLAN-nugetgo.md section 5).
#>

function Get-NugetgoPackedPackages {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RecurseRoot, [Parameter(Mandatory)][object[]]$Libraries)
    throw 'not implemented'
}

function Get-NugetgoThirdPartyRequires {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RecurseRoot, [Parameter(Mandatory)][object[]]$Libraries, [string[]]$ThirdPartyPackage = @())
    throw 'not implemented'
}

Export-ModuleMember -Function Get-NugetgoPackedPackages, Get-NugetgoThirdPartyRequires
