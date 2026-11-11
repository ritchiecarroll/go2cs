# DRAFT — the go.* 1.26.10.4 release notes

<!-- DRAFT, held for release day, the .3 precedent (docs/DRAFT-news-1-26-10-3.md, moved into docs/NEWS.md by its release
     batch). Nothing here is published. At release the items below join the 1.26.10.4 entry at the top of docs/NEWS.md.
     Started 2026-11-11 with the -warn item (claude/r-warn-option); other 1.26.10.4 items join this file. -->

---

**Some code instead of none: `go2cs -warn`.** A package that holds a site the converter refuses used to produce no
code at all. `-warn <code>` now downgrades the listed refusal to a warning, so the package converts and everything but
the refused site runs. It is opt-in per code and never a default, and a code is allowed only when every site it covers
fails loud when reached: the converted program panics there with the refusal text, rather than running on with a
silently wrong result. Today that is `GO2CS2002`, a generic instantiation whose self-referential constraint has a
method that cannot be forwarded (for example `Tree.Children` returning `[]N`). Each downgraded site is printed as
`warning GO2CS2002`, and every C# build of the project reports it again as the new `GO2CS0011`, so a project that is
never converted again still shows that it carries a known-broken path. See [`-warn`](CommandLine.md) and
[diagnostic codes](DiagnosticCodes.md).

**`-recurse` converts the rest of a module when one package is refused.** One package the converter refused (a
`GO2CS2001`, `GO2CS2002`, `GO2CS2003`, `GO2CS2005` or `GO2CS2006` error) used to end a `-recurse` run, so every package queued after it
went unconverted. Now the refused package is skipped: its diagnostics still print as errors and it gets no project,
but every other package converts. The run still ends with exit 1, naming each refused package and each converted
package that imports one, since those will not build until the refused package converts. A package that fails to
convert (it does not load, or the converter panics) was already skipped, but the run used to exit 0; it now exits 1
too, so a script that checks the exit code sees it. A single-package conversion still stops at a refusal and writes
nothing. See [`-recurse`](CommandLine.md).

**A refusal is renumbered: `GO2CS2005`.** The refusals for a pointer type argument to a generic with a
non-self-referential method-set constraint (new in 1.26.10.4, alongside the conversions that now work for that shape)
are reported as `GO2CS2005` instead of `GO2CS2002`. Their sites cannot fail loud if downgraded, so `-warn` does not
accept `GO2CS2005`. A build script that matched `GO2CS2002` for those sites should match `GO2CS2005`.

**A new refusal: `GO2CS2006`.** A generic call that passes a `[]*T` where the callee declares `[]N` (`N` constrained
by a method-set interface) is converted by copying the slice element by element, so a callee that writes the slice
(assigns an element, appends into it, sorts it, or hands it to something that does) wrote the copy, and the caller
never saw the change, with no diagnostic. go2cs now refuses such a call by name, naming the callee's first write, and
converts it as before when the callee only reads the slice. No standard-library package is refused. `-warn` does not
accept `GO2CS2006`, since a downgraded site would run on silently wrong. See [diagnostic codes](DiagnosticCodes.md).
