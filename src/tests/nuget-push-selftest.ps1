<#
.SYNOPSIS
    Guards the release's push (src/_nugetpush.ps1) against a MOCK NuGet feed, so it is exercised without a real one.

.DESCRIPTION
    Serves a v3 service index, a flat container and a publish endpoint from an HttpListener on 127.0.0.1, and pushes
    FABRICATED packages to it through Invoke-GoNuGetPush, the function push-nuget.ps1 -Push and release-nuget.ps1's
    Phase 3 both call. Nothing reaches a real feed and nothing is published.

      quota      five packages, one already published; the feed refuses the third PUT with 403 Quota Exceeded and
                 Retry-After 7 (the 1.26.10.1 failure) -> the published one is never PUT, the push waits exactly 7 s
                 (recorded, not slept), says when it resumes, pushes the refused package again, and finishes: 4 pushed
      resume     the same five again -> 0 PUTs, 5 already published (the re-run IS the resume)
      normalized a package versioned x.y.z.0 that the flat container keys as x.y.z -> found published, not PUT
      conflict   the feed answers 409 for a version its flat container does not list yet -> counted as published
      refused    403 with NO Retry-After (a bad API key) -> throws, naming the package and the count pushed before it
      resume-foreign  release-nuget.ps1 -ResumePush over a folder holding a package of another version -> refused by
                 name, nothing PUT, version.props untouched
      resume-release  release-nuget.ps1 -ResumePush, end to end, over three packages of the release (one published, a
                 403 with Retry-After 1 on the first PUT) -> exit 0, 2 pushed + 1 skipped + 1 wait, version.props
                 byte-identical (no re-bump), the published one never PUT

    The two release-nuget cases run a scratch copy of release-nuget.ps1 and the files it reads (global.json,
    _paths.ps1, Directory.Build.props, _nugetpush.ps1, version.props, and a no-op gencorpusrelease for Phase 4) in a
    child of this PowerShell, so they need dotnet and go on PATH, as a release does.

    Exit 0 clean, 1 on any violation. Writes only under the host's temp directory.
#>
#Requires -Version 5.1
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../_nugetpush.ps1')
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$failures = New-Object System.Collections.Generic.List[string]
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("nuget-push-selftest-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null

function Check([bool] $Condition, [string] $Message) { if (-not $Condition) { $failures.Add($Message) } }

function New-FakePackage([string] $Id, [string] $Version, [string] $Dir = $work) {
    $path = Join-Path $Dir "$Id.$Version.nupkg"
    $zip = [System.IO.Compression.ZipFile]::Open($path, 'Create')
    try {
        $writer = New-Object System.IO.StreamWriter($zip.CreateEntry("$Id.nuspec").Open())
        try {
            $writer.Write("<?xml version=`"1.0`" encoding=`"utf-8`"?><package xmlns=`"http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd`"><metadata><id>$Id</id><version>$Version</version></metadata></package>")
        }
        finally { $writer.Dispose() }
    }
    finally { $zip.Dispose() }
    Get-Item $path
}

# ---- The mock feed ------------------------------------------------------------------------------------------------
# State shared with the listener's runspace. Published holds "<id>/<version>" in the flat container's own spelling;
# Script is a queue of answers for the next PUTs (an empty queue accepts and publishes); Puts logs every PUT.
$probe = New-Object System.Net.Sockets.TcpListener([System.Net.IPAddress]::Loopback, 0)
$probe.Start(); $port = $probe.LocalEndpoint.Port; $probe.Stop()
$prefix = "http://127.0.0.1:$port/"

$state = [hashtable]::Synchronized(@{
    Prefix    = $prefix
    Published = [hashtable]::Synchronized(@{})
    Script    = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))
    Puts      = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))
    Errors    = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))
})

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add($prefix)
$listener.Start()

$feed = [powershell]::Create()
[void]$feed.AddScript({
    param($Listener, $State)
    Add-Type -AssemblyName System.IO.Compression

    # The flat container's version spelling, written independently of the code under test: a fourth part of 0 drops.
    function FlatVersion([string] $Version) { ($Version -replace '^(\d+\.\d+\.\d+)\.0$', '$1').ToLowerInvariant() }

    function Respond($Context, [int] $Status, [string] $Body, [hashtable] $Headers) {
        $Context.Response.StatusCode = $Status
        if ($Headers) { foreach ($k in $Headers.Keys) { $Context.Response.Headers.Add($k, $Headers[$k]) } }
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Body)
        $Context.Response.OutputStream.Write($bytes, 0, $bytes.Length)
        $Context.Response.Close()
    }

    while ($Listener.IsListening) {
        try { $context = $Listener.GetContext() } catch { break }

        try {
            $request = $context.Request
            $path = $request.Url.AbsolutePath

            if ($request.HttpMethod -eq 'GET' -and $path -eq '/v3/index.json') {
                Respond $context 200 ('{"version":"3.0.0","resources":[' +
                    '{"@id":"' + $State.Prefix + 'api/v2/package","@type":"PackagePublish/2.0.0"},' +
                    '{"@id":"' + $State.Prefix + 'flat/","@type":"PackageBaseAddress/3.0.0"}]}') $null
            }
            elseif ($request.HttpMethod -eq 'GET' -and $path -match '^/flat/([^/]+)/([^/]+)/([^/]+)\.nuspec$') {
                if ($State.Published.ContainsKey("$($Matches[1])/$($Matches[2])")) { Respond $context 200 '<package/>' $null }
                else { Respond $context 404 '' $null }
            }
            elseif ($request.HttpMethod -eq 'PUT' -and $path -eq '/api/v2/package/') {
                # The package is the multipart body's one part: between the end of its headers and the closing boundary.
                $buffer = New-Object System.IO.MemoryStream
                $request.InputStream.CopyTo($buffer)
                $bytes = $buffer.ToArray()
                $latin = [System.Text.Encoding]::GetEncoding(28591)
                $text = $latin.GetString($bytes)
                $boundary = [regex]::Match($request.ContentType, 'boundary="?([^";]+)"?').Groups[1].Value
                $start = $text.IndexOf("`r`n`r`n") + 4
                $end = $text.IndexOf("`r`n--$boundary", $start)
                $zip = New-Object System.IO.Compression.ZipArchive((New-Object System.IO.MemoryStream(, $latin.GetBytes($text.Substring($start, $end - $start)))))
                $reader = New-Object System.IO.StreamReader(@($zip.Entries | Where-Object { $_.Name -like '*.nuspec' })[0].Open())
                $nuspec = [xml]$reader.ReadToEnd()
                $reader.Dispose(); $zip.Dispose()

                $id = "$($nuspec.package.metadata.id)"
                $version = "$($nuspec.package.metadata.version)"
                [void]$State.Puts.Add("$id $version")

                $answer = $null
                if ($State.Script.Count -gt 0) { $answer = $State.Script[0]; $State.Script.RemoveAt(0) }

                if ($null -eq $answer -or $answer.Status -lt 300) {
                    $State.Published["$($id.ToLowerInvariant())/$(FlatVersion $version)"] = $true
                    Respond $context 201 '' $null
                }
                else {
                    Respond $context $answer.Status $answer.Body $answer.Headers
                }
            }
            else {
                [void]$State.Errors.Add("unrouted request: $($request.HttpMethod) $path")
                Respond $context 404 '' $null
            }
        }
        catch {
            [void]$State.Errors.Add("$_")
            try { $context.Response.StatusCode = 500; $context.Response.Close() } catch { }
        }
    }
}).AddArgument($listener).AddArgument($state)
$feedRun = $feed.BeginInvoke()

$source = "${prefix}v3/index.json"
$recordedWaits = New-Object System.Collections.Generic.List[int]
$recordWait = { param([int]$Seconds) $recordedWaits.Add($Seconds) }.GetNewClosure()

function Invoke-Case([System.IO.FileInfo[]] $Packages) {
    # Write-Host is the information stream (6): its records are the console text, and the one other object is the summary.
    $records = @(& { Invoke-GoNuGetPush -Packages $Packages -Source $source -ApiKey 'selftest-key' -Sleep $recordWait } 6>&1)
    $info = [System.Management.Automation.InformationRecord]
    $summary = @($records | Where-Object { $_ -isnot $info }) | Select-Object -Last 1
    [pscustomobject]@{ Summary = $summary; Text = (@($records | Where-Object { $_ -is $info } | ForEach-Object { "$($_.MessageData)" }) -join "`n") }
}

try {
    $version = '1.26.10.2'
    $packages = @('go.lib', 'go.gen', 'go.a', 'go.b', 'go.c' | ForEach-Object { New-FakePackage $_ $version })

    # ---- quota ----------------------------------------------------------------------------------------------------
    $state.Published["go.a/$version"] = $true
    [void]$state.Script.Add(@{ Status = 201 })
    [void]$state.Script.Add(@{ Status = 201 })
    [void]$state.Script.Add(@{ Status = 403; Body = 'Quota Exceeded'; Headers = @{ 'Retry-After' = '7' } })

    $case = $null
    try { $case = Invoke-Case $packages } catch { $failures.Add("quota: the push threw instead of waiting: $_") }

    $puts = @($state.Puts)
    Check (-not ($puts -contains "go.a $version")) "quota: the already-published go.a was PUT (PUTs: $($puts -join ', '))"
    Check (($puts -join ',') -eq "go.lib $version,go.gen $version,go.b $version,go.b $version,go.c $version") "quota: PUTs were $($puts -join ', '); want go.lib, go.gen, go.b (refused), go.b, go.c"
    Check (($recordedWaits -join ',') -eq '7') "quota: waits were [$($recordedWaits -join ',')]; want exactly one wait of 7 s"

    if ($case) {
        Check ($case.Summary.Pushed -eq 4 -and $case.Summary.AlreadyPublished -eq 1 -and $case.Summary.Waits -eq 1) "quota: summary $($case.Summary | Out-String) want 4 pushed, 1 already published, 1 wait"
        Check ($case.Text -match 'resumes at \d{4}-\d\d-\d\d \d\d:\d\d:\d\d UTC') "quota: no resume time was printed:`n$($case.Text)"
    }

    foreach ($id in 'go.lib', 'go.gen', 'go.a', 'go.b', 'go.c') {
        Check ($state.Published.ContainsKey("$id/$version")) "quota: $id is not on the feed after the push"
    }

    # ---- resume ---------------------------------------------------------------------------------------------------
    $state.Puts.Clear(); $recordedWaits.Clear()
    $case = $null
    try { $case = Invoke-Case $packages } catch { $failures.Add("resume: the re-run threw: $_") }
    Check ($state.Puts.Count -eq 0) "resume: the re-run PUT $($state.Puts.Count) package(s): $(@($state.Puts) -join ', ')"
    if ($case) { Check ($case.Summary.AlreadyPublished -eq 5 -and $case.Summary.Pushed -eq 0) "resume: summary $($case.Summary | Out-String) want 5 already published" }

    # ---- normalized -----------------------------------------------------------------------------------------------
    $state.Puts.Clear()
    $zero = New-FakePackage 'go.zero' '1.26.10.0'
    $state.Published['go.zero/1.26.10'] = $true
    try { [void](Invoke-Case @($zero)) } catch { $failures.Add("normalized: threw: $_") }
    Check ($state.Puts.Count -eq 0) "normalized: go.zero 1.26.10.0 is published as 1.26.10 and was PUT anyway"

    # ---- conflict -------------------------------------------------------------------------------------------------
    $state.Puts.Clear()
    $conflict = New-FakePackage 'go.conflict' $version
    [void]$state.Script.Add(@{ Status = 409; Body = 'A package with ID go.conflict and version exists' })
    $case = $null
    try { $case = Invoke-Case @($conflict) } catch { $failures.Add("conflict: a 409 threw: $_") }
    if ($case) { Check ($case.Summary.AlreadyPublished -eq 1 -and $case.Summary.Pushed -eq 0) "conflict: summary $($case.Summary | Out-String) want 1 already published" }

    # ---- refused --------------------------------------------------------------------------------------------------
    $state.Puts.Clear(); $recordedWaits.Clear()
    $refusedA = New-FakePackage 'go.refused.a' $version
    $refusedB = New-FakePackage 'go.refused.b' $version
    [void]$state.Script.Add(@{ Status = 201 })
    [void]$state.Script.Add(@{ Status = 403; Body = 'The specified API key is invalid' })
    $thrown = $null
    try { [void](Invoke-Case @($refusedA, $refusedB)) } catch { $thrown = "$_" }
    Check ($null -ne $thrown) 'refused: a 403 without Retry-After did not throw'
    if ($thrown) { Check ($thrown -match 'go\.refused\.b' -and $thrown -match 'after 1 pushed') "refused: the error does not name go.refused.b and the 1 pushed before it: $thrown" }
    Check ($recordedWaits.Count -eq 0) "refused: a 403 without Retry-After waited [$($recordedWaits -join ',')]"

    # ---- release-nuget.ps1 -ResumePush ----------------------------------------------------------------------------
    $repoSrc = Split-Path $PSScriptRoot -Parent
    $root = Join-Path $work 'release'
    $src = Join-Path $root 'src'
    New-Item -ItemType Directory -Path (Join-Path $src 'go2cs/internal/gencorpusrelease') -Force | Out-Null
    Copy-Item (Join-Path (Split-Path $repoSrc -Parent) 'global.json') $root
    foreach ($f in 'release-nuget.ps1', '_paths.ps1', '_nugetpush.ps1', 'version.props', 'Directory.Build.props') { Copy-Item (Join-Path $repoSrc $f) $src }
    Set-Content -Path (Join-Path $src 'go2cs/go.mod') -Value "module stub`n`ngo 1.21`n"
    Set-Content -Path (Join-Path $src 'go2cs/internal/gencorpusrelease/main.go') -Value "package main`n`nfunc main() {}`n"

    $propsPath = Join-Path $src 'version.props'
    $propsBefore = [System.IO.File]::ReadAllBytes($propsPath)
    $group = ([xml](Get-Content -Raw $propsPath)).Project.PropertyGroup
    $release = "$($group.GoStdLibVersion).$($group.GoBuildNumber)".Trim()
    $flat = ConvertTo-GoNuGetNormalizedVersion $release

    function Invoke-Release([string] $OutDir) {
        $ErrorActionPreference = 'Continue'
        $savedKey = $env:NUGET_API_KEY
        $env:NUGET_API_KEY = 'selftest-key'
        try {
            $out = @(& (Get-Process -Id $PID).Path -NoProfile -ExecutionPolicy Bypass -File (Join-Path $src 'release-nuget.ps1') `
                -ResumePush -Yes -Source $source -OutDir $OutDir 2>&1 | ForEach-Object { "$_" })
            $code = $LASTEXITCODE
        }
        finally { $env:NUGET_API_KEY = $savedKey }
        [pscustomobject]@{ Code = $code; Text = ($out -join "`n") }
    }

    function Test-PropsUntouched([string] $Case) {
        $after = [System.IO.File]::ReadAllBytes($propsPath)
        Check ([System.Convert]::ToBase64String($after) -eq [System.Convert]::ToBase64String($propsBefore)) "${Case}: version.props changed (re-bumped)"
    }

    $foreignDir = Join-Path $work 'out-foreign'
    New-Item -ItemType Directory -Path $foreignDir | Out-Null
    [void](New-FakePackage 'go.lib' $release $foreignDir)
    [void](New-FakePackage 'go.stale' '1.0.0.9' $foreignDir)
    $state.Puts.Clear(); $state.Script.Clear()
    $run = Invoke-Release $foreignDir
    Check ($run.Code -eq 1) "resume-foreign: exit $($run.Code); want 1:`n$($run.Text)"
    Check ($run.Text -match 'go\.stale\.1\.0\.0\.9\.nupkg' -and $run.Text -match "not version $([regex]::Escape($release))") "resume-foreign: the refusal does not name go.stale and the release:`n$($run.Text)"
    Check ($state.Puts.Count -eq 0) "resume-foreign: PUT $(@($state.Puts) -join ', ')"
    Test-PropsUntouched 'resume-foreign'

    $releaseDir = Join-Path $work 'out-release'
    New-Item -ItemType Directory -Path $releaseDir | Out-Null
    foreach ($id in 'go.x', 'go.gen', 'go.lib') { [void](New-FakePackage $id $release $releaseDir) }
    $state.Published["go.gen/$flat"] = $true
    $state.Puts.Clear()
    [void]$state.Script.Add(@{ Status = 403; Body = 'Quota Exceeded'; Headers = @{ 'Retry-After' = '1' } })
    $run = Invoke-Release $releaseDir
    Check ($run.Code -eq 0) "resume-release: exit $($run.Code); want 0:`n$($run.Text)"
    Check ($run.Text -match 'pushed\s+: 2; already published: 1; rate-limit waits: 1') "resume-release: no '2 pushed, 1 already published, 1 wait' line:`n$($run.Text)"
    Check ((@($state.Puts) -join ',') -eq "go.lib $release,go.lib $release,go.x $release") "resume-release: PUTs were $(@($state.Puts) -join ', '); want go.lib (refused), go.lib, go.x"
    Check ($run.Text -notmatch 'Phase 1: bump') "resume-release: Phase 1 ran:`n$($run.Text)"
    Test-PropsUntouched 'resume-release'

    foreach ($e in $state.Errors) { $failures.Add("mock feed: $e") }
}
finally {
    $listener.Stop()
    $listener.Close()
    try { [void]$feed.EndInvoke($feedRun) } catch { }
    $feed.Dispose()
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}

if ($failures.Count -gt 0) { $failures | ForEach-Object { Write-Host "FAIL: $_" }; exit 1 }
Write-Host 'nuget-push selftest: PASS (quota waited 7 s and resumed, 4 pushed + 1 skipped; re-run PUT nothing; x.y.z.0 found as x.y.z; 409 counted published; 403 without Retry-After refused by name; -ResumePush refused a foreign version, then pushed 2 + skipped 1 + waited 1 without re-bumping)'
exit 0
