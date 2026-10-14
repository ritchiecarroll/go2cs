# Draft: the GitHub Release body for 1.24.13.7

> DRAFT. Everything below the line is the release body, ready to paste or to pass to
> `gh release create nuget-1.24.13.7 --notes-file`, or to set as the notes of the draft Release the
> release-binaries workflow makes for the tag. It is the 1.24.13.7 NEWS entry
> (`claude/r-news-1-24-13-7-apply`) with its links made absolute, plus an install section. If the NEWS
> entry changes in review, regenerate this from it. **Release title:** `go2cs 1.24.13.7: Converted code keeps Go's comments and spacing, and HTTPS works on macOS`

---

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
whitespace, and every validated package holds its count.

**A multi-line raw string keeps its exact bytes.** A Go raw string literal that spans lines now converts
to a C# raw string literal, closed by a call that restores Go's rule for such literals. The string has
the same bytes as in Go however the file's line endings were checked out.

**HTTPS on macOS.** Verifying a certificate against the system's roots reached an unimplemented stub on
macOS, so an ordinary `https` request from a converted program failed there, on Intel and on Apple
silicon. It now calls the system's own verification, as Go does.

**Native AOT, fully trimmed.** Two defects that showed only in a Native AOT publish with `TrimMode` set
to `full` are fixed. The default publish and programs running under the JIT never had them.

- **`fmt` calls a value's own methods again.** A type's Go methods are no longer trimmed away when they
  are reached only at run time, so `fmt` calls `String()` or `Error()` and no longer prints the raw
  value. A `time.Duration` prints as `1.5s`, not as a count of nanoseconds. This is the defect 1.24.13.6
  shipped with and listed under Known issues.
- **Reflection reaches a struct's fields through a pointer.** Reading or setting a field by reflection on
  a struct reached through a pointer, with `reflect.ValueOf(&v).Elem().Field(i)` or `FieldByName`, and
  taking that field's address, now works. In 1.24.13.6 it stopped with a `PlatformNotSupportedException`.

The price is size again, and the choice is the same as before: behaving like Go comes first. Measured on
`win-x64`, a fully trimmed C# program that references the packages is about 26% larger than it was in
1.24.13.6.

**Two pointers to one field are equal, however the field was reached.** A field promoted from an embedded
struct can be named two ways, `&s.X` and `&s.Inner.X`. The two pointers now compare equal, as they do in
Go. Before, they could compare unequal. This one applied to every program, under the JIT as well.

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

## Install

The packages are on NuGet: the converted Go standard library as
[`go.*`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), the runtime as
[`go.lib`](https://www.nuget.org/packages/go.lib) and the source generators as
[`go.gen`](https://www.nuget.org/packages/go.gen), all at 1.24.13.7. A conversion made with
`go2cs -recurse=nuget` references them for you.

**Binaries.** The converter is attached below for six platforms (`win-x64`, `win-arm64`, `linux-x64`,
`linux-arm64`, `osx-x64`, `osx-arm64`), with `SHA256SUMS`. Each was built by the repository's own workflow
from this release's record commit, which `gh attestation verify <file> --repo ritchiecarroll/go2cs`
confirms, and each names 1.24.13.7 in `go2cs -version`. They are not code-signed. Converting still needs
Go 1.24.13 and the .NET 10 SDK on your machine, and a minimal Linux also needs git, curl, ICU and OpenSSL:
see [Requirements](https://go2cs.net/#requirements). Or build the converter yourself:
[Building go2cs from source](https://go2cs.net/BuildingGo2cs.html).

**Full announcement:** [go2cs News](https://go2cs.net/NEWS.html). **Limits:**
[Limitations](https://go2cs.net/Limitations.html) and [Known issues](https://go2cs.net/KnownIssues.html).
