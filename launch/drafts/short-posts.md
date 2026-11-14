# Draft: the short posts

> DRAFT for the owner's voice, refreshed 2026-11-13 for the go.* 1.26.11.1 soft release. COORD reviews it for accuracy
> first; the owner edits last. The four short texts the venues need besides the long posts: the GitHub Discussions
> announcement and the personal message (soft stage, day 1), the Gophers Slack post (soft stage, a later day), and the
> newsletter submissions (wide stage). Figures in [square brackets] are read on the day from the page that states
> them. Edit each in your own words; never send the same text to two places.
>
> **Links.** go2cs.net resolves (NEWS.html, read by COORD 2026-11-13); its roster and Limitations pages are the site's
> own. sqlite-cs is public (created public 2026-11-12). typescript-go-cs is **live once public** (gate 12; still private
> on 2026-11-13). nugetgo.net and the Tour README resolved on 2026-11-13. Open each again before posting.
>
> **If cgo 3.1 misses the release** (gate 14), every "cgo" line below says Phase 2 only: calling a shared library's C
> functions with numbers, pointers and plain structs.

## 1. GitHub Discussions, Announcements (soft stage, day 1, morning)

**Title:** go2cs 1.26.11.1: Go 1.26.11, four platforms, and two real programs converted

go.* 1.26.11.1 is on NuGet: the Go standard library converted from Go 1.26.11 and checked against Go's own tests on
Windows, Linux (x64 and arm64) and macOS ([roster](https://go2cs.net/ValidatedTestPackages.html)). The
[news post](https://go2cs.net/NEWS.html) has the detail. What I would most like you to look at:

- **TypeScript's own Go compiler, converted to C#**:
  [typescript-go-cs](https://github.com/ritchiecarroll/typescript-go-cs) (live once public). It parses [17,422 of
  17,422] test files, and type-checks and emits [7,404 of 7,404] byte-identical to the native build.
- **SQLite, converted and not working yet**: [sqlite-cs](https://github.com/ritchiecarroll/sqlite-cs). It
  is in public so the walls are visible as I hit them.
- **cgo**: calling a C library's functions from converted code, plus `C.CString`, `C.GoString`, `C.GoBytes`,
  `void*` and `defer C.free`. What it does not do is in [Limitations](https://go2cs.net/Limitations.html).
- **This release breaks compiled code on purpose, while nobody depends on the old shape**: assemblies are named after
  their packages (`go.fmt.dll`), methods move to holder classes, and Go's `internal` packages are marked as internal.
  The news post's *Upgrading* section lists each break.

Goroutines are OS threads today; M:N scheduling is next.

If you have starred or watched this repository, you are the people I would most like to hear from. Try a Go package
you know with `dotnet tool install -g go2cs`, and tell me the first thing that confused you or failed: a defect as an
[issue](https://github.com/ritchiecarroll/go2cs/issues/new/choose), anything else here.

Replies are best-effort; it is a personal project, and I read everything.

## 2. A personal message (soft stage, day 1, midday; to each person by name)

> Hi [name], go2cs, my Go-to-C# converter, has reached a point I am happy to show. The Go standard library converts
> and passes Go's own tests on .NET, on Windows and Linux and for [234 of 236] packages on macOS, and TypeScript's Go
> compiler converts, with its type-check and emit output byte-identical to the native build. Would you spend an hour trying it on a Go package you know (`dotnet tool install -g go2cs`)
> and tell me the first thing that tripped you up? Not praise; the stumble. Start here:
> https://go2cs.net/NEWS.html. Thanks either way.

Pick people who will actually try it: one or two Go developers, one or two C# developers, someone who has never seen
the project. A dozen or two is plenty.

## 3. Gophers Slack, `#show-and-tell` (soft stage, a later Tuesday to Thursday, 10:00 to 12:00 US Eastern)

Read the channel's pinned rules first. One post, answers in its thread, no cross-posting.

> go2cs converts Go to C# that reads like the Go and behaves like it. Each standard-library package's own `_test.go`
> suite is converted and checked against `go test`, verdict for verdict, and every difference is disclosed by name.
> It is on Go 1.26.11, validated on Windows, Linux (x64 and arm64) and macOS.
>
> - What's new in 1.26.11.1: https://go2cs.net/NEWS.html
> - Validated, package by package: https://go2cs.net/ValidatedTestPackages.html
> - TypeScript's own Go compiler, converted (parse, type-check and emit byte-identical to native):
>   https://github.com/ritchiecarroll/typescript-go-cs
> - Go modules as NuGet packages: https://nugetgo.net
> - Work in progress, failing in the open: SQLite (modernc.org/sqlite): https://github.com/ritchiecarroll/sqlite-cs
> - The Go Tour beside its C#: https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md
> - Where it stops being Go: https://go2cs.net/Limitations.html
>
> Goroutines are OS threads today; M:N scheduling is next. It is a personal project, and I'd like to hear which corner
> of Go you expect it to get wrong.

*Notes for the owner, not part of the post:* "byte-identical to native" is the TypeScript repository's measured
claim (parse [17,422 of 17,422]; type-check and emit [7,404 of 7,404]); quote it only as that repository's README
states it on the day. The typescript-go-cs link is live once public; if it is not public yet, drop its line rather
than posting a dead link.

## 4. Newsletter submissions (wide stage, after M:N and one minor hop)

Not refreshed for the soft release: the wide stage comes after M:N (.5), and these are rewritten then, with M:N as
their news. Submit once to each, through the newsletter's own form or editor address; the editors decide.

**Golang Weekly**

> go2cs converts Go to readable C# and validates the result with Go's own test suites: every standard-library
> package's `_test.go` is converted, run on .NET and compared with `go test`, at Go [release].
> https://github.com/ritchiecarroll/go2cs

**One .NET newsletter** (the owner's pick of the weekly .NET roundups, on the day)

> Use Go libraries from C# as ordinary NuGet packages: go2cs converts Go source to managed C#, and each converted
> package is checked against the original's own Go tests. Google's `uuid` is one `dotnet add package` away.
> https://nugetgo.net · https://github.com/ritchiecarroll/go2cs
