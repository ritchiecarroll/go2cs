# Draft: the short posts

> DRAFT for the owner's voice. The four short texts the plan's venues need besides the long posts: the
> GitHub Discussions announcement and the personal message (soft stage), the Gophers Slack post (soft
> stage, the day after), and the newsletter submissions (the Monday of the wide week). Each links the
> go2cs.net news post, which carries the detail. Figures in [square brackets] are read on the day.
> Edit each in your own words; never send the same text to two places.

## 1. GitHub Discussions, Announcements (soft stage, morning)

**Title:** go2cs on Go 1.26: try it and tell me where it breaks

The converted Go standard library is now on Go 1.26, published on NuGet as the `go.*` [version]
packages, and every package that can be validated passes Go's own tests again at the new release
([roster](https://go2cs.net/ValidatedTestPackages.html)). The converted modules on
[nugetgo.net](https://nugetgo.net) are rebuilt on it too.

If you have starred or watched this repository, you are the people I would most like to hear from. The
[news post](https://go2cs.net/NEWS.html) says what is new and what it still does not do. Try a Go package
you know, and tell me the first thing that confused you or failed: a defect as an
[issue](https://github.com/ritchiecarroll/go2cs/issues/new/choose), anything else here in Discussions.

Replies are best-effort; it is a personal project, and I read everything.

## 2. A personal message (soft stage, midday; to each person by name)

> Hi [name], I have finally got go2cs, my Go-to-C# converter, to a state I am happy to show: the Go
> standard library converts, and passes Go's own tests, on .NET. Would you spend an hour trying it on a
> Go package you know, and tell me the first thing that tripped you up? Not praise; the stumble.
> Start here: [news post link]. Thanks either way.

Pick people who will actually try it: one or two Go developers, one or two C# developers, someone who
has never seen the project. A dozen or two is plenty.

## 3. Gophers Slack, `#show-and-tell` (soft stage, the next day, 10:00 to 12:00 US Eastern)

Read the channel's pinned rules first. One post, answers in its thread, no cross-posting.

> go2cs converts Go source to C# that reads like the Go and behaves like it. The way it is checked may
> interest this channel: each standard-library package's own `_test.go` suite is converted and compared
> with `go test`, verdict for verdict, now at Go 1.26, with every difference disclosed by name. Goroutines
> are OS threads, so that is where it stops being Go; the limits are written up. A personal project, and
> I would value hearing which corner of Go you expect it to get wrong. [news post link]

## 4. Newsletter submissions (the Monday of the wide week)

Submit once to each, through the newsletter's own submission form or editor address; the editors decide.

**Golang Weekly**

> go2cs converts Go to readable C# and validates the result with Go's own test suites: every
> standard-library package's `_test.go` is converted, run on .NET and compared with `go test`, at Go
> 1.26. https://github.com/ritchiecarroll/go2cs

**One .NET newsletter** (choose one on the day: the owner's pick of the weekly .NET roundups)

> Use Go libraries from C# as ordinary NuGet packages: go2cs converts Go source to managed C#, and each
> converted package is checked against the original's own Go tests. Google's `uuid` is one
> `dotnet add package` away. https://nugetgo.net · https://github.com/ritchiecarroll/go2cs
