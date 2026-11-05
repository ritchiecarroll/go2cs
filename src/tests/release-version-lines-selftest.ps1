#Requires -Version 5.1
<#
.SYNOPSIS
    Guards the release tooling's maintenance-line rules (docs/PLAN-version-lines.md section 6, T1 to T4) with no
    release, tag or GitHub call.

.DESCRIPTION
    Each landed Go minor keeps a release/go1.N maintenance branch, so a security release can be cut on an older line
    after a newer line's first release. The tooling must treat each line on its own:

      T1  push-nuget.ps1's pre-flight (Get-GoNextReleaseVerification, read from the script by the parser): the next
          version must be newer than every recorded release OF ITS GO MINOR, and must exist on no line at all.
            maintenance   1.26.11.4 after 1.27.0.1 is recorded -> passes; the line's newest is 1.26.11.3
            regression    1.26.11.2 after 1.26.11.3 -> refused by name
            existence     1.26.11.4 already an origin tag -> refused, though no newer 1.26 release exists
            new line      1.27.0.1 with only 1.26 releases recorded -> passes, "none on this line yet"
      T2  release-binaries.yml's record step, run as the workflow runs it (bash, against fixture git repositories):
          a tag on release/go1.26 whose record commit lands on that branch is found there and reported as its branch;
          a master release is still found on master.
      T3  the same workflow's draft step, run against a stub gh: a maintenance release's draft is created with
          --latest=false and notes that say to keep the mark cleared; a master release's draft is not.
      T4  release-nuget.ps1's Test-GoReleaseBranch: master and release/go1.N pass; anything else is flagged.

    -RepoRoot names the tree whose tooling is read (default: this checkout), so the same cases can be run against an
    older tree to show them red. Needs git and bash on PATH. Exit 0 clean, 1 on any violation. Writes only under the
    host's temp directory.
#>
[CmdletBinding()]
param([string] $RepoRoot = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent))
$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]
function Check([bool] $Condition, [string] $Message) { if (-not $Condition) { $failures.Add($Message) } }

foreach ($tool in 'git', 'bash') {
    if (-not (Get-Command $tool -CommandType Application -ErrorAction SilentlyContinue)) { Write-Host "release-version-lines selftest: $tool is not on PATH"; exit 1 }
}

$pushNuget = Join-Path $RepoRoot 'src/push-nuget.ps1'
$releaseNuget = Join-Path $RepoRoot 'src/release-nuget.ps1'
$workflow = Join-Path $RepoRoot '.github/workflows/release-binaries.yml'
foreach ($file in $pushNuget, $releaseNuget, $workflow) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { Write-Host "release-version-lines selftest: $file does not exist"; exit 1 }
}

# A script's function, defined here from the script's own text, so the case reads the code that ships and runs none of
# the script's top level. A function the script does not define is a failure by name.
function Import-ScriptFunction([string] $Path, [string] $Name) {
    $tokens = $null; $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseInput([System.IO.File]::ReadAllText($Path), [ref] $tokens, [ref] $errors)
    $fn = $ast.Find({ param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq $Name }, $true)
    if (-not $fn) { $failures.Add("$(Split-Path $Path -Leaf) defines no $Name"); return $false }
    . ([scriptblock]::Create($fn.Extent.Text))
    # Re-export to this script's scope: the dot-source above defines it in this function's scope only.
    Set-Item -Path "function:script:$Name" -Value (Get-Item "function:$Name").ScriptBlock
    return $true
}

# One step's `run: |` block from the workflow, dedented: the first `run: |` after the line that matches $Anchor.
function Get-WorkflowRunBlock([string] $Anchor) {
    $lines = [System.IO.File]::ReadAllLines($workflow)
    $at = -1
    for ($i = 0; $i -lt $lines.Count; $i++) { if ($lines[$i] -match $Anchor) { $at = $i; break } }
    if ($at -lt 0) { throw "release-binaries.yml has no line matching $Anchor" }
    for ($i = $at; $i -lt $lines.Count; $i++) { if ($lines[$i] -match '^(\s*)run: \|\s*$') { $runIndent = $Matches[1].Length; break } }
    if ($i -ge $lines.Count) { throw "release-binaries.yml has no run block after $Anchor" }
    $body = New-Object System.Collections.Generic.List[string]
    $indent = -1
    for ($j = $i + 1; $j -lt $lines.Count; $j++) {
        $line = $lines[$j]
        if ($line.Trim() -eq '') { $body.Add(''); continue }
        $lead = $line.Length - $line.TrimStart().Length
        if ($lead -le $runIndent) { break }
        if ($indent -lt 0) { $indent = $lead }
        $body.Add($line.Substring([Math]::Min($indent, $lead)))
    }
    ($body -join "`n") + "`n"
}

# Native commands write progress to stderr; only their exit codes decide.
function Invoke-Native([string] $Exe, [string[]] $Arguments, [string] $Dir) {
    $ErrorActionPreference = 'Continue'
    if ($Dir) { Push-Location $Dir }
    try { $out = @(& $Exe @Arguments 2>&1 | ForEach-Object { "$_" }) } finally { if ($Dir) { Pop-Location } }
    if ($LASTEXITCODE -ne 0) { throw "$Exe $($Arguments -join ' ') exited $($LASTEXITCODE):`n$($out -join "`n")" }
    $out
}

# A bash script run with the given environment; returns its exit code and its output.
function Invoke-Bash([string] $Script, [hashtable] $Environment, [string] $Dir) {
    $ErrorActionPreference = 'Continue'
    $file = Join-Path $work ("step-" + [guid]::NewGuid().ToString('N') + '.sh')
    [System.IO.File]::WriteAllText($file, $Script.Replace("`r`n", "`n"))
    $saved = @{}
    foreach ($k in $Environment.Keys) { $saved[$k] = [Environment]::GetEnvironmentVariable($k); [Environment]::SetEnvironmentVariable($k, $Environment[$k]) }
    Push-Location $Dir
    try { $out = @(& bash $file 2>&1 | ForEach-Object { "$_" }); $code = $LASTEXITCODE }
    finally {
        Pop-Location
        foreach ($k in $saved.Keys) { [Environment]::SetEnvironmentVariable($k, $saved[$k]) }
    }
    [pscustomobject]@{ Code = $code; Text = ($out -join "`n") }
}

$work = Join-Path ([System.IO.Path]::GetTempPath()) ("release-lines-selftest-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null

try {
    # ---- T1: the pre-flight's monotonicity is per Go minor; existence spans every line. ----
    $loaded = (Import-ScriptFunction $pushNuget 'Compare-GoReleaseStamp') -and (Import-ScriptFunction $pushNuget 'Get-GoNextReleaseVerification')
    [void](Import-ScriptFunction $pushNuget 'Get-GoReleaseLine')
    if ($loaded) {
        function New-Snapshots([string[]] $Versions) {
            $dir = Join-Path $work ("snapshots-" + [guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $dir | Out-Null
            foreach ($v in $Versions) { New-Item -ItemType Directory -Path (Join-Path $dir $v) | Out-Null }
            $dir
        }
        function Props([string] $Base, [int] $Counter) { "<Project><PropertyGroup><GoStdLibVersion>$Base</GoStdLibVersion><GoBuildNumber>$Counter</GoBuildNumber></PropertyGroup></Project>" }
        function Verify([string] $Base, [int] $Counter, [string[]] $Snapshots, [string[]] $Tags, [string[]] $RemoteTags = @()) {
            try { Get-GoNextReleaseVerification -VersionPropsText (Props $Base $Counter) -SnapshotsDir (New-Snapshots $Snapshots) -Tags $Tags -RemoteTags $RemoteTags }
            catch { [pscustomobject]@{ Next = ''; Problems = @("threw: $($_.Exception.Message)"); Newest = ''; NewestFrom = '' } }
        }

        $r = Verify '1.26.11' 3 @('1.26.11.3', '1.27.0.1') @('nuget-1.26.11.3', 'nuget-1.27.0.1')
        Check ($r.Next -eq '1.26.11.4' -and @($r.Problems).Count -eq 0) "T1 maintenance: 1.26.11.4 after 1.27.0.1 was refused: $(@($r.Problems) -join ' | ')"
        Check ($r.Newest -eq '1.26.11.3') "T1 maintenance: the newest on 1.26 reads '$($r.Newest)', want 1.26.11.3"

        $r = Verify '1.26.11' 1 @('1.26.11.3', '1.27.0.1') @('nuget-1.26.11.3', 'nuget-1.27.0.1')
        Check (@($r.Problems | Where-Object { $_ -match 'not strictly newer than recorded release 1\.26\.11\.3' }).Count -eq 1) "T1 regression: 1.26.11.2 after 1.26.11.3 was not refused by name: $(@($r.Problems) -join ' | ')"
        Check (@($r.Problems | Where-Object { $_ -match '1\.27\.0\.1' }).Count -eq 0) "T1 regression: the refusal names 1.27.0.1, another line's release: $(@($r.Problems) -join ' | ')"

        $r = Verify '1.26.11' 3 @('1.26.11.3', '1.27.0.1') @('nuget-1.26.11.3') @('nuget-1.26.11.4', 'nuget-1.27.0.1')
        Check (@($r.Problems | Where-Object { $_ -match 'already EXISTS: tag nuget-1\.26\.11\.4 is on ORIGIN' }).Count -eq 1) "T1 existence: an origin tag nuget-1.26.11.4 was not refused: $(@($r.Problems) -join ' | ')"

        $r = Verify '1.27.0' 0 @('1.26.11.3') @('nuget-1.26.11.3')
        Check ($r.Next -eq '1.27.0.1' -and @($r.Problems).Count -eq 0) "T1 new line: 1.27.0.1 with only 1.26 recorded was refused: $(@($r.Problems) -join ' | ')"
        Check ($r.Newest -eq '' -and $r.NewestFrom -eq 'none on this line yet') "T1 new line: newest '$($r.Newest)' from '$($r.NewestFrom)', want none on this line yet"
    }

    # ---- T4: release-nuget.ps1 accepts master and release/go1.N without its warning. ----
    if (Import-ScriptFunction $releaseNuget 'Test-GoReleaseBranch') {
        foreach ($case in @(@('master', $true), @('release/go1.26', $true), @('release/go1.24', $true),
                            @('release/go1.26.1', $false), @('release/go1', $false), @('claude/some-seat', $false), @('', $false))) {
            Check ((Test-GoReleaseBranch $case[0]) -eq $case[1]) "T4: Test-GoReleaseBranch '$($case[0])' is not $($case[1])"
        }
    }

    # ---- T2: the record step finds a maintenance release's record on its line's branch. ----
    $record = Get-WorkflowRunBlock '^\s+id: tag\s*$'
    $origin = Join-Path $work 'origin.git'
    $dev = Join-Path $work 'dev'
    Invoke-Native git @('init', '--quiet', '--bare', $origin) | Out-Null
    Invoke-Native git @('init', '--quiet', '-b', 'master', $dev) | Out-Null
    foreach ($kv in @(@('user.name', 'selftest'), @('user.email', 'selftest@example.invalid'), @('commit.gpgsign', 'false'), @('tag.gpgsign', 'false'))) {
        Invoke-Native git @('-C', $dev, 'config', $kv[0], $kv[1]) | Out-Null
    }
    New-Item -ItemType Directory -Path (Join-Path $dev 'src/go2cs') | Out-Null
    function Commit([string] $Release, [string] $Message, [string] $Extra = '') {
        [System.IO.File]::WriteAllText((Join-Path $dev 'src/go2cs/corpus-release.txt'), "$Release`n")
        if ($Extra) { [System.IO.File]::WriteAllText((Join-Path $dev 'NOTES.txt'), "$Extra`n") }
        Invoke-Native git @('-C', $dev, 'add', '-A') | Out-Null
        Invoke-Native git @('-C', $dev, 'commit', '--quiet', '-m', $Message) | Out-Null
        @(Invoke-Native git @('-C', $dev, 'rev-parse', 'HEAD'))[0]
    }
    [void](Commit '1.26.11.3' 'release: 1.26.11.3')
    Invoke-Native git @('-C', $dev, 'branch', 'release/go1.26') | Out-Null
    # master moves to the next minor: its tag, then its record.
    [void](Commit '1.26.11.3' 'hop work' 'master after the branch')
    Invoke-Native git @('-C', $dev, 'tag', '-a', 'nuget-1.27.0.1', '-m', 'nuget-1.27.0.1') | Out-Null
    $masterRecord = Commit '1.27.0.1' 'release: 1.27.0.1'
    # the maintenance line: its tag, then its record, on release/go1.26 only.
    Invoke-Native git @('-C', $dev, 'checkout', '--quiet', 'release/go1.26') | Out-Null
    [void](Commit '1.26.11.3' 'security fix' 'a fix on the 1.26 line')
    Invoke-Native git @('-C', $dev, 'tag', '-a', 'nuget-1.26.11.4', '-m', 'nuget-1.26.11.4') | Out-Null
    $lineRecord = Commit '1.26.11.4' 'release: 1.26.11.4'
    Invoke-Native git @('-C', $dev, 'push', '--quiet', $origin, 'master', 'release/go1.26', '--tags') | Out-Null

    function Invoke-RecordStep([string] $Tag) {
        $ci = Join-Path $work ("ci-" + [guid]::NewGuid().ToString('N'))
        Invoke-Native git @('clone', '--quiet', $origin, $ci) | Out-Null
        $outFile = Join-Path $work ("output-" + [guid]::NewGuid().ToString('N'))
        $summary = Join-Path $work ("summary-" + [guid]::NewGuid().ToString('N'))
        $run = Invoke-Bash $record @{ REF_TYPE = 'branch'; REF_NAME = 'master'; INPUT_TAG = $Tag; GITHUB_OUTPUT = $outFile; GITHUB_STEP_SUMMARY = $summary } $ci
        $outputs = @{}
        if (Test-Path $outFile) { foreach ($l in [System.IO.File]::ReadAllLines($outFile)) { if ($l -match '^([^=]+)=(.*)$') { $outputs[$Matches[1]] = $Matches[2] } } }
        $head = @(Invoke-Native git @('-C', $ci, 'rev-parse', 'HEAD'))[0]
        [pscustomobject]@{ Code = $run.Code; Text = $run.Text; Outputs = $outputs; Head = $head }
    }

    $r = Invoke-RecordStep 'nuget-1.26.11.4'
    Check ($r.Code -eq 0) "T2 maintenance: the record step exited $($r.Code): $($r.Text)"
    Check ($r.Outputs['record'] -eq $lineRecord -and $r.Head -eq $lineRecord) "T2 maintenance: record '$($r.Outputs['record'])', HEAD $($r.Head); want the release/go1.26 record $lineRecord"
    Check ($r.Outputs['branch'] -eq 'release/go1.26') "T2 maintenance: branch '$($r.Outputs['branch'])', want release/go1.26"

    $r = Invoke-RecordStep 'nuget-1.27.0.1'
    Check ($r.Code -eq 0) "T2 master control: the record step exited $($r.Code): $($r.Text)"
    Check ($r.Outputs['record'] -eq $masterRecord) "T2 master control: record '$($r.Outputs['record'])', want the master record $masterRecord"
    Check ($r.Outputs['branch'] -eq 'master') "T2 master control: branch '$($r.Outputs['branch'])', want master"

    # ---- T3: the draft step clears the latest mark for a maintenance release only. ----
    $draft = Get-WorkflowRunBlock '^\s+- name: Create the draft'
    $bin = Join-Path $work 'bin'
    New-Item -ItemType Directory -Path $bin | Out-Null
    $ghLog = Join-Path $work 'gh-calls.log'
    $stub = @'
#!/bin/sh
# gh stub: no Release exists until `release create`; the created one is a draft.
printf '%s\n' "$*" >> "$GH_STUB_LOG"
case "$1 $2" in
  "release view")
    case "$*" in *--json*) echo true; exit 0;; esac
    grep -q '^release create' "$GH_STUB_LOG" && exit 0
    exit 1;;
  "release create") exit 0;;
esac
echo "gh stub: unexpected call: $*" >&2
exit 2
'@
    $stubPath = Join-Path $bin 'gh'
    [System.IO.File]::WriteAllText($stubPath, $stub.Replace("`r`n", "`n"))
    if ($env:OS -ne 'Windows_NT') { Invoke-Native chmod @('+x', $stubPath) | Out-Null }

    function Invoke-DraftStep([string] $RecordBranch) {
        $dir = Join-Path $work ("draft-" + [guid]::NewGuid().ToString('N'))
        $dist = Join-Path $dir 'dist'
        New-Item -ItemType Directory -Path $dist -Force | Out-Null
        foreach ($name in 'go2cs-win-x64.zip', 'go2cs-win-arm64.zip', 'go2cs-linux-x64.tar.gz', 'go2cs-linux-arm64.tar.gz',
                          'go2cs-osx-x64.tar.gz', 'go2cs-osx-arm64.tar.gz', 'SHA256SUMS', 'go2cs-attestation.sigstore.jsonl') {
            [System.IO.File]::WriteAllText((Join-Path $dist $name), "fixture`n")
        }
        Remove-Item $ghLog -ErrorAction SilentlyContinue
        $path = "$bin$([System.IO.Path]::PathSeparator)$env:PATH"
        $run = Invoke-Bash $draft @{ GH_TOKEN = 'x'; GH_REPO = 'owner/repo'; TAG = 'nuget-1.26.11.4'; VERSION = '1.26.11.4'; RECORD = 'abc123'
                                     RECORD_BRANCH = $RecordBranch; GITHUB_STEP_SUMMARY = (Join-Path $dir 'summary'); GH_STUB_LOG = $ghLog; PATH = $path } $dir
        $create = if (Test-Path $ghLog) { @([System.IO.File]::ReadAllLines($ghLog) | Where-Object { $_ -like 'release create*' }) } else { @() }
        [pscustomobject]@{ Code = $run.Code; Text = $run.Text; Create = ($create -join "`n") }
    }

    $r = Invoke-DraftStep 'release/go1.26'
    Check ($r.Code -eq 0) "T3 maintenance: the draft step exited $($r.Code): $($r.Text)"
    Check ($r.Create.Contains('--latest=false')) "T3 maintenance: gh release create was not given --latest=false: $($r.Create)"
    Check ($r.Create.Contains('MAINTENANCE RELEASE on release/go1.26')) "T3 maintenance: the draft's notes do not say to keep the latest mark cleared: $($r.Create)"

    $r = Invoke-DraftStep 'master'
    Check ($r.Code -eq 0) "T3 master control: the draft step exited $($r.Code): $($r.Text)"
    Check ($r.Create -and -not $r.Create.Contains('--latest=false') -and -not $r.Create.Contains('MAINTENANCE')) "T3 master control: a master release's draft was marked as maintenance: $($r.Create)"
}
catch {
    $failures.Add("selftest setup or case threw: $_")
}
finally {
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}

if ($failures.Count -gt 0) { $failures | ForEach-Object { Write-Host "FAIL: $_" }; exit 1 }
Write-Host 'release-version-lines selftest: PASS (T1 per-minor monotonicity with existence across lines; T2 the record found on release/go1.26 and on master; T3 --latest=false for a maintenance draft only; T4 master and release/go1.N accepted)'
exit 0
