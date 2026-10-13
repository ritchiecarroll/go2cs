# DRAFT: the 1.24.13.7 NEWS entry

> **State: DRAFT, 2026-10-13, HELD until the 1.24.13.7 train's record is on master.** Drafted from the train's
> rows as they stand at its union on that day (`claude/coord-trainT4-union` at `1438ed20ea`). The train is
> being re-assembled for one converter fix, so anything below marked **[CONFIRM]** is re-read at the landed
> record before this goes into `docs/NEWS.md`.
>
> **What applying it takes, on the release day:** the entry below goes into `docs/NEWS.md` above the
> October 12 entry, with the real date in its heading; the README's featured block takes the teaser at the
> end of this file; `launch/drafts/` on `claude/launch` gets the release body made from it; and the roster
> guard is run, because that block is guarded. The heading's date must equal the day the packages publish.
>
> **Where each paragraph comes from** is in the comment at the end of this file.

---

## [RELEASE DAY], 2026 — Converted code keeps Go's comments and spacing, and HTTPS works on macOS

**NuGet 1.24.13.7.** This is the last release built from Go 1.24. The next one moves the converted
standard library to Go 1.26.

**Comments and blank lines land where Go put them.** Converted C# is meant to read like the Go it came
from, and until now its comments and spacing did not always follow. A type's doc comment could be
missing, a comment inside a function could land after the code it described, and the blank lines that
group statements in a Go function were dropped or doubled. This release fixes those. One real example,
from the converted `internal/fmtsort`. The Go:

```go
// SortedMap is a slice of KeyValue pairs that simplifies sorting
// and iterating over map entries.
//
// Each KeyValue pair contains a map key and its corresponding value.
type SortedMap []KeyValue
```

Before, the declaration stood alone, with no comment:

```csharp
partial struct SortedMap /*[]KeyValue*/;
```

Now:

```csharp
// SortedMap is a slice of KeyValue pairs that simplifies sorting
// and iterating over map entries.
//
// Each KeyValue pair contains a map key and its corresponding value.
partial struct SortedMap /*[]KeyValue*/;
```

What changed, by kind:

- **Blank lines inside a function body** follow Go's, in every conversion.
- **Doc comments** stay with the type, alias or declaration group they document.
- **Comments inside expressions**, in a composite literal, between arguments, or beside an operand, are
  written at their Go position.

Comments are carried when you convert with `-comments`, as the converted standard library is. The blank
lines apply to every conversion. Nothing that runs is different: the change is in comments and
whitespace, and every validated package holds its count. **[CONFIRM at the landed record.]**

**A multi-line raw string keeps its exact bytes.** A Go raw string literal that spans lines now converts
to a C# raw string literal, closed by a call that restores Go's rule for such literals. The string has
the same bytes as in Go however the file's line endings were checked out.

**HTTPS on macOS.** Verifying a certificate against the system's roots reached an unimplemented stub on
macOS, so an ordinary `https` request from a converted program failed there, on Intel and on Apple
silicon. It now calls the system's own verification, as Go does.

**Native AOT, fully trimmed.** Three defects that showed only in a Native AOT publish with `TrimMode` set
to `full` are fixed. `fmt` finds a named type's `String`, `Error`, `Format` and `GoString` methods again,
so a `time.Duration` prints as `1.5s` and not as a count of nanoseconds; that was the defect 1.24.13.6
shipped with. Reading a struct's fields by reflection through a pointer works. And a field promoted from
an embedded struct is the same field whichever way it is reached **[CONFIRM the second and third
against the landed rows: this draft read them from the rows' one-line descriptions]**. The price is size again, and the choice is the same as before: behaving like Go comes first.
Measured on `win-x64`, a fully trimmed C# program that references the packages is about 26% larger than
it was in 1.24.13.6. The default publish was not
affected by any of the three.

**Starting a goroutine is cheaper.** The runtime library recorded where each goroutine was started by
walking the whole call stack. It now resolves that once for each `go` statement. Code that starts
goroutines from deep recursion was the worst case, and is the one that improves most.

**Converted modules.** Packages published through [nugetgo.net](https://nugetgo.net) can now carry
separate assets for each platform, as the `go.*` packages do, for a module whose Go source differs by
operating system.

**Also in this release.** The [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md)
pins the converter and package versions it runs with. The README says plainly what to expect from the
project: it is one person's work, replies are best-effort, and it says where to ask.

The `go.*` packages are built and versioned together, so use one version of them in a project.

---

## The README's featured block (the teaser)

```markdown
## 📰 NEWS — Converted code keeps Go's comments and spacing, and HTTPS works on macOS

**[RELEASE DAY], 2026, NuGet 1.24.13.7.** Converted C# now carries Go's doc comments, in-function
comments and blank lines where Go put them. An `https` request from a converted program works on macOS.
Three defects in fully trimmed Native AOT publishes are fixed, including the one that printed named
types raw. This is the last release built from Go 1.24; the next moves to Go 1.26.
[Full announcement](NEWS.md#[the anchor of the dated heading]).
```

## To remove from Known issues when this lands

The section "Under a fully trimmed Native AOT publish, fmt prints some named types raw" says its own
comment: delete it when a release carries the fix. 1.24.13.7 does.

<!-- Provenance, 2026-10-13, at claude/coord-trainT4-union 1438ed20ea over master 226afb810a.
     Comment parity: row claude/c1-comment-parity-g (seats A to G); the example is
       src/core/internal/fmtsort/sort.cs at master (line 17, bare) against the union (lines 20 to 24), and
       GOROOT go1.24.13 src/internal/fmtsort/sort.go lines 20 to 24. utf8.cs, which the brief named, does not
       differ between master and this union, so it shows nothing; fmtsort does.
       Held for the first train after the hop, so NOT claimed here: one-line functions (seat K) and aligned
       trailing comments (seat J).
     Raw strings: row claude/p2-rawstr-fix (golib Raw(), convBasicLit.go).
     macOS: row claude/c1-darwin-x509-roots (behavioral X509SystemVerify, red first on darwin).
     Full trim: row claude/g-promoted-field-identity, the chain ec6df12120 (method-set keep, the Stringer fix),
       864bf27871 (field-box tables), 0726750708 (promoted-field identity). The size cost is in the ledger as
       Size, from COORD 2026-10-13 (win-x64, full trim, the consumer program): the field-box tables took it
       from 30,568,960 to 38,228,480 bytes (+25.1%); promoted-field identity adds 207,872 bytes (+0.54%);
       about +26% combined over the pre-train base. Stated once in the entry, with its platform.
       Raw strings stay in the entry (COORD, the same message). Still marked CONFIRM: the roster counts at
       the landed record, and the two full-trim fixes beyond the Stringer one, whose user-visible effect G is
       to state.
     Goroutines: row claude/g-golib-creator-o1.
     Modules: row claude/c2-nugetgo-rid-packs.
     Tour pin and README line: on master already (3e918939fc merge; 226afb810a).
     Not in the entry, tooling only: the hop readiness rows, the retired-hand-own guard, the H5c instrument. -->
