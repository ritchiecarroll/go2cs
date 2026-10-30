# LiteralSiteBench (seat #52)

Does a hoisted string literal (`private static readonly @string helloˢ = "hello"u8;`) measure faster than a plain
one (`"hello"`, converted once through golib's `StringLiteralCache`) **in situ**? There is one Go-shaped kernel per
literal site class. The forms differ only at the literal site, and the literal's result sits on the loop's critical
path: it decides the next iteration's index.

This is hand-written C# against golib and the converted `strings` package. It is not a converted program and has no
`.go` file, so PerformanceRunner never picks it up. The pre-registered prediction is R's mailbox post `dc9d45fcfd`.

## Kernels (`LiteralSiteBench <kernel>`)

| kernel | class | site |
|:--|:--|:--|
| `c1` | C1 argument | `strings.HasPrefix(line, "//go:build")` |
| `c2` | C2 return | StringMatch's `kindName`, called from the loop (no literal in the loop itself) |
| `c3` | C3 map key | `counts["build"]++` |
| `c4` | C4 field store | `recs[idx & 7] = T{Name: "name", ...}` |
| `c5` | C5 local | `s := "line"`, then a use of `s` |
| `c6` | C6 any slot | `sink("hello")`, where `sink` takes `object` |
| `control` | control | `lines[idx] == "package main"u8`, identical in every form, so it must read flat |
| `c1i` | F4 (plain binary only) | C1 through a hand-written interceptor with an `@string` parameter (`LiteralSiteBench.Intercept.cs`) |
| `c4u8` | F1u8 (hoisted binary only) | C4 as `-hoist-literals` really emits a composite element: the literal inline as a u8 span |

Each run calls the kernel once to get the checksum (n = 1000), warms up for 600 ms, then times a 400 ms window in
chunks of 65,536 evaluations. It prints:
- `checksum:`, which must be equal for one kernel across every form;
- `evals:`;
- `ns_per_eval:`;
- `bytes_per_eval:`, from `GC.GetAllocatedBytesForCurrentThread` over the same window.

## Forms: one binary each

The form is the compile-time constant `LitForm`: `HOISTED`, or anything else for plain. The default is `PLAIN`, set
in `LiteralSiteBench.csproj`. Each form builds from its OWN copy of this folder, so every binary has its own `obj`,
`bin` and `Generated`.

| form | LitForm | golib | built from |
|:--|:--|:--|:--|
| F1 | HOISTED | master | copy `LSB-F1` |
| F5 | HOISTED | master | copy `LSB-F5`: F1's A/A twin, a SECOND copy built separately, never a file copy of F1's binary |
| F2 | PLAIN | master | copy `LSB-F2`; also runs F4 (`c1i`) |
| F3 | PLAIN | `d6-reconstructed.patch` applied | copy `LSB-F3`, in a SEPARATE checkout whose golib carries the patch |

`d6-reconstructed.patch` is "D6 reconstructed", rebuilt from its description because the original probe source no
longer exists. It changes `StringLiteralCache.cs` as follows:
- `Get` first reads a `static readonly`, direct-mapped, 4,096-slot primary table, with the same Fibonacci hash.
  Tier 1 can fold its address.
- A primary miss falls to master's `Slow`, so master's growable table serves as the overflow.
- `Insert` claims the empty primary slot under the lock, and overflows when that slot is taken.

It applies with `git apply` and round-trips byte for byte. It is never committed to golib.

## Build (bash; Windows, .NET 10 SDK; the AOT link needs the VS Installer directory on PATH)

```bash
export MSBUILDDISABLENODEREUSE=1
export PATH="$PATH:/c/Program Files (x86)/Microsoft Visual Studio/Installer"
REPO=/c/path/to/go2cs          # a checkout of this branch
D6=/c/path/to/go2cs-d6         # a second checkout (or worktree) at the same commit
git -C "$D6" apply "$REPO/src/tests/Performance/LiteralSiteBench/d6-reconstructed.patch"
SRC=$REPO/src/tests/Performance/LiteralSiteBench
for spec in F1:HOISTED:$REPO F5:HOISTED:$REPO F2:PLAIN:$REPO F3:PLAIN:$D6; do
  IFS=: read -r F FORM TREE <<< "$spec"
  D=$TREE/src/tests/Performance/LSB-$F
  rm -rf "$D" && mkdir -p "$D" && cp "$SRC"/*.cs "$SRC"/*.csproj "$SRC"/go2cs.ico "$D"/
  sed -i "s#''\">PLAIN</LitForm>#''\">$FORM</LitForm>#" "$D/LiteralSiteBench.csproj"
  G2="$(cygpath -w "$TREE/src")\\"
  (cd "$D" && dotnet build -c Release -nologo -clp:ErrorsOnly "-p:go2csPath=$G2")
  (cd "$D" && dotnet publish LiteralSiteBench.csproj -nologo -clp:ErrorsOnly -c Release -p:PerfAot=true \
     "-p:go2csPath=$G2" -o "$(cygpath -w "$D/bin/Release/aot")")
done
```

The JIT binary is `LSB-<F>/bin/Release/net10.0/LiteralSiteBench.exe`, and the AOT binary is
`LSB-<F>/bin/Release/aot/LiteralSiteBench.exe`. `PerfAot=true` is the perf folder's AOT gate
(`Directory.Build.targets`), as PerformanceRunner publishes. On R-LAPTOP one form's publish took 1,609 s while two
other AOT publishes ran beside it; alone, the first design's publish took 87 s.

Before you trust F4: `c1i` must be intercepted. The interceptor uses the file-path `InterceptsLocation` form, which
the compiler flags as CS9270 but still applies. A copy whose interceptor returns `!HasPrefix(...)` must print a
different `c1i` checksum from `c1`. On R-LAPTOP it printed 1672 against 600.

## Run (per runtime: 15 interleaved rounds, the arm order reversed each round, one warm-up round discarded)

The arms of each class:
- `c1`: F1, F5, F2, F3, and F4 (F2's binary, kernel `c1i`).
- `c4`: F1, F5, F2, F3, and F1u8 (F1's binary, kernel `c4u8`).
- every other class: F1, F5, F2, F3.

Run one class at a time. Within a round, run every arm of the class back to back, and reverse that order every
other round.

The readout, per (class, runtime):
- the median of the per-round ratios F2/F1 and F3/F1, each read against the A/A band F5/F1 (its range over the
  rounds);
- F4/F2 for C1.

A ratio whose range is disjoint from the A/A range is outside the noise. Every rc must be 0, and every checksum must
equal the class's first.

`run-literal-classes.py` is R's driver, and it does exactly this:

```bash
python -I -X utf8 "$SRC/run-literal-classes.py" "$(cygpath -w "$REPO")" "$(cygpath -w "$D6")" 15 literal-classes.tsv jit,aot
```

It writes every measurement to the `.tsv` and prints one readout line per (runtime, class). A run that exits
non-zero or prints a different checksum is reported as `BAD`, and its class reads `MISMATCH`.
