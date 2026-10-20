<#
.SYNOPSIS
    Table-driven arms for NugetgoBehaviorProof.psm1: the behavioral proof of a module with no Go tests (COORD ruling
    2026-10-12, the cobra increment; mousetrap first). One row per shipped RID, written ON that RID's own host by
    Invoke-NugetgoBehaviorRow.ps1; the pack refuses a release unless every RID has a row, each row's dotnet and go
    outputs are equal, each row's assembly is the packed bytes for its RID, and each row was recorded on its file's RID.
    A row never stands for another RID, however alike their assemblies. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoBehaviorProof.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
function Sha([byte[]]$b) { 'sha256-' + ([BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash($b)) -replace '-', '').ToLowerInvariant() }

$scratch = Join-Path ([System.IO.Path]::GetTempPath()) "nugetgo-behavior-$PID"
New-Item -ItemType Directory -Force $scratch | Out-Null
$module = 'github.com/inconshreveable/mousetrap'
$windowsDll = [System.Text.Encoding]::ASCII.GetBytes('windows flavor bytes')
$unixDll = [System.Text.Encoding]::ASCII.GetBytes('unix flavor bytes')
$assembly = 'github.com.inconshreveable.mousetrap.dll'
# What the package carries for each RID (runtimes/<rid>/lib/<tfm>/, or lib/ for every RID when neutral).
$packed = @{ 'win-x64' = @{ $assembly = $windowsDll }; 'linux-x64' = @{ $assembly = $unixDll }; 'osx-x64' = @{ $assembly = $unixDll }; 'osx-arm64' = @{ $assembly = $unixDll } }
$goosOf = @{ 'win-x64' = 'windows'; 'linux-x64' = 'linux'; 'osx-x64' = 'darwin'; 'osx-arm64' = 'darwin' }
function Write-Row([string]$rid, [hashtable]$over = @{}) {
    $fields = [ordered]@{
        'module' = $module; 'version' = 'v1.1.0'; 'package' = 'nugetgo.github.com.inconshreveable.mousetrap 1.1.0-int.1'
        'rid' = $rid; 'runtime-rid' = $rid; 'goos' = $goosOf[$rid]; 'host' = "hosted $rid runner, image 20261005.1"; 'os' = "an OS description for $rid"
        'go' = 'go1.24.13'; 'dotnet' = '10.0.112'
        'go-output' = 'sha256-aaaa'; 'dotnet-output' = 'sha256-aaaa'; 'outputs-equal' = 'true'
        'assembly' = "$assembly $(Sha $packed[$rid][$assembly])"; 'date' = '2026-10-12'
    }
    foreach ($k in $over.Keys) { $fields[$k] = $over[$k] }
    $text = "# nugetgo behavioral proof row`n" + (($fields.Keys | Where-Object { $null -ne $fields[$_] } | ForEach-Object { "${_}: $($fields[$_])" }) -join "`n") + "`n"
    $path = Join-Path $scratch "$rid.md"
    [System.IO.File]::WriteAllText($path, $text)
    $path
}
function Rows([hashtable]$overByRid = @{}) {
    $rows = @{}
    foreach ($rid in 'win-x64', 'linux-x64', 'osx-x64', 'osx-arm64') {
        $o = if ($overByRid.ContainsKey($rid)) { $overByRid[$rid] } else { @{} }
        if ($o -eq 'missing') { continue }
        $rows[$rid] = Read-NugetgoBehaviorRow -Path (Write-Row $rid $o)
    }
    $rows
}
function Reasons($rows) { @(Test-NugetgoBehaviorRows -Rows $rows -Assemblies $packed -ModulePath $module -GoVersion 'v1.1.0') }

Write-Host 'a row reads back'
$row = Read-NugetgoBehaviorRow -Path (Write-Row 'win-x64')
Check 'its fields' ($row -and $row.rid -ceq 'win-x64' -and $row.'outputs-equal' -ceq 'true' -and $row.assembly -like "$assembly sha256-*") "got $($row | Out-String)"

Write-Host 'the four refusals, and the clean set'
Check 'four rows, outputs equal, bytes bound, each on its own RID: accepted' ((Reasons (Rows)).Count -eq 0) "$((Reasons (Rows)) -join ' | ')"
$why = Reasons (Rows @{ 'osx-arm64' = 'missing' })
Check 'a missing row for a shipped RID is refused' (@($why | Where-Object { $_ -like '*osx-arm64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'linux-x64' = @{ 'dotnet-output' = 'sha256-bbbb'; 'outputs-equal' = 'false' } })
Check 'a row whose outputs differ is refused' (@($why | Where-Object { $_ -like '*linux-x64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'linux-x64' = @{ 'dotnet-output' = 'sha256-bbbb' } })
Check 'a row claiming equal outputs whose hashes differ is refused' (@($why | Where-Object { $_ -like '*linux-x64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'win-x64' = @{ 'assembly' = "$assembly $(Sha $unixDll)" } })
Check 'a row whose assembly is not the packed bytes for its RID is refused' (@($why | Where-Object { $_ -like '*win-x64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'osx-x64' = @{ 'runtime-rid' = 'linux-x64' } })
Check 'a row recorded on another RID than its file''s is refused (no row stands for another)' (@($why | Where-Object { $_ -like '*osx-x64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'osx-arm64' = @{ 'rid' = 'osx-x64' } })
Check 'a row whose own rid field is not its file''s is refused' (@($why | Where-Object { $_ -like '*osx-arm64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'win-x64' = @{ 'version' = 'v1.0.0' } })
Check 'a row for another module version is refused' (@($why | Where-Object { $_ -like '*v1.0.0*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'win-x64' = @{ 'host' = $null } })
Check 'a row with no host is refused' (@($why | Where-Object { $_ -like '*win-x64*host*' }).Count -ge 1) "$($why -join ' | ')"

$why = Reasons (Rows @{ 'osx-x64' = @{ 'goos' = 'linux' } })
Check 'a row whose recorded GOOS is not its file''s GOOS is refused (COORD: hosted rows too)' (@($why | Where-Object { $_ -like '*osx-x64*linux*darwin*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'win-x64' = @{ 'goos' = $null } })
Check 'a row that records no GOOS is refused' (@($why | Where-Object { $_ -like '*win-x64*goos*' }).Count -ge 1) "$($why -join ' | ')"

Write-Host 'the page says plainly that upstream has no Go tests'
$page = New-NugetgoBehaviorPage -ModulePath $module -GoVersion 'v1.1.0' -Rows (Rows)
Check 'its title' ("$page" -match '(?m)^# `github\.com/inconshreveable/mousetrap` -- behavioral proof \(upstream has no Go tests\)$') "got: $(("$page" -split "`n")[0])"
Check 'it says it is not a test comparison' ("$page" -like '*not a test comparison*') 'missing'
Check 'one table row per RID, with the assembly sha and the verdict' (@(("$page" -split "`n") | Where-Object { $_ -match '^\| (win-x64|linux-x64|osx-x64|osx-arm64) \|' }).Count -eq 4) "got:`n$page"
Check 'each row names its host' ("$page" -like '*image 20261005.1*') 'no host in the table'

Write-Host 'a module with Go tests takes the test proof, never this one'
$withTests = Join-Path $scratch 'with'; New-Item -ItemType Directory -Force (Join-Path $withTests 'sub') | Out-Null
[System.IO.File]::WriteAllText((Join-Path $withTests 'sub/x_test.go'), "package sub`n")
$without = Join-Path $scratch 'without'; New-Item -ItemType Directory -Force (Join-Path $without 'testdata') | Out-Null
[System.IO.File]::WriteAllText((Join-Path $without 'testdata/y_test.go'), "package testdata`n")
[System.IO.File]::WriteAllText((Join-Path $without 'trap.go'), "package trap`n")
Check 'a _test.go file means the module has Go tests' (Test-NugetgoModuleHasGoTests -ModuleDir $withTests) 'not seen'
Check 'a _test.go file under testdata/ is not a Go test' (-not (Test-NugetgoModuleHasGoTests -ModuleDir $without)) 'testdata counted'

Write-Host 'the assemblies the package carries for each RID, as NuGet selects them'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
function New-Nupkg([string]$path, [hashtable]$files) {
    if (Test-Path $path) { Remove-Item $path }
    $zip = [System.IO.Compression.ZipFile]::Open($path, 'Create')
    try { foreach ($name in $files.Keys) { $s = $zip.CreateEntry($name).Open(); try { $s.Write($files[$name], 0, $files[$name].Length) } finally { $s.Dispose() } } }
    finally { $zip.Dispose() }
    $path
}
$neutral = New-Nupkg (Join-Path $scratch 'neutral.nupkg') @{ "lib/net10.0/$assembly" = $unixDll; 'README.md' = $unixDll }
$byRid = Get-NugetgoPackedRidAssemblies -Package $neutral -TargetFramework net10.0
Check 'a neutral package carries lib/ for every shipped RID' (
    @('win-x64', 'linux-x64', 'osx-x64', 'osx-arm64' | Where-Object { $byRid -and $byRid[$_] -and [System.Linq.Enumerable]::SequenceEqual([byte[]]$byRid[$_][$assembly], [byte[]]$unixDll) }).Count -eq 4) "got: $($byRid | Out-String)"
$specific = New-Nupkg (Join-Path $scratch 'specific.nupkg') @{
    "lib/net10.0/$assembly" = $windowsDll; "runtimes/win-x64/lib/net10.0/$assembly" = $windowsDll; "runtimes/linux-x64/lib/net10.0/$assembly" = $unixDll
    "runtimes/osx-x64/lib/net10.0/$assembly" = $unixDll; "runtimes/osx-arm64/lib/net10.0/$assembly" = $unixDll }
$byRid = Get-NugetgoPackedRidAssemblies -Package $specific -TargetFramework net10.0
Check 'a RID-specific package carries runtimes/<rid>/ for each RID, never lib/' (
    $byRid -and $byRid['linux-x64'] -and $byRid['win-x64'] -and [System.Linq.Enumerable]::SequenceEqual([byte[]]$byRid['linux-x64'][$assembly], [byte[]]$unixDll) -and
    [System.Linq.Enumerable]::SequenceEqual([byte[]]$byRid['win-x64'][$assembly], [byte[]]$windowsDll)) "got: $($byRid | Out-String)"
Check 'the clean rows bind to the RID-specific package read back' ((@(Test-NugetgoBehaviorRows -Rows (Rows) -Assemblies $byRid -ModulePath $module -GoVersion 'v1.1.0')).Count -eq 0) 'refused'
$byRid = Get-NugetgoPackedRidAssemblies -Package $neutral -TargetFramework net10.0
$why = @(Test-NugetgoBehaviorRows -Rows (Rows) -Assemblies $byRid -ModulePath $module -GoVersion 'v1.1.0')
Check 'the same rows are refused against a package whose win-x64 bytes are not the ones the row ran' (@($why | Where-Object { $_ -like '*win-x64*not the packed bytes*' }).Count -eq 1) "$($why -join ' | ')"

Write-Host 'which proof a pack carries (D7, and the behavioral proof for a module with no Go tests)'
function Mode([hashtable]$a) { Get-NugetgoProofMode @a }
Check 'a validated module: -ValidationDir' ((Mode @{ ValidationDir = 'v'; ModuleHasGoTests = $true; Release = $true }).Mode -ceq 'validated') 'not validated'
Check 'a module with no Go tests: -BehaviorProofDir' ((Mode @{ BehaviorProofDir = 'b'; ModuleHasGoTests = $false; Release = $true }).Mode -ceq 'behavioral') 'not behavioral'
Check 'a rehearsal: -UnvalidatedReason' ((Mode @{ UnvalidatedReason = 'r'; ModuleHasGoTests = $false }).Mode -ceq 'unvalidated') 'not unvalidated'
Check 'a module WITH Go tests never takes a behavioral proof' ((Mode @{ BehaviorProofDir = 'b'; ModuleHasGoTests = $true; Release = $true }).Reason -like '*has Go tests*') 'not refused by name'
Check '-BehaviorProofDir and -ValidationDir are exclusive' ((Mode @{ BehaviorProofDir = 'b'; ValidationDir = 'v'; ModuleHasGoTests = $false }).Reason -like '*exclusive*') 'not refused'
Check '-BehaviorProofDir and -UnvalidatedReason are exclusive' ((Mode @{ BehaviorProofDir = 'b'; UnvalidatedReason = 'r'; ModuleHasGoTests = $false }).Reason -like '*exclusive*') 'not refused'
Check 'an unvalidated module is never a release' ((Mode @{ UnvalidatedReason = 'r'; Release = $true; ModuleHasGoTests = $false }).Reason -like '*never packed as a release*') 'not refused'
Check 'a release with no proof is refused, and names the behavioral proof for a module with no Go tests' ((Mode @{ Release = $true; ModuleHasGoTests = $false }).Reason -like '*-BehaviorProofDir*') 'not refused by name'

Write-Host 'a module WITH Go tests: hosted sample rows BESIDE the test proof, never in its place (COORD ruling 2026-10-12)'
$both = Mode @{ ValidationDir = 'v'; BehaviorProofDir = 'b'; ModuleHasGoTests = $true; Release = $true }
Check 'rows beside -ValidationDir: the test proof stays the validation, the rows are additional' ($both.Mode -ceq 'validated' -and $both.SampleRows -and -not $both.Reason) "got: $($both | Out-String)"
$alone = Mode @{ ValidationDir = 'v'; ModuleHasGoTests = $true; Release = $true }
Check '-ValidationDir alone is exactly today''s pack: no sample rows owed' ($alone.Mode -ceq 'validated' -and -not $alone.SampleRows -and -not $alone.Reason) "got: $($alone | Out-String)"
Check 'rows alone, for a module with Go tests, are refused and name -ValidationDir' ((Mode @{ BehaviorProofDir = 'b'; ModuleHasGoTests = $true; Release = $true }).Reason -like '*has Go tests*-ValidationDir*') 'not refused by name'
$section = New-NugetgoSampleBehaviorSection -ModulePath $module -GoVersion 'v1.1.0' -Rows (Rows)
Check 'the sample section is a level-2 heading under the test comparison' ("$section" -match '(?m)^## Sample behavior on 4 RIDs$') "got: $(("$section" -split "`n")[0])"
Check 'it says the rows are in addition to the test comparison, and never that there are no tests' ("$section" -like '*in addition to the test comparison*' -and "$section" -notlike '*not a test comparison*' -and "$section" -notlike '*no Go tests*') "got:`n$section"
Check 'one table row per RID' (@(("$section" -split "`n") | Where-Object { $_ -match '^\| (win-x64|linux-x64|osx-x64|osx-arm64) \|' }).Count -eq 4) "got:`n$section"

Write-Host 'a module of several packages: the row lists every assembly the package carries for its RID'
$second = 'github.com.example.mod.sub.dll'
$secondBytes = [System.Text.Encoding]::ASCII.GetBytes('the second package')
$packedTwo = @{}
foreach ($rid in $packed.Keys) { $packedTwo[$rid] = @{ $assembly = $packed[$rid][$assembly]; $second = $secondBytes } }
function RowsTwo([hashtable]$drop = @{}) {
    $o = @{}
    foreach ($rid in 'win-x64', 'linux-x64', 'osx-x64', 'osx-arm64') {
        $list = @("$assembly $(Sha $packed[$rid][$assembly])")
        if (-not $drop.ContainsKey($rid)) { $list += "$second $(Sha $secondBytes)" }
        $o[$rid] = @{ 'assembly' = ($list | Sort-Object) -join '; ' }
    }
    Rows $o
}
$why = @(Test-NugetgoBehaviorRows -Rows (RowsTwo) -Assemblies $packedTwo -ModulePath $module -GoVersion 'v1.1.0')
Check 'a row listing both packed assemblies, each at its packed bytes, is accepted' ($why.Count -eq 0) "$($why -join ' | ')"
$why = @(Test-NugetgoBehaviorRows -Rows (RowsTwo @{ 'linux-x64' = 1 }) -Assemblies $packedTwo -ModulePath $module -GoVersion 'v1.1.0')
Check 'a row that leaves out one of the packed assemblies is refused, naming it' (@($why | Where-Object { $_ -like "*linux-x64*$second*" }).Count -ge 1) "$($why -join ' | ')"

Write-Host 'a row travels as an annotation (C1: annotations come through the API; artifacts and summaries are blob-served)'
$rowText = [System.IO.File]::ReadAllText((Write-Row 'osx-arm64'))
$helper = Join-Path $PSScriptRoot '../../../.github/annotate-summary.ps1'
$emitted = @(& $helper -Title 'nugetgo-row osx-arm64' -Lines @(ConvertTo-NugetgoRowAnnotationText -Text $rowText) 6>&1 | ForEach-Object { "$_" })
Check 'the helper emits the row as ONE notice, titled exactly nugetgo-row <rid>' ($emitted.Count -eq 1 -and $emitted[0] -clike '::notice title=nugetgo-row osx-arm64::*') "got: $($emitted -join ' | ')"
# What GET .../check-runs/{id}/annotations returns for it: the title and the message, unescaped.
function Annotation([string]$line) {
    if ($line -notmatch '^::(?<level>\w+) title=(?<title>[^:]*)::(?<message>.*)$') { return $null }
    [pscustomobject]@{ annotation_level = $Matches['level']; title = $Matches['title'].Replace('%2C', ',').Replace('%3A', ':').Replace('%0A', "`n").Replace('%0D', "`r").Replace('%25', '%')
        message = $Matches['message'].Replace('%0A', "`n").Replace('%0D', "`r").Replace('%25', '%') }
}
$others = @([pscustomobject]@{ annotation_level = 'notice'; title = 'release-smoke linux/linux-x64'; message = 'a summary' },
    [pscustomobject]@{ annotation_level = 'warning'; title = 'nugetgo-row'; message = 'Node.js 20 actions are deprecated' })
$decoded = ConvertFrom-NugetgoRowAnnotations -Annotations (@($others) + @(Annotation $emitted[0]))
Check 'the row decodes back byte for byte, keyed by its RID; other annotations are ignored' ($decoded -and $decoded.Count -eq 1 -and $decoded['osx-arm64'] -ceq $rowText) "got: $($decoded | Out-String)"
$bad = $null; try { ConvertFrom-NugetgoRowAnnotations -Annotations @([pscustomobject]@{ annotation_level = 'notice'; title = 'nugetgo-row win-x64'; message = 'not base64 at all!' }) | Out-Null } catch { $bad = $_.Exception.Message }
Check 'a mangled row annotation is refused by name, never skipped' ($bad -like '*win-x64*') "got: $bad"
$bad = $null; try { ConvertFrom-NugetgoRowAnnotations -Annotations @((Annotation $emitted[0]) | ForEach-Object { [pscustomobject]@{ annotation_level = 'notice'; title = 'nugetgo-row linux-x64'; message = $_.message } }) | Out-Null } catch { $bad = $_.Exception.Message }
Check 'an annotation titled for one RID that carries another RID''s row is refused' ($bad -like '*linux-x64*osx-arm64*') "got: $bad"

Write-Host 'the host a row records (C1: the image name and version make a hosted row reproducible)'
$hostedEnv = @{ ImageOS = 'macos15'; ImageVersion = '20261005.1'; RUNNER_OS = 'macOS'; RUNNER_ARCH = 'ARM64'; NUGETGO_RUNNER = 'macos-15' }
$h = Get-NugetgoRowHost -Environment $hostedEnv
Check 'a hosted runner: its label, image and image version' ($h -ceq 'macos-15 (image macos15 20261005.1)') "got: $h"
$bad = $null; try { Get-NugetgoRowHost -Environment @{} | Out-Null } catch { $bad = $_.Exception.Message }
Check 'a host that is not a hosted runner must be named (-HostLabel)' ($bad -like '*-HostLabel*') "got: $bad"
Check 'a named host is recorded as named' ((Get-NugetgoRowHost -Environment @{} -HostLabel 'C2 lane box') -ceq 'C2 lane box') 'not as named'
$text = New-NugetgoBehaviorRowText -Fields ([ordered]@{ module = $module; rid = 'linux-x64' })
Check 'a row''s text is a title line, then one key: value per line' ($text -ceq "# nugetgo behavioral proof row`nmodule: $module`nrid: linux-x64`n") "got: $text"

Write-Host 'the hosted stage on the OS matrix (C1''s conventions)'
$yml = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot '../../../.github/workflows/os-matrix.yml'))
Check 'nugetgo-behavior is a dispatchable stage' ($yml -match '(?m)^\s+- nugetgo-behavior\s*$') 'no such option'
Check 'it has a job budget in $baseTimeout' ($yml -match "(?m)^\s+'nugetgo-behavior'\s+=\s+\d+") 'no budget'
$inputs = [regex]::Match($yml, '(?s)workflow_dispatch:\s*\n\s*inputs:\s*\n(?<body>.*?)\n(permissions|concurrency):').Groups['body'].Value
$names = @([regex]::Matches($inputs, '(?m)^      (?<n>[a-z_]+):\s*$') | ForEach-Object { $_.Groups['n'].Value })
Check 'workflow_dispatch keeps at most 10 inputs' ($names.Count -ge 7 -and $names.Count -le 10) "inputs: $($names -join ', ')"
Check 'each nugetgo input says it is stage-scoped' (@($names | Where-Object { $_ -like 'nugetgo_*' }).Count -ge 2 -and
    @($names | Where-Object { $_ -like 'nugetgo_*' } | Where-Object { $inputs -notmatch "(?s)      ${_}:\s*\n\s*description: 'nugetgo-behavior only: " }).Count -eq 0) "inputs: $($names -join ', ')"
Check 'no job reads inputs.nugetgo_* (inputs reach jobs only through plan''s outputs)' (
    @([regex]::Matches($yml, 'inputs\.nugetgo_\w+')).Count -eq @([regex]::Matches($yml, '(?m)^\s+INPUT_NUGETGO_\w+: \$\{\{ inputs\.nugetgo_\w+ \}\}\s*$')).Count) 'an inputs.nugetgo_ read outside plan''s env'
Check 'the row is annotated in its own step, titled nugetgo-row <rid>' ($yml -match 'annotate-summary\.ps1 -Title "nugetgo-row \$\(\$env:RID\)"') 'no row annotation step'
Check 'the leg runs Invoke-NugetgoBehaviorRow.ps1' ($yml -like '*src/tools/nugetgo/Invoke-NugetgoBehaviorRow.ps1*') 'the runner is not run'
Check 'the runner script exists' (Test-Path (Join-Path $PSScriptRoot 'Invoke-NugetgoBehaviorRow.ps1')) 'missing'
Check 'the fetch script exists' (Test-Path (Join-Path $PSScriptRoot 'Get-NugetgoBehaviorRows.ps1')) 'missing'

Remove-Item -Recurse -Force $scratch
Write-Host "ran $ran, failed $failed"
exit $failed
