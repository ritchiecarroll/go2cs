<#
.SYNOPSIS
    RED: not yet implemented (the nugetgo rehearsal, 2026-10-09, gap 4).
#>

function Get-NugetgoProofInputDigests([string]$ModuleSummary) { return @{} }

function Get-NugetgoProjectInputDigest([string]$ProjectFile) { return $null }

function Test-NugetgoValidationBinding([hashtable]$Proof, [hashtable]$Packed) { return @() }

Export-ModuleMember -Function Get-NugetgoProofInputDigests, Get-NugetgoProjectInputDigest, Test-NugetgoValidationBinding
