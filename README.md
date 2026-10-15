# nugetgo wave 2: preparation (mousetrap, then cobra), and the post-hop wave plan

Prepared by C2 on 2026-10-14 at master `7ca22d951f` (go.* 1.24.13.7), linux/amd64, Microsoft .NET 10.0.12, so that the
post-hop packs are re-packs against the go.* 1.26.x record and not new work (COORD amendment, 2026-10-14). **No
release candidate was packed. Nothing here is published or publishable.**

| Path | What it is |
|:--|:--|
| `WAVE-PLAN.md` | the post-hop wave: next version per module under B3, the order, and the PIN sessions |
| `mousetrap/OWNER-SHEET-DRAFT.md` | mousetrap's owner sheet, drafted from pflag's; `<...>` values are filled at the re-pack |
| `mousetrap/Test-SheetPaste.ps1` | the pflag kit's paste test; run it with `-Path mousetrap/OWNER-SHEET-DRAFT.md` (93 submissions, 0 findings) |
| `mousetrap/rows/linux-x64-SCRATCH.md` | a real linux-x64 behavior row, written against a SCRATCH 1.1.0-int.1 from a local folder feed; it binds no release |
| `cobra/MODULE-prep.md` | cobra's section-1 MODULE.md at the prep tree (non-root, writable copy) |
| `wave/PIN1-SHEET.md` | S1: ONE owner sheet for PIN session 1: six packages signed in one process, pushed in wave order |
| `wave/Test-PIN1Gates.ps1` | S1's test: block 0 both ways, and block 3's gates against the published packages as stand-ins |
| `wave/Invoke-NugetgoWaveRepack.ps1` | S2: the wave driver, one tested module per run (runbook sections 1 and 2) |
| `wave/wave-modules.psd1` | S2's table: each module's published pack arguments and its kit's counts |

## Readings

mousetrap v1.1.0 (58 lines over two files; no Go tests upstream):
- PR conversion rc 0. The scratch int.1 pack (`-UnvalidatedReason`) chose the RID-specific layout: `lib/` and
  `runtimes/win-x64` carry the windows assembly; linux-x64, osx-x64 and osx-arm64 carry one identical assembly.
  Dependencies: go.lib, go.syscall, go.unsafe at `[1.24.13.7, 1.25.0)`. Host paths 0.
- The linux-x64 row: go and dotnet outputs equal (25 bytes), and the row's assembly is the packed linux-x64 dll.

cobra v1.10.2, section 1, non-root (CAP_DAC_OVERRIDE and CAP_DAC_READ_SEARCH dropped), from a writable copy:
- cobra 405 matched, cobra/doc 19 matched, 0 disclosed; 6 doc benchmarks and examples excluded (Phase 4D), as on
  2026-10-11. Both PR input digests equal their MODULE.md rows. VR 361 s.

## Findings for the post-hop packs

1. **The registry is not reachable from a cloud lane.** The egress policy refuses `nugetgo.net`, so PR's
   `-recurse=nuget` mapped nothing and converted pflag locally into `PR/pkg`. cobra's pack needs pflag (and, on the
   windows flavor, mousetrap) referenced as packages, so it runs on a box that reaches the registry, or with
   `-nuget-map <file>` naming a local copy of the registry rows.
2. **A writable copy has no module version**, so `-module-disclosures` is not consulted for it ("not a module-cache
   directory"). cobra has no committed disclosures, so its counts are unaffected; a module that has some must be
   read from the module cache, or the converter must learn the version another way.
3. **The nuget-map package cache is keyed by ID and version only.** A scratch int.1 left from 2026-10-12 (built for
   1.24.13.6) shadowed today's int.1 of the same version until its cache entry was removed. Reported before as low.
