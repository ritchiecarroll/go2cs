<#
.SYNOPSIS
    Table-driven arms for NugetgoFlavors.psm1: a nugetgo package carries RID-specific assemblies exactly when its
    flavors differ in MEANING, decided by the same material check the go.* release uses (GoAssemblyMaterial.psm1), and
    in the same layout the go.* release ships (push-nuget.ps1). COORD ruling 2026-10-12, for mousetrap and cobra:
    the decision comes from a measurement, never from the module author or a list. Exit code = the number of failed
    cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoFlavors.psm1') -Force
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
# The material check's own fixtures (src/tools/pushnuget/testdata/build-fixtures.ps1): alpha and bravo are one source
# with an element type of Dep.Alpha or Dep.Bravo, equal in length and different in meaning; cascade-a and cascade-b are
# one source built against two Deps, different only in Roslyn's deterministic identity fields.
function Fixture([string]$name) { , [System.IO.File]::ReadAllBytes((Join-Path $PSScriptRoot "../pushnuget/testdata/$name")) }
$alpha = Fixture 'alpha.dll'
$bravo = Fixture 'bravo.dll'
$cascadeA = Fixture 'cascade-a.dll'
$cascadeB = Fixture 'cascade-b.dll'
# One assembly set per flavor, in the map's order; each arm64 flavor agrees with its GOOS's amd64 flavor unless a case
# gives it its own.
function Flavors($windows, $linux, $darwin, $linuxArm = $linux, $darwinArm = $darwin) { [ordered]@{ 'windows/amd64' = $windows; 'linux/amd64' = $linux; 'darwin/amd64' = $darwin; 'linux/arm64' = $linuxArm; 'darwin/arm64' = $darwinArm } }
# A fifth assembly for the layout arms, which copy bytes and never read them: unequal to every fixture above.
$darwinArmBytes = [byte[]][System.Text.Encoding]::UTF8.GetBytes('the darwin/arm64 flavor of App.dll')

Write-Host 'the RID map is the go.* release''s'
$rids = Get-NugetgoRidFlavors
$release = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot '../../push-nuget.ps1'))
$block = [regex]::Match($release, '(?s)\$ridFlavors = \[ordered\]@\{(?<body>.*?)\}')
$releaseMap = [ordered]@{}
foreach ($m in [regex]::Matches($block.Groups['body'].Value, "'(?<rid>[^']+)'\s*=\s*'(?<flavor>[^']+)'")) { $releaseMap[$m.Groups['rid'].Value] = $m.Groups['flavor'].Value }
Check 'push-nuget.ps1 has a $ridFlavors map to mirror' ($releaseMap.Count -gt 0) 'no map found: the release script moved'
Check 'the same RIDs, the same flavors, the same order (the first is the reference flavor)' (
    (@($rids.Keys) -join ',') -ceq (@($releaseMap.Keys) -join ',') -and (@($rids.Values) -join ',') -ceq (@($releaseMap.Values) -join ',')) `
    "nugetgo $(@($rids.Keys | ForEach-Object { "$_=$($rids[$_])" }) -join ',') vs release $(@($releaseMap.Keys | ForEach-Object { "$_=$($releaseMap[$_])" }) -join ',')"
Check 'the reference flavor is windows/amd64' ((Get-NugetgoReferenceFlavor) -ceq 'windows/amd64') "got $(Get-NugetgoReferenceFlavor)"
Check 'linux-arm64 ships linux/arm64, a flavor of its own and not linux-x64''s twin (S6)' (
    $rids.Contains('linux-arm64') -and $rids['linux-arm64'] -ceq 'linux/arm64' -and $rids['linux-x64'] -ceq 'linux/amd64') "got $(@($rids.Keys | ForEach-Object { "$_=$($rids[$_])" }) -join ',')"
Check 'each Mac RID ships its own flavor: osx-arm64 darwin/arm64, not osx-x64''s twin (D6)' ($rids['osx-x64'] -ceq 'darwin/amd64' -and $rids['osx-arm64'] -ceq 'darwin/arm64') "got osx-x64=$($rids['osx-x64']), osx-arm64=$($rids['osx-arm64'])"
Check 'the flavors are the five targets, one conversion each, in map order (the universe''s)' ((@(Get-NugetgoFlavors) -join ',') -ceq 'windows/amd64,linux/amd64,darwin/amd64,linux/arm64,darwin/arm64') "got $(@(Get-NugetgoFlavors) -join ',')"
Check 'a RID''s host target is its flavor (no RID ships another arch''s bytes)' (@($rids.Keys | Where-Object { (Get-NugetgoRidTarget $_) -cne $rids[$_] }).Count -eq 0) "got $(@($rids.Keys | ForEach-Object { "$_=$(Get-NugetgoRidTarget $_)" }) -join ',')"

Write-Host 'the decision is a measurement'
$split = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo } @{ 'App.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a GOOS-split pair (meaning differs) is RID-specific' ($split.RidSpecific) 'packed neutral'
Check 'its reason names the assembly and the flavor' (@($split.Reasons | Where-Object { $_ -like '*windows*App.dll*' }).Count -eq 1) "reasons: $($split.Reasons -join ' | ')"
$identity = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $cascadeA } @{ 'App.dll' = $cascadeB } @{ 'App.dll' = $cascadeA }) -ProofFlavor 'linux/amd64'
Check 'an identity-only pair is neutral (the cascade is not material)' (-not $identity.RidSpecific) "reasons: $($identity.Reasons -join ' | ')"
$planted = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha; 'Dep.dll' = $bravo } @{ 'App.dll' = $alpha; 'Dep.dll' = $bravo } @{ 'App.dll' = $alpha; 'Dep.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a planted identical pair is neutral' (-not $planted.RidSpecific) "reasons: $($planted.Reasons -join ' | ')"
$armOnly = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a difference at linux/arm64 alone is RID-specific (it is not linux/amd64''s twin)' ($armOnly.RidSpecific -and @($armOnly.Reasons | Where-Object { $_ -like '*linux/arm64*' }).Count -eq 1) "reasons: $($armOnly.Reasons -join ' | ')"
$darwinArmOnly = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a difference at darwin/arm64 alone is RID-specific (it is not darwin/amd64''s twin)' ($darwinArmOnly.RidSpecific -and @($darwinArmOnly.Reasons | Where-Object { $_ -like '*darwin/arm64*' }).Count -eq 1) "reasons: $($darwinArmOnly.Reasons -join ' | ')"
$darwinOnly = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a difference in one flavor alone is RID-specific' ($darwinOnly.RidSpecific -and @($darwinOnly.Reasons | Where-Object { $_ -like '*darwin*' }).Count -eq 1) "reasons: $($darwinOnly.Reasons -join ' | ')"
$missing = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha; 'Extra.dll' = $alpha } @{ 'App.dll' = $alpha; 'Extra.dll' = $alpha }) -ProofFlavor 'linux/amd64'
Check 'a different assembly set is RID-specific' ($missing.RidSpecific -and @($missing.Reasons | Where-Object { $_ -like '*Extra.dll*' }).Count -ge 1) "reasons: $($missing.Reasons -join ' | ')"
$noFlavor = $null
try { Get-NugetgoFlavorDecision -Assemblies ([ordered]@{ 'windows/amd64' = @{ 'App.dll' = $alpha }; 'linux/amd64' = @{ 'App.dll' = $alpha }; 'darwin/amd64' = @{ 'App.dll' = $alpha }; 'linux/arm64' = @{ 'App.dll' = $alpha } }) -ProofFlavor 'linux/amd64' | Out-Null }
catch { $noFlavor = $_.Exception.Message }
Check 'a missing flavor is refused, never read as neutral' ($noFlavor -like '*darwin/arm64*') "got: $noFlavor"
# A pack that ships a subset of the RIDs (a -Cgo pack leaves osx-arm64 out by name) measures the subset's flavors and no
# others: every flavor function takes the pack's own -RidFlavors, so a flavor the pack does not ship is never converted.
$subset = [ordered]@{}; foreach ($rid in $rids.Keys) { if ($rid -cne 'osx-arm64') { $subset[$rid] = $rids[$rid] } }
Check 'a subset map''s flavors are its own, in its order' ((@(Get-NugetgoFlavors -RidFlavors $subset) -join ',') -ceq 'windows/amd64,linux/amd64,darwin/amd64,linux/arm64') "got $(@(Get-NugetgoFlavors -RidFlavors $subset) -join ',')"
$four = [ordered]@{ 'windows/amd64' = @{ 'App.dll' = $alpha }; 'linux/amd64' = @{ 'App.dll' = $alpha }; 'darwin/amd64' = @{ 'App.dll' = $alpha }; 'linux/arm64' = @{ 'App.dll' = $alpha } }
$subsetWhy = $null; $subsetDecision = $null
try { $subsetDecision = Get-NugetgoFlavorDecision -Assemblies $four -ProofFlavor 'linux/amd64' -RidFlavors $subset } catch { $subsetWhy = $_.Exception.Message }
Check 'the decision over a subset map measures its flavors alone' ($null -eq $subsetWhy -and $subsetDecision -and -not $subsetDecision.RidSpecific) "got: $subsetWhy"
$subsetLine = Get-NugetgoFlavorDisclosure -ProofFlavor 'linux/amd64' -RidSpecific -RidFlavors $subset
Check 'the disclosure over a subset map names its flavors alone' ("$subsetLine" -like '*windows/amd64, darwin/amd64 and linux/arm64 flavors*') "got '$subsetLine'"
$subsetReq = @(try { Merge-NugetgoFlavorRequires -Requires ([ordered]@{ 'windows/amd64' = @(); 'linux/amd64' = @(); 'darwin/amd64' = @(); 'linux/arm64' = @() }) -RidFlavors $subset } catch { "threw: $($_.Exception.Message)" })
Check 'the requires over a subset map read its flavors alone' ($subsetReq.Count -eq 0) "got $($subsetReq -join ', ')"

Write-Host 'the RID-specific layout is the go.* release''s'
$scratch = Join-Path ([System.IO.Path]::GetTempPath()) "nugetgo-flavors-$PID"
New-Item -ItemType Directory -Force $scratch | Out-Null
function New-Package([string]$path, [hashtable]$lib) {
    if (Test-Path $path) { Remove-Item $path }
    $zip = [System.IO.Compression.ZipFile]::Open($path, 'Create')
    try {
        foreach ($name in @('nugetgo.example.nuspec', 'README.md') + @($lib.Keys | ForEach-Object { "lib/net10.0/$_" })) {
            $bytes = if ($name -like 'lib/*') { $lib[(Split-Path -Leaf $name)] } else { [System.Text.Encoding]::UTF8.GetBytes($name) }
            $s = $zip.CreateEntry($name).Open(); try { $s.Write($bytes, 0, $bytes.Length) } finally { $s.Dispose() }
        }
    }
    finally { $zip.Dispose() }
}
function Entries([string]$path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($path)
    try {
        $map = @{}
        foreach ($e in $zip.Entries) { $ms = New-Object System.IO.MemoryStream; $s = $e.Open(); try { $s.CopyTo($ms) } finally { $s.Dispose() }; $map[$e.FullName] = $ms.ToArray() }
        $map
    }
    finally { $zip.Dispose() }
}
function Same($a, $b) { $null -ne $a -and $null -ne $b -and [System.Linq.Enumerable]::SequenceEqual([byte[]]$a, [byte[]]$b) }

$pkg = Join-Path $scratch 'split.nupkg'
New-Package $pkg @{ 'App.dll' = $bravo }   # packed from the proof flavor (linux)
$assemblies = Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo } @{ 'App.dll' = $cascadeB } @{ 'App.dll' = $cascadeA } @{ 'App.dll' = $darwinArmBytes }
Set-NugetgoRidAssets -Package $pkg -Assemblies $assemblies -TargetFramework net10.0
$e = Entries $pkg
Check 'lib/ carries the reference (windows) flavor' (Same $e['lib/net10.0/App.dll'] $alpha) 'lib/ is not the windows assembly'
Check 'runtimes/win-x64 carries windows' (Same $e['runtimes/win-x64/lib/net10.0/App.dll'] $alpha) 'missing or wrong'
Check 'runtimes/linux-x64 carries linux' (Same $e['runtimes/linux-x64/lib/net10.0/App.dll'] $bravo) 'missing or wrong'
Check 'runtimes/osx-x64 carries darwin' (Same $e['runtimes/osx-x64/lib/net10.0/App.dll'] $cascadeB) 'missing or wrong'
Check 'runtimes/osx-arm64 carries darwin/arm64, not osx-x64''s bytes' (Same $e['runtimes/osx-arm64/lib/net10.0/App.dll'] $darwinArmBytes) 'missing or wrong'
Check 'runtimes/linux-arm64 carries linux/arm64, not linux-x64''s bytes' (Same $e['runtimes/linux-arm64/lib/net10.0/App.dll'] $cascadeA) 'missing or wrong'
Check 'nothing else moved (the nuspec and README are untouched)' (
    (Same $e['README.md'] ([System.Text.Encoding]::UTF8.GetBytes('README.md'))) -and @($e.Keys | Where-Object { $_ -like 'runtimes/*' }).Count -eq 5) "entries: $(@($e.Keys | Sort-Object) -join ', ')"
Check 'the read-back accepts it' (@(Test-NugetgoRidLayout -Package $pkg -Assemblies $assemblies -TargetFramework net10.0).Count -eq 0) "$(@(Test-NugetgoRidLayout -Package $pkg -Assemblies $assemblies -TargetFramework net10.0) -join ' | ')"

$bad = Join-Path $scratch 'mac-x64-bytes.nupkg'
New-Package $bad @{ 'App.dll' = $bravo }
Set-NugetgoRidAssets -Package $bad -Assemblies $assemblies -TargetFramework net10.0
$zip = [System.IO.Compression.ZipFile]::Open($bad, 'Update')
try { $old = $zip.GetEntry('runtimes/osx-arm64/lib/net10.0/App.dll'); if ($old) { $old.Delete() }; $s = $zip.CreateEntry('runtimes/osx-arm64/lib/net10.0/App.dll').Open(); try { $s.Write($cascadeB, 0, $cascadeB.Length) } finally { $s.Dispose() } }
finally { $zip.Dispose() }
$why = @(Test-NugetgoRidLayout -Package $bad -Assemblies $assemblies -TargetFramework net10.0)
Check 'the read-back refuses osx-arm64 carrying osx-x64''s darwin/amd64 bytes (the layout before D6)' (@($why | Where-Object { $_ -like '*osx-arm64*darwin/arm64*' }).Count -ge 1) "reasons: $($why -join ' | ')"
$short = Join-Path $scratch 'short.nupkg'
New-Package $short @{ 'App.dll' = $bravo }
$why = @(Test-NugetgoRidLayout -Package $short -Assemblies $assemblies -TargetFramework net10.0)
Check 'the read-back refuses a package with no runtimes/ folder' (@($why | Where-Object { $_ -like '*win-x64*' }).Count -ge 1) "reasons: $($why -join ' | ')"
$wrongSet = $null
try { New-Package $short @{ 'Other.dll' = $bravo }; Set-NugetgoRidAssets -Package $short -Assemblies $assemblies -TargetFramework net10.0 }
catch { $wrongSet = $_.Exception.Message }
Check 'a package whose lib/ is not the flavors'' assembly set is refused' ($wrongSet -like '*Other.dll*') "got: $wrongSet"

Write-Host 'the pack''s layout decision covers every package (P3''s INC 2b read: a -Cgo block re-parented its elseif)'
$neutral = Join-Path $scratch 'neutral.nupkg'
New-Package $neutral @{ 'App.dll' = $bravo }
Check 'a RID-specific package with the release''s layout is accepted' (@(Test-NugetgoPackedLayout -Package $pkg -Assemblies $assemblies -TargetFramework net10.0 -RidSpecific).Count -eq 0) "$(@(Test-NugetgoPackedLayout -Package $pkg -Assemblies $assemblies -TargetFramework net10.0 -RidSpecific) -join ' | ')"
Check 'a neutral package with no runtimes/ is accepted' (@(Test-NugetgoPackedLayout -Package $neutral -Assemblies $assemblies -TargetFramework net10.0).Count -eq 0) "$(@(Test-NugetgoPackedLayout -Package $neutral -Assemblies $assemblies -TargetFramework net10.0) -join ' | ')"
$why = @(Test-NugetgoPackedLayout -Package $pkg -Assemblies $assemblies -TargetFramework net10.0)
Check 'a package read as neutral that carries runtimes/ is refused, naming an entry' ($why.Count -eq 1 -and $why[0] -like '*neutral*runtimes/*') "reasons: $($why -join ' | ')"
$why = @(Test-NugetgoPackedLayout -Package $short -Assemblies $assemblies -TargetFramework net10.0 -RidSpecific)
Check 'a RID-specific package with no runtimes/ is refused' (@($why | Where-Object { $_ -like '*win-x64*' }).Count -ge 1) "reasons: $($why -join ' | ')"
$packText = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
$calls = [regex]::Matches($packText, '(?m)^(?<indent>[ \t]*)\$layout = @\(Test-NugetgoPackedLayout -Package \$nupkg ')
Check 'nugetgo-pack.ps1 makes the layout decision once, at top level, for every package' ($calls.Count -eq 1 -and $calls[0].Groups['indent'].Value -eq '' -and $packText -notmatch 'Test-NugetgoRidLayout -Package \$nupkg') "calls at top level: $(@($calls | Where-Object { $_.Groups['indent'].Value -eq '' }).Count) of $($calls.Count)"
Remove-Item -Recurse -Force $scratch

Write-Host 'requires are the union of the flavors'' (COORD ruling 2026-10-12, for cobra)'
function Req($module, $version, $id, $pv) { [pscustomobject]@{ Module = $module; Version = $version; NuGetId = $id; PackageVersion = $pv } }
$trap = Req 'github.com/inconshreveable/mousetrap' 'v1.1.0' 'nugetgo.github.com.inconshreveable.mousetrap' '1.1.0'
$pflag = Req 'github.com/spf13/pflag' 'v1.0.9' 'nugetgo.github.com.spf13.pflag' '1.0.9'
$named = @('github.com/inconshreveable/mousetrap=nugetgo.github.com.inconshreveable.mousetrap@1.1.0', 'github.com/spf13/pflag=nugetgo.github.com.spf13.pflag@1.0.9')
$union = @(try { Merge-NugetgoFlavorRequires -Requires (Flavors @($pflag, $trap) @($pflag) @($pflag)) -ThirdPartyPackage $named } catch { "threw: $($_.Exception.Message)" })
Check 'a require only windows has is in the union, once each, sorted by module' (
    (@($union | ForEach-Object { $_.Module }) -join ',') -ceq 'github.com/inconshreveable/mousetrap,github.com/spf13/pflag') "got $(@($union | ForEach-Object { if ($_.Module) { $_.Module } else { $_ } }) -join ', ')"
$same = @(Merge-NugetgoFlavorRequires -Requires (Flavors @($pflag) @($pflag) @($pflag)) -ThirdPartyPackage @($named[1]))
Check 'flavors that agree give their own requires' ((@($same | ForEach-Object { $_.Module }) -join ',') -ceq 'github.com/spf13/pflag') "got $(@($same | ForEach-Object { $_.Module }) -join ', ')"
$why = $null
try { Merge-NugetgoFlavorRequires -Requires (Flavors @($trap) @((Req 'github.com/inconshreveable/mousetrap' 'v1.0.0' 'nugetgo.github.com.inconshreveable.mousetrap' '1.0.0')) @($trap)) -ThirdPartyPackage @($named[0]) | Out-Null } catch { $why = $_.Exception.Message }
Check 'one module at two versions across flavors is refused' ($why -like '*mousetrap*v1.0.0*v1.1.0*' -or $why -like '*mousetrap*v1.1.0*v1.0.0*') "got: $why"
$why = $null
try { Merge-NugetgoFlavorRequires -Requires (Flavors @($pflag) @($pflag) @($pflag)) -ThirdPartyPackage $named | Out-Null } catch { $why = $_.Exception.Message }
Check 'a -ThirdPartyPackage no flavor references is refused' ($why -like '*mousetrap*no flavor*') "got: $why"

Write-Host 'a RID-specific package discloses the flavors its self-description does not describe (COORD ruling 2026-10-12)'
$line = Get-NugetgoFlavorDisclosure -ProofFlavor 'linux/amd64' -RidSpecific
Check 'it names the described flavor and the four others' ("$line" -like '*describes the linux/amd64 flavor*' -and "$line" -like '*windows/amd64, darwin/amd64, linux/arm64 and darwin/arm64 flavors*' -and "$line" -like '*source-metadata.txt*') "got '$line'"
Check 'it is one line' ("$line" -notmatch "`n") "got '$line'"
Check 'a neutral package carries no such line' ($null -eq (Get-NugetgoFlavorDisclosure -ProofFlavor 'linux/amd64')) 'a neutral package got a line'

Write-Host "ran $ran, failed $failed"
exit $failed
