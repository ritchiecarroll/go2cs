# DRAFT — the go.* 1.26.10.4 release notes

<!-- DRAFT for owner and COORD review, started 2026-11-11 by seat C43 (P2) from master 50dd9a065c, on the .3 precedent
     (docs/DRAFT-news-1-26-10-3.md, b3ca4ef593). Nothing here is published. At release the entries below move to the top
     of docs/NEWS.md (dated), and the same text is the GitHub Release body. Each item names the committed record its
     facts are read from in an HTML comment beside it. -->

---

## <DATE>, 2026 — <TITLE>

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
<!-- seat C43 (COORD, 2026-11-11), branch claude/p2-c43-recurse-tags. The default: src/go2cs/commandLineOptions.go
     defaultModuleBuildTags. The nine-package census, at both tag sets on windows/amd64, linux/amd64, linux/arm64,
     darwin/amd64 and darwin/arm64, and the cpuid/xxh3 control: docs/phase4/DATA-c43-recurse-tags.md. -->
