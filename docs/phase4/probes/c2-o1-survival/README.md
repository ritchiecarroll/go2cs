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
GOOS=<os> GOARCH=amd64 go build -a -gcflags=-m std 2> m-<os>.txt      # go1.24.13, the c2-escape-join input
GOOS=<os> go run . -m m-<os>.txt -core <repo>/src/core -params params-<os>.tsv > census-<os>.txt
go test .                                                                # the controls, both ways
```

Run everything with `GOTOOLCHAIN=go1.24.13`. `-m` prints absolute GOROOT paths, and the join matches them
against the GOROOT the program loads.

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
| hand-owned | the Go file's go2cs file (flat or per-GOOS) contains `GoManualConversion`, or the package is `unsafe` or `testing` |
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

Survival is the greatest fixed point. It is computed three times:
- **S0:** today;
- **S1:** with the gaps closed;
- **S2:** as S1, plus an adapter lambda the converter would emit at every func-value site, which keeps the
  value's `@string` signature and forwards a view.

## Controls (`main_test.go`, `testdata/fixture`)

The fixture's own `-gcflags=-m` is read by the same code. The test checks every class both ways:
- each SURVIVE function (len, index, compare, switch, alias, concatenation, `[]byte`, discard, a one-hop
  and a two-hop onward chain, a format-position parameter) survives in S0 and S1;
- each FAIL function carries exactly its class and fails;
- each GAP function fails in S0 and survives in S1;
- the adapter mode lifts only the func-value classes (production and test-only) and never a box;
- a literal reaches each parameter once, and the format-position predicate fires on `logf`.

The gate was made to fail once: removing the interface-variable arm made it name `boxes`. It was then
restored byte-identical.

## Outputs

- `census-<os>.txt`: the population (P), R2, S0/S1/S2 with every-reason and first-reason tables, the top
  callees, fmt's unexported helpers (F), the residual (R), and the pilot list.
- `params-<os>.tsv`: one row per reached parameter and every fmt unexported one. Columns: its S0/S1/S2
  status, literal and `string(b)` sites, the reasons at S0 and S2, the first failing onward parameter,
  and its func-value sites.

Paths are GOROOT-relative. A cgo-generated file outside GOROOT keeps only its base name.

## Caveats

- A verdict describes Go's function. Hand-owned go2cs files are flagged, not re-read.
- The interface-match class over-approximates: it matches any interface in the load with the same method
  name and types, whether or not the type is ever used as that interface.
- Returning a view (`truncateString`) is disqualifying here. An `sstring`-returning variant is a
  different, larger change.
- Test call sites are not in the population. A test's `string` variable passed to a flipped parameter
  converts implicitly, with no copy.
