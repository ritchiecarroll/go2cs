# Draft: r/golang

> DRAFT for the owner's voice, for the wide stage, **optional** (decision D5, estimated Tue Dec 1, 09:00
> US Eastern), on go.* 1.26. Read the subreddit's rules on self-promotion and on AI-assisted projects on
> the day, and follow them over this draft; if the rules send project posts to a weekly thread, post
> there or not at all. A Go audience has no use for "now you can leave Go"; the angle is what Go's
> semantics look like on another runtime, and that Go's own tests are the judge. Figures in [square
> brackets] are read from the roster on the day.

**Title:** I converted the Go standard library to C# and ran Go's own tests against it. Here is what
passed and what could not.

---

I maintain go2cs, a tool that converts Go source to C#. I am not here to tell anyone to stop writing Go.
The use is narrower: a .NET shop that needs a Go library, without a sidecar process or a cgo wrapper.

What I think this subreddit may find interesting is the test method and what it turned up.

**The method.** For each standard-library package, the package's own `_test.go` suite is converted to
C#, built against the converted library, run, and compared with `go test -json`, test by test. A package
counts only when every test either matches Go's verdict or is disclosed, by exact failure, as something
the port does not do. At Go 1.26, [N of N] packages that can be validated pass on Windows and Linux. The
results are here, with a proof page per package: https://go2cs.net/ValidatedTestPackages.html

**What needed real work to match Go:**

- `defer`, `panic` and `recover`, including a deferred closure changing a named result.
- Value semantics for arrays and structs, and slices that alias their backing array as Go's do, down to
  `cap()` after an append on a slice Go's escape analysis keeps on the stack.
- Structural interfaces on a runtime with nominal ones. Source generators write the glue at compile time.
- `reflect`, enough for `encoding/json` and `text/template` to pass their own suites.
- `runtime` itself, where the port implements the behavior the tests observe.

**What could not match, and is disclosed:**

- Goroutines are OS threads. There is no M:N scheduler, so the ceiling is thousands, not millions, and a
  deadlock usually hangs where Go would report it.
- Tests that assert exact allocation counts, where Go's escape analysis does better than the port.
- Tests that look at Go's runtime internals, which do not exist in .NET.
- cgo, which is refused, and assembly, which the port avoids by building with `purego`.

It is usually slower than Go. The numbers are at https://go2cs.net/Performance.html and I have not hidden
the bad rows.

The full list of limits: https://go2cs.net/Limitations.html. The repository:
https://github.com/ritchiecarroll/go2cs

[One or two sentences, in the owner's words, on how the project is built: decision D6. On r/golang this
goes near the top, not the bottom.]

I would value hearing from anyone who knows a corner of Go's semantics that they expect this to get
wrong. That is exactly the report I want.
