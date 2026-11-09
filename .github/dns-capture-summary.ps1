# dns-capture-summary.ps1 - Gbtc
# Copyright © 2026 The go2cs Authors. All rights reserved.
#
# Use of this source code is governed by an MIT-style license
# that can be found in the LICENSE file.

# Summarizes a `tcpdump -n -tt -l 'udp port 53'` text capture (os-matrix sweep-shard's dnscap=on option, darwin):
# every DNS query matched to its response by client endpoint and query id. The question it answers is the darwin
# NXDOMAIN row's (net's TestLookupNoSuchHost, whose subtests fail on either side with "read udp ... i/o timeout"
# after four attempts): does the resolver leave queries UNANSWERED under the sweep's load, or do answers arrive that
# a client then misses? Returns DNSCAP lines and never throws on a malformed line: a line it cannot
# read is counted. The instrument proves itself: -ControlName names a query the step made before the sweep, and the
# summary says whether the capture holds it.
param(
    [Parameter(Mandatory)] [string] $Path,
    [string] $ControlName = '',
    [int] $Top = 30
)

$ErrorActionPreference = 'Stop'

function Get-Percentile([double[]] $sorted, [double] $p) {
    if ($sorted.Count -eq 0) { return [double]::NaN }
    return $sorted[[Math]::Min($sorted.Count - 1, [int][Math]::Ceiling($p * $sorted.Count) - 1)]
}

$lines = if (Test-Path -LiteralPath $Path) { @(Get-Content -LiteralPath $Path) } else { @() }

# 1731150000.123456 [(meta, proc name:pid:...)] IP[6] SRC.PORT > DST.PORT: REST
$packet = '^(?<t>\d+\.\d+)\s+(?:\((?<meta>[^)]*)\)\s+)?IP6?\s+(?<src>\S+)\s+>\s+(?<dst>\S+?):\s+(?<rest>.*)$'
# A query: "4660+ [1au] MX? invalid.invalid. (44)"
$question = '^(?<id>\d+)\+?%?\s+(?:\[[^\]]*\]\s+)*(?<type>[A-Z0-9]+)\?\s+(?<name>\S+)'
# A response: "4660 NXDomain 0/1/0 (108)", "4660* 1/0/0 A ... (49)", "4660 q: MX? x. 0/1/0 ..."
$answer = '^(?<id>\d+)[\*\-\|\$]*\s+(?:q:\s+\S+\?\s+\S+\s+)?(?<rcode>NXDomain|ServFail|Refused|FormErr|NotImp|YXDomain|NXRRSet|NotAuth|NotZone)?'

$queries = @{}      # client|id -> record
$order = [System.Collections.Generic.List[string]]::new()
$unread = 0
$responses = 0
$orphanResponses = 0
$duplicateQueries = 0
$first = [double]::NaN

foreach ($line in $lines) {
    if ($line -notmatch $packet) { if ($line.Trim()) { $unread++ }; continue }

    $t = [double]$Matches['t']; $src = $Matches['src']; $dst = $Matches['dst']; $rest = $Matches['rest']; $meta = $Matches['meta']
    if ([double]::IsNaN($first)) { $first = $t }
    $proc = if ($meta -and $meta -match 'proc\s+(?<p>[^:,\s]+)') { $Matches['p'] } else { '' }

    if ($dst -match '\.53$' -and $rest -match $question) {
        $key = "$src|$($Matches['id'])"

        if ($queries.ContainsKey($key)) { $duplicateQueries++; continue }   # the same packet on a second interface, or a client retry on one port

        $queries[$key] = [pscustomobject]@{ At = $t; Type = $Matches['type']; Name = $Matches['name'].TrimEnd('.').ToLowerInvariant(); Server = ($dst -replace '\.53$', ''); Proc = $proc; AnsweredAt = [double]::NaN; Rcode = '' }
        $order.Add($key)
        continue
    }

    if ($src -match '\.53$' -and $rest -match $answer) {
        $key = "$dst|$($Matches['id'])"
        $responses++

        if (-not $queries.ContainsKey($key)) { $orphanResponses++; continue }

        $record = $queries[$key]

        if ([double]::IsNaN($record.AnsweredAt)) {
            $record.AnsweredAt = $t
            $record.Rcode = if ($Matches['rcode']) { $Matches['rcode'] } else { 'NoError' }
        }

        continue
    }

    $unread++
}

$out = [System.Collections.Generic.List[string]]::new()
$all = @($order | ForEach-Object { $queries[$_] })
$unanswered = @($all | Where-Object { [double]::IsNaN($_.AnsweredAt) })
$span = if ($all.Count) { ($all[-1].At - $first) } else { 0 }

$out.Add("DNSCAP capture: $($lines.Count) lines, $($all.Count) queries, $responses responses ($orphanResponses matching no query), $($unanswered.Count) queries unanswered, $duplicateQueries repeated (client|id), $unread lines unread, over $([Math]::Round($span, 1)) s")

if ($ControlName) {
    $control = @($all | Where-Object { $_.Name -eq $ControlName.TrimEnd('.').ToLowerInvariant() })
    $verdict = if ($control.Count -eq 0) { 'ABSENT -- the capture did not see the control query, so its silence below proves nothing' } else { "present ($($control.Count) queries, $(@($control | Where-Object { -not [double]::IsNaN($_.AnsweredAt) }).Count) answered)" }
    $out.Add("DNSCAP control query $ControlName : $verdict")
}

foreach ($server in @($all | Group-Object Server | Sort-Object Count -Descending)) {
    $out.Add("DNSCAP server $($server.Name): $($server.Count) queries, $(@($server.Group | Where-Object { [double]::IsNaN($_.AnsweredAt) }).Count) unanswered")
}

foreach ($group in @($all | Group-Object { "$($_.Proc)|$($_.Type)|$($_.Name)" } | Sort-Object Count -Descending | Select-Object -First $Top)) {
    $parts = $group.Name -split '\|', 3
    $answered = @($group.Group | Where-Object { -not [double]::IsNaN($_.AnsweredAt) })
    [double[]] $latency = @($answered | ForEach-Object { ($_.AnsweredAt - $_.At) * 1000.0 } | Sort-Object)
    $rcodes = (@($answered | Group-Object Rcode | Sort-Object Count -Descending | ForEach-Object { "$($_.Name) $($_.Count)" }) -join ', ')
    $who = if ($parts[0]) { "$($parts[0]) " } else { '' }
    $lat = if ($latency.Count) { "latency ms p50 $([Math]::Round((Get-Percentile $latency 0.5), 1)) p90 $([Math]::Round((Get-Percentile $latency 0.9), 1)) max $([Math]::Round($latency[-1], 1))" } else { 'no answer' }
    $out.Add("DNSCAP $who$($parts[1]) $($parts[2]): $($group.Count) queries, $($group.Count - $answered.Count) unanswered; rcodes $(if ($rcodes) { $rcodes } else { '-' }); $lat")
}

# The unanswered queries in time: 10 s buckets from the first packet, with every query in the bucket beside them, so a
# burst that the resolver stops answering reads as a run of buckets.
if ($unanswered.Count) {
    foreach ($bucket in @($all | Group-Object { [int][Math]::Floor(($_.At - $first) / 10) } | Sort-Object { [int]$_.Name })) {
        $missed = @($bucket.Group | Where-Object { [double]::IsNaN($_.AnsweredAt) })

        if ($missed.Count) {
            $names = (@($missed | Group-Object { "$($_.Type) $($_.Name)" } | Sort-Object Count -Descending | Select-Object -First 4 | ForEach-Object { "$($_.Name) x$($_.Count)" }) -join '; ')
            $out.Add("DNSCAP unanswered at +$([int]$bucket.Name * 10)s: $($missed.Count) of $($bucket.Count) queries -- $names")
        }
    }
}

$peak = @($all | Group-Object { [int][Math]::Floor($_.At) } | Sort-Object Count -Descending | Select-Object -First 1)
if ($peak.Count) { $out.Add("DNSCAP peak: $($peak[0].Count) queries in one second (+$([int]$peak[0].Name - [int][Math]::Floor($first)) s)") }

return $out
