# whea.ps1 -- prints the WHEA-Logger events newer than the given ISO time (or "WHEA 0 since <t>").
param([string]$Since)
$t = [datetime]::Parse($Since)
$ev = @(Get-WinEvent -FilterHashtable @{LogName='System';ProviderName='Microsoft-Windows-WHEA-Logger';StartTime=$t} -ErrorAction SilentlyContinue)
if ($ev.Count -eq 0) { "WHEA 0 since $Since" } else { $ev | ForEach-Object { "WHEA {0} id={1}" -f $_.TimeCreated.ToString('o'), $_.Id } }
