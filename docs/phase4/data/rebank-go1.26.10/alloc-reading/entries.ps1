# entries.ps1 -- the reading-owed entry list for the go1.26.10 alloc-units run, from the manifests at a tree.
# Every disclosure entry whose class is an allocation label (deferred, structural, alloc-count-semantics), plus the
# entries new at go1.26.10 that have no manifest line yet. Writes reading-owed.txt ("<package> <test>" per line) and
# rows.txt (the distinct packages), both sorted, into -Out (keep it OUTSIDE the repo).
param([Parameter(Mandatory)] [string] $Tree, [Parameter(Mandatory)] [string] $Out)
$core = Join-Path $Tree 'src/core'
$labels = @('deferred', 'structural', 'alloc-count-semantics')
$pairs = New-Object System.Collections.Generic.List[string]
Get-ChildItem -Path $core -Recurse -Filter 'go2cs_test_disclosures.json' | ForEach-Object {
    $pkg = [System.IO.Path]::GetRelativePath($core, $_.DirectoryName).Replace('\', '/')
    $j = Get-Content $_.FullName -Raw | ConvertFrom-Json
    foreach ($d in @($j.disclosures)) {
        if ($labels -contains $d.class -and $d.name) { $pairs.Add("$pkg $($d.name)") }
    }
}
# New at go1.26.10, reading-owed and not yet in any manifest (G's re-sign list, items 1, 4, 7, 8, 10, 11).
foreach ($p in @(
    'crypto/sha256 TestAllocatonsWithTypeAsserts',
    'strings TestReplace',
    'math/big TestSqrAlloc',
    'crypto/internal/fips140test TestMLDSAAllocations',
    'log/slog TestAlloc/2_pairs_disabled_inline',
    'log/slog TestJSONAllocs/attrs',
    'log/slog TestJSONAllocs/message')) { $pairs.Add($p) }
$sorted = @($pairs | Sort-Object -Unique)
$sorted | Set-Content -Encoding ascii (Join-Path $Out 'reading-owed.txt')
@($sorted | ForEach-Object { ($_ -split ' ')[0] } | Sort-Object -Unique) | Set-Content -Encoding ascii (Join-Path $Out 'rows.txt')
"entries=$($sorted.Count) rows=$(@($sorted | ForEach-Object { ($_ -split ' ')[0] } | Sort-Object -Unique).Count)"
