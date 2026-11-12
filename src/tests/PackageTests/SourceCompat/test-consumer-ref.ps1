#Requires -Version 7
<#
.SYNOPSIS
    Self-test of arm H's ref logic (ConsumerRef.ps1) against a local origin: no fetch changes a full clone's depth.

.DESCRIPTION
    Builds, under -WorkRoot, a three-commit origin with release tags and a branch, a full clone and a depth-1 clone.
    The control arm runs the command arm H used before (git fetch --depth 1) on a full clone and must see the clone
    turn shallow, so the instrument can see the defect. Then Get-ConsumerCommit must:
      - resolve a tag already in a full clone with no fetch, the clone staying full;
      - fetch a branch into a full clone without --depth, the clone staying full;
      - fetch a tag into a shallow clone with --depth 1, and resolve it;
    and Resolve-ConsumerRef must pick the newest nuget-<a.b.c.d> tag below a prerelease and below a release.
    Test-ConsumerOverride must accept a citation of the tree's moved break floor and its Upgrading entry, and refuse
    each malformed or untrue one by name.
    Exit 0 when every check passes.
#>
param([Parameter(Mandatory)] [string] $WorkRoot)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ConsumerRef.ps1')

$failed = 0
function Check([string]$Name, [bool]$Ok, [string]$Detail = '') {
    if ($Ok) { Write-Host "PASS $Name" } else { Write-Host "FAIL $Name $Detail"; $script:failed++ }
}
# Named apart from `git`: PowerShell resolves command names case-insensitively, so a function named Git would call itself.
function Invoke-GitIn([string]$Dir) { & git -C $Dir -c commit.gpgsign=false -c tag.gpgsign=false @args 2>&1 | Out-Null; if ($LASTEXITCODE -ne 0) { throw "git $args failed in $Dir" } }
function Depth([string]$Dir) { [int](& git -C $Dir rev-list --count HEAD) }
function IsShallow([string]$Dir) { (& git -C $Dir rev-parse --is-shallow-repository).Trim() -eq 'true' }

New-Item -ItemType Directory -Force $WorkRoot | Out-Null
$WorkRoot = (Resolve-Path $WorkRoot).Path
if (@(Get-ChildItem -Force $WorkRoot).Count -gt 0) { throw "-WorkRoot must be empty: $WorkRoot" }

$src = Join-Path $WorkRoot 'src'
& git init -q -b main $src
foreach ($i in 1..3) {
    Set-Content -LiteralPath (Join-Path $src 'f') -Value $i
    Invoke-GitIn $src add f
    Invoke-GitIn $src -c user.email=t@example.invalid -c user.name=t commit -q -m "c$i"
}
Invoke-GitIn $src tag nuget-1.0.0.1 HEAD~2
Invoke-GitIn $src tag nuget-1.0.0.2 HEAD~1
Invoke-GitIn $src tag nuget-1.0.0.3 HEAD
Invoke-GitIn $src branch consumer-red HEAD~1
$origin = Join-Path $WorkRoot 'origin.git'
& git clone -q --bare $src $origin

$url = 'file://' + ($origin -replace '\\', '/')
function FullClone([string]$Name) { $d = Join-Path $WorkRoot $Name; & git clone -q --no-tags $origin $d; Invoke-GitIn $d fetch -q origin 'refs/tags/nuget-1.0.0.2:refs/tags/nuget-1.0.0.2'; $d }

# Control: the command arm H ran before turns a full clone shallow.
$control = FullClone 'control'
Check 'control: a fresh full clone is not shallow' (-not (IsShallow $control))
Invoke-GitIn $control fetch -q --depth 1 --no-tags origin nuget-1.0.0.1
Check 'control: the old `git fetch --depth 1` turns the full clone shallow' (IsShallow $control) "depth $(Depth $control)"

# A tag already present: no fetch, the clone stays full.
$full = FullClone 'full'
$before = Depth $full
$log = Join-Path $WorkRoot 'tag.log'
$commit = Get-ConsumerCommit $full 'nuget-1.0.0.2' $log
Check 'a tag in the repository resolves' ($commit -eq (& git -C $src rev-parse 'nuget-1.0.0.2^{commit}').Trim()) $commit
Check 'a tag in the repository is not fetched' ((Get-Content -Raw $log) -match 'no fetch')
Check 'a full clone stays full after a tag read' ((-not (IsShallow $full)) -and (Depth $full) -eq $before)

# A branch: fetched without --depth, the clone stays full.
$commit = Get-ConsumerCommit $full 'consumer-red' (Join-Path $WorkRoot 'branch.log')
Check 'a branch resolves' ($commit -eq (& git -C $src rev-parse 'consumer-red').Trim()) $commit
Check 'a full clone stays full after a branch fetch' ((-not (IsShallow $full)) -and (Depth $full) -eq $before) "shallow $(IsShallow $full), depth $(Depth $full)"
Check 'a full clone has no .git/shallow' (-not (Test-Path (Join-Path $full '.git/shallow')))

# A shallow clone: a tag it lacks is fetched at depth 1 and resolves.
$shallow = Join-Path $WorkRoot 'shallow'
& git clone -q --depth 1 --no-tags $url $shallow
Check 'a depth-1 clone is shallow' (IsShallow $shallow)
$commit = Get-ConsumerCommit $shallow 'nuget-1.0.0.1' (Join-Path $WorkRoot 'shallow.log')
Check 'a shallow clone fetches and resolves a tag it lacked' ($commit -eq (& git -C $src rev-parse 'nuget-1.0.0.1^{commit}').Trim()) $commit

# The tag choice.
$pick = Resolve-ConsumerRef $full '1.0.0.3-ci.7'
Check 'a prerelease of build 3 picks the build-2 tag' ($pick -eq 'nuget-1.0.0.2') $pick
$pick = Resolve-ConsumerRef $full '1.0.0.3'
Check 'a published build 3 picks the build-2 tag' ($pick -eq 'nuget-1.0.0.2') $pick
$pick = Resolve-ConsumerRef $full '1.0.0.1'
Check 'no tag below the first release reads none' ($null -eq $pick) $pick

# The override citation, against a scratch tree.
$tree = Join-Path $WorkRoot 'tree'
New-Item -ItemType Directory -Force (Join-Path $tree 'src/core/golib/buildTransitive'), (Join-Path $tree 'docs') | Out-Null
Set-Content -LiteralPath (Join-Path $tree 'src/core/golib/buildTransitive/go.lib.targets') -Value '<Project><PropertyGroup><GoBreakFloor>1.0.0.3</GoBreakFloor></PropertyGroup></Project>'
Set-Content -LiteralPath (Join-Path $tree 'docs/NEWS.md') -Value "## 1.0.0.3`n`n**Upgrading from 1.0.0.2.** Renamed X to Y."
$ok = Test-ConsumerOverride $tree 'floor=1.0.0.3;news=docs/NEWS.md' '1.0.0.2'
Check 'an override citing the moved floor and the Upgrading entry is accepted' ($null -eq $ok.Error) $ok.Error
foreach ($case in @(
        @{ Name = 'a citation in the wrong form'; Cite = 'floor 1.0.0.3'; Previous = '1.0.0.2'; Want = 'is not floor=' },
        @{ Name = "a floor that is not the tree's"; Cite = 'floor=1.0.0.4;news=docs/NEWS.md'; Previous = '1.0.0.2'; Want = "is not this tree's GoBreakFloor" },
        @{ Name = 'a floor that has not moved'; Cite = 'floor=1.0.0.3;news=docs/NEWS.md'; Previous = '1.0.0.3'; Want = 'has not moved above' },
        @{ Name = 'release notes that are not in the tree'; Cite = 'floor=1.0.0.3;news=docs/NONE.md'; Previous = '1.0.0.2'; Want = 'are not a file in this tree' },
        @{ Name = 'release notes whose Upgrading entry is for another release'; Cite = 'floor=1.0.0.3;news=docs/NEWS.md'; Previous = '1.0.0.1'; Want = "hold no 'Upgrading from 1.0.0.1' entry" })) {
    $r = Test-ConsumerOverride $tree $case.Cite $case.Previous
    Check "refused: $($case.Name)" ($r.Error -and $r.Error.Contains($case.Want)) $r.Error
}
Set-Content -LiteralPath (Join-Path $tree 'docs/NEWS.md') -Value "## 1.0.0.3`n`nNo upgrading entry."
$r = Test-ConsumerOverride $tree 'floor=1.0.0.3;news=docs/NEWS.md' '1.0.0.2'
Check 'refused: release notes with no Upgrading entry' ($r.Error -and $r.Error.Contains("hold no 'Upgrading from 1.0.0.2' entry")) $r.Error

Write-Host "checks failed: $failed"
exit $failed
