# dns-probe.ps1 -- READ-ONLY DNS lane probe, Windows leg: does this box's DNS answer NXDOMAIN the way Go 1.24.13
# net's TestLookupNoSuchHost needs? Changes NO setting; no go2cs, no dotnet, no worktree. Output is ADDRESS-FREE:
# resolvers print as KINDS, the raw logs as %TEMP%-relative names; no path, host name or account name is printed.
# -ShowAddresses is OWNER-CONSOLE-ONLY: a Claude lane's tool captures the console, so a lane never passes it.
# The raw logs can hold resolver addresses and the host name: they stay local, never on a pushed surface.
# Save and run it from a LOCAL disk (never a share or a UNC path): an error record would print the script's own path.
# Usage: pwsh -NoProfile -File dns-probe.ps1 -GoRoot <go1.24.13 root, backslash spelling> -PublicResolver <v4>[,<v6>] [-LookupsOnly]
# Exit: 0 = ran to its VERDICT line (ANY verdict: the VERDICT line is the result), 2 = ABORT (nothing measured), 1 = script fault.
# Dot-sourcing (the unit tests) defines the functions and returns before anything runs.
param([string]$GoRoot = "$HOME\sdk\go1.24.13", [string]$PublicResolver = '', [switch]$ShowAddresses, [switch]$LookupsOnly)
function Canon([string]$a) {  # an address compared AS an address: zone dropped, canonical text (2001:DB8:0:0:0:0:0:53 = 2001:db8::53)
  $b = ($a -split '%')[0]; $ip = $null; if ([Net.IPAddress]::TryParse($b, [ref]$ip)) { $ip.ToString() } else { $b.ToLowerInvariant() } }
$Pub = @($PublicResolver -split '[,;\s]+' | Where-Object { $_ } | ForEach-Object { Canon $_ })
$Markers = 'panic: test timed out', 'unexpected success', 'IsNotFound is set to false', 'server misbehaving', 'DNS server failure',
  'i/o timeout', 'timeout period expired', 'temporary error'
function Kind([string]$a) { $a = Canon $a
  if ($Pub -contains $a) { return 'ctl-match' }
  switch -Regex ($a) { '^fec0:' {'fec0'; break} '^fe[89ab]' {'ll6'; break} '^f[cd]' {'ula6'; break} '^(127\.|::1$)' {'loop'; break}
    '^(10\.|192\.168\.|172\.(1[6-9]|2\d|3[01])\.)' {'lan4'; break} '^100\.(6[4-9]|[7-9]\d|1[01]\d|12[0-7])\.' {'cgnat'; break}
    '^169\.254\.' {'ll4'; break} default {'public'} } }
function Redact([string]$s) { $s -replace '\d{1,3}(\.\d{1,3}){3}', '<ip4>' -replace '[0-9a-f]*(:[0-9a-f]*){2,}(%\w+)?', '<ip6>' }
function ErrClass([string]$id) { $f = ($id -split ',')[0]
  switch -Wildcard ($f) { '*NAME_ERROR*' {'NXDOMAIN'; break} '*NO_RECORDS*' {'NODATA'; break} '*SERVER_FAILURE*' {'SERVFAIL'; break}
    '*REFUSED*' {'REFUSED'; break} '*TIMEOUT*' {'TIMEOUT'; break} '*1460*' {'TIMEOUT'; break} default {'ERR:' + (Redact $f)} } }
function DockerLines([string[]]$hostsLines, $ips) {  # active hosts lines naming *docker.internal; with $ips, only those whose address is in $ips
  @($hostsLines | Where-Object { $_ -match '^\s*([^#\s]+)\s+.*docker\.internal' -and ($null -eq $ips -or @($ips) -contains $Matches[1]) }).Count }
function Probe([string]$n, [string]$type, [string]$s) {
  $p = @{ Name = $n; DnsOnly = $true; NoHostsFile = $true; ErrorAction = 'Stop' }; if ($type) { $p.Type = $type }; if ($s) { $p.Server = $s }
  $t = [Diagnostics.Stopwatch]::StartNew()
  try { $o = @(Resolve-DnsName @p); $r = if (@($o | Where-Object { "$($_.Section)" -eq 'Answer' }).Count) { 'ANSWERED' } else { 'NODATA' } }
  catch { $r = ErrClass $_.FullyQualifiedErrorId }
  '{0}/{1:n2}s' -f $r, $t.Elapsed.TotalSeconds }
function Arm([string]$label, [string]$s, [string]$nx) {  # example.com control, a fresh nx .com, then every type the NoSuchHost leaves query
  $cells = foreach ($q in ('ex.com', 'example.com', ''), ('nx.com', $nx, ''), ('host', 'invalid.invalid.', ''), ('CNAME', 'invalid.invalid.', 'CNAME'),
      ('MX', 'invalid.invalid.', 'MX'), ('NS', 'invalid.invalid.', 'NS'), ('TXT', 'invalid.invalid.', 'TXT'), ('SRV', '_unknown._tcp.invalid.invalid.', 'SRV')) {
    '{0}={1}' -f $q[0], (Probe $q[1] $q[2] $s) }
  '{0,-52} {1}' -f $label, ($cells -join ' ') }
function TestLine([string[]]$L, [string]$n) {  # one test's root result, read from ITS OWN call's log
  $m = @($L | Select-String -Pattern "^--- (PASS|FAIL|SKIP): $n \(([\d.]+s)\)")
  if ($m) { return $m[0].Matches[0].Groups[1].Value + ' ' + $m[0].Matches[0].Groups[2].Value }
  if (-not @($L -match "^=== RUN\s+$n$").Count) { return 'NOT REACHED' }
  if (@($L | Select-String -SimpleMatch 'panic: test timed out').Count) { 'KILLED (-timeout 40m)' } else { 'DIED (no result line; read the raw log locally)' } }
function Summarize([string[]]$L1, [string[]]$L2, $rcNsh, [int]$dockGw, [int]$cache0) {
  # pure: $L1 = the CNAME/LocalPTR call's go test -v log, $L2 = the NoSuchHost call's -> readout lines + VERDICT (never echoes a log line).
  # Everything that judges NoSuchHost (totals, modes, markers, the kill, the class) reads $L2 ONLY.
  foreach ($n in 'TestLookupCNAME', 'TestLookupLocalPTR') { '{0,-22} {1}' -f $n, (TestLine $L1 $n) }
  '{0,-22} {1}' -f 'TestLookupNoSuchHost', (TestLine $L2 'TestLookupNoSuchHost')
  if (@($L1 | Select-String -SimpleMatch 'panic: test timed out').Count) { 'CNAME/LocalPTR call killed by -timeout 40m: its two tests are partial (not a NoSuchHost verdict)' }
  $leaf = @($L2 | Select-String -Pattern '^\s+--- FAIL: TestLookupNoSuchHost/([^/\s]+)/(\S+)' | ForEach-Object { $_.Matches[0] })
  $par = @($L2 -match '^\s+--- FAIL: TestLookupNoSuchHost/[^/\s]+ \(').Count; $root = @($L2 -match '^--- FAIL: TestLookupNoSuchHost ').Count
  'NoSuchHost failing verdicts: root {0} + parents {1} + leaves {2} = {3}' -f $root, $par, $leaf.Count, ($root + $par + $leaf.Count)
  foreach ($g in $leaf | Group-Object { $_.Groups[1].Value }) { '  {0,-22} failing modes: {1}' -f $g.Name, (($g.Group | ForEach-Object { $_.Groups[2].Value -replace '_resolver$' }) -join ' ') }
  $c = @{}; foreach ($k in $Markers) { $c[$k] = @($L2 | Select-String -SimpleMatch $k).Count; '  {0,-28} x{1}' -f $k, $c[$k] }
  $ran = @($L2 -match '^=== RUN\s+TestLookupNoSuchHost$').Count -gt 0; $pass = @($L2 -match '^--- PASS: TestLookupNoSuchHost ').Count -gt 0
  $rootLine = @($L2 -match '^--- (PASS|FAIL|SKIP): TestLookupNoSuchHost \(').Count -gt 0
  $skip = @($L2 -match '^\s*--- SKIP: TestLookupNoSuchHost').Count; $to = $c['i/o timeout'] + $c['timeout period expired']; $sf = $c['server misbehaving'] + $c['DNS server failure']
  $cls = if ($c['unexpected success']) { 'HIJACK class (a nonexistent name answered)' } elseif ($to -and $sf) { 'MIXED class (SERVFAIL + TIMEOUT)' } elseif ($to) { 'TIMEOUT class' } elseif ($sf) { 'SERVFAIL class' } else { 'class unread (see the counts)' }
  $ptr = ''
  if (@($L1 -match '^--- FAIL: TestLookupLocalPTR ').Count) {
    if ($dockGw -gt 0) { 'TestLookupLocalPTR FAIL: excusable (a docker.internal line holds the outbound IPv4)' }
    else { 'TestLookupLocalPTR FAIL: NOT excused (no docker.internal line on the outbound IPv4)'; $ptr = '; LocalPTR FAIL NOT excused' } }
  'VERDICT: ' + $(if (-not $ran) { 'NO RUN (build/launch failed: no "=== RUN   TestLookupNoSuchHost" in its call''s log)' }
    elseif ($c['panic: test timed out']) { 'BROKEN, TIMEOUT class (killed by -timeout 40m; the leaf counts are partial) -- never a pass' }
    elseif ($skip) { 'NOT JUDGEABLE (TestLookupNoSuchHost or a leaf SKIPPED)' }
    elseif (-not $rootLine) { 'NO VERDICT (test binary died; read the raw log locally)' }
    elseif ($pass -and $cache0 -gt 0) { 'NOT JUDGEABLE (DNS Client cache already held *invalid.invalid before go test)' }
    elseif ($pass -and $rcNsh -eq 0) { 'DNS-CONFORMING (TestLookupNoSuchHost passed, rc=0; the CNAME drift is tolerated)' }
    elseif ($pass -or $rcNsh -eq 0) { "INCONSISTENT (log PASS=$([int]$pass) but rc=$rcNsh): read the raw log locally" }
    else { "BROKEN, $cls" }) + $ptr }
if ($MyInvocation.InvocationName -eq '.') { return }
# An unforeseen terminating error prints its record with the script's path: print only its line, category and type instead.
trap { 'SCRIPT FAULT at line {0}: {1} {2} (the error record is not printed: it can carry paths)' -f $_.InvocationInfo.ScriptLineNumber, $_.CategoryInfo.Category, $_.Exception.GetType().Name
  Pop-Location -StackName dnsprobe -ErrorAction SilentlyContinue; exit 1 }
function Done([int]$rc) { Pop-Location -StackName dnsprobe -ErrorAction SilentlyContinue; exit $rc }

$stamp = Get-Date -Format yyyyMMddHHmmss
"dns-probe.ps1 (Windows leg) stamp $stamp -- read-only; resolvers print as KINDS only"
'NOTE: DNS-CONFORMING is NOT host qualification: banking net still needs go test -count=1 -timeout 40m net, its failing set judged by the ledger.'
if ("$env:GODEBUG" -match 'netdns') { 'ABORT: GODEBUG names netdns: it changes which resolver the default leaves use'; exit 2 }
$cleared = 'GODEBUG', 'GOFLAGS', 'GO_BUILDER_FLAKY_NET', 'GOCACHEPROG', 'GOOS', 'GOARCH', 'GOEXPERIMENT'  # FLAKY_NET turns every DNS failure into a SKIP
$was = foreach ($k in $cleared) { '{0}={1}' -f $k, $(if ([Environment]::GetEnvironmentVariable($k)) { 'SET' } else { 'unset' }) }
$env:GOROOT = $GoRoot; $env:GOTOOLCHAIN = 'local'; $env:CGO_ENABLED = '0'; $env:GOENV = 'off'; $env:GOWORK = 'off'  # GOENV=off: a 'go env -w' GOFLAGS cannot apply
foreach ($k in $cleared) { Remove-Item "Env:$k" -ErrorAction SilentlyContinue }
$go = [IO.Path]::Combine($GoRoot, 'bin', 'go.exe'); if (-not (Test-Path -LiteralPath $go)) { 'ABORT: no bin\go.exe under -GoRoot'; exit 2 }
Push-Location -StackName dnsprobe -LiteralPath $env:TEMP  # before the FIRST go call: a go.mod/go.work in the caller's cwd can refuse GOTOOLCHAIN=local
$ver = "$(& $go version 2>$null)"; $rc = $LASTEXITCODE    # stderr dropped: a cmd/go warning can quote a path; the guards judge the values
$ej = "$(& $go env -json CGO_ENABLED GOFLAGS GOROOT GOENV 2>$null)"; $e = $null; if ($ej.Trim()) { try { $e = $ej | ConvertFrom-Json -ErrorAction Stop } catch { } }
$rootOk = $null -ne $e -and ("$($e.GOROOT)".TrimEnd('\', '/') -replace '/', '\') -eq ($GoRoot.TrimEnd('\', '/') -replace '/', '\')
"go: $ver (rc=$rc) CGO_ENABLED=$($e.CGO_ENABLED) GOTOOLCHAIN=local GOENV=off GOWORK=off (env file: $(if ($e.GOENV) {'IN USE'} else {'none'})) GOFLAGS=$(if ($e.GOFLAGS) {'SET'} else {'empty'})" +
  " GOROOT=$(if ($rootOk) {'matches -GoRoot'} else {'MISMATCH'})"
"env the probe cleared (value before: SET/unset; values are never printed): $($was -join ' ')"
if ($rc -ne 0 -or $ver -notmatch 'go1\.24\.13 windows/' -or $e.CGO_ENABLED -ne '0' -or $e.GOFLAGS -or $e.GOENV -or -not $rootOk) {
  'ABORT: need go1.24.13 windows, CGO_ENABLED=0, empty GOFLAGS, no go env file, go env GOROOT = -GoRoot'; Done 2 }

'--- resolver readout, KINDS only (Go''s forced-go leg lists every server of adapters that are Up AND have a gateway; skips fec0; DROPS any zone)'
$arms = [ordered]@{}
foreach ($c in @(Get-NetIPConfiguration -All -ErrorAction SilentlyContinue)) {
  if ($c.NetAdapter.Status -ne 'Up') { continue }
  $gw = [bool]($c.IPv4DefaultGateway -or $c.IPv6DefaultGateway)
  $s = @(Get-DnsClientServerAddress -InterfaceIndex $c.InterfaceIndex -ErrorAction SilentlyContinue | ForEach-Object { $_.ServerAddresses } | Where-Object { $_ })
  'adapter#{0} gw={1} dns=[{2}]{3}' -f $c.InterfaceIndex, $gw, (($s | ForEach-Object { Kind $_ }) -join ','), $(if ($gw) { '  <- in Go''s list' })
  if ($gw) { foreach ($a in @($s | ForEach-Object { Canon $_ } | Where-Object { $_ -notlike 'fec0:*' })) {  # keys compared as addresses
      if ((Kind $a) -eq 'll6') { $arms["$a%$($c.InterfaceIndex)"] = 'll6+zone (DNS Client path; NOT Go''s)'; $arms[$a] = 'll6 no-zone (Go''s path; UNVERIFIED probe)' }
      else { $arms[$a] = Kind $a } } }
  if ($ShowAddresses) { Write-Host "  OWNER CONSOLE ONLY - REDACT: $($c.InterfaceAlias) $($s -join ' ')" -ForegroundColor Yellow } }
foreach ($a in $Pub) { $arms[$a] = $(if ($arms.Contains($a)) { 'public-ctl, also in Go''s list' } else { 'public-ctl' }) + $(if ($a -like '*:*') { ' v6' } else { ' v4' }) }
$r4 = @(Get-NetRoute -DestinationPrefix '0.0.0.0/0' -PolicyStore ActiveStore -ErrorAction SilentlyContinue | Sort-Object { $_.RouteMetric + $_.InterfaceMetric })
$out4 = @(if ($r4.Count) { Get-NetIPAddress -InterfaceIndex $r4[0].InterfaceIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue | ForEach-Object { $_.IPAddress } })
$hl = @(Get-Content "$env:windir\System32\drivers\etc\hosts" -ErrorAction SilentlyContinue); $dockGw = DockerLines $hl $out4
'NRPT rules: {0}   hosts docker.internal lines: {1} total, {2} on the outbound IPv4 (LocalPTR is excusable only when > 0)' -f @(Get-DnsClientNrptPolicy -ErrorAction SilentlyContinue).Count, (DockerLines $hl $null), $dockGw
function CacheInv { @(Get-DnsClientCache -ErrorAction SilentlyContinue | Where-Object { $_.Entry -like '*invalid.invalid*' }).Count }

if (-not $LookupsOnly) {  # go test FIRST: a lookup arm must not seed the DNS Client's negative cache before the default/cgo leaves read it
  $cache0 = CacheInv
  "DNS Client cache entries for *invalid.invalid before go test: $cache0 (> 0: a NoSuchHost pass reads NOT JUDGEABLE -- the default/cgo leaves may be cache reads)"
  $logs = "dns-probe-$stamp-1.log", "dns-probe-$stamp-2.log"; $rcNsh = $null; $i = 0  # one log per call: only call 2's judges NoSuchHost
  foreach ($pat in '^(TestLookupCNAME|TestLookupLocalPTR)$', '^TestLookupNoSuchHost$') {
    $log = $logs[$i]; $i++; $t = [Diagnostics.Stopwatch]::StartNew()
    & $go test -count=1 -timeout 40m -run $pat -v net *> $log; $rc = $LASTEXITCODE; $rcNsh = $rc
    'go test -run {0} : rc={1} wall={2:n0}s' -f $pat, $rc, $t.Elapsed.TotalSeconds }
  $L1 = @(Get-Content -LiteralPath $logs[0] -ErrorAction SilentlyContinue); $L2 = @(Get-Content -LiteralPath $logs[1] -ErrorAction SilentlyContinue)  # a missing log reads NO RUN
  "DNS Client cache entries for *invalid.invalid after go test: $(CacheInv) (> 0: the system arm's invalid.invalid rows may be cache reads)" }
else { 'CAUTION -LookupsOnly: these lookups can seed the DNS Client negative cache (TTL UNVERIFIED, ~15 min); start no net, crypto/tls or net/http row here inside that window' }
'--- timed lookups: class/seconds (system = the Windows DNS Client: Go''s default and forced-cgo legs; server arms = Go''s forced-go list)'
'system flap x6 (fresh nx .com): ' + ((1..6 | ForEach-Object { Probe "nx-$stamp-$_.com" '' '' }) -join ' ')
Arm 'system (DNS Client)' '' "nx-$stamp-a0.com"
$i = 0; foreach ($a in $arms.Keys) { $i++; Arm ('server#{0} {1}' -f $i, $arms[$a]) $a "nx-$stamp-a$i.com" }
if ($LookupsOnly) { Done 0 }
'--- go test summary (rc of the NoSuchHost call is a valid discriminator: 0 = passed, no kill; the CNAME call''s rc is not)'
Summarize $L1 $L2 $rcNsh $dockGw $cache0
"raw logs, LOCAL ONLY (may hold addresses and the host name): %TEMP%\$($logs[0]) (CNAME/LocalPTR) and %TEMP%\$($logs[1]) (NoSuchHost)"
'NOTE: DNS-CONFORMING is NOT host qualification: banking net still needs go test -count=1 -timeout 40m net, its failing set judged by the ledger.'
Done 0
