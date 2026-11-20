# DATA: what go2cs-gen's method-set arms cost under Native AOT (the MethodSet scout)

Lane G, 2026-11-15. COORD routed the question from i9's B4 record ([`DATA-ts-aot-b4.md`](DATA-ts-aot-b4.md),
section 8). go2cs-gen's `MethodSet.cs` writes `IGoMethodSet.ᴛMethod` on every value type with exported methods.
Its bound arms capture a case-local receiver, so C# mints one closure class per arm (57,542 in the converted
`tsc/internal/ast`). The questions: are the arms why full trim stops trimming, and does a non-capturing form pay?
This is a record. No code landed, and COORD accepted the recommendation not to seat a change on these numbers.

## 1. Subject and method

- **Program:** `src/tests/Performance/PerfStartup` (one `fmt` round trip), at master `b2214e00e4` (batch 308,
  lever G in).
- **Build:** a Native AOT publish, `-p:PerfAot=true -p:TrimMode=full`, one ILC at a time on G's Windows x64
  laptop. Every image prints Go's output.
- **Arms:** each is an edit to the generator only, `src/gen/go2cs-gen/MethodSet.cs`, applied in a scratch tree
  and restored after.
  - A0 is master as it stands.
  - A1 turns the arms off: `MethodSet.Source` answers null ([`arm-a1.pl`](data/methodset-scout/arm-a1.pl)).
  - A2 hoists the receivers ([`arm-a2.pl`](data/methodset-scout/arm-a2.pl)). The captured `ᴛr` and `ᴛp` are
    declared once at `ᴛMethod`'s top and assigned per case, so C# mints one display class per type instead of one
    per case.
- **Reading the object file:** the symbol string table is read from the `.obj`'s COFF header and table directly
  ([`coffstr.py`](data/methodset-scout/coffstr.py)). ILC's memory is sampled every 5 s
  ([`sample-ilc.ps1`](data/methodset-scout/sample-ilc.ps1)). Both run under
  [`aot-arm.sh`](data/methodset-scout/aot-arm.sh).
- **Reading the IL:** IL types are counted from the metadata of the 64 converted assemblies PerfStartup loads
  ([`typecount.ps1`](data/methodset-scout/typecount.ps1), via [`jit-build.sh`](data/methodset-scout/jit-build.sh)).
- **JIT start-up:** 40 interleaved pairs, run twice, with an A/A control
  ([`startup_pair.py`](data/methodset-scout/startup_pair.py)).
- **Placeholders** in the scripts: `<tree>` is the scratch go2cs tree, `<scratch>` its output directory and
  `<dotnet10>` the .NET 10 root.

## 2. Readings

| arm | string table | names | image | `DisplayClass` names | `IGoMethodSet` names |
|:--|--:|--:|--:|--:|--:|
| A0, master | 203.45 MB | 1,328,556 | 107.85 MB | 41,347 | 5,402 |
| A1, arms off | 200.78 MB (−1.3%) | 1,308,027 | 105.98 MB (−1.7%) | 33,355 | 65 |
| A2, receivers hoisted | 203.16 MB (−0.14%) | 1,324,972 | 107.53 MB (−0.3%) | 37,763 | 5,402 |

- **ILC peak private memory:** 7.58 GB for A1 and 8.06 GB for A2. A0's sampler missed its process, so A0 has no
  reading. Each publish took about 17 minutes.
- **IL, A0 to A2:** types 7,083 to 5,373; display classes 2,171 to 461; methods 55,268 to 53,558; lambda methods
  3,197 in both; bytes 28.70 MB to 28.55 MB.
- **JIT start-up, A2 minus A0:** +2.4 ms and +1.6 ms on about 488 ms. The A/A control read −2.5 ms and +0.7 ms, so
  there is no measurable difference.

## 3. What the readings say

1. **The arms are not why full trim stops trimming on this program.** All of them together are 1.3% of the names.
   They may still matter on `tsc/internal/ast`, whose 57,542 closure classes are the outlier.
2. **Under AOT the cost is the lambda methods, not the closure types.** Each lambda calls its target and so keeps
   it. Hoisting removes 79% of the display classes, but every lambda body stays, so the string table moves 0.14%.
3. **Hoisting changes no semantics.** Each `ᴛMethod` call still gets its own closure, and a bound delegate holds
   that call's receiver. The one behavioral difference is cost: one allocation per `ᴛMethod` call, unbound
   requests included. Today an unbound request allocates nothing extra.
4. **The non-capturing generic binder was not built.** That is a golib `Bind<TRecv,…>` taking the receiver as an
   argument. Under AOT each value-type instantiation gets its own code and a longer generic name, and the name
   length is the metric i9's record turns on. The prediction, not measured, is that it does no better than A2 on
   names.

## 4. Recommendation (accepted)

No seat from PerfStartup's numbers. If the TypeScript string table still needs margin after lever E, measure A1 and
A2 on `tsc/internal/ast`, where the 57,542 closure classes would become about 243. That measure is conditional,
and i9's: COORD asks for it only when that reading exists.
