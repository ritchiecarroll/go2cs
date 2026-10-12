# NugetgoBehaviorProof.psm1 -- RED STUB (the cobra increment, part 3). Today a module with no Go tests has no proof at
# all: it packs only as an unvalidated rehearsal, and nothing reads a behavioral row.

function Read-NugetgoBehaviorRow { [CmdletBinding()] param([Parameter(Mandatory)][string]$Path) $null }
function Test-NugetgoBehaviorRows { [CmdletBinding()] param([Parameter(Mandatory)]$Rows, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$ModulePath, [Parameter(Mandatory)][string]$GoVersion) @() }
function New-NugetgoBehaviorPage { [CmdletBinding()] param([Parameter(Mandatory)][string]$ModulePath, [Parameter(Mandatory)][string]$GoVersion, [Parameter(Mandatory)]$Rows) '' }
function Test-NugetgoModuleHasGoTests { [CmdletBinding()] param([Parameter(Mandatory)][string]$ModuleDir) $false }

Export-ModuleMember -Function Read-NugetgoBehaviorRow, Test-NugetgoBehaviorRows, New-NugetgoBehaviorPage, Test-NugetgoModuleHasGoTests
