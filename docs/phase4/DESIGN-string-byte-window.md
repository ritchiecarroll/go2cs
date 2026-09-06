# DESIGN -- the string byte-window: candidate C of the `os` want-zero residue (segment 1)

> Drafted 2026-09-05 by lane G, proposed to the coordinator at mailbox `15a668bf2` and cut on that
> proposal. It is the THIRD step of the `os` row's retirement plan (B, then E, then C) and the plan
> the `TestUTF16Alloc` entry needs before it can be written as a legal `deferred` entry at all --
> which is why it is cut now rather than after E. Every price that depends on a landed seat is marked
> **re-read at the landing**. Nothing is cut by this record.

## 0. The site

Go's `os.(*File).WriteString` is two lines (`os/file.go:300`, go1.23.12):

```go
b := unsafe.Slice(unsafe.StringData(s), len(s))
return f.Write(b)
```

The idiom is Go's canonical zero-copy string-to-bytes VIEW: `StringData` takes the interior pointer
`&s[0]`, `unsafe.Slice` rebuilds a slice header over it, and no byte is copied. It is the last object
on the `os` want-zero row -- segment 1 of the residue decomposition.

The converted emission is faithful, construct by construct, and that is exactly why it allocates:

```csharp
var b = @unsafe.Slice(@unsafe.StringData(s), len(s));
```

`@unsafe.StringData(s)` ends in `Ꮡ(str.Slice(0, str.Length), 0)` -- and each half of that is already
correct on its own. `@string.Slice` returns `new slice<byte>(m_value, m_offset + start, ...)`: a
**zero-copy window over the string's own backing array**, a struct, allocating nothing. `@unsafe.Slice`
over a managed element box returns that window back, also without copying. What allocates is the
**element box in the middle** -- the `Ꮡ(window, 0)` that exists only to be immediately consumed by the
call that rebuilds the window it came from.

## 1. The population -- exactly two production sites, censused at the pin

`unsafe.Slice(unsafe.StringData(` over `go1.23.12`'s `src`, production files:

| site | package |
|:--|:--|
| `os/file.go:300` | `os` -- the row's own site |
| `hash/maphash/maphash_runtime.go:37` | `hash/maphash` |

**Zero** in `_test.go` files. The bare `unsafe.StringData(` census is wider (runtime 6, then single
sites in `syscall`, `os`, `log/slog`, `hash/maphash`, `go/types`, `crypto/x509/internal/macos`), but
those are not this idiom: they take the pointer for their own purposes rather than immediately
rebuilding a window over it, and the rule below does not touch them. This confirms E's §3 sizing
("population two in std") as measured rather than estimated.

## 2. The mechanism -- recognize the composite, emit the window that already exists

The rule is a converter-side pattern over ONE expression shape: `unsafe.Slice(unsafe.StringData(x),
len(x))`, where the two `x` are the SAME string expression and the length argument is `len` of that
same expression. Emit the window directly:

```csharp
var b = s.Slice(0, len(s));
```

Three properties make this the cheap member of the residue rather than another box-reduction arc.
**The target form already exists and is already correct** -- `@string.Slice` is the zero-copy view
over the string's own backing array, which is precisely what Go's idiom denotes, so there is no golib
change and no new API. **Nothing is minted**: no element box, no interface temp, no reconstruction.
And **the aliasing contract is preserved exactly** -- both forms produce a window over the string's
own storage, so a reader observing the string's bytes through `b` sees the same bytes at the same
addresses; Go forbids writing through it, and nothing about this changes what a write would do.

Refusals, each because the shapes are not the same expression: a differing length argument
(`unsafe.Slice(unsafe.StringData(s), n)`), two different string expressions, a `StringData` whose
result is stored, returned, compared or passed anywhere other than that one `unsafe.Slice` call, and
any use of `StringData` that is not immediately consumed by it. The census's wider `StringData`
population is exactly that refused class, and it stays on today's emission.

## 3. Prediction, on record

- **The `os` row (Release, tiering off, the same-tree A/B), AFTER B and E: 64.25 B / 1 obj → 0.25 B /
  0 obj** -- the bank condition, and the assertion `TestWriteStringAlloc` wants. Segment 1 is the
  last object. Falsifiers: any count other than 0; bytes above 0.25 with the count at 0 (a temp the
  read did not name); and any movement in the row's earlier segments, which this rule cannot reach.
- **`hash/maphash`** takes the same rule at its own site and is the second measurable consumer; its
  row is banked, so its verdict count must not move -- a changed count there is a falsifier, not a
  bonus.
- Corpus footprint: the two-seeded three-target diff confined to `os/<goos>/file.cs` and
  `hash/maphash/maphash_runtime.cs`, and nowhere else, because the refused class is everything else.
  **Re-read at the landing** -- the figures above assume B and E have landed; before them the same
  cut removes the same one object from a larger row.

## 4. Gates (if ruled)

The converter suite with the predicate's guard (the composite recognized; each refusal shape left on
today's emission, including the same-string-different-length case); the two-seeded three-target
`-stdlib` diff applied by hunk with its path set predicted first; CNR; `go2cs.slnx`; the `os` row's
own sweep and `hash/maphash`'s banked row at its banked count; the os-row A/B on this box against the
table above. No golib change, so the golib gate list is not owed -- which is what makes this the
cheapest of the three and the reason it is nonetheless LAST: it can only be measured once B and E
have taken the objects above it.

## 5. Why this record exists before its increment

`TestUTF16Alloc`'s `deferred` entry needs a plan that EXISTS (the owner's strengthening: an entry with
no executable plan is refused), and its string-materialization component is this family. The entry
references this record; the record's own increment runs in phase 4D behind B and E.
