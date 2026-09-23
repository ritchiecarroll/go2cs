# net at go1.24.13 on the i7 -- the reading for C1's net labels (evidence only; nothing banked)

**Tree:** `claude/version-go1.24.13` = `fc6269b0bf` (the batch-8e stamp). **Mode:** the tree's own `src/run-h10-recon.ps1`
(blob `f17cc5b437`), `-tests -test-action all -test-config Release`, tiering OFF (argv without `-test-tiered`; both
records read `tiered=false`), `-test-timeout 120m` (net's floor). **Host qualification (measurement-discipline):** Go's own
`go test -count=1 -timeout 40m net` at go1.24.13 ran TWICE on this host just before the row and failed ONLY
`TestLookupCNAME` (the tolerated upstream drift), about 50 s each, after the host's resolver was made DNS-conforming
(public IPv4 resolvers; IPv6 unbound on the adapter; ledger 2026-09-23).

## Result
- **Word:** DIVERGED (comparison status `failing`). Go 479 = 434 pass / 1 fail / 44 skip; C# 479 = 431 pass / 4 fail / 44 skip;
  excluded 54; disclosed 2. Converter wall 332 s (Go's test run 53.8 s, the C# host 118.5 s). The tail's last event is net's
  own TestMain `os.Exit(1)`, recorded by the host -- not a crash, not a deadline.
- **The divergence set** equals the i9 recon at 0dc65a8e8d: `TestAllocs` (pass/fail, disclosed), `TestTCPReadWriteAllocs`
  (pass/fail, disclosed), `TestIPAppendTextNoAllocs` (pass/fail, NOT disclosed -- the one verdict that diverges).
- **DNS:** all 23 `TestLookupNoSuchHost` verdicts the i9 read fail/fail now read **pass/pass**; every `TestNSLookup*`
  (CNAME/MX/NS/TXT, 20 names) pass/pass; `TestLookupLocalPTR` pass/pass (the i9's fail/fail was its Docker hosts entries);
  `TestLookupCNAME` fail/fail with the same text on both sides (the live record now CNAMEs to a CDN). 24 verdict pairs moved
  fail/fail -> pass/pass between the i9 recon and this reading; nothing else moved.

## The three allocation entries (the host records the unit only for a test's FIRST nonzero AllocsPerRun call)

| test | go / cs | unit (first nonzero call) | per run | C# failure text |
|:--|:--|:--|--:|:--|
| `TestAllocs` | pass / fail | COUNT: 35,000 objects (2,866,296 B) over 1,000 runs | 35 | WriteMsgUDPAddrPort/ReadMsgUDPAddrPort 35; WriteToUDPAddrPort/ReadFromUDPAddrPort 26; WriteTo/ReadFromUDP 25 (Go wants 1 on the third) |
| `TestTCPReadWriteAllocs` | pass / fail | COUNT: 2,000 objects (648,000 B) over 1,000 runs | 2 | "got 2; want 0" (t.Fatalf stops at the first call) |
| `TestIPAppendTextNoAllocs` | pass / fail | COUNT: 3,000 objects (152,000 B) over 1,000 runs | 3 | per address: the IPv4 cases 3 each; the IPv6 cases 71-147; the nil IP 1 (unit unrecorded) |

**The committed disclosure figures are STALE:** TestAllocs's reason cites about 71 per run (this reading: 35);
TestTCPReadWriteAllocs's cites about 35 per run (this reading: 2). No proof page was written (the converter publishes pages
only for a validated row); the raw results stay on the coordinator's host.
