# CENSUS: the C# samples on the published pages against their cited sources (2026-10-27)

Record of the audit COORD dispatched on 2026-10-27 (the owner's ask), at master `5931bad29b`. A record: it is not
rewritten; a later reading is a dated block below.

## Method

- **Pages:** every tracked `docs/*.md` the site publishes (333: not `docs/validation/<version>/`, not under an exclude
  prefix of `docs/_config.yml`), plus the root `README.md`.
- **Blocks:** every fence whose language is `csharp`, `cs` or `c#`. Its citation is the nearest preceding
  `<!-- source: path:lines -->` with only blank lines between. A citation names one or more files, each with lines and
  ranges (`a-b, c`); `and`, `;` and `, :` continue it.
- **Equality:** the block's runs between `…` lines (allowed elisions) appear in order, each contiguous, in the cited
  lines. Indentation is compared after a common leading strip, over the whole citation or per cited range, and one
  blank line may separate two cited ranges. A block that fails is then compared with blank lines ignored, then with
  comment-only lines ignored too, which names the kind of difference.
- **Uncited blocks** are searched for in every tracked `.cs` and `.cs.target` under `src/` (8,228 files):
  `verbatim` when one file holds all its lines in order (blank lines aside), `adapted` when one file holds at least
  half of its distinctive lines (12 characters or more), `written` otherwise.

## Counts

| | at `5931bad29b` | after this audit |
|:--|--:|--:|
| C# blocks on published pages | 835 | 835 |
| cited and checkable | 439 | 524 |
| ... matching | 415 | 524 |
| ... differing in blank lines only | 13 | 0 |
| ... differing in comment lines only | 1 | 0 |
| ... differing in code | 10 | 0 |
| cited but not checkable | 1 | 1 |
| uncited | 395 | 310 |
| ... verbatim emission (now cited) | 85 | 0 |
| ... adapted from emission | 125 | 125 |
| ... written for the page | 185 | 185 |

## Every mismatch at `5931bad29b`, and what was done

Fixed by re-copying from the source at `5931bad29b` (no source was edited), or, where the block is current emission
and the citation named the wrong lines, by citing the lines it shows. Line numbers are the fence's at `5931bad29b`.

| Block | Cited source | Difference | Done |
|:--|:--|:--|:--|
| `docs/ConversionStrategies-Reference/interfaces.md:517` | src/tests/Behavioral/HeapKeywordVar/main.cs.target:11-12, 42 | code | stale: the box is now `static readonly ... StaticBox` read through a cast `ValueSlot`; re-copied |
| `docs/ConversionStrategies-Reference/manual-conversions.md:2178` | src/tests/Behavioral/ReflectStructTagCopy/main.cs.target:16-24 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies-Reference/maps-and-channels.md:415` | src/tests/Behavioral/GenericDefinedMapChan/GenericDefinedMapChan.cs.target:11-51 | code | citation: names 11-51, the block is lines 11-12 and 49-51; cited exactly |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:665` | src/core/crypto/internal/fips140/sha256/sha256.cs:51-57, 210, 213-215 | code | citation: starts one line early and leaves out the closing brace; cited 52-58, 210, 213-215 |
| `docs/ConversionStrategies-Reference/source-generators.md:92` | src/core/io/package_info.cs:94-152 (an excerpt of the section) | code | citation: an excerpt named by its section; now lists its 11 lines |
| `docs/ConversionStrategies-Reference/struct-types.md:243` | src/core/path/filepath/linux/path.cs:315-316 | code | stale: the same StaticBox change; re-copied |
| `docs/ConversionStrategies-Reference/variable-initialization-order.md:213` | src/core/unicode/tables.cs:10-23 | code | stale: line 10 now declares `ᏑCategories`, a StaticBox; re-copied |
| `docs/ConversionStrategies.md:1469` | src/core/path/path_test.cs:4-277 | code | stale: `using Δpath = path_package;` now precedes `using static path_package;`; re-copied |
| `docs/ConversionStrategies.md:1890` | src/tests/Behavioral/UntypedConstDefine/main.cs.target:7-25 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:2025` | src/core/compress/lzw/reader.cs:28-34 | comment lines | stale: the source now carries the Go comments; re-copied |
| `docs/ConversionStrategies.md:2246` | src/tests/Behavioral/NarrowArithmeticArg/main.cs.target:45-49 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:2400` | src/tests/Behavioral/GoShiftSemantics/main.cs.target:14-19 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:2520` | src/core/time/time.cs:938-1127 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:3598` | src/tests/Behavioral/ParallelAssignmentHazard/main.cs.target:28-44 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:5352` | src/tests/Behavioral/EmptyStructMapSet/EmptyStructMapSet.cs.target:56-64 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:5732` | src/tests/Behavioral/ConstrainedSliceParamInPlace/main.cs.target:39-50 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:6634` | src/tests/Behavioral/VariadicFuncValues/main.cs.target:21-27 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:7077` | src/tests/Behavioral/RangeStatements/RangeStatements.cs.target:8-21 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:8468` | src/tests/Behavioral/GoStmtValueReturn/main.cs.target:10-14, 26-28, 30-34, 48-49, 60 | code | stale: two `goǃ` calls now pass `GoStack.Bounded`; re-copied |
| `docs/ConversionStrategies.md:8558` | src/tests/Behavioral/GoroutinePanicExitCode/main.cs.target:8-18 | code | stale: the `goǃ` call now passes `GoStack.Bounded`; re-copied |
| `docs/ConversionStrategies.md:8692` | src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:94-100 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:8774` | src/tests/Behavioral/SelectStatement/SelectStatement.cs.target:189-191 | code | stale: the `goǃ` call now passes `GoStack.Bounded`, and a blank line; re-copied |
| `docs/ConversionStrategies.md:9384` | src/tests/Behavioral/StructPromotion/StructPromotion.cs.target:8-81 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:10273` | src/tests/Behavioral/DescriptorCarrierFieldName/main.cs.target:1-31 | blank lines | re-copied (the doc dropped or added blank lines) |
| `docs/ConversionStrategies.md:11710` | the NoInliningPartialGenerator output for src/tests/Behavioral/NoInlinePartial (main.noinline.g.cs), abridged | not checkable | LISTED for a ruling: generator output is not committed, so nothing can check it |

The 85 verbatim uncited blocks took a source comment. 79 matched as cited; 3 took a blank-line re-copy; and 3 took
an indentation re-copy, restoring the source indentation the doc had flattened (`pointers.md`, `strings.md`,
`value-receiver-delegates.md`).

## The 310 uncited blocks after this audit (for a ruling)

The doc rules ask for real, complete samples with a source. None of these is a committed file's lines in order:
`adapted` blocks are emission with lines changed or annotated for the page, `written` blocks are not copied from any
committed C#. Line numbers are the fence's after this audit. "Best file" is the file holding most of the block's
distinctive lines, with how many.

| Block | Class | Best file (lines found / distinctive) |
|:--|:--|:--|
| `docs/BeforeAndAfterTheFaceLift.md:33` | written | none (0/1) |
| `docs/BeforeAndAfterTheFaceLift.md:64` | adapted | `src/core/container/list/list.cs` (2/3) |
| `docs/BeforeAndAfterTheFaceLift.md:98` | written | none (0/1) |
| `docs/BeforeAndAfterTheFaceLift.md:126` | written | none (0/1) |
| `docs/BeforeAndAfterTheFaceLift.md:158` | adapted | `src/core/sort/sort.cs` (2/4) |
| `docs/BeforeAndAfterTheFaceLift.md:198` | written | `src/core/internal/reflectlite/value.cs` (1/7) |
| `docs/BeforeAndAfterTheFaceLift.md:237` | written | none (0/1) |
| `docs/BeforeAndAfterTheFaceLift.md:265` | written | none (0/1) |
| `docs/BeforeAndAfterTheFaceLift.md:293` | adapted | `src/core/time/tzdata/tzdata.cs` (2/3) |
| `docs/ConsumingGoFromCSharp.md:27` | adapted | `src/tests/CSharpConsumer/Program.cs` (4/4) |
| `docs/ConsumingGoFromCSharp.md:45` | adapted | `src/tests/CSharpConsumer/Program.cs` (4/4) |
| `docs/ConsumingGoFromCSharp.md:193` | adapted | `src/tests/CSharpConsumer/Program.cs` (2/3) |
| `docs/ConsumingGoFromCSharp.md:214` | adapted | `src/tests/CSharpConsumer/Program.cs` (5/5) |
| `docs/ConsumingGoFromCSharp.md:229` | adapted | `src/tests/CSharpConsumer/Program.cs` (2/3) |
| `docs/ConsumingGoFromCSharp.md:242` | adapted | `src/tests/CSharpConsumer/Program.cs` (4/4) |
| `docs/ConsumingGoFromCSharp.md:281` | adapted | `src/tests/CSharpConsumer/Program.cs` (1/2) |
| `docs/ConsumingGoFromCSharp.md:309` | adapted | `src/tests/CSharpConsumer/Program.cs` (2/4) |
| `docs/ConsumingGoFromCSharp.md:344` | adapted | `src/tests/CSharpConsumer/Program.cs` (3/4) |
| `docs/ConsumingGoFromCSharp.md:361` | adapted | `src/tests/CSharpConsumer/Program.cs` (2/3) |
| `docs/ConsumingGoFromCSharp.md:384` | adapted | `src/tests/CSharpConsumer/Program.cs` (1/2) |
| `docs/ConsumingGoFromCSharp.md:398` | adapted | `src/tests/CSharpConsumer/Program.cs` (6/6) |
| `docs/ConsumingGoFromCSharp.md:412` | adapted | `src/tests/CSharpConsumer/Program.cs` (2/3) |
| `docs/ConsumingGoFromCSharp.md:422` | adapted | `src/tests/CSharpConsumer/Program.cs` (5/5) |
| `docs/ConsumingGoFromCSharp.md:443` | adapted | `src/tests/CSharpConsumer/Program.cs` (6/6) |
| `docs/ConsumingGoFromCSharp.md:466` | adapted | `src/tests/CSharpConsumer/Program.cs` (3/4) |
| `docs/ConsumingGoFromCSharp.md:485` | adapted | `src/tests/CSharpConsumer/Program.cs` (5/6) |
| `docs/ConsumingGoFromCSharp.md:499` | adapted | `src/tests/CSharpConsumer/Program.cs` (2/3) |
| `docs/ConsumingGoFromCSharp.md:527` | written | `src/tests/CSharpConsumer/Program.cs` (1/3) |
| `docs/ConsumingGoFromCSharp.md:537` | adapted | `src/tests/CSharpConsumer/Program.cs` (3/4) |
| `docs/ConsumingGoFromCSharp.md:559` | adapted | `src/tests/CSharpConsumer/Program.cs` (5/6) |
| `docs/ConsumingGoFromCSharp.md:578` | adapted | `src/tests/CSharpConsumer/Program.cs` (1/2) |
| `docs/ConsumingGoFromCSharp.md:591` | adapted | `src/tests/CSharpConsumer/Program.cs` (4/5) |
| `docs/ConsumingGoFromCSharp.md:602` | adapted | `src/tests/CSharpConsumer/Program.cs` (1/2) |
| `docs/ConversionStrategies-Reference/constants.md:6` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/constants.md:12` | adapted | `src/tests/Behavioral/FirstClassFunctions/FirstClassFunctions.cs` (1/2) |
| `docs/ConversionStrategies-Reference/constants.md:45` | written | `src/core/compress/flate/inflate.cs` (1/3) |
| `docs/ConversionStrategies-Reference/constants.md:104` | written | `src/tests/Behavioral/UntypedConstDefine/main.cs` (1/3) |
| `docs/ConversionStrategies-Reference/constants.md:126` | adapted | `src/core/math/cbrt.cs` (4/4) |
| `docs/ConversionStrategies-Reference/constants.md:196` | adapted | `src/tests/Behavioral/UntypedFloatDefault/main.cs.target` (2/2) |
| `docs/ConversionStrategies-Reference/constants.md:302` | adapted | `src/tests/Behavioral/UntypedConstDefine/main.cs` (1/2) |
| `docs/ConversionStrategies-Reference/constants.md:405` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/constants.md:434` | adapted | `src/tests/Behavioral/UntypedIntExactConstArithmetic/UntypedIntExactConstArithmetic.cs` (2/2) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:143` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:165` | adapted | `src/tests/Behavioral/DeferFinallyLowering/main.cs.target` (9/10) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:270` | adapted | `src/core/html/template/examplefiles_test.cs` (5/6) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:351` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:384` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:409` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:469` | adapted | `src/core/golib/GoFrame.cs` (4/7) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:799` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1020` | adapted | `src/core/context/x_test.cs` (9/9) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1069` | written | `src/core/time/sleep_test.cs` (1/5) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1095` | adapted | `src/core/regexp/syntax/parse.cs` (5/9) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1129` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1144` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1180` | written | none (0/5) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1204` | adapted | `src/core/log/slog/handler.cs` (2/4) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1211` | adapted | `src/core/log/slog/handler.cs` (2/3) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1219` | adapted | `src/core/crypto/x509/cert_pool.cs` (4/4) |
| `docs/ConversionStrategies-Reference/defer-panic-recover.md:1229` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/empty-interface.md:51` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/empty-interface.md:228` | written | `src/tests/Behavioral/UntypedIntInterfaceBox/main.cs` (2/12) |
| `docs/ConversionStrategies-Reference/expression-switch.md:33` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/expression-switch.md:57` | adapted | `src/core/net/http/socks_bundle.cs` (3/5) |
| `docs/ConversionStrategies-Reference/expression-switch.md:135` | written | `src/core/encoding/json/decode.cs` (2/5) |
| `docs/ConversionStrategies-Reference/expression-switch.md:219` | written | `src/core/runtime/mgclimit.cs` (1/4) |
| `docs/ConversionStrategies-Reference/expression-switch.md:272` | adapted | `src/tests/Behavioral/SwitchBreakContinueWrapper/main.cs` (11/13) |
| `docs/ConversionStrategies-Reference/floating-point-formatting.md:95` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:192` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:322` | adapted | `src/core/go/ast/walk.cs` (5/6) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:446` | written | none (0/8) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:618` | written | `src/core/net/http/client_test.cs` (1/3) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:705` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:843` | written | none (0/11) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:965` | written | none (0/6) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:1018` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/generic-constraints.md:1042` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/golib-namespace.md:55` | adapted | `src/tests/Behavioral/CrossPkgUser/main.cs` (2/3) |
| `docs/ConversionStrategies-Reference/golib-namespace.md:227` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/interfaces.md:56` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/interfaces.md:69` | written | `src/tests/GolibTests/ConstraintProxyReflectionTests.cs` (1/6) |
| `docs/ConversionStrategies-Reference/interfaces.md:107` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:116` | written | none (0/15) |
| `docs/ConversionStrategies-Reference/interfaces.md:149` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/interfaces.md:222` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:276` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:408` | written | none (0/5) |
| `docs/ConversionStrategies-Reference/interfaces.md:461` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/interfaces.md:497` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:538` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:561` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:576` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:598` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:626` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:646` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:691` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:793` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:858` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:901` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:926` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:1010` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/interfaces.md:1077` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/interfaces.md:1317` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:1566` | written | none (0/5) |
| `docs/ConversionStrategies-Reference/interfaces.md:1706` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/interfaces.md:1781` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/interfaces.md:1805` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/interfaces.md:1830` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:20` | written | none (0/8) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:51` | written | none (0/7) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:89` | adapted | `src/tests/Behavioral/RangeVarReassign/main.cs` (2/4) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:114` | written | `src/core/net/http/fcgi/fcgi_test.cs` (1/3) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:152` | adapted | `src/tests/Behavioral/ForLoopPerIterationVars/main.cs` (2/3) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:163` | adapted | `src/tests/Behavioral/ForLoopPerIterationVars/main.cs` (4/6) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:178` | adapted | `src/tests/Behavioral/ForLoopPerIterationVars/main.cs` (2/3) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:215` | written | `src/tests/Behavioral/ForLoopPerIterationVars/main.cs` (1/5) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:282` | adapted | `src/core/unique/clone.cs` (5/5) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:319` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/labels-and-loop-variables.md:437` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:270` | adapted | `src/core/internal/runtime/syscall/linux/linux/syscall_linux_impl.cs` (2/3) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:867` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:1149` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:1218` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:1538` | adapted | `src/core/crypto/x509/internal/macos/darwin/package_info.cs` (1/2) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:2230` | adapted | `src/core/database/sql/convert.cs` (2/4) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:2319` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:2348` | written | `src/core/text/template/exec_test.cs` (1/4) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:3056` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:3636` | adapted | `src/tests/GolibTests/ExecutionTracerParserTests.cs` (1/2) |
| `docs/ConversionStrategies-Reference/manual-conversions.md:4244` | adapted | `src/core/reflect/value_impl.cs` (9/10) |
| `docs/ConversionStrategies-Reference/maps-and-channels.md:10` | adapted | `src/core/runtime/map_test.cs` (1/2) |
| `docs/ConversionStrategies-Reference/maps-and-channels.md:78` | written | `src/core/runtime/map_test.cs` (1/3) |
| `docs/ConversionStrategies-Reference/maps-and-channels.md:121` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/maps-and-channels.md:154` | adapted | `src/tests/Behavioral/NilMapKey/main.cs` (5/5) |
| `docs/ConversionStrategies-Reference/maps-and-channels.md:247` | adapted | `src/core/go/types/initorder.cs` (1/2) |
| `docs/ConversionStrategies-Reference/maps-and-channels.md:343` | adapted | `src/tests/Behavioral/AnyStringLitChanSend/main.cs.target` (2/3) |
| `docs/ConversionStrategies-Reference/maps-and-channels.md:676` | adapted | `src/tests/Behavioral/SelectOperandOnceEval/main.cs` (3/3) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:22` | written | `src/core/container/heap/heap_test.cs` (1/3) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:65` | written | `src/core/runtime/linux/mheap.cs` (1/3) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:98` | adapted | `src/tests/Behavioral/MultiValueFuncLiteralDefine/MultiValueFuncLiteralDefine.cs` (4/4) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:166` | written | `src/core/regexp/onepass.cs` (1/5) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:191` | written | `src/core/internal/trace/internal/tracev1/parser.cs` (1/6) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:278` | adapted | `src/core/text/tabwriter/tabwriter.cs` (7/11) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:521` | adapted | `src/tests/Behavioral/LocalStructFieldAddr/main.cs` (4/5) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:601` | written | `src/core/vendor/golang.org/x/net/dns/dnsmessage/message.cs` (1/4) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:710` | adapted | `src/core/encoding/xml/xml.cs` (3/3) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:742` | written | `src/core/internal/poll/windows/fd_windows.cs` (2/6) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:782` | adapted | `src/tests/Behavioral/MethodValueReceiverEscape/main.cs` (2/3) |
| `docs/ConversionStrategies-Reference/multi-assignment.md:813` | written | no line long enough to identify |
| `docs/ConversionStrategies-Reference/multi-result-and-comma-ok.md:6` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/multi-result-and-comma-ok.md:13` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/multi-result-and-comma-ok.md:117` | adapted | `src/core/unicode/utf16/utf16.cs` (4/7) |
| `docs/ConversionStrategies-Reference/multi-result-and-comma-ok.md:148` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:38` | adapted | `src/tests/Behavioral/UnsignedNamedNumeric/main.cs` (1/2) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:117` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:158` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:207` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:236` | adapted | `src/tests/Behavioral/AppendUntypedConst/main.cs` (2/3) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:297` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:330` | written | `src/tests/Behavioral/UntypedConstArithmetic/main.cs` (1/3) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:352` | adapted | `src/tests/Behavioral/BigUntypedConstComparison/main.cs` (3/5) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:396` | adapted | `src/tests/Behavioral/UntypedConstWideMask/main.cs` (1/2) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:435` | adapted | `src/core/net/textproto/reader.cs` (3/4) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:529` | adapted | `src/tests/Behavioral/NamedBooleanLogic/main.cs` (1/2) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:544` | adapted | `src/tests/Behavioral/CastNegativeNamedType/main.cs` (1/2) |
| `docs/ConversionStrategies-Reference/named-numeric-types.md:563` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/native-and-narrow-integers.md:9` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/native-and-narrow-integers.md:81` | written | `src/core/math/rand/rng.cs` (1/6) |
| `docs/ConversionStrategies-Reference/nil-and-zero-values.md:17` | adapted | `src/core/os/user/darwin/cgo_lookup_unix.cs` (1/2) |
| `docs/ConversionStrategies-Reference/nil-and-zero-values.md:101` | adapted | `src/core/errors/wrap.cs` (1/2) |
| `docs/ConversionStrategies-Reference/nil-and-zero-values.md:229` | adapted | `src/core/syscall/linux/zsyscall_linux_amd64_impl.cs` (1/2) |
| `docs/ConversionStrategies-Reference/nil-and-zero-values.md:339` | adapted | `src/tests/Behavioral/TypedNilInterface/TypedNilInterface.cs` (2/4) |
| `docs/ConversionStrategies-Reference/nil-and-zero-values.md:381` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/package-conversion.md:95` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/package-conversion.md:354` | written | `src/core/crypto/internal/entropy/v1.0.0/sha384.cs` (1/5) |
| `docs/ConversionStrategies-Reference/package-conversion.md:407` | written | none (0/8) |
| `docs/ConversionStrategies-Reference/package-conversion.md:909` | adapted | `src/tests/Behavioral/CrossPkgLib/package_info.cs` (3/3) |
| `docs/ConversionStrategies-Reference/package-conversion.md:917` | adapted | `src/tests/Behavioral/CrossPkgBox/package_info.cs` (3/3) |
| `docs/ConversionStrategies-Reference/package-conversion.md:1022` | written | `src/core/runtime/windows/os_windows.cs` (1/4) |
| `docs/ConversionStrategies-Reference/package-conversion.md:1312` | written | `src/core/image/geom_test.cs` (1/4) |
| `docs/ConversionStrategies-Reference/package-conversion.md:1434` | adapted | `src/core/net/http/pprof/package_test_info.cs` (8/10) |
| `docs/ConversionStrategies-Reference/pointers.md:9` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/pointers.md:23` | adapted | `src/tests/Behavioral/RangeVarHeapBox/main.cs.target` (1/2) |
| `docs/ConversionStrategies-Reference/pointers.md:37` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:241` | adapted | `src/core/crypto/internal/fips140/nistec/fiat/p224.cs` (1/2) |
| `docs/ConversionStrategies-Reference/pointers.md:289` | adapted | `src/tests/Behavioral/GenericReceiverFieldAddress/main.cs` (4/4) |
| `docs/ConversionStrategies-Reference/pointers.md:303` | adapted | `src/tests/Behavioral/ForVariants/ForVariants.cs` (3/3) |
| `docs/ConversionStrategies-Reference/pointers.md:343` | adapted | `src/tests/Behavioral/GuardedNilPointerParamDeref/main.cs` (2/2) |
| `docs/ConversionStrategies-Reference/pointers.md:361` | written | `src/tests/Behavioral/GlobalPointerWalk/main.cs` (2/5) |
| `docs/ConversionStrategies-Reference/pointers.md:378` | written | `src/tests/Behavioral/FuncLitArgCapture/main.cs` (1/4) |
| `docs/ConversionStrategies-Reference/pointers.md:401` | written | `src/tests/Behavioral/ClosureWriteVisibility/main.cs` (5/12) |
| `docs/ConversionStrategies-Reference/pointers.md:449` | adapted | `src/core/regexp/onepass.cs` (3/3) |
| `docs/ConversionStrategies-Reference/pointers.md:463` | adapted | `src/tests/Behavioral/FuncLitArgCapture/main.cs` (7/9) |
| `docs/ConversionStrategies-Reference/pointers.md:507` | adapted | `src/core/crypto/x509/windows/verify.cs` (2/3) |
| `docs/ConversionStrategies-Reference/pointers.md:525` | written | `src/core/regexp/onepass.cs` (1/3) |
| `docs/ConversionStrategies-Reference/pointers.md:563` | written | `src/core/time/format_rfc3339.cs` (3/7) |
| `docs/ConversionStrategies-Reference/pointers.md:611` | written | `src/core/time/time_test.cs` (1/4) |
| `docs/ConversionStrategies-Reference/pointers.md:626` | adapted | `src/tests/Behavioral/ReceiverFieldMethodCall/main.cs` (1/2) |
| `docs/ConversionStrategies-Reference/pointers.md:643` | written | `src/tests/Behavioral/CaptureModeValueParamUser/main.cs` (1/4) |
| `docs/ConversionStrategies-Reference/pointers.md:656` | written | `src/tests/Behavioral/CaptureModeFuncLitParam/main.cs` (2/5) |
| `docs/ConversionStrategies-Reference/pointers.md:692` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:751` | adapted | `src/core/debug/elf/file.cs` (2/2) |
| `docs/ConversionStrategies-Reference/pointers.md:821` | adapted | `src/core/unsafe/unsafe.cs` (4/4) |
| `docs/ConversionStrategies-Reference/pointers.md:928` | adapted | `src/core/strings/builder.cs` (3/6) |
| `docs/ConversionStrategies-Reference/pointers.md:942` | written | `src/tests/Behavioral/TypedPointerCastDeref/main.cs` (1/4) |
| `docs/ConversionStrategies-Reference/pointers.md:949` | adapted | `src/tests/Behavioral/TypedPointerCastDeref/main.cs` (1/2) |
| `docs/ConversionStrategies-Reference/pointers.md:1000` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:1156` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:1227` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:1310` | adapted | `src/core/runtime/pinner.cs` (1/2) |
| `docs/ConversionStrategies-Reference/pointers.md:1573` | adapted | `src/core/crypto/x509/x509.cs` (1/2) |
| `docs/ConversionStrategies-Reference/pointers.md:1615` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/pointers.md:1633` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/pointers.md:1641` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/pointers.md:1663` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/pointers.md:1705` | written | `src/core/sync/oncefunc_test.cs` (1/3) |
| `docs/ConversionStrategies-Reference/pointers.md:1744` | adapted | `src/core/context/example_test.cs` (3/3) |
| `docs/ConversionStrategies-Reference/pointers.md:1778` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/pointers.md:1802` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/pointers.md:1931` | adapted | `src/core/os/darwin/file.cs` (4/7) |
| `docs/ConversionStrategies-Reference/pointers.md:2013` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:2098` | written | `src/core/time/zoneinfo.cs` (1/3) |
| `docs/ConversionStrategies-Reference/pointers.md:2117` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/pointers.md:2167` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:2297` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:2472` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/pointers.md:2715` | adapted | `src/tests/Behavioral/PointerValueToInterfaceArg/main.cs` (4/5) |
| `docs/ConversionStrategies-Reference/shadowing.md:22` | adapted | `src/tests/Behavioral/BuiltinShadowLocal/main.cs` (4/5) |
| `docs/ConversionStrategies-Reference/shadowing.md:68` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/shadowing.md:177` | written | none (0/5) |
| `docs/ConversionStrategies-Reference/shadowing.md:225` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/shadowing.md:235` | adapted | `src/core/html/template/package_test_info.cs` (1/2) |
| `docs/ConversionStrategies-Reference/shadowing.md:569` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/shadowing.md:592` | adapted | `src/tests/Behavioral/BuiltinShadowLocal/main.cs` (1/2) |
| `docs/ConversionStrategies-Reference/shadowing.md:628` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/shadowing.md:691` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/shadowing.md:721` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/shadowing.md:828` | adapted | `src/tests/Behavioral/DotImportNameCollision/main.cs` (4/4) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:22` | written | `src/tests/Behavioral/AdapterNameInterfaceCollision/main.cs` (2/5) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:162` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:317` | written | `src/tests/Behavioral/ZeroValueArrayNamedResult/main.cs.target` (1/6) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:599` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:684` | written | `src/core/runtime/arena_impl.cs` (1/8) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1013` | adapted | `src/core/crypto/tls/handshake_client.cs` (1/2) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1126` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1162` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1207` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1250` | written | `src/tests/Behavioral/CompositeLiteralElements/main.cs` (1/4) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1274` | adapted | `src/tests/Behavioral/CompositeLiteralElements/main.cs` (2/3) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1309` | adapted | `src/tests/Behavioral/CompositeLiteralElements/main.cs` (2/4) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1356` | adapted | `src/tests/Behavioral/CompositeLiteralElements/main.cs` (3/4) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1406` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1436` | adapted | `src/tests/Behavioral/NestedFixedArrays/main.cs` (3/3) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1452` | adapted | `src/core/compress/flate/huffman_code.cs` (1/2) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1580` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1658` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/slices-and-arrays.md:1721` | adapted | `src/tests/Behavioral/MapArrayValueZero/MapArrayValueZero.cs.target` (4/4) |
| `docs/ConversionStrategies-Reference/source-generators.md:436` | written | `src/core/runtime/metrics/sample.cs` (1/3) |
| `docs/ConversionStrategies-Reference/strings.md:140` | adapted | `src/tests/Behavioral/StringLiteralSliceConversion/main.cs.target` (2/3) |
| `docs/ConversionStrategies-Reference/strings.md:195` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/strings.md:233` | adapted | `src/core/internal/strconv/atob.cs` (6/6) |
| `docs/ConversionStrategies-Reference/strings.md:285` | adapted | `src/core/internal/strconv/atoi.cs` (4/4) |
| `docs/ConversionStrategies-Reference/strings.md:318` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/strings.md:368` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/strings.md:392` | written | none (0/4) |
| `docs/ConversionStrategies-Reference/strings.md:404` | adapted | `src/core/text/template/parse/parse_test.cs` (1/2) |
| `docs/ConversionStrategies-Reference/strings.md:421` | adapted | `src/core/fmt/package_info.cs` (3/3) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:161` | written | none (0/5) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:215` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:350` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:366` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:415` | adapted | `src/gen/go2cs-gen/Templates/InheritedType/InheritedTypeTemplate.cs` (4/8) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:471` | adapted | `src/core/golib/uintptr.cs` (1/2) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:500` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:658` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:689` | written | `src/tests/Behavioral/EmbeddedPointerNilAssign/EmbeddedPointerNilAssign.cs` (3/7) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:719` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/struct-embedding.md:732` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/struct-types.md:207` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/struct-types.md:310` | adapted | `src/tests/Behavioral/TypeSwitchImpureTag/main.cs.target` (5/6) |
| `docs/ConversionStrategies-Reference/struct-types.md:586` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/struct-types.md:608` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/struct-types.md:630` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/struct-types.md:652` | adapted | `src/tests/Behavioral/FuncLiteralDeclaredResultIface/main.cs` (3/5) |
| `docs/ConversionStrategies-Reference/struct-types.md:715` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/struct-types.md:735` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/struct-types.md:785` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/struct-types.md:817` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/struct-types.md:1053` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/struct-types.md:1125` | adapted | `src/core/golib/variadic.cs` (2/2) |
| `docs/ConversionStrategies-Reference/struct-types.md:1382` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/struct-types.md:1525` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/type-aliasing.md:19` | adapted | `src/tests/Behavioral/TypeConversionReturnType/TypeConversionReturnType.cs.target` (2/3) |
| `docs/ConversionStrategies-Reference/type-aliasing.md:42` | adapted | `src/tests/Behavioral/PackageAliasRootedTypeArgs/main.cs` (3/3) |
| `docs/ConversionStrategies-Reference/type-aliasing.md:82` | adapted | `src/tests/Behavioral/GenericTypeAlias/main.cs.target` (6/6) |
| `docs/ConversionStrategies-Reference/type-switch.md:21` | written | `src/tests/Behavioral/PanicRecover/PanicRecover.cs` (2/7) |
| `docs/ConversionStrategies-Reference/type-switch.md:44` | written | `src/tests/Behavioral/TypeSwitchCollapsedFuncCase/main.cs` (1/4) |
| `docs/ConversionStrategies-Reference/type-switch.md:64` | adapted | `src/core/go/types/expr.cs` (3/6) |
| `docs/ConversionStrategies-Reference/value-receiver-delegates.md:85` | written | none (0/2) |
| `docs/ConversionStrategies-Reference/value-receiver-delegates.md:133` | written | `src/tests/Behavioral/DeferValueFieldPtrReceiver/main.cs` (1/5) |
| `docs/ConversionStrategies-Reference/value-receiver-delegates.md:250` | written | `src/tests/Behavioral/TypedNilFuncBoundaries/main.cs.target` (1/7) |
| `docs/ConversionStrategies-Reference/value-receiver-delegates.md:302` | written | `src/core/text/template/parse/lex.cs` (1/3) |
| `docs/ConversionStrategies-Reference/value-receiver-delegates.md:322` | adapted | `src/tests/Behavioral/BlankIdentifierCollision/main.cs` (4/4) |
| `docs/ConversionStrategies-Reference/value-receiver-delegates.md:342` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/value-receiver-delegates.md:356` | written | none (0/3) |
| `docs/ConversionStrategies-Reference/value-receiver-delegates.md:371` | written | none (0/1) |
| `docs/ConversionStrategies-Reference/variable-initialization-order.md:104` | adapted | `src/tests/Behavioral/PackageVarInitOrder/entries.cs.target` (2/3) |
| `docs/ConversionStrategies-Reference/variable-initialization-order.md:146` | written | none (0/1) |
| `docs/ConversionStrategies.md:12173` | adapted | `src/core/sync/atomic/value.cs` (10/10) |
| `docs/ConvertingAModule.md:130` | adapted | `src/go2cs/testdata/markercomments/main.cs` (4/7) |
