# c2-o1-survival -- which string parameters an `sstring` can actually take (point-in-time record)

The O1 survival census that
[`DESIGN-string-literal-allocation.md`](../../DESIGN-string-literal-allocation.md) §8.2.3 named "the first
census to run". COORD assigned it after the owner's ruling on `c555d91c55`, which made sstring-first the
primary tier. Read-only, and not a gate. Results: [`RESULTS.md`](RESULTS.md).

**O1** retypes a string PARAMETER as golib's `sstring`, a ref-struct view, so that a literal or
`string(b)` argument binds with no copy. Go's escape analysis says which parameters Go keeps off the heap.
This census asks which of those a C# ref struct can take. It reads every use of the parameter in the
callee's Go body against the C# filters, then closes the set under onward passing.

```
CGO_ENABLED=0 GOOS=<os> GOARCH=amd64 go build -a -gcflags=-m std 2> m-<os>.txt   # go1.24.13
CGO_ENABLED=0 GOOS=<os> go run . -cgo 0 -m m-<os>.txt -core <repo>/src/core -params params-<os>.tsv > census-<os>.txt
go test .                                                                          # the controls, both ways
```

Run everything with `GOTOOLCHAIN=go1.24.13`. `-m` prints absolute GOROOT paths, and the join matches them
against the GOROOT the program loads.

**CGO is pinned to 0**, the setting the corpus is emitted at (`.claude/rules/corpus.md`). The program sets
it for its own load (`-cgo`, default 0) and prints it in the header, and the `-m` input must match.
- A cross build (windows, darwin) defaults to 0.
- A native linux build defaults to 1. The first commit's linux column (`3b53fec4f2`) was cgo ON, and COORD's
  verification read the difference. The linux `-m` is now built at 0.

## The population (P)

A production call site whose argument is either:
- a string literal (a `BasicLit` of kind STRING), or
- `string(b)` with `b` a byte slice.

It joins to the callee's parameter with the same exclusions as `c2-escape-join`'s J rows, each counted:
- no callee declaration (a func value, a conversion, a builtin);
- an interface method;
- a generic instantiation;
- a variadic tail.

The parameter's verdict comes from `-m`'s parameter lines at its declaration position. "Format position"
is the converter's own predicate: the parameter before a variadic tail, of a callee whose name ends in `f`.

**Reconciliation, checked.** The noescape literal rows reproduce `c2-escape-join`'s J rows exactly:
2,344 + 2,491 = 4,835 (windows), 2,313 + 2,489 (linux), 2,314 + 2,489 (darwin). The `string(b)` rows sum
to that census's I6 row (111 / 115 / 115).

## The unit and the classes

The unit is the callee PARAMETER. Every call site inherits its parameter's result. Only production
function bodies are classified; test files are read only for func-value uses.

A class has one of four kinds:
- **disqualifying:** the parameter cannot be an `sstring` without a copy or a compile error;
- **gap:** disqualifying until golib's `sstring` gains the missing member;
- **safe:** recorded, not disqualifying;
- **onward:** resolved by the fixed point.

**Signature classes** (the parameter's type is fixed by something other than its own body):

| class | predicate |
|:--|:--|
| hand-owned | at FUNCTION level: the package is `unsafe` or `testing`; or the Go file's go2cs file (flat or per-GOOS) carries `[module: GoManualConversion]` as an attribute LINE (a whole-file hand-own); or it carries the converter's placeholder for THIS function (`funcPlaceholderLead` + name + ` is hand-converted`, visitFuncDecl.go:350). The first commit matched the text `GoManualConversion` anywhere, so every placeholder comment hand-owned its whole file (COORD: 18 parameters, 133 sites). The placeholder names the function only, so two methods of one name in one file are not told apart |
| defer/go callee | the function is the static callee of a `defer` or `go` statement anywhere in the load, tests included: go2cs lowers the call into golib's generic defer, whose type arguments cannot be a ref struct (CS0306; COORD's case is fmt.(pp).catchPanic at print.cs:813) |
| no Go body | the `FuncDecl` has no body (assembly or linkname) |
| used as a value | the function object is referenced anywhere other than as a call's `Fun` (through selectors, generic indexing and parentheses). Split into a production value site and a test-file-only value site |
| interface match | a method whose name and parameter/result types equal some interface method's in the load: the binder demands exact parameter types (TypeExtensions.GoMethodSets.cs:515-545) |
| exported method | reflection can reach it by name |

**Use classes.** Each use of the parameter, and of every local it is bound to (tracked as an alias), is
classified by what consumes it:

| class | predicate |
|:--|:--|
| captured by a closure | the use is inside a `FuncLit` |
| defer/go argument | the use is inside a `defer` or `go` statement (the lowering captures it) |
| box | converted to an interface: an explicit conversion, an interface parameter, a `...any` tail, an interface variable, or `print`/`println`/`panic` |
| generic argument | an argument of a generic function or of a method of a generic type |
| store | a struct field, a package-level variable, a slice, array or map element (`append` of it as an element too), a composite literal, a channel send, through a pointer |
| map key | an index into a map, or `delete` |
| returned | a return operand, or an assignment to a named result |
| address taken | `&s` |
| `...string` tail | an argument in a variadic string tail |
| fixed signature | an argument to a func value or an interface method |
| named string | converted to a named string type, or the parameter's own type is a named string |
| **gap:** range | `for … range s` (sstring has no enumerator) |
| **gap:** copy | `copy(dst, s)` |
| **gap:** spread | `append(b, s...)` |
| safe | `len`/`cap`, `s[i]`, a comparison, `switch s` (lowered to a temp and comparisons), a concatenation (sstring's `+` exists for every operand pairing and is uncounted until V-fix 11), `[]byte(s)`/`[]rune(s)` (copies, as Go does), `_ = s`, a reassignment of the parameter |
| onward | an argument to a static callee's string parameter: an edge to that parameter |

**The fixed point.** A parameter survives when three things hold:
- `-m` says noescape;
- no disqualifying class holds;
- every onward parameter survives.

Survival is the greatest fixed point. It is computed four times:
- **S0:** today;
- **S1:** with the gaps closed;
- **S2, the FLIP shape:** as S1, plus an adapter lambda the converter would emit at every func-value site,
  which keeps the value's `@string` signature and forwards a view;
- **S3, the TWIN shape** (COORD's preferred form): as S1, with the `@string` member kept and an `sstring`
  overload added under `[OverloadResolutionPriority(1)]`. A value use, the binder, reflection and a defer
  lowering keep binding the `@string` member, so every signature class stops disqualifying except
  hand-owned and no-body. A literal site whose call is itself deferred binds the `@string` member and gains
  nothing; it is reported and subtracted.

**Determinism.** The fixed point runs over parameters and edges in sorted order. A parameter's culprit is
read from the FINAL state, as its first failing edge in sorted order. Only a parameter that fails by the
cascade alone gets one, so a root is the first parameter that fails for its own reason. Root keys sort
their reasons. The first commit took the culprit from map order, and COORD saw per-root splits move over
300 runs.

**Per evaluation (the Tier C column).** A site that Tier C already hoists to a static field allocates once
per process, not per evaluation. The census applies Tier C's own site predicate to every non-format
literal site (`hoisted` in main.go, copied from hoistedLiteralOperations.go and convBasicLit.go; format
position never hoists). It reports survivors split into inline-today and hoisted-today, and a
PER-EVALUATION line. The predicate is checked against the emitted corpus: every site predicted hoisted must
find a field in its package whose decoded bytes are its value (the `H` rows in P).

## Controls (`main_test.go`, `testdata/fixture`)

The fixture's own `-gcflags=-m` is read by the same code. The test checks every class both ways:
- each SURVIVE function (len, index, compare, switch, alias, concatenation, `[]byte`, discard, a one-hop
  and a two-hop onward chain, a format-position parameter) survives in S0 and S1;
- each FAIL function carries exactly its class and fails;
- each GAP function fails in S0 and survives in S1;
- the adapter mode lifts only the func-value classes (production and test-only) and never a box;
- the defer/go callee class fires on `deferSink` (reached only by `defer`) and not on `safeLen`;
- the twin mode lifts the signature classes (value, test-only value, binder, reflection, defer callee)
  and no use class (box, capture, store, return, fixed signature, cascade);
- the hoist predicate: seven literal sites reach `hoistSink`, and only the plain one hoists. It stays
  inline for a degenerate slug, a parenthesised literal, the byte-array path, the empty literal, `init`
  and a package-level initializer;
- `TestHandOwnAtFunctionLevel`: a placeholder hand-owns only the function it names, only an attribute
  LINE hand-owns the whole file, and the C# literal decoder is checked;
- a literal reaches each parameter once, and the format-position predicate fires on `logf`.

The gate was made to fail twice, then restored byte-identical each time:
- removing the interface-variable arm named `boxes`;
- dropping the defer-callee record named `deferSink`.

## Outputs

- `census-<os>.txt`:
  - the population (P) with R2 and the hoist check (H);
  - S0-S3, each with every-reason and first-reason tables and the top callees;
  - cascade roots at S2 and S3 (C2, C3);
  - fmt's unexported helpers (F);
  - the residual at S2 and S3 (R);
  - the pilot list for the flip and the twin (PILOT2, PILOT3).
- `params-<os>.tsv`: one row per reached parameter and every fmt unexported one. Columns:
  - its S0-S3 status;
  - its literal sites (hoisted, format, deferred, leaking) and `string(b)` sites;
  - the reasons at S0, S2 and S3;
  - the culprit at S2 and S3;
  - its function's value sites.

Paths are GOROOT-relative. A cgo-generated file outside GOROOT keeps only its base name.

## Caveats

- A verdict describes Go's function. Hand-owned go2cs files are flagged, not re-read.
- The interface-match class over-approximates: it matches any interface in the load with the same method
  name and types, whether or not the type is ever used as that interface.
- Returning a view (`truncateString`) is disqualifying here. An `sstring`-returning variant is a
  different, larger change.
- Test call sites are not in the population. A test's `string` variable passed to a flipped parameter
  converts implicitly, with no copy.
