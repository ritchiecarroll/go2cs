# Design: plain-path operand order

Go's specification orders only the calls, method calls, receive operations and binary logical operations of a
statement: they run in lexical left-to-right order. It leaves the order of the statement's other operands against
those calls unspecified. gc settles it the same way every time. Its order pass spills every call and receive of the
statement to a temporary first, in lexical order, and then evaluates the statement from those temporaries and the
remaining operands. Under gc every plain operand is read after every call of its statement. gc's ordered set is
wider than calls and receives: a set of EARLY forms (below) also runs at its lexical place among the calls.

C# evaluates operands strictly left to right. Where a call writes something an operand to its left reads, the
converted program reads the old value and gc's program reads the new one:

```go
fmt.Println("param-later-arg", readWith(pp, <-kc))   // a goroutine sets pp before it sends on kc
```

gc prints `param-later-arg 1405`: the receive runs, then `pp` is read and holds the goroutine's value. Read left to
right, `pp` is read before the receive, which prints `1505`, and on the plain arm it is also a data race. Users see
gc's order, so the converter matches it.

## The rule

In one statement, every call or receive up to the LAST one that follows a mutable read moves, in lexical order, into
a statement prelude, and so does every early form before that call. The reads stay where they are:

```csharp
var evalᴛ1 = ᐸꟷ(kc);
fmt.Println("param-later-arg"u8, readWith(ref (pp).DerefOrNull(), evalᴛ1));
```

Moving every call up to the last one that matters, not just that one, keeps the calls' own order, which the
specification does fix. A statement where no mutable read precedes a call emits exactly as before.

A read that sits inside an earlier call or early form does not count: it is evaluated when that unit runs, which is
before the later call in both orders. `two(f(g), mut())` is left alone, and so is `fmt.Sprintf("%v %d", h.got == 1,
next())`, whose comparison is boxed early.

## The early forms

Measured at go1.26.11 with `go run`, one row per form, each in a typed and a boxed context. These run at their lexical
place among the calls, reading their own operands then:

- the builtins `len`, `cap`, `append`, `min`, `max`, `make`, `real`, `imag`, `complex` and `copy`;
- a conversion from a string to `[]byte` or `[]rune`, and from a slice to an array;
- a type assertion `x.(T)`, a slice expression `s[i:j]`, and a map index whose key is `string(b)` for a byte slice;
- an `&&` or `||` that holds a call, as one unit;
- a value assigned to an interface parameter or element (boxed) that is not addressable and is a `bool`, a one-byte
  integer, a struct, an array or `&T{...}`: `fmt.Println(g, g > 1, mut())` prints the old comparison and the new `g`.

An early form never makes a call move. When a call moves ahead of it, it moves too, in order, as its own temp, which
holds the value before boxing (`var evalᴛ1 = g > 1;`). Every other conversion, builtin and boxed value is transparent:
its operands are read after the calls. A constant reads nothing and never moves.

## What counts as a mutable read

A read is mutable when a call of the same statement can write what it reads (`operandIsMutable` in
`src/go2cs/operandOrder.go`):

- a package-level variable;
- a local variable, parameter or result that a func literal captures, or whose address is taken: `&x`, a
  pointer-method call or method value on a value `x`, or slicing an array `x`. The converter's heap-escape set is not
  consulted: it also holds locals boxed for reasons no call can observe;
- a dereference `*p`;
- a field reached through a pointer, or a field of an operand that is itself mutable;
- an element of a slice or a map, or of an array behind a pointer;
- an array or string element whose base or index is mutable, and a slice expression with a mutable part.

**A plain local is never a mutable read**, and the reason matters. It is not that a call cannot reach it. It is that
no call can WRITE it: a local that is neither captured nor address-taken can be changed only by the statement's own
assignments, which run after its operands are evaluated. Its read is therefore the same in either order, and the
difference is unobservable. That is the owner's rule for this class (2026-10-16): leave alone what no program can
observe. It is also what keeps the footprint narrow. The common statement `x := f(a, g(b))` over plain locals is
untouched.

## What is never moved

- An operand of `&&` or `||` on its own: the right one runs only after the left one. An `&&` or `||` that holds a
  call moves whole or not at all, so its short circuit stands.
- A func literal's body, which is another function.
- A map literal: gc reads its elements in place.
- `&x`, which takes an address and reads no value a call can change.
- A multi-value call, and a statement whose right side is one (`a, b = f()`) or a comma-ok form. A statement with
  such a call among its hoists keeps C#'s order.

## Statements and headers

| Statement | Treatment |
|---|---|
| expression statement, send, `return`, assignment with matching sides | the prelude goes before the statement |
| assignment | the left side's index and pointer operands are walked as reads, per the specification's two-phase rule; `x op= f()` also reads `x` |
| `select` send case | the prelude goes before the case's channel and value temps |
| `if`, and `if` with an init | the prelude goes before the `if`, in the block form an init statement already uses |
| `else if` with no init | a non-empty prelude makes it `} else {`, the prelude indented one level, then `if (…)`; a prelude written straight after `else` would not compile |
| loop condition | the condition leaves the header and opens the body as `<prelude> if (!(cond)) break;`, the converter's existing spill, so it runs before every pass |
| loop post | the post leaves the header and closes the body, the existing post spill |
| `range` operand | the prelude goes before the loop; Go evaluates the operand once too |
| local `var` declaration with one value per name | the prelude goes before the declaration |
| `switch` tag | the prelude goes before the tag's own temp, after any init statement |
| `go` and `defer` arguments | the prelude goes into the statement's capture block, beside the temps that hold the arguments until the call runs; only the calls among the function value and the arguments move, never the deferred call |
| a labeled statement | the label stays first, so it lands on the first prelude line (`again: var evalᴛ1 = bump();`) and a `goto` re-runs the prelude |
| `else if` with an init | the init's prelude, the init, then the condition's prelude, in the one braced `else` |

**Limits, which keep C#'s order:**

- `case` expressions: Go evaluates them lazily, top to bottom, so none can move ahead of the switch;
- package-level initializers, which have no statement prelude to write into;
- a statement with a multi-value call or a comma-ok form among its hoists, such as `x := g + sum(two())`.
- a division or other operation that can panic: gc runs the statement's calls before it, and C# panics at its place.

A `defer` that the converter lowers into the function's `finally` is not a limit: lowering admits only calls with no
arguments, so nothing in it can read a value a call changes.

Each limit is a place where C#'s order still differs from gc's when a mutable read precedes a call. [Known issues](KnownIssues.md#operand-order-limits) lists them for users.

## Emission

A hoist is `var evalᴛN = <call>;`, numbered per function. A func literal shares its enclosing function's counter, so
a literal's hoist inside a statement that has one of its own takes the next number and never shadows it (CS0136). It does not take the converter's per-file tuple counter
(`tupleTempIndex`): a hoist numbered there would renumber every later tuple temp in the file, and the change would
reach lines it does not touch. A loop header learns that its condition spilled from `operandHoistCount`, as it learns
of a tuple spread from `tupleTempIndex`.

The return statement's existing spill (`returnOperandOrder.go`, see
[Multi-Assignment and Evaluation Order](ConversionStrategies-Reference/multi-assignment.md#a-multi-value-return-reads-its-plain-operands-after-its-calls))
is the narrower special case of this rule for multi-value returns. The two compose: a call this rule hoists renders as
its temp, so the return spill sees an operand with no call in it.

## Evidence

The red set is nine behavioral projects: OperandOrderCallArgs, Assign, Return, If, Send, CompositeLit, MultiAssign,
Binary, and the control OperandOrderInPlace. Each prints one line per shape and is compared with `go run`. At the
base converter, eight of them differ from Go on 31 lines. The in-place control matches, and it must keep matching.
With the change, all nine match. A tenth project, OperandOrderStmtForms, covers the forms added after the
design review: a local `var` declaration, `defer` and `go` arguments, and a switch tag. OperandOrderEarly has one row
per early form; at the base it reads 22 lines away from Go, and at the cut before the early forms were added it read
23 lines away (it moved calls past early forms and the `&&` unit). OperandOrderLabels covers a `goto` to a labeled
prelude, a labeled range with `continue`, a labeled `if`, an `else if` with an init, a func literal's prelude and a loop condition whose call moves into the body. MnLaterArg (`src/tools/mnspike2`) reads 1405 under all four arms: the plain and
twinned builds, each with M:N on and off.
