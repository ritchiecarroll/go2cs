<#
.SYNOPSIS
    push-nuget.ps1's platform-neutral check: does a package's lib/ entry differ MATERIALLY between two GoTargetOS
    flavors, or only in the fields every rebuild moves?

.DESCRIPTION
    Get-GoLibEntryKey returns a key per lib/ entry; two flavors' entries differ materially exactly when their keys
    differ. Get-GoLibEntryDifference says WHY, by name, for the warning push-nuget prints.

    This is the rule push-nuget.ps1 has applied since increment 4: an entry differs materially when its LENGTH does.
#>
#Requires -Version 5.1
Set-StrictMode -Version 2

function Get-GoLibEntryKey {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$Bytes
    )

    return "length:$($Bytes.Length)"
}

function Get-GoLibEntryDifference {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$A,
        [Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$B
    )

    if ($A.Length -ne $B.Length) { return "$Name length differs ($($A.Length) vs $($B.Length) bytes)" }
    return $null
}

Export-ModuleMember -Function Get-GoLibEntryKey, Get-GoLibEntryDifference
