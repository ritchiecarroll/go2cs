# DATA — per-row sweep wall times (the hop shard map's input)

> Point-in-time measurement DATA, labeled by (OS, corpus SHA, machine). This is the number the
> hop-era shard map (`docs/PLAN-hop-campaign.md`, H5) is parameterized by: the SWEEP's per-row
> wall clock (convert + build + both test hosts + compare), NOT the Go-side `go test -json`
> Time fields, which invert exactly the rows that dominate a shard. Native `[NNNs]` per-row
> timing exists in `run-validated-sweep.ps1` since `4e91a03e2`; these first tables predate it
> and were derived as described per section. Add a section per new (OS, SHA, machine)
> measurement at each hop recon; do not overwrite old sections — supersede them.

## windows · corpus `18770d083` · i9-13900K (ritchie-desk2) · 2026-08-23 (JOB-007)

162 rows, all PASS, 18,569 verdicts, aggregate 7,697 s. Derivation: artifact-mtime deltas in
roster order (run-start from the log file's creation time); self-check: the 162 deltas sum to
7,701 s vs the sweep's own 7,697 s. The two Go-vs-C# inversions to respect when packing shards:
`crypto/dsa` 1,317 s for 4 verdicts (the largest row) and `hash/maphash` 898 s for 22.

```
archive/tar                              97           60s
archive/zip                              100         354s
bufio                                    80            9s
bytes                                    82           18s
cmp                                      4             7s
compress/bzip2                           4             9s
compress/flate                           64          106s
compress/gzip                            15           21s
compress/lzw                             17            9s
compress/zlib                            6            18s
container/heap                           7             7s
container/list                           10            8s
container/ring                           8             7s
context                                  57           13s
crypto                                   6            15s
crypto/aes                               13            9s
crypto/des                               18            8s
crypto/dsa                               4          1317s
crypto/ecdh                              47           15s
crypto/ecdsa                             82           32s
crypto/ed25519                           8            15s
crypto/elliptic                          82           16s
crypto/hmac                              172           9s
crypto/internal/alias                    1             7s
crypto/internal/bigmod                   14           12s
crypto/internal/boring                   3             8s
crypto/internal/edwards25519/field       16           66s
crypto/internal/hpke                     19           10s
crypto/internal/mlkem768                 12          228s
crypto/md5                               11            8s
crypto/rand                              298          13s
crypto/rc4                               2             9s
crypto/rsa                               559         119s
crypto/sha1                              12            8s
crypto/sha256                            23            9s
crypto/sha512                            36            9s
crypto/subtle                            7            16s
crypto/tls                               400         659s
database/sql                             137          47s
database/sql/driver                      1             9s
debug/buildinfo                          197          18s
debug/dwarf                              40           11s
debug/elf                                31           12s
debug/gosym                              10           11s
debug/macho                              7            10s
debug/plan9obj                           2             9s
encoding/ascii85                         9             7s
encoding/asn1                            38           10s
encoding/base32                          26            8s
encoding/base64                          17            8s
encoding/binary                          137           9s
encoding/csv                             71           10s
encoding/hex                             12            9s
encoding/json                            491          28s
encoding/xml                             386          54s
encoding/pem                             8           105s
errors                                   61           10s
expvar                                   11           13s
flag                                     24           12s
fmt                                      63           10s
go/ast                                   9            11s
go/build/constraint                      89            9s
go/constant                              9             9s
go/doc/comment                           10059         18s
go/format                                4            10s
go/importer                              3            20s
go/internal/gccgoimporter                4            11s
go/internal/gcimporter                   583         306s
go/internal/srcimporter                  7            19s
go/parser                                173         259s
go/printer                               45           11s
go/scanner                               11           10s
go/token                                 31            9s
go/types                                 557         137s
go/version                               3             8s
hash                                     18           10s
hash/adler32                             2             7s
hash/crc32                               10            9s
hash/crc64                               5             8s
hash/fnv                                 19            8s
hash/maphash                             22          898s
html/template                            243          28s
image                                    8            12s
image/color                              10            8s
image/draw                               9            10s
image/gif                                28           14s
image/jpeg                               14           11s
image/png                                28           14s
index/suffixarray                        12          573s
internal/abi                             2            10s
internal/buildcfg                        3             8s
internal/coverage/cformat                2             9s
internal/coverage/cmerge                 2             9s
internal/coverage/pods                   1             9s
internal/coverage/slicereader            1             9s
internal/coverage/slicewriter            1             8s
internal/cpu                             8             9s
internal/dag                             6             9s
internal/diff                            13            9s
internal/fmtsort                         3             8s
internal/fuzz                            52           10s
internal/godebugs                        1           177s
internal/gover                           5             7s
internal/itoa                            3             8s
internal/profile                         1            10s
internal/reflectlite                     30           10s
internal/saferio                         17            7s
internal/singleflight                    5             9s
internal/sysinfo                         1             8s
internal/testenv                         7             9s
internal/types/errors                    155          14s
internal/xcoff                           3             9s
internal/zstd                            536          10s
io                                       60            9s
io/fs                                    18           10s
io/ioutil                                28            9s
log                                      8             9s
log/slog/internal/benchmarks             3             9s
maps                                     14            8s
math                                     76            9s
math/bits                                26            7s
math/cmplx                               24            8s
math/rand                                43           31s
math/rand/v2                             36           34s
mime                                     17           10s
mime/multipart                           52           71s
mime/quotedprintable                     5            10s
net/http/fcgi                            12           14s
net/http/internal/ascii                  13            8s
net/mail                                 11           10s
net/rpc/jsonrpc                          9            14s
net/textproto                            26           10s
net/url                                  48           10s
os/exec                                  74           43s
os/exec/internal/fdtest                  1             8s
os/signal                                1            12s
path                                     9             8s
path/filepath                            61           13s
plugin                                   1             8s
regexp                                   45          226s
regexp/syntax                            12           20s
runtime/debug                            4            11s
runtime/internal/math                    1             8s
runtime/internal/sys                     4             7s
runtime/metrics                          2            10s
sort                                     63           16s
strconv                                  55           16s
strings                                  68           56s
sync                                     44           18s
sync/atomic                              108           82s
syscall                                  62           14s
testing/iotest                           18            9s
testing/quick                            8             9s
testing/slogtest                         17           10s
text/scanner                             18            9s
text/tabwriter                           3             8s
text/template                            52           25s
text/template/parse                      52           10s
time                                     159         197s
unicode                                  28            8s
unicode/utf16                            8             9s
unicode/utf8                             14            6s
```

## linux · corpus `18770d083` · Ryzen 7 PRO 6850U (R-LAPTOP, WSL2 Ubuntu 22.04) · 2026-08-23 (JOB-007 Linux leg)

162 rows, **149 PASS / 10 FAIL / 3 CVAC** (152 green), aggregate **19113 s (5.3 h)**. Derivation: NOT
mtime-derived — each row's wall clock is recorded directly by the per-row driver as it runs
(`pkg⇥verdict⇥seconds⇥rc⇥verdict-line`), so these are measured per row rather than differenced, and
the aggregate is their exact sum. Columns: package, verdict, matching-verdict count where the row
produced one, wall seconds. Rows ran one at a time on an otherwise idle box; the linux-native
`go2cs-stdlib.slnx` gate (465 s) ran before the first row and is NOT included in the aggregate.

**Shard-packing notes for the hop map, and they differ from the Windows leg's.** `crypto/dsa` is the
largest row here too but three times worse — **4,366 s for 4 verdicts** against Windows' 1,317 s —
and `hash/maphash` 1,994 s for 22 against 898 s. The Linux/Windows wall ratio is ~2.5x overall
(19,113 s vs 7,697 s) but is NOT uniform: it is dominated by a handful of compute-bound crypto and
hashing rows, while most rows sit within ~1.5x. Three FAIL rows are also expensive
(`sync/atomic` 1,258 s — the W7 ring, `time` 857 s, `os/exec` 740 s) and a shard planner should
treat them as full-cost, because a FAIL costs its whole runtime.

```
encoding/json                            PASS   491       78s
crypto/tls                               FAIL            711s
path/filepath                            PASS   54        19s
crypto/sha1                              CVAC   13        26s
bytes                                    CVAC   86        38s
time                                     FAIL            857s
os/exec                                  FAIL            740s
debug/buildinfo                          PASS   204       46s
debug/gosym                              FAIL             27s
mime                                     PASS   18        25s
go/internal/gcimporter                   CVAC   582      423s
crypto/rand                              PASS   302       33s
internal/cpu                             FAIL             26s
os/signal                                FAIL             36s
sync/atomic                              FAIL           1258s
syscall                                  FAIL             35s
plugin                                   FAIL              6s
archive/tar                              PASS   97        40s
archive/zip                              PASS   100      649s
bufio                                    PASS   80        22s
cmp                                      PASS   4         15s
compress/bzip2                           PASS   4         19s
compress/flate                           PASS   64       214s
compress/gzip                            PASS   15        42s
compress/lzw                             PASS   17        19s
compress/zlib                            PASS   6         34s
container/heap                           PASS   7         23s
container/list                           PASS   10        19s
container/ring                           PASS   8         20s
context                                  PASS   57        23s
crypto                                   PASS   6         41s
crypto/aes                               PASS   13        21s
crypto/des                               PASS   18        21s
crypto/dsa                               PASS   4       4366s
crypto/ecdh                              PASS   47        32s
crypto/ecdsa                             PASS   82        63s
crypto/ed25519                           PASS   8         29s
crypto/elliptic                          PASS   82        32s
crypto/hmac                              PASS   172       17s
crypto/internal/alias                    PASS   1         15s
crypto/internal/bigmod                   PASS   14        23s
crypto/internal/boring                   PASS   3         16s
crypto/internal/edwards25519/field       PASS   16       136s
crypto/internal/hpke                     PASS   19        19s
crypto/internal/mlkem768                 PASS   12       361s
crypto/md5                               PASS   11        18s
crypto/rc4                               PASS   2         18s
crypto/rsa                               PASS   559      383s
crypto/sha256                            PASS   23        17s
crypto/sha512                            PASS   36        18s
crypto/subtle                            PASS   7         36s
database/sql                             PASS   137       72s
database/sql/driver                      PASS   1         20s
debug/dwarf                              PASS   40        24s
debug/elf                                PASS   31        32s
debug/macho                              PASS   7         27s
debug/plan9obj                           PASS   2         24s
encoding/ascii85                         PASS   9         21s
encoding/asn1                            PASS   38        26s
encoding/base32                          PASS   26        23s
encoding/base64                          PASS   17        20s
encoding/binary                          PASS   137       18s
encoding/csv                             PASS   71        19s
encoding/hex                             PASS   12        18s
encoding/xml                             PASS   386      116s
encoding/pem                             PASS   8        177s
errors                                   PASS   61        18s
expvar                                   PASS   11        26s
flag                                     PASS   24        23s
fmt                                      PASS   63        21s
go/ast                                   PASS   9         22s
go/build/constraint                      PASS   89        17s
go/constant                              PASS   9         18s
go/doc/comment                           PASS   10059     29s
go/format                                PASS   4         20s
go/importer                              PASS   3         26s
go/internal/gccgoimporter                PASS   4         22s
go/internal/srcimporter                  PASS   7         55s
go/parser                                PASS   173      777s
go/printer                               PASS   45        24s
go/scanner                               PASS   11        19s
go/token                                 PASS   31        24s
go/types                                 PASS   557      181s
go/version                               PASS   3         17s
hash                                     PASS   18        22s
hash/adler32                             PASS   2         17s
hash/crc32                               PASS   10        20s
hash/crc64                               PASS   5         16s
hash/fnv                                 PASS   19        18s
hash/maphash                             PASS   22      1994s
html/template                            PASS   243       66s
image                                    PASS   8         24s
image/color                              PASS   10        19s
image/draw                               PASS   9         21s
image/gif                                PASS   28        28s
image/jpeg                               PASS   14        24s
image/png                                PASS   28        28s
index/suffixarray                        PASS   12      1056s
internal/abi                             PASS   2         23s
internal/buildcfg                        PASS   3         19s
internal/coverage/cformat                PASS   2         19s
internal/coverage/cmerge                 PASS   2         16s
internal/coverage/pods                   PASS   1         17s
internal/coverage/slicereader            PASS   1         17s
internal/coverage/slicewriter            PASS   1         17s
internal/dag                             PASS   6         17s
internal/diff                            PASS   13        17s
internal/fmtsort                         PASS   3         17s
internal/fuzz                            PASS   52        21s
internal/godebugs                        PASS   1        350s
internal/gover                           PASS   5         14s
internal/itoa                            PASS   3         17s
internal/profile                         PASS   1         19s
internal/reflectlite                     PASS   30        20s
internal/saferio                         PASS   17        16s
internal/singleflight                    PASS   5         19s
internal/sysinfo                         PASS   1         17s
internal/testenv                         PASS   7         19s
internal/types/errors                    PASS   155       45s
internal/xcoff                           PASS   3         19s
internal/zstd                            PASS   536       20s
io                                       PASS   60        19s
io/fs                                    PASS   18        21s
io/ioutil                                PASS   28        18s
log                                      PASS   8         18s
log/slog/internal/benchmarks             PASS   3         21s
maps                                     PASS   14        19s
math                                     PASS   76        22s
math/bits                                PASS   26        19s
math/cmplx                               PASS   24        19s
math/rand                                PASS   43        78s
math/rand/v2                             PASS   36        78s
mime/multipart                           PASS   52       251s
mime/quotedprintable                     PASS   5         19s
net/http/fcgi                            PASS   12        29s
net/http/internal/ascii                  PASS   13        16s
net/mail                                 PASS   11        19s
net/rpc/jsonrpc                          PASS   9         29s
net/textproto                            PASS   26        19s
net/url                                  PASS   48        20s
os/exec/internal/fdtest                  PASS   1         16s
path                                     PASS   9         16s
regexp                                   PASS   45       465s
regexp/syntax                            PASS   12        40s
runtime/debug                            FAIL             32s
runtime/internal/math                    PASS   1         15s
runtime/internal/sys                     PASS   4         14s
runtime/metrics                          PASS   2         22s
sort                                     PASS   63        38s
strconv                                  PASS   55        33s
strings                                  PASS   68       120s
sync                                     PASS   44        38s
testing/iotest                           PASS   18        19s
testing/quick                            PASS   8         19s
testing/slogtest                         PASS   17        19s
text/scanner                             PASS   18        18s
text/tabwriter                           PASS   3         19s
text/template                            PASS   52        59s
text/template/parse                      PASS   52        19s
unicode                                  PASS   28        17s
unicode/utf16                            PASS   8         18s
unicode/utf8                             PASS   14        14s
```
