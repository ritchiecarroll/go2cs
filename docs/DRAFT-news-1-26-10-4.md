# DRAFT — the go.* 1.26.10.4 release notes

<!-- DRAFT for owner and COORD review, started 2026-11-11 by seat C43 (P2) from master 50dd9a065c, on the .3 precedent
     (docs/DRAFT-news-1-26-10-3.md, b3ca4ef593). Nothing here is published. At release the entries below move to the top
     of docs/NEWS.md (dated), and the same text is the GitHub Release body. Each item names the committed record its
     facts are read from in an HTML comment beside it. -->

---

## <DATE>, 2026 — <TITLE>

<!-- One section per item, each owned by the seat that writes it, so the seats' merges touch separate lines (COORD,
     2026-11-11). At release the sections are merged into the entry's prose. -->

### Converting a module: default build tags (C43)

**Converting a module picks a library's pure-Go code over its assembly.** `go2cs -recurse`, in every form (local
references, `-recurse=module` and `-recurse=nuget`), now builds a module with the build tags `safe`, `purego` and
`noasm`, where it used `safe` alone, and prints them at the start of the run. A library that ships hand-written
assembly behind `!purego` or `!noasm` now converts its pure-Go version instead of methods that throw when called.
github.com/klauspost/cpuid/v2 is the case that prompted it: it ran its assembly CPU detection as the package
loaded, so every converted program that imported it, github.com/zeebo/xxh3 among them, stopped at startup. With
`noasm` it uses its pure-Go detection. An explicit `-tags` still replaces the default entirely, and
`-module-safe-tag=false` drops all three tags. None of the nine published nugetgo packages selects a different
file under the new tags, so none needs a rebuild for this change. See
[Build tags for a module](ConvertingAModule.md#build-tags-for-a-module).

A known limit: no tag removes assembly that a library gates on the architecture alone. Its declarations still
convert to methods that throw, and they stay reachable through code that forces the fast path on. xxh3's
vector routines are the case: hashing takes the portable path, because the converted cpuid reports no CPU
features, but xxh3's own tests switch the vector paths on and reach the throwing methods.
<!-- seat C43 (COORD, 2026-11-11), branch claude/p2-c43-recurse-tags. The default: src/go2cs/commandLineOptions.go
     defaultModuleBuildTags. The nine-package census, at both tag sets on windows/amd64, linux/amd64, linux/arm64,
     darwin/amd64 and darwin/arm64, and the cpuid/xxh3 control: docs/phase4/DATA-c43-recurse-tags.md. -->

### Some code instead of none: `go2cs -warn` (R)

**Some code instead of none: `go2cs -warn`.** A package that holds a site the converter refuses used to produce no
code at all. `-warn <code>` now downgrades the listed refusal to a warning, so the package converts and everything but
the refused site runs. It is opt-in per code and never a default, and a code is allowed only when every site it covers
fails loud when reached: the converted program panics there with the refusal text, rather than running on with a
silently wrong result. Today that is `GO2CS2002`, a generic instantiation whose self-referential constraint has a
method that cannot be forwarded (for example `Tree.Children` returning `[]N`). Each downgraded site is printed as
`warning GO2CS2002`, and every C# build of the project reports it again as the new `GO2CS0011`, so a project that is
never converted again still shows that it carries a known-broken path. See [`-warn`](CommandLine.md) and
[diagnostic codes](DiagnosticCodes.md).
<!-- the -warn seat (owner's .4, COORD 2026-11-10/11), branch claude/r-warn-option: src/go2cs/warnOption.go (the
     allow-list), src/core/golib/GoRefusalDowngradedAttribute.cs, src/gen/go2cs-gen/RefusalDowngradedAnalyzer.cs,
     behavioral RefusalDowngradeProxy. -->

### `-recurse` converts the rest of a module when one package is refused (R)

**`-recurse` converts the rest of a module when one package is refused.** One package the converter refused (a
`GO2CS2001`, `GO2CS2002`, `GO2CS2003` or `GO2CS2005` error) used to end a `-recurse` run, so every package queued after it
went unconverted. Now the refused package is skipped: its diagnostics still print as errors and it gets no project,
but every other package converts. The run still ends with exit 1, naming each refused package and each converted
package that imports one, since those will not build until the refused package converts. A package that fails to
convert (it does not load, or the converter panics) was already skipped, but the run used to exit 0; it now exits 1
too, so a script that checks the exit code sees it. A single-package conversion still stops at a refusal and writes
nothing. See [`-recurse`](CommandLine.md).
<!-- COORD rulings 20261111T053529Z and 20261111T060642Z, branch claude/r-warn-option: src/go2cs/recurseRefusal.go,
     moduleConverter.go convertAll / ConvertModule; the caller census in R's post 20261111T062125Z. -->

### A refusal is renumbered: `GO2CS2005` (R)

**A refusal is renumbered: `GO2CS2005`.** The refusals for a pointer type argument to a generic with a
non-self-referential method-set constraint (new in 1.26.10.4, alongside the conversions that now work for that shape)
are reported as `GO2CS2005` instead of `GO2CS2002`. Their sites cannot fail loud if downgraded, so `-warn` does not
accept `GO2CS2005`. A build script that matched `GO2CS2002` for those sites should match `GO2CS2005`.
<!-- branch claude/r-warn-option: src/go2cs/diagnosticOutput.go diagnosticNonSelfProxyRefused; docs/DiagnosticCodes.md. -->
