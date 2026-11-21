# box-health.ps1 -- one line of host health for a lane's reads before and after a heavy job, and the comparison that
# decides whether anything HAPPENED in between. Windows only; read-only. Runs under Windows PowerShell 5.1 and pwsh 7.
#
# Why not the WHEA event COUNT: the System log is circular (20 MB by default) and a full one drops its oldest events, so
# the count FALLS with no hardware event, and a new event arriving while an old one ages out leaves it UNCHANGED.
# Measured on the i9, 2026-11-20: 306 -> 303 between two reads, with the newest WHEA event still 2026-10-16, the last
# crash event and the boot time unchanged. The signal is the NEWEST WHEA event, the newest crash event (System 41
# Kernel-Power or 1001 BugCheck) and the boot time. The retained count is still printed, as information.
#
# An event field reads `stamp/eventId/recordId`, or `none` when the log holds no such event, or `unread` when the read
# itself failed. "Newer" is decided by the System log's RecordId, which rises with every event written whatever the
# clock says, so a clock stepped back between reads, or a second event in the same second, still reads newer. The one
# limit: clearing the System log restarts RecordId, so the first events after a clear can read older than the last one
# before it. And Win32_OperatingSystem.LastBootUpTime is derived from uptime and can move by a second after a time
# adjustment: that reads as a "reboot", the safe direction.
#
#   box-health.ps1                      print the line
#   box-health.ps1 -Compare '<line>'    print the line; exit 3 naming what happened since <line>, 0 if nothing did,
#                                       2 refusing an empty or unparseable <line>, or an unread field on either side
#                                       (a broken read never passes)
#   box-health.ps1 -SelfTest            the decision against simulated reads: a wrap, a new event, both at once, ...
param([string] $Compare = '', [switch] $SelfTest, [string] $Drive = 'D')

$ErrorActionPreference = 'Stop'

$Invariant = [System.Globalization.CultureInfo]::InvariantCulture

# Get-WinEvent raises this, and only this, for a read that worked and matched nothing. Measured 2026-11-21 on Windows
# PowerShell 5.1 and pwsh 7.6 alike; an access-denied read raises UnauthorizedAccessException and a missing provider
# NoMatchingProvidersFound, and both of those are `unread`, never `none`.
$NoEventsId = 'NoMatchingEventsFound,Microsoft.PowerShell.Commands.GetWinEventCommand'
$EventPattern = '^(none|\d{4}-\d\d-\d\dT\d\d:\d\d:\d\dZ/\d+/\d+)$'
$StampPattern = '^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\dZ$'

function Get-UtcStamp([datetime] $Time) { $Time.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ', $Invariant) }

# The newest event a reader returns, as stamp/eventId/recordId; `none` for the no-match error, `unread` for any other.
function Read-NewestEvent([scriptblock] $Reader) {
    try {
        $e = & $Reader

        if ($null -eq $e) { return 'none' }

        [string]::Format($Invariant, '{0}/{1}/{2}', (Get-UtcStamp $e.TimeCreated), $e.Id, $e.RecordId)
    }
    catch {
        if ($_.FullyQualifiedErrorId -eq $NoEventsId) { 'none' } else { 'unread' }
    }
}

function Read-BoxHealthLine {
    $wheaFilter = @{ LogName = 'System'; ProviderName = 'Microsoft-Windows-WHEA-Logger' }
    $whea = Read-NewestEvent { Get-WinEvent -FilterHashtable $wheaFilter -MaxEvents 1 }
    $crash = Read-NewestEvent { Get-WinEvent -FilterHashtable @{ LogName = 'System'; Id = 41, 1001 } -MaxEvents 1 }

    $retained = try { [string]@(Get-WinEvent -FilterHashtable $wheaFilter).Count }
                catch { if ($_.FullyQualifiedErrorId -eq $NoEventsId) { '0' } else { 'unread' } }

    $boot = Get-UtcStamp (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
    $free = [math]::Round((Get-PSDrive $Drive).Free / 1GB, 1).ToString($Invariant)
    "now $(Get-UtcStamp (Get-Date)) newestWhea $whea lastCrash $crash boot $boot wheaRetained $retained free$Drive ${free}GB"
}

# A line's fields; $null when the line is not a box-health line. Each event field must be `none`, `unread` or a
# stamp/eventId/recordId and boot a stamp, so a hand-edited or foreign token is refused rather than read.
function ConvertFrom-BoxHealthLine([string] $Line) {
    $m = [regex]::Match($Line, 'newestWhea (\S+) lastCrash (\S+) boot (\S+) wheaRetained (\S+)')

    if (-not $m.Success) { return $null }

    $fields = @{ newestWhea = $m.Groups[1].Value; lastCrash = $m.Groups[2].Value; boot = $m.Groups[3].Value; wheaRetained = $m.Groups[4].Value }

    foreach ($k in 'newestWhea', 'lastCrash') {
        if ($fields[$k] -ne 'unread' -and $fields[$k] -notmatch $EventPattern) { return $null }
    }

    if ($fields.boot -notmatch $StampPattern) { return $null }

    $fields
}

# Whether After names a later event than Before, by RecordId. An event that ages out moves the newest to an OLDER
# record, or to none: retention, not an event.
function Test-Newer([string] $Before, [string] $After) {
    if ($After -eq 'none') { return $false }
    if ($Before -eq 'none') { return $true }

    return ([long]$After.Split('/')[2] -gt [long]$Before.Split('/')[2])
}

# What happened between two reads; an empty list when nothing did.
function Get-BoxHealthChange([hashtable] $Before, [hashtable] $After) {
    $happened = @()

    if (Test-Newer $Before.newestWhea $After.newestWhea) { $happened += "new WHEA event (newest $($Before.newestWhea) -> $($After.newestWhea))" }
    if (Test-Newer $Before.lastCrash $After.lastCrash) { $happened += "new crash event ($($Before.lastCrash) -> $($After.lastCrash))" }
    if ($After.boot -ne $Before.boot) { $happened += "reboot (boot $($Before.boot) -> $($After.boot))" }

    , $happened
}

# The -Compare decision, apart from the event log so the self-test drives it: the exit code and its message. $Bound
# is whether -Compare was passed at all, so an empty value is refused, never mistaken for "not comparing".
function Get-CompareVerdict([bool] $Bound, [string] $BeforeLine, [string] $AfterLine) {
    if (-not $Bound) { return 0, '' }
    if ([string]::IsNullOrWhiteSpace($BeforeLine)) { return 2, 'REFUSE: -Compare was given an empty line (a before-read that failed, or an unset variable)' }

    $before = ConvertFrom-BoxHealthLine $BeforeLine
    if ($null -eq $before) { return 2, "REFUSE: not a box-health line: $BeforeLine" }

    $after = ConvertFrom-BoxHealthLine $AfterLine
    if ($null -eq $after) { return 2, "REFUSE: this read is not a box-health line: $AfterLine" }

    foreach ($k in 'newestWhea', 'lastCrash') {
        if ($before[$k] -eq 'unread') { return 2, "REFUSE: the before-read could not read $k" }
        if ($after[$k] -eq 'unread') { return 2, "REFUSE: this read could not read $k" }
    }

    $happened = Get-BoxHealthChange $before $after
    if ($happened.Count -gt 0) { return 3, "HAPPENED: $($happened -join '; ')" }

    return 0, 'nothing happened (the retained WHEA count is not a signal)'
}

if ($SelfTest) {
    function New-Line([string] $Newest, [string] $Crash, [string] $Boot, [string] $Count) {
        "now 2026-11-20T19:49:23Z newestWhea $Newest lastCrash $Crash boot $Boot wheaRetained $Count freeD 7870.5GB"
    }

    $w0 = '2026-10-16T04:17:25Z/19/4100'; $w1 = '2026-11-20T19:40:00Z/19/4200'
    $c0 = '2026-11-16T21:10:23Z/1001/3900'; $c1 = '2026-11-20T19:45:00Z/41/4210'
    $b0 = '2026-11-17T02:06:05Z'; $b1 = '2026-11-20T19:46:30Z'
    $steady = New-Line $w0 $c0 $b0 '303'

    $cases = @(
        @{ name = 'steady'; before = $steady; after = $steady; rc = 0 },
        @{ name = 'circular-log wrap (count falls, no event)'; before = (New-Line $w0 $c0 $b0 '306'); after = $steady; rc = 0 },
        @{ name = 'new WHEA event'; before = $steady; after = (New-Line $w1 $c0 $b0 '304'); rc = 3 },
        @{ name = 'new WHEA event during a wrap (count unchanged)'; before = $steady; after = (New-Line $w1 $c0 $b0 '303'); rc = 3 },
        @{ name = 'new crash event'; before = $steady; after = (New-Line $w0 $c1 $b0 '303'); rc = 3 },
        @{ name = 'reboot'; before = $steady; after = (New-Line $w0 $c0 $b1 '303'); rc = 3 },
        @{ name = 'the last WHEA event ages out (newest -> none)'; before = (New-Line $w0 $c0 $b0 '1'); after = (New-Line 'none' $c0 $b0 '0'); rc = 0 },
        @{ name = 'first WHEA event on a clean log'; before = (New-Line 'none' $c0 $b0 '0'); after = (New-Line $w1 $c0 $b0 '1'); rc = 3 },
        @{ name = 'an old-format line is refused, not read'; before = 'now 2026-11-20 whea 306 lastcrash 2026-11-16 21:10:23Z id 1001'; after = $steady; rc = 2 },
        @{ name = 'an empty -Compare is refused'; before = ''; after = $steady; rc = 2 },
        @{ name = 'a whitespace -Compare is refused'; before = '   '; after = $steady; rc = 2 },
        @{ name = 'an unread WHEA field after the job is refused'; before = $steady; after = (New-Line 'unread' $c0 $b0 'unread'); rc = 2 },
        @{ name = 'an unread crash field before the job is refused'; before = (New-Line $w0 'unread' $b0 '303'); after = $steady; rc = 2 },
        @{ name = 'a foreign token in an event field is refused'; before = (New-Line 'n/a' $c0 $b0 '303'); after = (New-Line $w1 $c0 $b0 '304'); rc = 2 },
        @{ name = 'a foreign token in boot is refused'; before = (New-Line $w0 $c0 'yesterday' '303'); after = $steady; rc = 2 },
        @{ name = 'a clock stepped back: newer record, older stamp'; before = $steady; after = (New-Line '2026-10-16T04:00:00Z/19/4101' $c0 $b0 '304'); rc = 3 },
        @{ name = 'a second WHEA event in the same second'; before = $steady; after = (New-Line '2026-10-16T04:17:25Z/19/4101' $c0 $b0 '304'); rc = 3 }
    )

    $failed = 0

    foreach ($case in $cases) {
        $rc, $message = Get-CompareVerdict $true $case.before $case.after

        if ($rc -eq $case.rc) { "PASS  $($case.name)" } else { $failed++; "FAIL  $($case.name): want rc $($case.rc), got rc $rc ($message)" }
    }

    # The read side: a failed read is `unread`, never `none`, and a read that matched nothing is `none`.
    $denied = Read-NewestEvent { throw [System.UnauthorizedAccessException]::new('simulated access denied') }
    if ($denied -eq 'unread') { 'PASS  a simulated access-denied read is unread' } else { $failed++; "FAIL  a simulated access-denied read read as '$denied'" }

    $empty = Read-NewestEvent { Get-WinEvent -FilterHashtable @{ LogName = 'System'; Id = 65000 } -MaxEvents 1 }
    if ($empty -eq 'none') { 'PASS  a real read that matches nothing is none' } else { $failed++; "FAIL  a real read that matches nothing read as '$empty'" }

    # The stamp does not follow the current culture (a Buddhist-calendar culture would print year 2569).
    $saved = [System.Globalization.CultureInfo]::CurrentCulture

    try {
        [System.Globalization.CultureInfo]::CurrentCulture = [System.Globalization.CultureInfo]::GetCultureInfo('th-TH')
        $stamp = Get-UtcStamp ([datetime]::SpecifyKind([datetime]'2026-11-21T10:00:00', 'Utc'))
    }
    finally {
        [System.Globalization.CultureInfo]::CurrentCulture = $saved
    }

    if ($stamp -eq '2026-11-21T10:00:00Z') { 'PASS  the stamp is culture-invariant' } else { $failed++; "FAIL  under th-TH the stamp read '$stamp'" }

    "self-test: $failed failed of $($cases.Count + 3)"
    exit ([int]($failed -gt 0))
}

$line = Read-BoxHealthLine
$line

$rc, $message = Get-CompareVerdict $PSBoundParameters.ContainsKey('Compare') $Compare $line

if ($message) { $message }

exit $rc
