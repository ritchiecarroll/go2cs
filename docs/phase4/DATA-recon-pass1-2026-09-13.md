# DATA — the recon leg: the roster measured TWICE, in both dispatch modes

> **Scope.** Point-in-time measurement DATA for the hop shard map, labeled by (OS, corpus SHA, machine):
> **windows · corpus `a02ac3df3` · i9-13900K (i9) · 2026-09-13**. The file is named for pass 1 because
> that is the artifact COORD commissioned; it carries **both passes**, because the second one is what
> makes the first interpretable.
>
> **What was measured, and why twice.** The shard map's existing `t_r` values
> (`DATA-sweep-row-walltimes.md`) were taken inside a FULL-ROSTER SWEEP on an older corpus and toolchain,
> but the map **dispatches per row** (`-Filter <row> -Exact`). Those are different measurements, and on
> one calibration row they differed by 3.9×. So the roster was re-measured under the mode the map
> actually schedules (**pass 1**, isolated), and then again as one full-roster sweep on the SAME box, at
> the SAME tree, with the SAME converter binary (**pass 2**, in-sweep). Pass 2 is the one-axis arm: the
> dispatch mode is the only variable between the two columns below.
>
> **This record produces DATA and attributes nothing.** The older figures stay as provenance.

## The pin and the method

```
  tree            a02ac3df346db4dc0bcfcbe040f060a8290e01cd, clean, both passes
  corpus/oracle   GOROOT go1.23.12, asserted from $GOROOT/VERSION and `go env GOROOT`, CGO_ENABLED=0
  converter       built ONCE under go1.24.13 (GOTOOLCHAIN=local); pass 2 reused pass 1's binary
  population      read from docs/ValidatedTestPackages.md at run time, asserted 204 unique non-empty
  t_r             the sweep's OWN per-row [NNNs], never a shell wall (larger by process start-up)
  pass 1          204 rows, each -Filter <row> -Exact -SkipBuild, tree restored between rows
  pass 2          ONE full-roster -SkipBuild run, no filter
  controls        pass 1 ran compress/flate and archive/tar to spec BEFORE the 204, and would have
                  stopped had either been off (a control that dies has not controlled)
```

## Totals

```
                        rows   PASS  FAIL  COUNT   aggregate
  pass 1 (isolated)      204    201     3      1     6,867 s  (114.5 min)
  pass 2 (in-sweep)      204    199     4      1     9,011 s  (150.2 min)

  excluding net, which is not a measurement in either pass:
  pass 1                                              6,189 s
  pass 2                                              5,904 s
```

The two aggregates differ almost entirely because of `net`, whose figure is an artifact of when a human
stopped it — see **`net`** below. **Over the 199 rows that PASS in both passes the totals are 5,728 s
isolated against 5,455 s in-sweep.**

## ⚠ THE ONE-AXIS RESULT: the dispatch mode costs nothing for 95% of the roster

Same box, same tree, same converter binary; only the mode differs. Over the 199 rows PASS in both:

```
  |isolated - in-sweep| = 0 s : 72 rows   <= 1 s : 142   <= 5 s : 186   <= 10 s : 190 of 199
  distribution   min -16   p25 +0   MEDIAN +0   p75 +1   max +40   mean +1.37
  totals         isolated 5,728 s   in-sweep 5,455 s   difference 273 s
  concentration  top 5 rows carry 173 s = 63% of the difference; top 10 carry 217 s = 80%
                 72 identical rows carry 0 s; 33 rows are FASTER isolated, carrying -95 s
```

⚠ **The mean of +1.37 s/row describes almost none of the rows and must not be used as a correction
factor.** The median row costs the same in both modes. The difference is nine rows:

```
  internal/dag        +40      internal/godebugs   -16
  internal/buildcfg   +40      internal/abi        -13
  crypto/dsa          +39      internal/trace      -13
  internal/concurrent +37
  go/build            +17
  net/http/cgi        +16
  crypto/ecdsa        +10
```

**This run does not establish why those nine differ**, and nothing here should be read as if it did.

**The heavy rows are where a per-dispatch cost would hide, and it is not there:**

```
  time 359 -> 357     net/http 238 -> 240     regexp 189 -> 193
  go/internal/gcimporter 267 -> 263           hash/maphash 147 -> 142
```

**Consequences for the map.** A map that dispatches per row may use either column with **no mode
adjustment**. There is **no per-dispatch setup to amortise**: the ~10 s floor is intrinsic per-row work
(convert, build, host start) that a full-roster sweep pays row for row, so batching rows buys nothing.

## Cost is not work: correlation(verdict count, wall seconds) = 0.08

A property of pass 1 alone, needing no comparison:

```
  floor 10 s   p10 12 s   median 16 s   p90 52 s   max 359 s
  rows under 20 s : 131      20-60 s : 57      over 60 s : 13
  the ten cheapest rows carry 1-10 verdicts and cost 10-11 s
  go/doc/comment carries 10,059 verdicts and costs 17 s
```

**Ordering shards by `t_r` orders most of the roster by noise.** For the light bulk, balance ROW COUNT;
place the heavy rows first.

## ⚠ Against the older record: the offset is NOT uniform, so the ratio cannot cancel

Joined on name against `DATA-sweep-row-walltimes.md`'s windows/`18770d083` block — **159 rows in common**:

```
  ratio DATA / pass1 :  min 0.15x   p25 0.64x   MEDIAN 0.71x   p75 0.83x   max 21.24x
  rows where the RECORDED figure is FASTER than isolated : 128 of 159
  totals over the joined set : DATA 7,019 s vs pass1 4,265 s
  smallest : internal/buildcfg  DATA 8s -> pass1 52s   0.15x
  largest  : crypto/dsa         DATA 1317s -> pass1 62s  21.24x
```

**A 140-fold spread is not a uniform offset of any kind, so no single scalar reconciles the two and the
LPT ordering moves.** Load, corpus and toolchain all differ between those datasets — a three-axis
confound this record cannot separate. What pass 2 does establish is that **MODE is not one of the three**:
it was held as the only variable and produced no systematic difference. So the re-derivation needs `t_r`
re-measured at the campaign's own corpus, which is this record, and no mode correction.

⚠ One direction claim is **corrected here**: an earlier note reported `compress/flate` at 27 s against a
recorded 106 s and framed it as *isolated is ~3.9× faster*. That is true of that row and false as a
direction — for **128 of 159** rows the recorded figure is the smaller one. The older record
over-estimates a few heavy rows and under-estimates most light ones, which is worse for an ordering than
a uniform bias would be.

## The non-PASS rows

```
  crypto/tls       FAIL  400 -> 402 s, 0 verdicts   known-flaky class; a red here is NOT drift until the
                                                    Go side is read. Real cost, unreliable verdict.
  syscall          FAIL   20 ->  21 s, 0 verdicts   a REAL divergence, and the sweep says so itself:
                                                    TestGetStartupInfo Go=pass C#=fail -- an oracle
                                                    flake is Go=fail with C#=pass; anything else is
                                                    this corpus's own divergence.
  encoding/binary  COUNT 140 measured vs 137 banked, identical in both modes -- a count that MOVED UP by 3
  crypto/rsa       PASS isolated (559 verdicts, 27 s) -> FAIL in-sweep (14 s)   the only row whose
                                                    VERDICT differs by mode -- but the FAILURE is
                                                    neither mode-dependent nor crypto/rsa's. Below.
  net              FAIL  678 -> 3,107 s              NOT A MEASUREMENT. See below.
```

`encoding/binary` and `syscall` are findings for whoever owns the roster's health; neither is chased here
and neither blocks the recon. **Recorded, not attributed.**

### ⚠ `crypto/rsa` — the row whose VERDICT differs, and the cause is NOT what the row names

The row-level fact is that `crypto/rsa` PASSes isolated (559 verdicts, 27 s) and FAILs in-sweep (14 s).
**The causal reading first attached to it was wrong, and the sweep log corrects it.** Both diagnostics
name a DIFFERENT project:

```
  CSC : warning CS8785: Generator 'TypeGenerator' failed to generate source. It will not contribute
        to the output and compilation errors may occur as a result. Exception was of type
        'NullReferenceException' with message 'Object reference not set to an instance of an object.'.
        [ ...\src\core\crypto\x509\crypto.x509.csproj ]

  ...\src\core\crypto\x509\cert_pool.cs(251,42): error CS9248: Partial property
        'x509_package.AppendCertsFromPEM_lazyCert.Once' must have an implementation part.
        [ ...\src\core\crypto\x509\crypto.x509.csproj ]
```

`crypto/rsa` did not fail to build. A project in its build closure did, and the row is the messenger.

⚠ **And the same sweep contains the counter-example:**

```
  FAIL  crypto/rsa            [14s]    <- the generator throws on crypto.x509.csproj
  PASS  crypto/x509    341    [36s]    <- the SAME csproj, the SAME sweep, twelve rows later
```

**So this is not a mode-dependent FAILURE**, even though the row-level verdicts differ by mode: the same
project compiled cleanly later in the same run. In the whole 204-row sweep there are exactly TWO such
diagnostics, both from that one compilation.

CS9248 is downstream of CS8785 — the generator emits the implementing half of the partial property, so a
throw makes every property it owed report as unpaired. **One defect wearing two codes**, and there is
nothing to fix at CS9248.

The generator holds no static mutable state (read by G), so its output should be a function of its
compilation input; that the same file compiled both ways means the INPUTS differed — references, or the
state of the dependency graph at that moment. **No site is named here and none should be guessed at.**
The CS8785 text carries no throwing frame, only the generic message quoted above, so it does not settle
it; the discriminating experiment is to build `crypto.x509.csproj` in both contexts with the
generated-files dump on and diff the generated trees.

The row banked nothing because the harness refused to score a stale artifact: *"the comparison record
predates this attempt — a warm tree's record from an earlier run is not this run's evidence"*. **The 14 s
is the cost of failing early, not the cost of the row.** Recorded, not attributed.

### ⚠ `net` — UNMEASURED IN BOTH MODES, and both figures are a human's

**What is measured** (live capture, 617 s into the pass-2 row):

```
  go2cs PID 33592   CPU 4 s over 617 s of wall   owns no TCP connections
  its child: go.exe   go test -json -count=1 -timeout 40m0s .
  -> the CONVERTER was not hung. It was blocked waiting on its go-test child.
```

**What is NOT measured, and was asserted in a draft before being withdrawn:** the Go child's own CPU. It
was never sampled, so **whether the Go side stalled or was merely slow is unknown.** An earlier claim
that `net.test.exe` showed "0 CPU across 883 s" has no corresponding capture and is withdrawn.

**And the row was stopped by hand, twice.** After the 40-minute Go timeout a second capture was taken
whose own header had been written as *"children now (the go test child is gone)"* — while the same
capture's rows listed a live `net.tests.exe`, the **converted C# test host**. The converter had proceeded
to the C# side; the parent was killed anyway.

```
  net pass 1: FAIL   678 s  <- stopped by hand at ~10 min
  net pass 2: FAIL 3,107 s  <- stopped by hand at 3,105 s, with the C# side running
```

**Both numbers are lower bounds produced by a human decision, not costs of the row.** They are excluded
from every comparison in this record. **A shard planner must treat `net` as UNKNOWN and large.** What is
still owed is one isolated run, alone, with the grandchild's CPU sampled and nothing killed by hand —
the 40-minute `go test` timeout is the instrument, and a row that dies under it is a finding.

## ⚠ Instrument defects in the pass-1 runner — all three, and what each cost

**(a) The parser knew four of the sweep's seven row words.** It read `PASS|FAIL|HOP|HOPNONE`;
`run-validated-sweep.ps1` also emits **COUNT, CVAC, DISC, ORACLE, RERUN**. Any row printing one of the
five unknown words recorded `word=? verdicts=0` — wrong for exactly the rows worth reading, since a
COUNT row is a count that moved. Caught on `encoding/binary` at row ~90. The running script was **not**
edited and the run **not** restarted (bash reads a script incrementally; editing one mid-run corrupts
it); every per-row log was retained, so the fix was a **re-parse afterwards**, with the vocabulary read
from the sweep's own source rather than discovered one row at a time. That is why pass 1 has two TSVs and
why **the re-parsed one is the record**.

That defect also corrupted the run's own summary: a crude classifier counted `verdicts==0 && word!=PASS`
as "host deaths" and reported **8**. The true figure is **0** — four non-PASS rows plus four mis-parsed
ones. A summary computed from a broken parse is broken in the same way.

**(b) One log was written under an empty name**, through a shell assignment that did not expand as
intended. It holds the `archive/tar` control. Filename-against-content was then verified for all 205
logs: **204 matched, 0 mismatched.**

**(c) 206 runs left 205 logs.** `compress/flate` ran twice — as the control and again as a roster row —
and **both runs wrote the same filename**, so the control's log was overwritten. **Nothing was lost**: the
as-run TSV retained both readings of each duplicated row (flate 27/26 s, tar 17/16 s), and the join is on
the 204 unique names, whose symmetric difference between the two passes is **empty**. Reported because a
log count that does not equal a run count reads as fine until someone joins on it.

**Where a duplicate name exists, the table below takes the ROSTER run**, not the control run.

## Inputs on origin

```
  hopA-inputs/recon-pass1-20260913T123456Z-reparsed.tsv   pass 1, isolated
                                                          row word verdicts banked sweep_s exit last_test
  hopA-inputs/recon-pass2-20260913T144128Z.tsv            pass 2, in-sweep
                                                          row word verdicts banked sweep_s
```

**Use `sweep_s`. Never a shell wall.** Drop `net` from any fit.

## The 204 rows, both passes

`verdict` shows the word both passes agreed on, or `pass1/pass2` where they differed. `diff` is
**pass1 minus pass2**, i.e. isolated minus in-sweep: positive means the isolated run was slower.

```
package                                verdict verdicts   pass1_s   pass2_s    diff
-------------------------------------- ------- -------- --------- --------- -------
archive/tar                            PASS          97        16        17      -1
archive/zip                            PASS         100        45        47      -2
bufio                                  PASS          80        13        13      +0
bytes                                  PASS          82        16        16      +0
cmp                                    PASS           4        10        10      +0
compress/bzip2                         PASS           4        11        13      -2
compress/flate                         PASS          64        25        25      +0
compress/gzip                          PASS          15        17        17      +0
compress/lzw                           PASS          17        12        12      +0
compress/zlib                          PASS           6        14        13      +1
container/heap                         PASS           7        11        11      +0
container/list                         PASS          10        10        10      +0
container/ring                         PASS           8        10        10      +0
context                                PASS          57        24        23      +1
crypto                                 PASS           6        22        23      -1
crypto/aes                             PASS          13        13        12      +1
crypto/cipher                          PASS          13        14        14      +0
crypto/des                             PASS          18        12        11      +1
crypto/dsa                             PASS           4        62        23     +39
crypto/ecdh                            PASS          47        18        15      +3
crypto/ecdsa                           PASS          82        32        22     +10
crypto/ed25519                         PASS           8        17        16      +1
crypto/elliptic                        PASS          82        14        14      +0
crypto/hmac                            PASS         172        13        12      +1
crypto/internal/alias                  PASS           1        11        11      +0
crypto/internal/bigmod                 PASS          14        12        12      +0
crypto/internal/boring                 PASS           3        12        12      +0
crypto/internal/boring/bcache          PASS           1        18        18      +0
crypto/internal/edwards25519           PASS          54       109       107      +2
crypto/internal/edwards25519/field     PASS          16        24        24      +0
crypto/internal/hpke                   PASS          19        15        14      +1
crypto/internal/mlkem768               PASS          12        26        26      +0
crypto/internal/nistec                 PASS        2195        37        37      +0
crypto/md5                             PASS          11        12        12      +0
crypto/rand                            PASS         298        13        12      +1
crypto/rc4                             PASS           2        11        11      +0
crypto/rsa                             PASS/FAIL      559        27        14     +13
crypto/sha1                            PASS          12        12        12      +0
crypto/sha256                          PASS          23        14        12      +2
crypto/sha512                          PASS          36        12        12      +0
crypto/subtle                          PASS           7        29        29      +0
crypto/tls                             FAIL                   400       402      -2
crypto/x509                            PASS         341        38        36      +2
database/sql                           PASS         138        50        47      +3
database/sql/driver                    PASS           1        12        12      +0
debug/buildinfo                        PASS         197        22        22      +0
debug/dwarf                            PASS          40        19        18      +1
debug/elf                              PASS          31        19        16      +3
debug/gosym                            PASS          10        15        14      +1
debug/macho                            PASS           7        13        13      +0
debug/pe                               PASS          10        19        20      -1
debug/plan9obj                         PASS           2        11        11      +0
encoding/ascii85                       PASS           9        11        11      +0
encoding/asn1                          PASS          38        14        12      +2
encoding/base32                        PASS          26        12        12      +0
encoding/base64                        PASS          17        12        12      +0
encoding/binary                        COUNT        140        14        12      +2
encoding/csv                           PASS          71        14        12      +2
encoding/gob                           PASS         106        29        29      +0
encoding/hex                           PASS          12        12        11      +1
encoding/json                          PASS         491        56        56      +0
encoding/xml                           PASS         386        32        31      +1
encoding/pem                           PASS           8        15        13      +2
errors                                 PASS          61        17        17      +0
expvar                                 PASS          11        53        53      +0
flag                                   PASS          24        17        17      +0
fmt                                    PASS          63        23        22      +1
go/ast                                 PASS           9        18        17      +1
go/build                               PASS          57        39        22     +17
go/build/constraint                    PASS          89        12        12      +0
go/constant                            PASS           9        14        13      +1
go/doc                                 PASS          85        18        18      +0
go/doc/comment                         PASS       10059        16        16      +0
go/format                              PASS           4        14        14      +0
go/importer                            PASS           3        33        35      -2
go/internal/gccgoimporter              PASS           4        16        15      +1
go/internal/gcimporter                 PASS         583       267       263      +4
go/internal/srcimporter                PASS           7        22        22      +0
go/parser                              PASS         173       130       134      -4
go/printer                             PASS          45        15        15      +0
go/scanner                             PASS          11        14        13      +1
go/token                               PASS          31        14        13      +1
go/types                               PASS         557       109       108      +1
go/version                             PASS           3        11        11      +0
hash                                   PASS          18        12        12      +0
hash/adler32                           PASS           2        12        11      +1
hash/crc32                             PASS          10        13        11      +2
hash/crc64                             PASS           5        11        12      -1
hash/fnv                               PASS          19        13        12      +1
hash/maphash                           PASS          22       147       142      +5
html                                   PASS           3        12        12      +0
html/template                          PASS         243        21        22      -1
image                                  PASS           8        16        15      +1
image/color                            PASS          10        11        12      -1
image/draw                             PASS           9        15        14      +1
image/gif                              PASS          28        17        17      +0
image/jpeg                             PASS          14        15        14      +1
image/png                              PASS          28        16        15      +1
index/suffixarray                      PASS          12        93        95      -2
internal/abi                           PASS           1        56        69     -13
internal/buildcfg                      PASS           3        52        12     +40
internal/chacha8rand                   PASS           4        54        58      -4
internal/concurrent                    PASS          20        49        12     +37
internal/coverage/cfile                PASS          15        28        27      +1
internal/coverage/cformat              PASS           2        13        13      +0
internal/coverage/cmerge               PASS           2        12        11      +1
internal/coverage/pods                 PASS           1        13        12      +1
internal/coverage/slicereader          PASS           1        12        12      +0
internal/coverage/slicewriter          PASS           1        12        12      +0
internal/cpu                           PASS           8        53        53      +0
internal/dag                           PASS           6        52        12     +40
internal/diff                          PASS          13        13        12      +1
internal/fmtsort                       PASS           3        13        12      +1
internal/fuzz                          PASS          52        21        19      +2
internal/godebug                       PASS           5        21        22      -1
internal/godebugs                      PASS           1        38        54     -16
internal/gover                         PASS           5        11        11      +0
internal/itoa                          PASS           3        11        10      +1
internal/platform                      PASS           1        17        17      +0
internal/poll                          PASS          19        27        26      +1
internal/profile                       PASS           1        17        14      +3
internal/reflectlite                   PASS          30        24        26      -2
internal/runtime/atomic                PASS          15        22        17      +5
internal/saferio                       PASS          17        12        11      +1
internal/singleflight                  PASS           5        12        12      +0
internal/syscall/windows               PASS           2        15        15      +0
internal/syscall/windows/registry      PASS           6        18        17      +1
internal/sysinfo                       PASS           1        15        11      +4
internal/testenv                       PASS           7        14        14      +0
internal/trace                         PASS          92       102       115     -13
internal/trace/internal/oldtrace       PASS           3        16        15      +1
internal/types/errors                  PASS         155        39        38      +1
internal/weak                          PASS           4        13        13      +0
internal/xcoff                         PASS           3        13        14      -1
internal/zstd                          PASS         536        13        14      -1
io                                     PASS          60        16        16      +0
io/fs                                  PASS          18        17        16      +1
io/ioutil                              PASS          28        14        13      +1
iter                                   PASS          28        16        17      -1
log                                    PASS           8        15        13      +2
log/slog                               PASS         194        51        52      -1
log/slog/internal/benchmarks           PASS           3        14        13      +1
log/slog/internal/buffer               PASS           1        12        12      +0
maps                                   PASS          14        11        11      +0
math                                   PASS          76        16        16      +0
math/big                               PASS         224        31        28      +3
math/bits                              PASS          26        15        15      +0
math/cmplx                             PASS          24        15        12      +3
math/rand                              PASS          43        19        19      +0
math/rand/v2                           PASS          36        23        24      -1
mime                                   PASS          17        14        14      +0
mime/multipart                         PASS          52        27        23      +4
mime/quotedprintable                   PASS           5        14        15      -1
net                                    FAIL                   678      3107   -2429
net/http                               PASS        1345       238       240      -2
net/http/cgi                           PASS          38        56        40     +16
net/http/cookiejar                     PASS          17        20        19      +1
net/http/fcgi                          PASS          12        20        19      +1
net/http/httptest                      PASS          55        21        20      +1
net/http/httptrace                     PASS           2        17        15      +2
net/http/httputil                      PASS          53        44        39      +5
net/http/internal                      PASS          14        14        14      +0
net/http/internal/ascii                PASS          13        11        12      -1
net/mail                               PASS          11        14        13      +1
net/netip                              PASS         210        26        23      +3
net/rpc                                PASS          15        55        48      +7
net/rpc/jsonrpc                        PASS           9        22        19      +3
net/smtp                               PASS          19        23        18      +5
net/textproto                          PASS          26        14        13      +1
net/url                                PASS          48        14        14      +0
os                                     PASS         683        49        55      -6
os/exec                                PASS         116       128       127      +1
os/exec/internal/fdtest                PASS           1        11        10      +1
os/signal                              PASS           1        17        16      +1
os/user                                PASS           5        44        44      +0
path                                   PASS           9        15        14      +1
path/filepath                          PASS          61        21        19      +2
plugin                                 PASS           1        11        11      +0
regexp                                 PASS          45       189       193      -4
regexp/syntax                          PASS          12        14        14      +0
runtime/debug                          PASS           4        14        14      +0
runtime/internal/math                  PASS           1        12        13      -1
runtime/internal/sys                   PASS           4        12        11      +1
runtime/metrics                        PASS           2        21        17      +4
slices                                 PASS         119        17        18      -1
sort                                   PASS          63        19        15      +4
strconv                                PASS          55        18        20      -2
strings                                PASS          68        25        22      +3
sync                                   PASS          47        28        31      -3
sync/atomic                            PASS         108        99        94      +5
syscall                                FAIL                    20        21      -1
testing                                PASS          37        28        24      +4
testing/fstest                         PASS           7        13        14      -1
testing/iotest                         PASS          18        12        12      +0
testing/quick                          PASS           8        13        13      +0
testing/slogtest                       PASS          17        14        14      +0
text/scanner                           PASS          18        12        12      +0
text/tabwriter                         PASS           3        12        11      +1
text/template                          PASS          52        23        22      +1
text/template/parse                    PASS          52        15        16      -1
time                                   PASS         169       359       357      +2
unicode                                PASS          28        16        12      +4
unicode/utf16                          PASS           8        19        18      +1
unicode/utf8                           PASS          14        17        11      +6
```
