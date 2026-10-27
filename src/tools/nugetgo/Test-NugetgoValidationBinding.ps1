<#
.SYNOPSIS
    Table-driven arms for NugetgoValidationBinding.psm1: the proof a module pack ships as VALIDATION.md must be a proof of
    the packed bytes (the nugetgo rehearsal, 2026-10-09, gap 4). The rehearsal packed a -recurse=nuget root beside a
    MODULE.md from a separate -recurse -tests root, and nothing compared them. Each converted module package's project now
    records GoInputDigest; MODULE.md records the same value per package; the pack refuses a proof whose digest is not the
    packed tree's. A case that should refuse names a fragment of its reason. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoValidationBinding.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoReadme.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

$a = 'sha256-' + ('a' * 64)
$b = 'sha256-' + ('b' * 64)
$c = 'sha256-' + ('c' * 64)

Write-Host 'MODULE.md is read'
$summary = @"
# ``example.com/mod`` -- module validation summary

| Package | Matched | Disclosed | Input digest | Proof |
|:--|--:|--:|:--|:--|
| ``example.com/mod`` | 37 | 0 | ``$a`` | [index.md](index.md) |
| ``example.com/mod/sub`` | 3 | 1 | ``$b`` | [sub.md](sub.md) |

**Total: 40 matched, 1 disclosed** across 2 package(s).
"@
$proof = Get-NugetgoProofInputDigests -ModuleSummary $summary
Check 'each row''s import path and digest' ($proof.Count -eq 2 -and $proof['example.com/mod'] -eq $a -and $proof['example.com/mod/sub'] -eq $b) "read [$(($proof.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ', ')]"
$old = "| Package | Matched | Disclosed | Proof |`n|:--|--:|--:|:--|`n| ``example.com/mod`` | 37 | 0 | [index.md](index.md) |`n"
$proofOld = Get-NugetgoProofInputDigests -ModuleSummary $old
Check 'a summary written before the binding reads as a row with no digest' ($proofOld.Count -eq 1 -and $proofOld['example.com/mod'] -eq '') "read [$(($proofOld.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ', ')]"

Write-Host 'the packed project is read'
$dir = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-binding-$PID-" + [DateTime]::UtcNow.Ticks)
New-Item -ItemType Directory -Force $dir | Out-Null
try {
    $withDigest = Join-Path $dir 'with.csproj'
    [System.IO.File]::WriteAllText($withDigest, "<Project Sdk=`"Microsoft.NET.Sdk`">`r`n  <PropertyGroup>`r`n    <OutputType>Library</OutputType>`r`n    <GoInputDigest>$a</GoInputDigest>`r`n  </PropertyGroup>`r`n</Project>`r`n")
    $without = Join-Path $dir 'without.csproj'
    [System.IO.File]::WriteAllText($without, "<Project Sdk=`"Microsoft.NET.Sdk`">`r`n  <PropertyGroup>`r`n    <OutputType>Library</OutputType>`r`n  </PropertyGroup>`r`n</Project>`r`n")
    Check 'a project''s GoInputDigest' ((Get-NugetgoProjectInputDigest -ProjectFile $withDigest) -eq $a) "read '$(Get-NugetgoProjectInputDigest -ProjectFile $withDigest)'"
    Check 'a project with none reads empty' ((Get-NugetgoProjectInputDigest -ProjectFile $without) -eq '') "read '$(Get-NugetgoProjectInputDigest -ProjectFile $without)'"
}
finally { Remove-Item -Recurse -Force $dir }

Write-Host 'the proof is bound to the packed tree'
$cases = @(
    # name, proof, packed, expected reason fragment (or $null)
    @('a proof of exactly the packed inputs', @{ 'example.com/mod' = $a; 'example.com/mod/sub' = $b }, @{ 'example.com/mod' = $a; 'example.com/mod/sub' = $b }, $null),
    @('a packed package with no Go tests has no proof row', @{ 'example.com/mod' = $a }, @{ 'example.com/mod' = $a; 'example.com/mod/notests' = $c }, $null),
    @('a proof made from other inputs', @{ 'example.com/mod' = $a; 'example.com/mod/sub' = $c }, @{ 'example.com/mod' = $a; 'example.com/mod/sub' = $b }, "example.com/mod/sub: the proof was made from inputs $c, the packed tree from $b"),
    @('a proof with no digest', @{ 'example.com/mod' = '' }, @{ 'example.com/mod' = $a }, 'the proof records no input digest for example.com/mod'),
    @('a packed tree with no digest', @{ 'example.com/mod' = $a }, @{ 'example.com/mod' = '' }, 'example.com/mod is packed from a conversion that records no input digest'),
    @('a proof row for a package not packed', @{ 'example.com/mod' = $a; 'example.com/mod/gone' = $b }, @{ 'example.com/mod' = $a }, 'example.com/mod/gone has a proof row and is not packed'),
    @('an empty proof', @{}, @{ 'example.com/mod' = $a }, 'the proof lists no package')
)
foreach ($case in $cases) {
    $reasons = @(Test-NugetgoValidationBinding -Proof $case[1] -Packed $case[2])
    if ($case[3]) { Check $case[0] (@($reasons | Where-Object { $_.Contains($case[3]) }).Count -eq 1) "reasons [$($reasons -join ' | ')], want one naming '$($case[3])'" }
    else { Check $case[0] ($reasons.Count -eq 0) "reasons [$($reasons -join ' | ')], want none" }
}

Write-Host 'a packed package with Go tests and no proof row is refused (COORD ruling 2026-10-13, the go-cmp dry run)'
# The two real MODULE.md files of go-cmp v0.7.0 at r6 39b58d0d8d: at the default -test-timeout 2m the root package cmp
# lost its comparison to the package deadline and the summary simply has no row for it; at 6m every tested package has
# one. The packed digests are that run's -recurse=nuget root's, for the same G.
$gcmpPacked = @{
    'github.com/google/go-cmp/cmp' = 'sha256-11c6f392ca735ab58a82483770d01ef027887075817f6a1f380456c1d7975200'
    'github.com/google/go-cmp/cmp/cmpopts' = 'sha256-a9c1290c92729b05c3b750d02d38dd48b66b25a45062c30590ad68108823e0d9'
    'github.com/google/go-cmp/cmp/internal/diff' = 'sha256-5ac44f8574c52f432d82d26c8cf023f05ab9095e557026b3bd0e2f930460f08f'
    'github.com/google/go-cmp/cmp/internal/function' = 'sha256-6688f052f93b9c56451ab0044f607fdae7cf24c774a4a598f80385ecc9ffd2f0'
    'github.com/google/go-cmp/cmp/internal/value' = 'sha256-29abf7c2a9cbae38c7c47cbcb82329e269a27796989571858339142757d28a7c'
    'github.com/google/go-cmp/cmp/internal/flags' = 'sha256-c0ab592d010ce33d9707946ddb58fc615d21219209c9b98269aa918ad5faa44d'
}
$gcmpTested = @('github.com/google/go-cmp/cmp', 'github.com/google/go-cmp/cmp/cmpopts', 'github.com/google/go-cmp/cmp/internal/diff',
    'github.com/google/go-cmp/cmp/internal/function', 'github.com/google/go-cmp/cmp/internal/value')
$fixtures = Join-Path $PSScriptRoot 'testdata/binding'
$at2m = Get-NugetgoProofInputDigests -ModuleSummary ([System.IO.File]::ReadAllText((Join-Path $fixtures 'go-cmp-v0.7.0-timeout-2m.MODULE.md')))
$at6m = Get-NugetgoProofInputDigests -ModuleSummary ([System.IO.File]::ReadAllText((Join-Path $fixtures 'go-cmp-v0.7.0-timeout-6m.MODULE.md')))
Check 'the 2m fixture has no row for cmp; the 6m fixture has all five' (-not $at2m.ContainsKey('github.com/google/go-cmp/cmp') -and $at2m.Count -eq 4 -and $at6m.Count -eq 5) "2m rows [$(@($at2m.Keys | Sort-Object) -join ', ')], 6m rows [$(@($at6m.Keys | Sort-Object) -join ', ')]"
$reasons = @(Test-NugetgoValidationBinding -Proof $at2m -Packed $gcmpPacked -Tested $gcmpTested)
Check 'the 2m proof does not bind a packed cmp, which has Go tests' (@($reasons | Where-Object { $_.Contains('github.com/google/go-cmp/cmp has Go tests and no proof row') -and $_.Contains('step 1.2') }).Count -eq 1 -and $reasons.Count -eq 1) "reasons [$($reasons -join ' | ')], want exactly one naming cmp and step 1.2"
$reasons = @(Test-NugetgoValidationBinding -Proof $at6m -Packed $gcmpPacked -Tested $gcmpTested)
Check 'the 6m proof binds the same packed tree (flags has no tests and no row)' ($reasons.Count -eq 0) "reasons [$($reasons -join ' | ')], want none"
$reasons = @(Test-NugetgoValidationBinding -Proof $at2m -Packed @{ 'github.com/google/go-cmp/cmp/cmpopts' = $gcmpPacked['github.com/google/go-cmp/cmp/cmpopts']; 'github.com/google/go-cmp/cmp/internal/diff' = $gcmpPacked['github.com/google/go-cmp/cmp/internal/diff']; 'github.com/google/go-cmp/cmp/internal/function' = $gcmpPacked['github.com/google/go-cmp/cmp/internal/function']; 'github.com/google/go-cmp/cmp/internal/value' = $gcmpPacked['github.com/google/go-cmp/cmp/internal/value'] } -Tested $gcmpTested)
Check 'a tested package left out of the pack is not a reason' ($reasons.Count -eq 0) "reasons [$($reasons -join ' | ')], want none"

Write-Host 'which packed packages have Go tests is read from the module source'
$mod = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-tested-$PID-" + [DateTime]::UtcNow.Ticks)
try {
    foreach ($f in @('root.go', 'root_test.go', 'sub/sub.go', 'sub/sub_test.go', 'notests/n.go', 'deep/d.go', 'deep/inner/i_test.go', 'testdata/x_test.go')) {
        $path = Join-Path $mod $f
        New-Item -ItemType Directory -Force (Split-Path $path) | Out-Null
        [System.IO.File]::WriteAllText($path, "package x`n")
    }
    $tested = @(Get-NugetgoTestedImportPaths -ModuleDir $mod -ModulePath 'example.com/mod' -ImportPaths @('example.com/mod', 'example.com/mod/sub', 'example.com/mod/notests', 'example.com/mod/deep') | Sort-Object)
    Check 'a package whose own directory holds *_test.go files, and only those' (($tested -join ',') -ceq 'example.com/mod,example.com/mod/sub') "read [$($tested -join ', ')], want example.com/mod and example.com/mod/sub (deep's tests are in a subdirectory)"
}
finally { if (Test-Path $mod) { Remove-Item -Recurse -Force $mod } }

# An -ExcludePackage'd package that has a proof row (COORD ruling 2026-10-26: cobra/doc, 19 + 0, left out because its
# imports go-md2man and yaml.v3 are not nugetgo packages). Its row binds nothing the package carries, so the binding
# accepts it, and VALIDATION.md counts the packed packages only, with one line naming the validated, unpacked package.
Write-Host 'a proven package left out by -ExcludePackage'
$cobraSummary = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'testdata/binding/cobra-v1.10.2.MODULE.md'))
$cobraProof = Get-NugetgoProofInputDigests -ModuleSummary $cobraSummary
$cobraPacked = @{ 'github.com/spf13/cobra' = $cobraProof['github.com/spf13/cobra'] }
$reasons = @(Test-NugetgoValidationBinding -Proof $cobraProof -Packed $cobraPacked)
Check 'without -Excluded, the unpacked row is refused (the control)' (($reasons -join '; ') -like '*github.com/spf13/cobra/doc has a proof row and is not packed*') "got [$($reasons -join '; ')]"
$reasons = @(Test-NugetgoValidationBinding -Proof $cobraProof -Packed $cobraPacked -Excluded @('github.com/spf13/cobra/doc'))
Check 'with -Excluded naming it, the binding holds' ($reasons.Count -eq 0) "got [$($reasons -join '; ')]"
$reasons = @(Test-NugetgoValidationBinding -Proof $cobraProof -Packed @{} -Excluded @('github.com/spf13/cobra/doc'))
Check '-Excluded never excuses a packed package''s own missing binding' (($reasons -join '; ') -like '*github.com/spf13/cobra has a proof row and is not packed*') "got [$($reasons -join '; ')]"
$sel = Select-NugetgoPackedProof -ModuleSummary $cobraSummary -Excluded @('github.com/spf13/cobra/doc')
$selLines = @($sel.Text -split "`r?`n")
Check 'the excluded row is gone and the packed one stays' (-not $sel.Text.Contains('| `github.com/spf13/cobra/doc` |') -and $sel.Text.Contains('| `github.com/spf13/cobra` | 405 | 0 |')) $sel.Text
$dot = [char]0x00B7
Check 'the total counts the packed packages only' ($selLines -ccontains "**Total: 405 matched $dot 0 disclosed** across 1 package(s).") $sel.Text
Check 'one line names the validated, unpacked package with its counts' (@($selLines | Where-Object { $_.StartsWith("``github.com/spf13/cobra/doc`` was validated (19 matched $dot 0 disclosed) and is not packed", [StringComparison]::Ordinal) }).Count -eq 1) $sel.Text
Check 'its excluded declarations go with it, the section too when none remain' (-not $sel.Text.Contains('## Excluded declarations') -and -not $sel.Text.Contains('BenchmarkGenManToFile')) $sel.Text
Check 'its proof page is named for the pack to leave out' (($sel.DroppedPages -join ',') -ceq 'doc.md') "got [$($sel.DroppedPages -join ', ')]"
$totals = Get-NugetgoProofTotals -ModuleSummary $sel.Text
Check 'the README''s totals read from it are the packed ones' ($totals.Matched -eq 405 -and $totals.Disclosed -eq 0 -and $totals.Packages -eq 1) "got $($totals.Matched) / $($totals.Disclosed) / $($totals.Packages)"
$same = Select-NugetgoPackedProof -ModuleSummary $cobraSummary -Excluded @()
Check 'with nothing excluded, the summary is unchanged byte for byte' ($same.Text -ceq $cobraSummary -and $same.DroppedPages.Count -eq 0) 'changed'
$jwtLike = Select-NugetgoPackedProof -ModuleSummary $cobraSummary -Excluded @('github.com/spf13/cobra/nosuch')
Check 'an excluded package with no proof row (jwt/v5''s test) changes nothing' ($jwtLike.Text -ceq $cobraSummary) 'changed'

# A timing-sensitive disclosed test (COORD ruling 2026-10-26: uuid's TestVersion6). A disclosure counts only when the
# converted side fails with its signature; when it passes, the row reads one more matched and the record lists the
# entry as orphaned. The prediction accepts exactly those two shapes for the named entry, and nothing else.
Write-Host 'a prediction with an orphan-shaped alternative'
$alt = @{ Counts = @(54, 0); Orphan = 'TestVersion6' }
$p = Test-NugetgoPrediction -Want @(53, 1) -Alternate $alt -Matched 53 -Disclosed 1 -Orphaned @()
Check 'the disclosed shape: 53 + 1, met' ($p.Met -and $p.Shape -ceq 'predicted') "met $($p.Met) shape $($p.Shape)"
$p = Test-NugetgoPrediction -Want @(53, 1) -Alternate $alt -Matched 54 -Disclosed 0 -Orphaned @('TestVersion6')
Check 'the orphan shape: 54 + 0 with TestVersion6 orphaned, met' ($p.Met -and $p.Shape -ceq 'orphaned TestVersion6') "met $($p.Met) shape $($p.Shape)"
$p = Test-NugetgoPrediction -Want @(53, 1) -Alternate $alt -Matched 54 -Disclosed 0 -Orphaned @()
Check '54 + 0 without the orphan record: missed' (-not $p.Met) "met $($p.Met)"
$p = Test-NugetgoPrediction -Want @(53, 1) -Alternate $alt -Matched 54 -Disclosed 0 -Orphaned @('TestSomethingElse')
Check '54 + 0 with another entry orphaned: missed' (-not $p.Met) "met $($p.Met)"
$p = Test-NugetgoPrediction -Want @(53, 1) -Alternate $alt -Matched 52 -Disclosed 1 -Orphaned @('TestVersion6')
Check 'any other count: missed' (-not $p.Met) "met $($p.Met)"
$p = Test-NugetgoPrediction -Want @(37, 0) -Matched 37 -Disclosed 0 -Orphaned @()
Check 'no alternate: the one count, as before' ($p.Met) "met $($p.Met)"
$p = Test-NugetgoPrediction -Want @(37, 0) -Matched 38 -Disclosed 0 -Orphaned @('TestX')
Check 'no alternate: an orphan excuses nothing' (-not $p.Met) "met $($p.Met)"

Write-Host 'the pack uses it'
$pack = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
Check 'nugetgo-pack.ps1 passes -Excluded to the binding and packs the selected summary' (
    $pack -match 'Test-NugetgoValidationBinding [^\r\n]*-Excluded \$ExcludePackage' -and $pack.Contains('Select-NugetgoPackedProof -ModuleSummary') -and
    $pack.Contains('$proofSummary') -and -not $pack.Contains('[System.IO.File]::ReadAllText($validationPage) +')) 'the pack still binds or packs MODULE.md unselected'
Check 'nugetgo-pack.ps1 reads each packed project''s digest and the proof''s' (
    $pack.Contains('Get-NugetgoProjectInputDigest') -and $pack.Contains('Get-NugetgoProofInputDigests')) 'the pack does not read both digests'
Check 'nugetgo-pack.ps1 tells the binding which packed packages have Go tests' ($pack -match 'Get-NugetgoTestedImportPaths' -and $pack -match 'Test-NugetgoValidationBinding [^\r\n]*-Tested') 'the pack does not pass -Tested'
$bound = $pack.IndexOf('Test-NugetgoValidationBinding')
Check 'nugetgo-pack.ps1 checks the binding before it packs' ($bound -ge 0 -and $bound -lt $pack.IndexOf('& dotnet pack')) 'no binding check ahead of dotnet pack'

Write-Host "ran $ran, failed $failed"
exit $failed
