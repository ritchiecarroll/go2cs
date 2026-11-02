# derive-argv.ps1 -- H10 linux act: each row's converter argv, DERIVED at the tree from the recon wrapper's own
# functions and the sweep's tables, never typed (docs/PLAN-rebank-go126.md §3.2, §12 R block).
#   pwsh -NoProfile -File derive-argv.ps1 -Tree <clone> -GoRoot <GOROOT> -List <rows.txt> -Out <argv.tsv>
# Out: one line per row: row, then every converter argument (tab-separated, in order). Exit non-zero on any gap.
param([Parameter(Mandatory)] [string] $Tree, [Parameter(Mandatory)] [string] $GoRoot,
      [Parameter(Mandatory)] [string] $List, [Parameter(Mandatory)] [string] $Out)
$ErrorActionPreference = 'Stop'
function Fail([string] $m) { [Console]::Error.WriteLine("DERIVE REFUSED: $m"); exit 2 }

$wrapperPath = Join-Path $Tree 'src/run-h10-recon.ps1'
$sweepPath   = Join-Path $Tree 'src/run-validated-sweep.ps1'
$rosterLib   = Join-Path $Tree 'src/_roster.ps1'
$rosterPath  = Join-Path $Tree 'docs/ValidatedTestPackages.md'
foreach ($p in $wrapperPath, $sweepPath, $rosterLib, $rosterPath) { if (-not (Test-Path -LiteralPath $p)) { Fail "missing $p" } }

# The functions, by AST, dot-sourced at SCRIPT scope (a definition dot-sourced inside a function dies with it).
$wrapperAst = [System.Management.Automation.Language.Parser]::ParseFile($wrapperPath, [ref] $null, [ref] $null)
$sweepAst   = [System.Management.Automation.Language.Parser]::ParseFile($sweepPath, [ref] $null, [ref] $null)
$wanted = @(
    @($wrapperAst, 'Resolve-RowDeadline'), @($wrapperAst, 'Get-RowExecutionArgs'), @($wrapperAst, 'Get-RowConverterArgs'),
    @($wrapperAst, 'Get-SweepTestDeadlineFloors'),
    @($sweepAst, 'ConvertTo-GoDuration'), @($sweepAst, 'Resolve-RowTestDeadline'), @($sweepAst, 'Get-RowTestDeadlineArgs'),
    @($sweepAst, 'Get-TestBuildMaxCpuArgs'))
foreach ($w in $wanted) {
    $name = $w[1]
    $def = $w[0].Find({ param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq $name }, $true)
    if (-not $def) { Fail "no function $name" }
    . ([scriptblock]::Create($def.Extent.Text))
}
. $rosterLib
foreach ($n in 'Get-RosterExecutionArgs', 'Get-ValidatedRosterRows') {
    if (-not (Get-Command -Name $n -CommandType Function -ErrorAction SilentlyContinue)) { Fail "_roster.ps1 defines no $n" }
}

# The package floors: the sweep's $longTimeouts, read exactly as the wrapper reads it, plus the wrapper's own
# floored-relocation map (taken from its AST, not copied) fanned out to successors.
$sweepSrc = [System.IO.File]::ReadAllText($sweepPath)
$anchor = [regex]::Match($sweepSrc, '\$longTimeouts\s*=\s*@\{')
if (-not $anchor.Success) { Fail 'no $longTimeouts table' }
$close = $sweepSrc.IndexOf('}', $anchor.Index + $anchor.Length)
$Floors = @{}
foreach ($m in [regex]::Matches($sweepSrc.Substring($anchor.Index + $anchor.Length, $close - ($anchor.Index + $anchor.Length)), "'([^']+)'\s*=\s*'([^']+)'")) {
    $Floors[$m.Groups[1].Value] = $m.Groups[2].Value
}
if ($Floors.Count -lt 5) { Fail "only $($Floors.Count) package floors parsed" }
$mapAst = $wrapperAst.Find({ param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and $n.Left.Extent.Text -eq '$FlooredSuccessors' }, $true)
if (-not $mapAst) { Fail 'no $FlooredSuccessors map in the wrapper' }
. ([scriptblock]::Create($mapAst.Extent.Text))
foreach ($old in $FlooredSuccessors.Keys) {
    if ($Floors.ContainsKey($old)) { foreach ($new in $FlooredSuccessors[$old]) { if (-not $Floors.ContainsKey($new)) { $Floors[$new] = $Floors[$old] } } }
}
$tdd = Get-SweepTestDeadlineFloors $sweepSrc $Floors
if ($tdd.Problems.Count -gt 0) { Fail ($tdd.Problems -join '; ') }
$TestDeadlineFloors = $tdd.Floors
if ((Resolve-RowDeadline '60m' '30m') -ne '60m' -or (Resolve-RowDeadline '30m' '60m') -ne '60m') { Fail 'the deadline rule failed its own control' }

$rosterRows = @(Get-ValidatedRosterRows -Path $rosterPath)
if ($rosterRows.Count -eq 0) { Fail 'the roster parsed zero rows' }
$RosterByName = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([System.StringComparer]::Ordinal)
foreach ($r in $rosterRows) { $RosterByName[$r.Package] = $r }

$rows = @(Get-Content -LiteralPath $List | Where-Object { $_.Trim() })
$lines = foreach ($row in $rows) {
    $rowFloor = ''; if ($Floors.ContainsKey($row)) { $rowFloor = $Floors[$row] }
    $rowTimeout = Resolve-RowDeadline '30m' $rowFloor
    $rowExec = @(Get-RowExecutionArgs $row $RosterByName 'Release')
    $rowTestDeadline = Resolve-RowTestDeadline '15m' $TestDeadlineFloors[$row]
    $argv = @(Get-RowConverterArgs $rowTimeout ($rowExec + @(Get-RowTestDeadlineArgs $rowTestDeadline) + @(Get-TestBuildMaxCpuArgs 0)) `
              (Join-Path $Tree 'src') @() (Join-Path $GoRoot ("src/" + $row)) (Join-Path $Tree ("src/core/" + $row)))
    $pin = if ($RosterByName.ContainsKey($row) -and $RosterByName[$row].Execution) { $RosterByName[$row].Execution } else { '-' }
    [Console]::Error.WriteLine(("{0}: floor={1} timeout={2} test-deadline={3} pin={4} inRoster={5}" -f $row, $(if ($rowFloor) { $rowFloor } else { '-' }), $rowTimeout, $rowTestDeadline, $pin, $RosterByName.ContainsKey($row)))
    (@($row) + $argv) -join "`t"
}
[System.IO.File]::WriteAllText($Out, (($lines -join "`n") + "`n"))
[Console]::Error.WriteLine("derived $($rows.Count) row(s); roster rows $($rosterRows.Count); package floors $($Floors.Count); per-test floors $($TestDeadlineFloors.Count)")
