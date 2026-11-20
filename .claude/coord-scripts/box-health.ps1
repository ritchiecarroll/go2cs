# box-health.ps1 -- one line of host health for a lane's reads before and after a heavy job, and the comparison that
# decides whether anything HAPPENED in between. Windows only; read-only. Runs under Windows PowerShell 5.1 and pwsh 7.
#
# Why not the WHEA event COUNT: the System log is circular (20 MB by default) and a full one drops its oldest events, so
# the count FALLS with no hardware event, and a new event arriving while an old one ages out leaves it UNCHANGED.
# Measured on the i9, 2026-11-20: 306 -> 303 between two reads, with the newest WHEA event still 2026-10-16, the last
# crash event and the boot time unchanged. The signal is the NEWEST WHEA event's time, the newest crash event (System
# 41 Kernel-Power or 1001 BugCheck) and the boot time. The retained count is still printed, as information.
#
#   box-health.ps1                      print the line
#   box-health.ps1 -Compare '<line>'    print the line; exit 3 naming what happened since <line>, 0 if nothing did,
#                                       2 if <line> is not a box-health line (a broken read never passes)
#   box-health.ps1 -SelfTest            the decision against simulated reads: a wrap, a new event, both at once, ...
param([string] $Compare = '', [switch] $SelfTest, [string] $Drive = 'D')

$ErrorActionPreference = 'Stop'

function Get-UtcStamp([datetime] $Time) { $Time.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }

function Read-BoxHealthLine {
    $whea = @(Get-WinEvent -ProviderName Microsoft-Windows-WHEA-Logger -ErrorAction SilentlyContinue)
    $newest = if ($whea.Count -gt 0) { "$(Get-UtcStamp $whea[0].TimeCreated)/$($whea[0].Id)" } else { 'none' }
    $crashEvent = Get-WinEvent -FilterHashtable @{ LogName = 'System'; Id = 41, 1001 } -MaxEvents 1 -ErrorAction SilentlyContinue
    $crash = if ($crashEvent) { "$(Get-UtcStamp $crashEvent.TimeCreated)/$($crashEvent.Id)" } else { 'none' }
    $boot = Get-UtcStamp (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
    $free = [math]::Round((Get-PSDrive $Drive).Free / 1GB, 1)
    "now $(Get-UtcStamp (Get-Date)) newestWhea $newest lastCrash $crash boot $boot wheaRetained $($whea.Count) free$Drive ${free}GB"
}

# A line's fields; $null when the line is not a box-health line.
function ConvertFrom-BoxHealthLine([string] $Line) {
    $m = [regex]::Match($Line, 'newestWhea (\S+) lastCrash (\S+) boot (\S+) wheaRetained (\d+)')
    if (-not $m.Success) { return $null }
    @{ newestWhea = $m.Groups[1].Value; lastCrash = $m.Groups[2].Value; boot = $m.Groups[3].Value; wheaRetained = [int] $m.Groups[4].Value }
}

# 'yyyy-MM-ddTHH:mm:ssZ/<id>' or 'none'; the stamp sorts as text. An event that ages out of the log moves the newest
# stamp OLDER (or to none): retention, not an event, so only a NEWER stamp counts.
function Test-Newer([string] $Before, [string] $After) {
    if ($After -eq 'none') { return $false }
    if ($Before -eq 'none') { return $true }
    return ([string]::CompareOrdinal($After.Split('/')[0], $Before.Split('/')[0]) -gt 0)
}

# What happened between two reads; an empty list when nothing did. The retained WHEA count is not consulted.
function Get-BoxHealthChange([hashtable] $Before, [hashtable] $After) {
    $happened = @()
    if (Test-Newer $Before.newestWhea $After.newestWhea) { $happened += "new WHEA event (newest $($Before.newestWhea) -> $($After.newestWhea))" }
    if (Test-Newer $Before.lastCrash $After.lastCrash) { $happened += "new crash event ($($Before.lastCrash) -> $($After.lastCrash))" }
    if ($After.boot -ne $Before.boot) { $happened += "reboot (boot $($Before.boot) -> $($After.boot))" }
    , $happened
}

if ($SelfTest) {
    function New-Line([string] $Newest, [string] $Crash, [string] $Boot, [int] $Count) {
        "now 2026-11-20T19:49:23Z newestWhea $Newest lastCrash $Crash boot $Boot wheaRetained $Count freeD 7870.5GB"
    }
    $w0 = '2026-10-16T04:17:25Z/19'; $w1 = '2026-11-20T19:40:00Z/19'; $c0 = '2026-11-16T21:10:23Z/1001'; $c1 = '2026-11-20T19:45:00Z/41'
    $b0 = '2026-11-17T02:06:05Z'; $b1 = '2026-11-20T19:46:30Z'
    $cases = @(
        @{ name = 'steady';                               before = (New-Line $w0 $c0 $b0 306); after = (New-Line $w0 $c0 $b0 306); trip = $false },
        @{ name = 'circular-log wrap (count falls, no event)'; before = (New-Line $w0 $c0 $b0 306); after = (New-Line $w0 $c0 $b0 303); trip = $false },
        @{ name = 'new WHEA event';                       before = (New-Line $w0 $c0 $b0 303); after = (New-Line $w1 $c0 $b0 304); trip = $true },
        @{ name = 'new WHEA event during a wrap (count unchanged)'; before = (New-Line $w0 $c0 $b0 303); after = (New-Line $w1 $c0 $b0 303); trip = $true },
        @{ name = 'new crash event';                      before = (New-Line $w0 $c0 $b0 303); after = (New-Line $w0 $c1 $b0 303); trip = $true },
        @{ name = 'reboot';                               before = (New-Line $w0 $c0 $b0 303); after = (New-Line $w0 $c0 $b1 303); trip = $true },
        @{ name = 'the last WHEA event ages out (newest -> none)'; before = (New-Line $w0 $c0 $b0 1); after = (New-Line 'none' $c0 $b0 0); trip = $false },
        @{ name = 'first WHEA event on a clean log';      before = (New-Line 'none' $c0 $b0 0); after = (New-Line $w1 $c0 $b0 1); trip = $true }
    )
    $failed = 0
    foreach ($case in $cases) {
        $happened = Get-BoxHealthChange (ConvertFrom-BoxHealthLine $case.before) (ConvertFrom-BoxHealthLine $case.after)
        $tripped = $happened.Count -gt 0
        if ($tripped -eq $case.trip) { "PASS  $($case.name)" } else { $failed++; "FAIL  $($case.name): want trip=$($case.trip), got trip=$tripped ($($happened -join '; '))" }
    }
    if ($null -ne (ConvertFrom-BoxHealthLine 'now 2026-11-20 whea 306 lastcrash 2026-11-16 21:10:23Z id 1001')) { $failed++; 'FAIL  an old-format line parsed' } else { 'PASS  an old-format line is refused, not read' }
    "self-test: $failed failed"
    exit ([int]($failed -gt 0))
}

$line = Read-BoxHealthLine
$line
if ($Compare) {
    $before = ConvertFrom-BoxHealthLine $Compare
    if ($null -eq $before) { "REFUSE: not a box-health line: $Compare"; exit 2 }
    $happened = Get-BoxHealthChange $before (ConvertFrom-BoxHealthLine $line)
    if ($happened.Count -gt 0) { "HAPPENED: $($happened -join '; ')"; exit 3 }
    'nothing happened (the retained WHEA count is not a signal)'
}
exit 0
