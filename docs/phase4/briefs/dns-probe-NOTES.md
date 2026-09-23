# DNS lane probe: interpretation (amended 2026-09-22)

Scripts: `dns-probe.ps1` (Windows leg) and `dns-probe.sh` (WSL/Linux leg), both in the COORD scratchpad. They are
read-only and address-free. They answer one question per physical lane: does this box's DNS answer NXDOMAIN the way
Go 1.24.13 net's `TestLookupNoSuchHost` needs, or does it fail (fast SERVFAIL, or TIMEOUT)?
Source citations below are `$GOROOT(go1.24.13)/src/net/...`. Mailbox citations are archive line numbers only.
This file holds no address, host name, account name or profile path. Keep it that way.

## 0. Verdict names (the scripts' last lines)

Each go test call writes its own raw log: `dns-probe-<stamp>-1.log` (TestLookupCNAME + TestLookupLocalPTR) and
`dns-probe-<stamp>-2.log` (TestLookupNoSuchHost). **Everything that judges NoSuchHost reads the `-2` log only**: the
totals, the failing modes, the marker counts, the kill and the class. The first call's lines never reach the verdict.

| VERDICT | Meaning |
|---|---|
| `DNS-CONFORMING` | `TestLookupNoSuchHost` PASSED **and** the NoSuchHost call's rc = 0, with no kill and no SKIP, and (Windows) the DNS Client cache held no `*invalid.invalid` entry before go test. The CNAME drift is tolerated. |
| `BROKEN, SERVFAIL class` / `TIMEOUT class` / `MIXED class` / `HIJACK class` / `class unread` | NoSuchHost failed. The class comes from the marker counts in the NoSuchHost call's log (section 4). |
| `BROKEN, TIMEOUT class (killed by -timeout 40m ...)` | `panic: test timed out` in the NoSuchHost call. The leaf counts are partial. This is **never** a pass. |
| `NO RUN (build/launch failed ...)` | `=== RUN   TestLookupNoSuchHost` is absent from its call's log (or that log is missing): a toolchain, GOROOT or build fault, **not** a DNS verdict. Report the VERDICT line only; COORD decides the re-run. |
| `NO VERDICT (test binary died ...)` | NoSuchHost started but has no `--- PASS/FAIL/SKIP` root line and there is no timeout panic: a crash or runtime fault, **not** a DNS verdict. Report the VERDICT line only. |
| `NOT JUDGEABLE (TestLookupNoSuchHost or a leaf SKIPPED)` | For example `-short` or GO_BUILDER_FLAKY_NET leaked in. No DNS verdict. |
| `NOT JUDGEABLE (DNS Client cache already held *invalid.invalid before go test)` | Windows only: NoSuchHost passed, but its default/cgo leaves may have been cache reads. No DNS verdict; re-run after the negative-cache TTL (~15 min, UNVERIFIED). A FAILED NoSuchHost with a seeded cache still reads BROKEN. |
| `INCONSISTENT` | The log's PASS line and the NoSuchHost call's rc disagree. No verdict. Report the VERDICT line only. |
| `...; LocalPTR FAIL NOT excused` (suffix, Windows) | `TestLookupLocalPTR` failed and no docker.internal line holds the outbound IPv4 (section 2). Appended to whatever the verdict is, so the VERDICT line alone carries it. |
| `ABORT: ...` (exit 2) | A pre-flight guard refused. Nothing was measured. |
| `SCRIPT FAULT at line N: ...` (exit 1, Windows) | An unforeseen terminating error. Its record is not printed (it can carry paths); only the line, category and type are. No verdict. |

**Exit codes.** `0` = the script ran to its VERDICT line, **whatever the verdict** (BROKEN, NO RUN and NOT JUDGEABLE
exit 0 too); `2` = ABORT, nothing measured; `1` (Windows) = SCRIPT FAULT. The VERDICT line is the result, never the
exit code. A lane reports the exit code AND the VERDICT line.
A kill in the FIRST call (CNAME/LocalPTR) prints `CNAME/LocalPTR call killed by -timeout 40m ...` on its own line and
never feeds the NoSuchHost verdict or class.

**DNS-CONFORMING is not host qualification.** A three-test `-run` subset can show DNS conformance, but it cannot
qualify a host (measurement-discipline SKILL.md:292-300). Before a box reads or banks `net` after any resolver change,
it still needs the full-suite qualification: `go test -count=1 -timeout 40m net`, with its FAILING SET judged by the
ledger (CNAME tolerated with evidence; any other failing leaf ABORTS). The i7's 27-row reading run may serve as that
reading, if its failing set is judged by the ledger. Both scripts print this note in the header and again in the footer.

## 1. Order of operations, and why

1. **Guards.** Both scripts set `GOENV=off`, which stops a `go env -w` GOFLAGS from applying (cmd/go's cfg.Getenv falls
   through to the env file). They unset GOFLAGS and GO_BUILDER_FLAKY_NET (FLAKY_NET turns every DNS failure into a SKIP),
   and unset GODEBUG, **aborting** first if GODEBUG names `netdns`, which changes which resolver the default leaves use.
   They also clear GOCACHEPROG (in 1.24 it makes cmd/go launch the named helper as the build cache, a child process
   the probe does not control), GOOS, GOARCH and GOEXPERIMENT (a cross target turns the run into NO RUN).
   They set GOTOOLCHAIN=local, CGO_ENABLED=0 and GOWORK=off, and pin GOROOT: Windows to `-GoRoot`, bash to the go
   binary's own `../` (never an inherited GOROOT). Each then asserts `go env` agrees. The header echoes all of this as
   values or SET/empty flags, and each cleared variable's prior state as SET/unset. It never prints a GOROOT, env-file,
   GOFLAGS or GOCACHEPROG path, nor GODEBUG's value (ABORT says only that it names netdns).
   Both move to `%TEMP%` / `/tmp` BEFORE the first go call: a go.mod or go.work with a newer `go` line in the caller's
   cwd or a parent makes GOTOOLCHAIN=local refuse (a false ABORT). The Windows script restores the caller's location
   on every exit. The stderr of `go version` / `go env` is discarded: a cmd/go warning can quote a path (for example
   `ignoring go.mod in system temp root <%TEMP% expanded>`), and the guards judge the values, not the warnings.
   *Where the scripts live:* the .ps1 on a LOCAL disk (never a share or UNC path) and the .sh as `/tmp/dns-probe.sh`
   inside the distro: any error record prints the script's own path. The .ps1 also traps unforeseen terminating
   errors and prints only `SCRIPT FAULT at line N: <category> <type>` (exit 1); its readout and log reads run with
   `-ErrorAction SilentlyContinue`, and a missing log reads NO RUN.
   *Live evidence for the GOROOT pin:* on the i7 a plain go1.24.13 `go build` under the inherited machine-scope GOROOT
   failed with `compile: version "go1.23.1" does not match go tool version "go1.24.13"`. That is exactly the case the old
   bash probe would have reported as a DNS failure.
2. **Readout** (kinds only).
3. **`go test` FIRST, then the lookups.** A lookup arm could otherwise seed the DNS Client's negative cache for
   `invalid.invalid.` before the default/cgo leaves read it: DnsQuery options 0 and GetAddrInfoW both read that cache.
   Windows prints `DNS Client cache entries for *invalid.invalid` before go test (> 0 = the default/cgo leaves may be
   cache reads; the count reaches the verdict, so a NoSuchHost pass then reads **NOT JUDGEABLE**) and after it (> 0 =
   the system arm's invalid.invalid rows may be cache reads).
   The same cache links the two legs of one box: a WSL that follows the Windows host (DNS tunneling or the NAT DNS
   proxy, both answered by the Windows DNS Client) can read the negative cache the Windows leg's lookups seeded. So
   either run the WSL leg first or wait ~16 min after the Windows lookups end (the G dispatch waits); otherwise a
   host-following WSL's verdict is not independent evidence.
4. **Lookups**, then the **summary** and **VERDICT**.

`-timeout 40m` is set explicitly on both go test calls. The 10-minute default once produced a pass-shaped false green
(:161068-161080). The old 15m cap would almost certainly kill a TIMEOUT-class host (section 3).
`-LookupsOnly` / `lookups-only` skips go test. On Windows it prints a caution: the lookups can seed the negative cache
(TTL UNVERIFIED, ~15 min), so start no net, crypto/tls or net/http row on that box inside that window.

## 2. Readout: signatures and meaning

Kinds, first match wins: `ctl-match` (equals a `-PublicResolver` / `PUBLIC_RESOLVER` value), `fec0` (Windows'
placeholder, which Go skips), `ll6` (fe80/10), `ula6` (fc/fd), `loop`, `lan4` (RFC 1918), `cgnat` (100.64/10), `ll4`
(169.254/16), and `public` (any other global address).
Addresses are compared **as addresses**, not as text: both sides are canonicalized first (any `%zone` dropped, IPv6 in
compressed lowercase RFC 5952 form; PowerShell via `[Net.IPAddress]`, bash via an awk canonicalizer), so a control
given as `2001:DB8:0:0:0:0:0:53` matches an adapter's `2001:db8::53`, and the arm list is deduplicated the same way.

| Signature | Meaning |
|---|---|
| `adapter#N gw=True dns=[lan4,ll6,ll6]  <- in Go's list` | The unfixed shape (the i7 today). Go's forced-go leg and the DNS Client both use only the LAN resolver, over IPv4 and IPv6. |
| Every server in Go's list reads `ctl-match` | **Fully fixed per box.** Pass the resolvers you actually set as `-PublicResolver`. |
| A `public` kind in Go's list | NOT proof of a fix. It is some other global address: a router's global IPv6 advertised by RA/RDNSS, an ISP resolver, a VPN resolver. |
| `lan4` / `ll6` still in Go's list next to `ctl-match` | Incomplete fix: the R 08-29 shape (IPv4 fixed, IPv6 left on the router). |
| `cgnat` / `ll4` | VPN or overlay DNS (typical of R-LAPTOP's VPN adapter). Leave VPN DNS alone. |
| More than one adapter with `gw=True`, or `NRPT rules` > 0 | VPN / split DNS. Go lists the servers of every gateway adapter; the DNS Client routes by NRPT/interface. |
| `ll6+zone (DNS Client path; NOT Go's)` and `ll6 no-zone (Go's path; UNVERIFIED probe)` | Go **drops the zone**: dnsReadConfig copies only `sa.Addr` (dnsconfig_windows.go:49-51, JoinHostPort at :63), so it dials link-local servers WITHOUT a zone. The zoned row is the path the DNS Client can take. **A zoned ll6 NXDOMAIN never proves the forced-go leaves pass.** The no-zone row is Go's path, but how Resolve-DnsName handles a zone-less link-local `-Server` is UNVERIFIED. A non-DNS `ERR:` there is a probe artefact, not a verdict. On multi-vEthernet boxes (Hyper-V, WSL, Docker) a zone-less send may leave by another interface. |
| `hosts docker.internal lines: T total, M on the outbound IPv4` | LocalPTR compares `LookupAddr(localIP())` with `ping -a`, where localIP() is the UDP-dial source (lookup_windows_test.go:153-176, :323-333). Only lines whose address equals the outbound IPv4 can cause a LocalPTR mismatch. The outbound IPv4 is taken from the adapter of the lowest-metric default route. **LocalPTR FAIL is excusable only when M > 0.** On the i7, T = 3 and M = 0, so an i7 LocalPTR FAIL is not Docker's. (If localIP() dials over IPv6, the IPv4 lines cannot matter.) |
| WSL `wsl.conf: generateResolvConf = false` + `resolv.conf: regular file, no WSL header (pinned)` + nameservers with no `lan4/ll6/loop` | The 09-02 pin is intact. |
| WSL `SYMLINK -> WSL-generated`, or `regular file with the WSL-generated header` | The WSL follows the Windows host and inherits the Windows leg's fault. On G-LAPTOP this means the pin has lapsed. Its go test verdict is then **not independent** of the Windows leg unless ~15 min separated the legs (section 1, item 3). |
| WSL `SYMLINK -> systemd-resolved STUB` (nameservers `loop`) | A different case, labelled separately. Go's servers are the stub, and the real upstreams live in resolved, not in the file. Read the dig arms against the stub. **On a systemd-resolved stub, invalid.invalid may be answered locally by resolved** (it treats the RFC 6761 `invalid` TLD as special-use and does not forward it; UNVERIFIED, from memory of systemd's dns_name_dont_resolve, not re-read), so a DNS-CONFORMING verdict there says nothing about the upstream: read the `nx.com` cells, which do reach it. |
| WSL `nameservers: <kinds> \| beyond Go's 3, not queried by Go (no arm): <kinds>` | Go 1.24.13's unix resolver keeps only the first 3 `nameserver` lines whose value parses as an IP (dnsconfig_unix.go:52, `len(conf.servers) < 3`). Those are Go's list and get dig arms; later lines are only labelled, never probed. |
| WSL `readlink -f = ...` | Printed only for system paths (`/etc`, `/run`, `/mnt/wsl`, `/var`). Anything else prints as `other (leaf ...)`. |
| WSL `nsswitch hosts: ...` | Python's getaddrinfo follows this (it may go through nss-resolve). Go with cgo off reads resolv.conf directly, so all three Go modes take the dig arms' path. |

## 3. Timed lookups: signatures and meaning

Each cell is `CLASS/seconds`. The Windows classes are NXDOMAIN, NODATA, SERVFAIL, REFUSED, TIMEOUT, ANSWERED and
`ERR:<id>` (redacted). dig adds `no-reply`. Python adds TEMPFAIL.
Each arm probes: `ex.com` (the example.com control), `nx.com` (a fresh `nx-<stamp>` name), `host` (A/AAAA), and CNAME,
MX, NS, TXT on `invalid.invalid.`, plus SRV on `_unknown._tcp.invalid.invalid.` (the name the SRV leaf queries,
lookup_windows.go:305-310). Arms: `system` (the Windows DNS Client = Go's default and forced-cgo legs), each server in
Go's list (the forced-go leg), and `public-ctl v4` / `v6`. On WSL, the `system` arm is getaddrinfo (host only) and every
per-type arm runs through dig against Go's list (resolv.conf's first 3 IP nameservers) plus the public controls. The
WSL dig arms carry `host` (A) **and** `AAAA` cells: Go's LookupHost sends both, so read the worse of the two (a
resolver that answers NXDOMAIN for A but fails AAAA still fails the LookupHost leaves). Arm labels are padded to 52
columns so every row's cells line up.

| Signature | Meaning |
|---|---|
| NXDOMAIN in well under 1 s on every invalid.invalid type | Conforming. |
| NODATA on invalid.invalid | Go accepts it: winError maps DNS_INFO_NO_RECORDS to errNoSuchHost, and Go's resolver maps no-answer to errNoSuchHost. But a correct resolver says NXDOMAIN. Note it; it is not a failure. |
| SERVFAIL, fast (Linux: TEMPFAIL, fast) | The recorded fault through 09-08. |
| TIMEOUT after seconds (Linux dig: TIMEOUT at the 5 s cap, matching Go's per-attempt timeout) | The resolver drops the query. This is the likely class now (see *Error class*). |
| ANSWERED on a nonexistent name | Hijacking resolver. Go fails with `unexpected success` (HIJACK class). |
| `ex.com` not ANSWERED on an arm | That arm is unreachable, and its row means nothing. |
| `public-ctl v6` `ex.com` not ANSWERED | The box lacks working IPv6 to the internet. Do NOT set public IPv6 resolvers per box (each dead v6 server costs Go 5 s x 2 attempts); prefer the LAN-resolver option (c). |
| `system flap x6` not all NXDOMAIN | The flap (R 08-29 was 11 of 12). |
| Per-type split: `host` NXDOMAIN but CNAME/MX/NS/SRV/TXT fail on the same arm | The i9 recon shape (LookupHost default/cgo pass, every other kind fails). Still broken. |
| server `lan4`/`ll6` rows fail while `public-ctl` answers NXDOMAIN on every type | The resolver is at fault and fixable: option (c) or per box (a). |
| `public-ctl` ALSO fails while its `ex.com` answers | Path interception. Per-box changes will not help; the router or ISP must be fixed. |
| invalid.invalid fails but `nx.com` is NXDOMAIN on the same server | Reserved-TLD special case. The test uses invalid.invalid., so it is still broken. |
| WSL `system ... A_AAAA=NXDOMAIN` (errno -2) | **WSL acceptance check.** `A_AAAA=TEMPFAIL` (errno -3) = broken. getent's exit code is NOT a criterion: its hosts database exits 2 for NXDOMAIN and SERVFAIL alike (UNVERIFIED against glibc source, but the check no longer depends on it). With nss-resolve in the nsswitch line this row is advisory; the go test is the verdict. |

## 4. go test: signatures and meaning

| Signature | Meaning |
|---|---|
| `TestLookupNoSuchHost PASS <s>` in seconds, 0 failing verdicts, `panic: test timed out x0`, NoSuchHost rc=0 | DNS-CONFORMING (after its fix, G's WSL ran the whole net suite in 35.3 s). `TestLookupCNAME FAIL` is tolerated (drift). |
| `NoSuchHost failing verdicts: root 1 + parents 6 + leaves 16 = 23` | The i9 recon / G 09-22 shape. The six NXDOMAIN kinds fail in all modes, except LookupHost default/cgo, which pass. `failing modes:` per kind shows which. |
| `... leaves 18 = 25` | Every NXDOMAIN leaf fails in every mode (the i9 09-08 shape). The NODATA kinds always pass. |
| Marker counts (all rows below) | Counted over the **NoSuchHost call's log only**. TestLookupCNAME retries real names and ends in t.Fatal (lookup_test.go:356-371), so its lines can carry `server misbehaving` or `i/o timeout`; they must not turn a SERVFAIL-class NoSuchHost into MIXED, and now cannot. |
| Markers `server misbehaving` (forced-go) / `DNS server failure` (Windows legs), no timeouts | SERVFAIL class: ~36-42 s per leaf, ~10-13 min for the call. `IsNotFound is set to false` is high. |
| Markers `i/o timeout` (forced-go) / `timeout period expired` (Windows legs) | TIMEOUT class. Per forced-go leaf roughly (5 s x 2 attempts x N servers) x 4 tries + 36 s backoff, plus DNS Client timeouts on the other legs: ~25-30+ min for the call. |
| Both marker families | MIXED class. |
| `temporary error` | Ambiguous (WSATRY_AGAIN from GetAddrInfoW); counted, never classed alone. |
| `panic: test timed out` x1, `TestLookupNoSuchHost KILLED (-timeout 40m)` | Killed by -timeout 40m in the NoSuchHost call: BROKEN, TIMEOUT class, never a pass. |
| `CNAME/LocalPTR call killed by -timeout 40m ...`, and `TestLookupCNAME` / `TestLookupLocalPTR` `KILLED` | CORRECTED: a kill in the FIRST call used to be read from the combined log and blamed on NoSuchHost (a later NoSuchHost PASS printed `BROKEN, TIMEOUT class (killed ...)`). Each call now has its own log: the first call's kill is reported on its own line, and NoSuchHost is judged from its own call. |
| `TestLookupNoSuchHost DIED (no result line ...)` | The test binary died after `=== RUN` without a root result or a timeout panic: VERDICT NO VERDICT, not a DNS verdict. `NOT REACHED` is printed only when the test never started. |
| rc of the **NoSuchHost** call | A valid discriminator: rc 0 = passed with no kill. The **CNAME** call's rc is not (CNAME drifts, so it is 1 on essentially every host). Name and verdict parsing stay primary; a disagreement prints INCONSISTENT. |
| `TestLookupLocalPTR FAIL: NOT excused` | LocalPTR must PASS unless a docker.internal line holds the outbound IPv4 (section 2). The VERDICT line then ends in `; LocalPTR FAIL NOT excused`, so a DNS-CONFORMING verdict cannot hide it. |

**Adapter-list signal (corrected).** tryOneName (dnsclient_unix.go:314-349) **continues** to the next server after a
SERVFAIL or a timeout, and **returns** on the first NXDOMAIN (:329-333, :338-343). A forced-go leaf therefore fails only
when **NO server in Go's list answers NXDOMAIN**; one conforming server anywhere in the list is enough.
So "only the forced_go leaves fail" means: no gateway-adapter server answers NXDOMAIN, while the Windows DNS Client
gets a not-found some other way (a server Go does not list, the cache, or an LLMNR/NetBIOS fallback; which one is
UNVERIFIED). Fix: give a gateway adapter at least one conforming server.
It does NOT mean "one server in Go's list is still bad".

**Both-families rule (re-based).** A Windows fix must still cover BOTH address families on every adapter with a
gateway. The basis is the DNS Client legs (default/cgo), not the forced-go list: R's 08-29 ~8% flap with IPv6 left on
the router (:36511-36530).

**Error class (re-worded).** The fault was fast SERVFAIL through 09-08. It is likely TIMEOUT class by ~09-20, which is
UNVERIFIED until the lookups arm reads it. The evidence:
- The i9 at go1.24.13 went from 705 s for all of net with 25 NoSuchHost verdicts on 09-08 (:149278-149315) to 1,865 s
  with 23 on 09-22.
- G's whole suite took 1,834 s on 09-22.
- lookup_windows.go, dnsconfig_windows.go, main_conf_test.go and the NoSuchHost body are byte-identical between
  go1.23.12 and go1.24.13, so the change is environmental.
- R's 08-28 record already held an intermittent `timeout period expired` (:35964).

**The 19:34 split.** '5 parents + 18 leaves' is internally inconsistent: 18 failing leaves across 6 NXDOMAIN kinds x 3
modes forces all 6 parents to fail. It is likely 1 + 6 + 16, pending the 19:34 run's own log.

## 5. Acceptance after any change

1. Re-run the same script on that box and read **VERDICT: DNS-CONFORMING**. The Windows leg also needs every server in
   Go's list to read `ctl-match` (or the LAN resolver conforming on every type), `ex.com` answered on every arm, and
   the flap x6 all NXDOMAIN. The WSL leg also needs `A_AAAA=NXDOMAIN`.
2. Then run the full-suite qualification (section 0) before the box reads or banks net.

## 6. Findings routed elsewhere (not scripts, interpretation or the G dispatch; owed to their homes)

- **Owner steps, WSL step 1:** use `wsl -e grep -i generateResolvConf /etc/wsl.conf` (add `-d <distro>` if needed).
  Without `-e`, the distro shell turns `^\[network\]` into a bracket expression and a pinned file prints nothing
  (UNVERIFIED; reasoned from wsl.exe's shell-vs-exec behaviour). The G dispatch launches the probe with `-e` for the
  same reason.
- **Owner steps, Windows revert:** record `netsh interface ipv4|ipv6 show dnsservers name="<alias>"` (static vs DHCP)
  before any change, revert to exactly that state, and re-read both families afterwards.
- **Owner steps, Windows step 4:** set public IPv6 per box only if the probe's `public-ctl v6 ex.com` answered.
- **`wsl --shutdown`** (Windows step 5, LAN-resolver step 6): only when no lane work and no Docker workload runs in WSL
  on that box, at a moment COORD names.
- **LAN-resolver option (c):** schedule a window when every LAN lane is idle, no push is in flight and the ab8b battery
  is finished. Change the upstream servers only, never rebind protection or filtering. Right after, check that one box
  resolves example.com and gets NXDOMAIN for a fresh nx name. Keep the revert screenshot.
- **i7 per-box change:** afterwards, check that the standing mapped drives still open (targeted Test-Path, never a
  recursive walk), and revert if they do not.
- **History:** R's 09-08 two-networks statement (:150081) is UNVERIFIED. R-LAPTOP's 09-08 readings bind to its network
  that day, so rest the LAN-resolver claim on G-LAPTOP's 09-08 .com pair (:161547-161556) plus the i9. The 09-02
  default-user inference is **contradicted** (G's :70987-70989 and :71006 describe a wsl.conf with no [user] section),
  so the cause is unknown; keep the back-up-and-edit advice.
- **Lane order:** R's crypto/tls evidence row FINISHED (LEDGER.md:182), so R is free for its probe. The only constraint
  is that R's HELD deciding arm must not overlap it. Whether R is on the home LAN is UNVERIFIED. Re-read the ledger
  tail before dispatch. None of this gates G.
- **i9 dispatch (later):** check the box is idle first (one task at a time), use `-LookupsOnly` (no compile), carry
  "stop and report on any fault", and heed the negative-cache caution above.
- **i7 `-LookupsOnly`:** omit the public-ctl arm, or run it only when no net, crypto/tls or net/http row starts within
  ~15 min.
- **Ledger CORRECTION and relay text:** cite `$GOROOT(go1.24.13)/src/net/...` and "COORD scratchpad" only. Run the
  identifier census (case-insensitive `Users\`, `/home/`, the account token) on the line before the signed push.

## 7. Still UNVERIFIED

- Resolve-DnsName's handling of a zoned or zone-less link-local `-Server`.
- Resolve-DnsName's cache semantics (`-CheckCache` exists; there is no local help).
- The negative-cache TTL.
- glibc getent's exit code on TRY_AGAIN.
- wsl.exe's shell quoting.
- Whether zone-less link-local sends reach the router on multi-vEthernet hosts.
- Why GetAddrInfoW passes LookupHost on the i9 (LLMNR/NetBIOS, another server, or the cache).
- Whether R-LAPTOP's "11 resolvers" double-counts across adapters.
- The self-heal of a CRLF-saved dns-probe.sh on real Linux bash: MSYS bash tolerates CRLF by itself, so the i7 tests
  prove only that the heal re-exec path triggers and cleans up.
- Whether systemd-resolved answers the RFC 6761 `invalid` TLD locally (section 2's stub row).
- wsl.exe's argument passing for `-e sh -c '<cmd>'` (the dispatch's WSL hash check).

## 8. How the scripts were checked (i7, no lookups, no go test)

- **Syntax:** pwsh 7.4.6 and Windows PowerShell 5.1 ParseFile report 0 errors; `bash -n` is clean.
- **Unit tests** (`dns-probe-tests/`, pure functions + guards): `make-fixtures.sh` builds synthetic go test -v logs
  (combined, plus the per-call `X.1.log` / `X.2.log` splits and the cross-call cases) and dig outputs;
  `fx-expect.tsv` holds one row per call-1/call-2/rc/dockGw/cache case. `test-ps.ps1` runs green under pwsh 7 and
  PS 5.1; `test-sh.sh` runs green under Git Bash. They cover:
  - Kind (including cgnat, ll4, and ctl-match compared AS addresses), Canon/canon, ErrClass/Redact, DockerLines, dig
    classing, and TestLine (PASS/FAIL/SKIP, KILLED, DIED, NOT REACHED).
  - The summarizer on 20 per-call fixture cases (16 run on both shells, 4 Windows-only: LocalPTR, the DNS Client
    cache). PS 7, PS 5.1 and bash VERDICT/totals lines are identical (CRs aside).
  - The guards, with a fake go that refuses `test` and every lookup tool shadowed: exit 2, no path, no env value
    (GODEBUG, GOCACHEPROG), go run from `%TEMP%` / `/tmp` (the fake records its cwd), the caller's location restored;
    the CRLF heal, and a stray `DNS_PROBE_LF` deleting nothing and skipping no heal.
- **Adversarial end-to-end runs** (`dns-probe-tests/adv/adv-e2e.ps1` and `adv-e2e.sh`, the MAIN-BODY coverage): every
  DNS/network cmdlet or tool is shadowed and go is a fake that replays split synthetic logs. They check the readout,
  the arm list (canonical dedupe, Go's first 3 nameservers, the AAAA cell, column alignment), go test before any
  lookup, the env scrub, go run from `%TEMP%` / `/tmp` with the caller elsewhere, one log per call, exit 0 on every
  verdict, and the sanitized SCRIPT FAULT (exit 1). Green under pwsh 7, PS 5.1 and Git Bash; red on the pre-fix
  scripts at every fixed site.
- **Mutation run** (`mutate.ps1`): each plant names the harness that must catch it (unit or adv) and must turn that
  harness red, naming the site. The comment-only NEUTRAL plant in each script must stay green. The earlier "neutral"
  zone-strip plant was not neutral (nothing tested that path); Canon's zone strip is now unit-tested, so it is a real
  plant. Earlier, the guard tests caught a real bug: the bash guard's `case` string let a `/`-leading GOENV path
  satisfy the empty-field pattern and proceed past the abort. It is fixed with explicit tests.
- **Lesson for anyone driving these tests from PowerShell:** bare `bash` on the i7's PATH is the WSL launcher (it boots
  the VM). Call Git Bash by its full path, as mutate.ps1 does.
