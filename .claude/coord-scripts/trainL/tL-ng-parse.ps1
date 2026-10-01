# TRAIN L leg NG(b): parse each named PowerShell file with the RUNNING edition's parser (run it under Windows
# PowerShell 5.1 AND pwsh 7). Read-only. Prints one 'PARSE <file> errors=<n> first=<message>' line per file and
# 'NGPARSE edition=<v> files=<n> errors=<total>'; exits 1 when any file has a parse error.
# MEASURED before the battery (2026-10-01, the seat spec): nugetgo-pack.ps1 reads 10 errors under 5.1 (first at line
# 116, 'Unexpected token'), 0 under pwsh 7 -- a BOM-less UTF-8 em dash inside a double-quoted string, read as cp1252.
# DRAFT 2: RULED R2 -- the 5.1 parse is MANDATORY (release-path scripts run there); the battery's NGb51 leg is GATED.
# This file stays pure ASCII so that it parses under 5.1 itself.
param([Parameter(Mandatory, ValueFromRemainingArguments)][string[]] $Paths)
$total = 0
foreach ($p in $Paths) {
    $tokens = $null; $errors = $null
    [void][System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path -LiteralPath $p).Path, [ref]$tokens, [ref]$errors)
    $n = @($errors).Count
    $total += $n
    $first = if ($n -gt 0) { "line $($errors[0].Extent.StartLineNumber): $($errors[0].Message)" } else { '' }
    "PARSE $p errors=$n first=$first"
}
"NGPARSE edition=$($PSVersionTable.PSVersion) files=$($Paths.Count) errors=$total"
if ($total -gt 0) { exit 1 } else { exit 0 }
