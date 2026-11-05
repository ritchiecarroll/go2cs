# PLAN — init statements without scope braces

> **STATUS: PROPOSED (H, 2026-11-04). A scout for an owner ask. It changes no converter code and no emission.**
> Document type: a [strategy plan](doctrine/ProcessGlossary.md#plan), proposed and not yet ruled. Every number was
> measured at master `33589333b2` (batch 155) on H. The instruments are in
> [`phase4/probes/h-init-scope-census/`](phase4/probes/h-init-scope-census/README.md), and section 7 gives the
> commands that reproduce them.

## 1. The ask and today's shape

The owner, 2026-11-04: *"for a function like this
<https://github.com/ritchiecarroll/go2cs/blob/nuget-1.26.10.2/src/core/io/io.cs#L322> that uses braces to block scope
the temporary variable declaration (declared inline in Go), consider ways to make this read more like Go: clever
ways to handle block scope braces, or dropping block scope braces and handling differently -- certainly in a simple
case like in the linked example, dropping the scope braces doesn't change code behavior..."*

Go scopes an `if`, `switch` or type-switch init statement to that statement. C# has no init clause, so the converter
opens a block for it (`visitIfStmt.go`, `visitSwitchStmt.go`, `visitTypeSwitchStmt.go`; "Any declared variable will
be scoped to if statement, so create a sub-block for it"). `io.WriteString`:

```go
if sw, ok := w.(StringWriter); ok {
	return sw.WriteString(s)
}
return w.Write([]byte(s))
```

```csharp
{
    var (sw, ok) = w._<StringWriter>(ᐧ); if (ok) {
        return sw.WriteString(s);
    }
}
return w.Write(slice<byte>(s));
```

The same wrapper surrounds a `switch` with an init. An else-if with its own init opens it on the `} else` line, as
`} else {`, and closes it after the chain.

## 2. The census

**What was counted.** It covers every emitted file of the converted standard library on all three targets, plus the
behavioral goldens. A target's files are read from its own position maps: a package's
`<goos>/package_info.cs` when the package is L3, otherwise its flat `package_info.cs`. Each `.cs` path resolves in the
GOOS folder first, then flat. The census covers 2,671 unique files. It skips 206 hand-owned files, 3 map entries with
no file and 63 golden entries with no `.cs.target`.

**Two instruments, reconciled file by file.**

- A go/ast pass over each file's Go source lists, in source order, every `if`, `switch` and type switch that carries
  an init, plus every bare Go block. It classifies each by statement kind and by init kind.
- A Roslyn pass over the emitted C# lists every bare block, and every else block whose last statement is an `if`
  that sits on the line where the init ends (the emitter's signature).

The two lists must agree in count and in kind, file by file. They do: 5,077 Go items against 5,077 C# blocks, with 0
mismatched files. Every joined block declares the names its Go init declares (0 name-check failures). 77 of the
5,077 are Go's own bare blocks, which are not init blocks and are left out below.

**Red first.** One `if bw, ok := w.(ByteWriter); ok` was planted in copies of `io.go` and `io.cs`. Both instruments
read 5,078, and io went from 8 to 9 items. A block planted in the C# copy alone raises only the C# count, which the
per-file reconciliation reports by file.

**5,000 init blocks**, by statement kind and init kind:

| init kind | if | if/else | if heading an else-if chain | switch | else-if, its own init | type switch | total |
|---|---|---|---|---|---|---|---|
| plain `:=`, one name | 2,494 | 86 | 45 | 91 | 19 | 0 | 2,735 |
| a call with multiple results | 982 | 29 | 49 | 2 | 16 | 1 | 1,079 |
| comma-ok assertion | 537 | 106 | 15 | 2 | 12 | 0 | 672 |
| comma-ok map lookup | 228 | 19 | 4 | 1 | 0 | 0 | 252 |
| assignment `=` | 194 | 6 | 0 | 0 | 0 | 0 | 200 |
| parallel `:=` or `=` | 14 | 2 | 0 | 14 | 0 | 0 | 30 |
| `++` or `--` | 21 | 2 | 0 | 0 | 0 | 0 | 23 |
| comma-ok channel receive | 3 | 0 | 0 | 0 | 0 | 0 | 3 |
| channel receive, one value | 3 | 0 | 0 | 0 | 0 | 0 | 3 |
| expression (a call) | 3 | 0 | 0 | 0 | 0 | 0 | 3 |
| **total** | **4,479** | **250** | **113** | **110** | **47** | **1** | **5,000** |

The multiple-result row includes 269 inits that assign with `=` rather than declare. Assertions and map lookups
include 4 such inits.

| population | init blocks | files |
|---|---|---|
| windows | 4,052 | 624 |
| linux | 4,177 | 652 |
| darwin | 4,149 | 648 |
| standard library, unique files across the three | 4,740 | |
| behavioral goldens | 260 | |

## 3. The C# rules the shapes depend on, measured

Each rule below was compiled with Roslyn 4.14 (language version preview, Release), and each case that can run was
run. The cases are in `scoperules/Program.cs`. Two rows differ from the working assumption that a condition's
expression variables always land in the enclosing scope. An else-if's stay nested (R3), and a `var` pattern keeps its
names assigned in the else branch (R10).

| | case | result |
|---|---|---|
| R1 | a pattern variable in an `if` condition, then a local of the same name in the block | CS0128: the pattern variable is in the enclosing block's scope |
| R2 | that pattern variable used after the `if` | compiles: it leaked |
| R3 | a pattern variable in an else-if's condition, and the same name declared after the chain | CS0136: the else-if's variable is in a nested scope, which still may not reuse an enclosing name |
| R3b | two sibling else-ifs declaring one name | CS0136: each else-if nests inside the one before it |
| R4 | two dropped-brace inits declaring `ok` in one block | CS0128 |
| R5 | a dropped-brace init and a braced sibling declaring `ok` | CS0136, whichever comes first |
| R6 | a static field `x`, a braced `var x` init, then `x` read after it | braced reads 42 (the field); braces dropped reads **1** (the local), with no diagnostic |
| R7 | `x` read **before** a dropped-brace `var x` | CS0844 |
| R8 | a backward `goto` loop over an init whose `if` body captures the variable in a lambda | braced: `0,1,2`; dropped: **`2,2,2`** |
| R8c | the same capture when the enclosing block is a loop body | dropped: `0,1,2` (a loop body is a fresh scope each iteration) |
| R9 | `if (e is (var s, true)) { } else { use(s); }` | CS0165: `s` is not definitely assigned in the else |
| R10 | `if (e is var (n, err) && err != null) { } else { use(n); }` | **compiles**, with no warning: a `var` pattern is irrefutable, so `n` is assigned in both branches |
| R10c | `if (a + 1 is var x && x > 5) { } else { use(x); }` | compiles |
| R11 | an `out var` in an `if` condition, then a local of the same name | CS0128: it leaks like a pattern variable |
| R13 | a pattern variable in a `switch`'s governing expression | CS0128: it leaks to the enclosing block too |
| R14 | a declaration directly under a label (`L: var (a, ok) = …;`) | compiles at this language version |
| R15 | `e is (_, true)` | compiles and runs |
| R16 | `if (e is not (var v, true)) { return; }` then `v` read after the `if` | compiles |

Two of these are behavior changes, not compile errors: R6 and R8. Nothing the compiler reports catches either. A
predicate that admits a drop must therefore rule both out by construction. That is why the predicate in section 4
refuses on **any** occurrence of the name, not only on a declaration.

## 4. Candidate shapes

### (a) Drop the braces

```csharp
var (sw, ok) = w._<StringWriter>(ᐧ); if (ok) {
    return sw.WriteString(s);
}
return w.Write(slice<byte>(s));
```

This is Go's own token order: the init, then the condition, on one line. Only the scope moves, from the init block to
the enclosing block.

**The predicate.** Let D be the names the block itself declares: its init's names, any hoisted capture snapshot,
the converter's temps (`exprᴛ1`), and the expression variables of the `if` condition or switch expression. Let E be
the enclosing declaration space: the parent block, or the whole switch block for a switch section. The braces may go
when all three hold:

1. No identifier token spelling a name in D occurs anywhere in E outside the block. This is a declaration, a use, a
   lambda parameter or a type name, before the block or after it. The only exclusions are member names after a `.`,
   named-argument labels and object-initializer targets, which bind to no local. This refuses R4, R5 and R7 (the
   compile errors) and **R6, the silent rebinding**, by construction.
2. The block contains no lambda, anonymous method or local function. Moving a captured variable out of its block
   moves its closure display class to E's entry. Under R8 that is a behavior change; elsewhere it moves an
   allocation, and it is never needed for readability.
3. E holds no label when the block holds a closure (R8). Condition 2 implies this one; the census reports it
   separately.

The block must also not be an else block. An else-if's braces are its `else`'s own block; shapes (b) and (c) address
it.

**The population.** 4,183 of the 4,953 if, switch and type-switch blocks qualify (84.5%): 3,777 if, 207 if/else, 100 heading an
else-if chain and 99 switch. 489 of them have an init that declares nothing (`=`, `++`, a call), so condition 1 is
empty for them.

The 770 refused:

| refusal | blocks |
|---|---|
| the name also occurs only in sibling init blocks (the next `err`, the next `ok`) | 531 |
| the name occurs elsewhere in the enclosing block (a later declaration, a field or package of that name) | 195 |
| a closure in the block | 43 |
| a label in scope and a closure in the block | 1 |

The colliding names, with any Δ suffix stripped, are `err` (418), `ok` (170), then single letters (`t` 14, `c` 11, `i` 10,
`r` 10). Of the 726 blocks that collide, 415 meet the name before the block and 468 after it.

**Shown, not argued: the corpus built both ways and compared method by method.** The 3,981 qualifying blocks in the
standard library were dropped in a copy of the tree. Each `{` and `}` line was blanked, which keeps every line
number, so the position maps and caller-line records stay valid. One deliberate change was also planted, in
`io.WriteString`. Then both trees were built, Release and Debug, and every method body of all 361 package assemblies
was compared:

- **Release:** 220,630 bodies are byte-identical. 1,552 are equivalent: the same instruction stream once local slots
  are numbered by first use, branch targets are read as instruction indices and metadata tokens are read as the
  names they resolve to. Two differ.
  - `io.WriteString` is the plant, named as the control requires.
  - `flag.PrintDefaults`: with the braces gone, Roslyn's optimizer keeps the ref that `heap(...)` returns on the
    evaluation stack across one call instead of in a ref local. `heap` still runs once, before the call, and the
    ref is read after it. The Debug build settles it below.
- **Debug**, where Roslyn emits a `nop` per brace and schedules nothing on the stack. With the `nop`s stripped,
  219,480 bodies are byte-identical, 2,990 are equivalent and **one differs: the plant**. `flag.PrintDefaults` is
  equivalent here, so its Release difference is the optimizer's own choice over two programs Debug reads as one.
- **linux and darwin** compile the transformed tree too (`-p:GoTargetOS=linux` and `darwin`, Release), each with rc 0
  and 0 errors. IL was compared on windows only. Warning totals were not compared: each build's total counts only the
  projects that build recompiled.

The comparer's own controls: base against base reads every body identical (222,184 Release, 222,471 Debug), and the
plant is named in both configurations. Two of its drafts were wrong, and each was caught by a body it named:

- The first compared signature blobs as bytes. Reordered locals renumber TypeRef rows, so it read 104 Release bodies
  different. Decoding each blob to names took that to the 2 above.
- The second counted stripped `nop`s when it numbered branch targets, so it read 2,154 Debug bodies different, all
  with identical instructions.

**The cost.** To the generator, a predicate in `visitIfStmtChain` and the two switch emitters. It must run on the final
C# names, after Δ renames, capture snapshots (`ʗ`) and temps are minted, so it belongs beside
`performVariableAnalysis` (`variableAnalysisOperations.go`), which already knows every name a function declares. go/types
lets it be exact where the census is textual: a use that resolves to another object is condition 1's rebinding case.
To go2cs-gen and the analyzers: nothing; neither keys on the block. To performance: none on the windows corpus (above).

### (b) A pattern in the condition

| form | for | example |
|---|---|---|
| b1 | comma-ok, condition `ok`, `ok` used nowhere else, the value not used in the else (R9) | `if (w._<StringWriter>(ᐧ) is (var sw, true)) {` |
| b2 | comma-ok, condition `!ok`, the value not used in the body | `if (m[k, ꟷ] is not (var v, true)) {` |
| b3 | any `:=` init declared with `var` | `if (f() is var (n, err) && err != nil) {` |

b3 is universal for the `var` forms, because the `var` pattern is irrefutable and keeps every name assigned in every
branch (R10). b1 and b2 are the shorter comma-ok spellings, and they declare no `ok` at all.

**Semantics.** The pattern variables of an `if` condition leak to the enclosing block (R1, R2), so on a head `if`
shape (b) needs exactly predicate (a). On an **else-if** they stay in a nested scope (R3), the same nesting as the
`else` block they replace, so the shape adds no collision there. Typed declarations (`nint n = …`, `@string s = …`)
are out. The converter writes the type when the value's own type differs, and a type pattern tests the runtime type,
so `5 is nint x` is false. That would change behavior, so only `var` declarations qualify.

**The population.** 3,885 of the 4,889 if and else-if init blocks have a (b) form: 3,198 b3, 609 b1 and 78 b2. 505
`:=` inits are not in the plain `var` shape: typed (`nint` 259, `@string` 107, `unsafe.Pointer` 69), `ref` heap boxes
(13) and hoisted snapshots.

**The cost.** b1 and the dropped braces compile to the same 29 bytes as today, instruction for instruction. b3 adds one
local and 2 bytes of IL (the deconstructed value is stored before it is tested). What the JIT makes of that was not
measured here.

### (c) What goes further

- **golib's out-parameter assertion, which exists today.** `builtin.cs` already has
  `bool _<T>(this object target, out T? value)`, and it sets `GoZero<T>()` on failure exactly as the tuple form
  does. `if sw, ok := w.(StringWriter); ok` can read `if (w._<StringWriter>(out var sw)) {`, with no golib change,
  and the value stays usable in the else branch. It applies to 429 assertion blocks. Its call site is 19 bytes of
  IL against 29, with no tuple; the two golib overloads' own bodies were not timed.
- **The `ok` rescue.** b1, b2 and (c) never declare `ok`, so a block refused under (a) only because a sibling also
  declares `ok` has nothing left to collide. io.Copy's pair is the case:

  ```csharp
  // If the reader has a WriteTo method, use it to do the copy.
  // Avoids an allocation and a copy.
  if (src._<WriterTo>(out var wt)) {
      return wt.WriteTo(dst);
  }
  // Similarly, if the writer has a ReadFrom method, use it to do the copy.
  if (dst._<ReaderFrom>(out var rf)) {
      return rf.ReadFrom(src);
  }
  ```

  96 refused head-if blocks are rescued this way: 68 by (c), 25 by b1 and 3 by b2. That is a lower bound. Each
  rescue removes an `ok` from the enclosing block, which can free a sibling that was refused only because of it,
  and the census does not iterate.
- **A Δ rename for the remaining collisions.** The converter already renames a Go name that C# would see shadowing
  (`errΔ1`). Treating an init's names as declared in the enclosing block would rename the second sibling `err` to
  `errΔ1` and drop every brace except the 44 with closures. That is about 630 more blocks, at the cost of a renamed
  variable where Go has none. It reads less like Go than the braces it replaces, so it is offered, not recommended.
- **Merging sibling declarations** (the second `err :=` becoming `err = …`) was considered and is **not recommended**.
  It is correct only when the types agree and nothing captures either variable, and it turns Go's `:=` into `=`.
- **Scoping tricks:** a one-pass `for`, or a `switch` on the init's tuple, would scope the names without braces. Both
  are rejected: a Go `break` or `continue` in the body would bind to the new statement. A `switch` expression in the
  condition leaks like an `if` (R13) and gains nothing.

### (d) Else-if chains, switch init, and the fallback

- **An if heading an else-if chain** keeps its init visible in every branch whether or not the braces drop, as Go does.
  (a) covers 100 of the 113.
- **An else-if's own init** (47) cannot use (a): its braces are the `else`'s own. (b) and (c) give the flat Go chain
  for 41 of them:

  ```csharp
  } else if (node._<ж<ast.File>>(out var n)) {
      // use ast.File comments, if any
      p.comments = n.Comments;
  }
  ```

  The other 6 assign rather than declare (`(rsa, _) = syscall.Getpeername(…)`) and keep today's `else` block.
- **A switch init** takes (a) only: 99 of 110 qualify. The governing expression's variables leak (R13), so a pattern
  there buys nothing.
- **The type switch** (1, in go/types) is refused under (a): its `u` occurs twice more in the enclosing block.
- **The fallback is today's shape**: a block any predicate refuses keeps its braces, so it stays correct.

## 5. Recommendation

| population | shape | blocks |
|---|---|---|
| an if/switch/type-switch init the predicate admits | (a) dropped braces | 4,183 |
| an else-if's own `var` init | (c) for an assertion, else b1/b2, else b3 | 41 |
| a head if refused only for `ok` | (c) for an assertion, else b1/b2 | 96 |
| everything else | today's braces | 680 |

**4,320 of the 5,000 init blocks lose their braces (86.4%)**: windows 3,545 of 4,052 (87.5%), linux 3,624 of 4,177
(86.8%), darwin 3,599 of 4,149 (86.7%), goldens 226 of 260 (86.9%). (a) is the most Go-like spelling, since it keeps
Go's token order. (b) and (c) apply only where (a) cannot, and the Δ rename stays an owner choice.

**Risks, and how each is closed:**

- **Rebinding (R6)** and **closure scope (R8)**: closed by the predicate, by construction rather than by a later
  diagnostic. The windows IL comparison above is the evidence for (a).
- **The predicate must see the final C# names.** A temp, a Δ rename or a capture snapshot minted after the decision
  could collide unseen. Decide it where names are final, and keep the converter's own compile of the corpus as the
  backstop (R4, R5 and R7 are compile errors, not silent).
- **Position maps.** Each dropped block removes two lines, so the map of every touched file re-encodes. (a) and the
  else-if shapes alone touch 608 files on windows, 634 on linux and 629 on darwin; the rescue adds a few more. The train is a normal corpus regen, measured by the two-seeded diff.
- **Goldens.** 130 behavioral goldens change. They are re-minted after the converter change, with
  `check-no-regression.ps1` proving no other drift.
- **(b) and (c) were not corpus-built here.** Their rules are measured (section 3) and their IL is compared on the
  microbenchmarks, but no transformed corpus compiled them. The seat that lands them owes the same two builds and the
  same comparison as (a).

## 6. Limits of this scout

- The census predicate is textual, over Roslyn tokens. It is conservative: a type or member spelled like a local
  counts as a collision. The converter's version, with go/types, can only admit more, and it must be re-measured.
- The Go-side name uses are by name, not by object, so a nested shadowing declaration counts as a use. That is also
  conservative.
- IL was compared on windows. linux and darwin were compiled, not compared. The goldens were counted, not built.
- (b), (c) and the rescue are populations, not proofs: see the last risk above.

## 7. Reproducing it

Every instrument is in [`phase4/probes/h-init-scope-census/`](phase4/probes/h-init-scope-census/README.md), and its
README gives the commands. The census reads a checkout and `go env GOROOT`; the IL comparison needs two built trees.
