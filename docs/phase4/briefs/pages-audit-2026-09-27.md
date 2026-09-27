<!-- Report of a multi-agent audit (2026-09-27) of a REAL GitHub Pages build of docs/ at master 4fb6e460c6 (the scratch branch claude/coord-pages-check runs actions/jekyll-build-pages@v1). 43 findings survived an independent refutation pass; 0 were refuted. Counts are measured, not estimated. One owner-only item is withheld. -->
# go2cs GitHub Pages deployment audit

Scope: the site built from `b912b7d8cd`, whose `docs/` tree is identical to master `4fb6e460c6` (tree `30845e060f`). I compared it with the source at `H:/go2cs-tmp-coord/pages-fix` and with the live site. Built-site paths below are under `site-b912/` in the scratchpad. Source paths are under `docs/` unless another path is given.

## 1. Verdict

The failed run you linked (36330044561, job 108650253970) broke on one Liquid token at `ConversionStrategies.md:8760`. Commit `4fb6e460c6` fixed it. Run 36341195472 deployed that fix at 18:38:11Z with 0 Liquid warnings and 2164 pages. go2cs.net now serves `4fb6e460c6`, not `02b1b58793` as the brief assumed. The site was stale for about 3 hours.

The build is healthy now. What remains is older content and layout damage that nothing checks before deploy. The three largest problems:

- **Pages cut off after a few sections.** On 5 pages kramdown prints the rest of the page as raw text, including the Go-hop runbook `GoCorpusMigration.md`. The cause is a wrapped code span that leaves `<placeholder>` at the start of a line.
- **Three broken layout links on almost every page.** The layout's three relative URLs break on 2137 of 2163 pages.
- **Links out of `docs/` go to a 404.** 85 links climb out of `docs/`; 38 of them came in today with `1dae85e093`.

One item is an owner-only security settings item (reported to the owner directly; details withheld from this public record until it is fixed). No CI runs the new Liquid guard. Most fixes are small source edits. About half need an owner decision, because they touch frozen snapshots, records, DNS or site-wide settings.

## 2. Issues by severity

| # | Issue | Severity | Measured scope | Broken on github.com? | Fix | Owner decision? |
|---|---|---|---|---|---|---|
| 1 | A line starting with `<placeholder>` makes kramdown print the rest of the page raw | visitor-broken | 18 source lines on 10 pages. 5 pages lose their tail and 127 headings. 128 of 144 broken fragments. 30 tables. | No | Move the line break so no line starts with `<name`. Add a guard. | Yes for records and JOURNAL. No for the runbook and reference pages. |
| 2 | Layout URLs `css/octicon.css`, `SECURITY`, `coding-style` are page-relative | visitor-broken | 6411 refs on 2137 pages. The icon shows on 10,007 headings. | No | Use `relative_url` at `default.html:27,56` | No |
| 3 | Withheld: an owner-only security settings item (reported to the owner directly; details withheld from this public record until it is fixed) | security | n/a | n/a | owner | Yes |
| 4 | No docs check before the Pages deploy | process gap (filed as build-breaking) | Every docs push. 3 h stale site. 17 unread Liquid warnings in 7 green logs. | n/a | `docs-site.yml` workflow. Ship the Go guard job first. | Yes (CI) |
| 5 | Links that climb out of `docs/` (`../src/...`, `../CLAUDE.md`) | visitor-degraded | 85 hrefs on 20 pages, 48 targets. 91 once #1 is fixed. | No | Rewrite them in the layout to `blob/<build_revision>/` | No |
| 6 | Bare `<T>` or `<placeholder>` in prose | visitor-degraded | 175 regions on 101 pages. 50 show raw Markdown on 30 pages. Visitor pages: 28 on 17. | Yes, the token disappears there (78 tokens on 18 visitor pages) | Code spans. Escape in the proof-page generator. Real date in `Roadmap.md:8`. | Partly (records) |
| 7 | Tables render as pipe text | visitor-degraded | 40 tables on 10 pages. 30 are inside #1; 10 are on 8 other pages. | No | Blank lines; balance backticks | Frozen copies only |
| 8 | `\|` inside a table code span shows its backslash | visitor-degraded | 73 spans on 24 pages. 56 wrong and fixable. 12 correct on the site and wrong on GitHub. | Yes for the 12 grep spans | `<code>` with `&#124;` | Records |
| 9 | A wrapped line starting `N. ` becomes a list | visitor-degraded | 8 lists on 8 pages (5 living) | No | Rewrap | Frozen copies only |
| 10 | A line containing `\|` becomes a one-row table | visitor-degraded | 7 tables on 7 pages | No (one line degrades on GitHub) | Escape, code span, or header row | No |
| 11 | Fences inside list items misparsed | visitor-degraded | 2 pages, 4 fences, 1 H2 lost, 2 fragments | No | Blank lines | No |
| 12 | Link text wrapped across lines is not rewritten; the link serves raw `.md` | visitor-degraded | 4 hrefs on 2 pages; 5 source sites | No | Move the break before `[` | No |
| 13 | Legacy reference map and README index: wrong slugs and damaged labels | visitor-degraded | 14 broken fragments; 19 of 417 labels | Yes (the 14) | Fix 7 ids; restore code-span labels | No |
| 14 | Misrooted links in phase3 and phase4 records | visitor-degraded | 12 hrefs on 6 pages; 14 in source | Yes | Path corrections | Record edit |
| 15 | The journal's repo-root links | visitor-degraded (low exposure on the site) | 20 hrefs on 1 page | Yes | Exclude the page from the site, or exempt it | Yes |
| 16 | Validation proof links to package paths the 1.24 hop renamed | visitor-degraded | 68 links on 60 pages; 1157 `master` links will rot the same way | Yes | Retarget the links and amend `Update-FrozenProofPage` together | Partly |
| 17 | Pages whose first line is not the H1 get the default title | visitor-degraded | 8 pages | No | Put the H1 first; extend the guard | No |
| 18 | Probe folder cited by DESIGN-zh-box-b1 was never merged | visitor-broken (low impact) | 1 href | Yes | Land it from `f632a942bb`, or link its tree | Yes |
| 19 | `validation/current` has no index | cosmetic (1 link) | 1 href | No | Permalink page outside `current/` | Yes |
| 20 | Heading permalinks have no accessible name | cosmetic | 10,471 anchors on 2163 pages | No | `anchorAttrs` | No |
| 21 | The 3 frozen rosters keep the default title | cosmetic | 3 pages | No | `_config.yml` defaults | Yes |
| 22 | No custom 404 page | cosmetic | Every 404 (4393 broken links) | No | `docs/404.md` | Optional |
| 23 | Banner repeats the H1; one description for every page; no `og:image` | cosmetic | 2163 pages | No | `site.title` in the banner; defaults | Yes |
| 24 | kramdown typesets `--`, `---`, `...` and quotes | informational | 69 heading ids on 20 files; 12 CLI flags on 7 pages | No | `gfm_quirks` | Yes |
| 25 | Every `.md` source is also published raw | informational | 2163 files, 36,968,111 B (21.8% of the site) | No | `remove_originals` for two plugins | Yes |
| 26 | DarkReader 4.9.31 from a CDN with no SRI | informational | 2163 pages | No | `darkreader.js` with SRI | Yes |
| 27 | Remote theme fetched unpinned at HEAD | informational | Every build | No | `theme: jekyll-theme-cayman` | No |
| 28 | Layout header claims it is Gemstone shared content | informational | 1 file | No | Replace the comment | No |
| 29 | Proof page titles carry no version | informational | 1914 pages share 233 titles | No | Snapshot name in the layout | Yes |
| 30 | Evidence files published with no inbound link | informational | 91 files (2.31 MiB); 34 in 3 folders with no pages | No | `exclude` | Optional |
| 31 | Build time growth | informational | 63.9 s to 137.2 s in a week; about 16 to 20 snapshots of headroom | No | Markdown byte ceiling guard | No |
| 32 | Every master push rebuilds the site | informational | 85 of 299 builds had no docs change | No | `build_type: workflow` | Yes |
| 33 | Node 20 warnings | informational | Pages workflow (GitHub-owned) and the repo's workflows | No | Bump action majors | No |

Items 14, 15, 18 and 19 account for 34 broken in-site refs: 20 in the journal, 9 in phase3, 3 in phase4, 1 probe and 1 `../current`.

## 3. Details and fixes

### 1. A line starting with `<placeholder>` swallows the rest of the page

This merges three lens findings: anchors, rendering-fidelity and build-health.

**Cause**
- In GitHub's renderer a code span can cross a line break, and an HTML block cannot interrupt a paragraph.
- kramdown ends the paragraph at any line that starts with `<name`, where name is not a span element, and opens a raw HTML block there.
- With no closing tag, it treats everything to the end of the page or list item as raw text. Headings inside it get no id.
- Source: kramdown REL_2_4_0 `parser/kramdown/paragraph.rb:20-25` (LAZY_END_HTML_START) and `parser/html.rb:180-187` (parse_raw_html).

**Evidence**
- Built `ConversionStrategies-Reference/manual-conversions.html:1296-1297`: the paragraph closes at `` `//go:linkname</p> ``, and the next line is `` <thisFunc>` handle. ``.
- `GoCorpusMigration.html:1403-5013` is raw text, and the footer is at :5016.
- GitHub renders these pages in full. Headings on GitHub vs on the site: GoCorpusMigration 74/20, manual-conversions 63/13, interfaces 44/31, CENSUS-preservation 13/5, hopA-time-prestage 4/2.
- 102 broken fragments point into manual-conversions and 26 into interfaces. All 128 point at headings after the break.
- Raw ``` fences visible on the site: 74 on GoCorpusMigration, 84 on manual-conversions, 22 on interfaces.
- The live site shows the same. It is older than `4fb6e460c6`.

**The 18 trigger lines**

| File | Lines |
|---|---|
| `GoCorpusMigration.md` | 1075; 1276 (blockquote `> <incoming-sha>`); 2472 |
| `ConversionStrategies-Reference/manual-conversions.md` | 1101 |
| `ConversionStrategies-Reference/interfaces.md` | 1318 |
| `phase4/CENSUS-preservation-2026-09-12.md` | 123 |
| `phase4/hopA-time-prestage.md` | 24 |
| `doctrine/JOURNAL-2026-09-12.md` | 455, 614, 1559, 3471, 3813, 6126, 6476 |
| `phase4/BOARD-next-validation-candidates.md` | 19737 |
| `phase4/DESIGN-managed-pointer-token.md` | 754 |
| `phase4/Phase4-Autonomous-Loop-Charter.md` | 459 |
| `phase4/SESSION-ROLL-2026-09-01-EVENING.md` | 170 |

The 11 lines in list items lose no heading. They still publish long raw stretches, for example `JOURNAL-2026-09-12.html` lines 592-781 and 4045-4336.

**Fix**
- Move the line break earlier so the code span starts the next line. That keeps the same bytes and the same line count. Example for `manual-conversions.md:1100-1101`: `**one-argument**`, then a newline, then `` `//go:linkname <thisFunc>` handle. ``. GitHub output does not change, because a soft break renders as a space.
- `GoCorpusMigration.md:2472` must be fixed too. Otherwise lines 2472-4057 stay raw after 1075 is fixed. Fix `:1276` as well: once the others are fixed, it swallows lines 1277-1320 on its own.
- `Charter.md:459` `<class>` is plain prose, not inside a code span. Write it as `\<class>` or in backticks.

**Guard**
Add a sibling test next to `src/go2cs/internal/repoguard/docsJekyllLiquid_test.go`. It fails on a line that meets all of these conditions:
- it is outside fenced code, including fences inside blockquotes;
- after stripping `>` and list or indent prefixes, it matches `<name` followed by attributes or `>`, where name is not a kramdown span element;
- the line before it, stripped the same way, is non-blank text.

The test must name file:line. On today's tree this rule hits exactly these 18 lines. Exempt the `frozenSnapshot` paths. Positive control: undo the rewrap at `manual-conversions.md:1101` and the guard must name that line.

**Owner decision**
- Seven lines are in `doctrine/JOURNAL-2026-09-12.md`. Its blob identity is enforced by `contextBudget_test.go:75,284`. Either allow a whitespace-only rewrap there, or exempt the file in the guard.
- One line is in the append-only BOARD. Five more are in other phase4 records.
- `GoCorpusMigration.md` is a runbook and can be amended in place.

### 2. Layout relative URLs break on nested pages

**Cause**
- `docs/_layouts/default.html:27` reads `href="css/octicon.css"`.
- Line 56 reads `href="SECURITY"` and `href="coding-style"`.
- They were written when every page lived at the root.

**Evidence**
- Broken on 201 pages at depth 1, 1921 at depth 2 and 15 at depth 3. They resolve on the 26 root pages.
- Live: `/phase4/css/octicon.css` returns 404 and `/css/octicon.css` returns 200.
- Without the stylesheet, the anchor icon is always visible, filled black on a dark background, and the heading margins are lost.

**Fix** (in `docs/_layouts/default.html`)
```html
<link rel="stylesheet" href="{{ '/css/octicon.css' | relative_url }}">
```
```html
<span class="site-footer-credits"><a href="{{ '/SECURITY.html' | relative_url }}">security policy</a>&nbsp;&nbsp;&bull;&nbsp;&nbsp;<a href="{{ '/coding-style.html' | relative_url }}">coding style</a></span>
```

Optional changes:
- `{{ '/images/go2cs.ico' | relative_url }}` at :25 and `{{ '/' | relative_url }}` at :39.
- Writing the footer with `{% link SECURITY.md %}` makes a rename fail the build instead of producing a silent 404.

**Guard:** every `href` or `src` in `docs/_layouts/*.html` and `docs/_includes/*.html` must start with a scheme, `#`, `/` or `{{`.

### 3. Withheld

An owner-only security settings item (reported to the owner directly; details withheld from this public record until it is fixed).

### 4. No docs check before the Pages deploy

**Evidence**
- The Liquid break was introduced by `9b99543526` at 10:27:10 -0500 and merged by direct push 6 minutes later as `1dae85e093`.
- Branch protection: classic protection returns 404. Ruleset 19315293 has only `copilot_code_review`, so there are no required checks.
- `licensing.yml:5-28` path filters have no `docs/**`, and `:44` runs only `^TestLicensing`.
- `dco.yml` triggers on pull_request only, and PRs are not used (the last one is #31, from 2026-06-27). `os-matrix.yml` has no `go test`.
- `TestDocsSurviveJekyllLiquid` (`docsJekyllLiquid_test.go:181`) therefore runs in no workflow.
- 17 Liquid warnings sat unread in each of 7 green logs.

**Fix** (owner approval for a CI addition)
- Ship a cheap Go-guard job first: `go test ./internal/repoguard -run Liquid -count=1`. It runs in 0.76 s, and a positive control on the `1dae85e093` blob named line 8760.
- Then add `.github/workflows/docs-site.yml`:
  - Triggers: `push` on all branches plus `pull_request`, both with `paths: [docs/**, .github/workflows/docs-site.yml, src/go2cs/internal/repoguard/docsJekyll*]`, plus `workflow_dispatch`.
  - `branches-ignore: [claude/mailbox, claude/coord-handover]`.
  - `permissions: contents: read`, and a concurrency group per ref.
  - Steps: checkout (a Node-24 major), the Go guard, `actions/jekyll-build-pages@v1` with `source: ./docs`, then a site checker. Keep the checker under `.github/scripts/`, never in `docs/`, or it gets published.
  - Upload `_site` on failure with a short `retention-days`; the site is 173 MB.
- Baseline by (page, url, class) identity, not by total count. The layout class grows by 3 per new nested page, and master added 542 nested pages in a week. Fix #2 first so that class baselines at 0.
- Keep the outside-site class report-only until an owner ruling.
- Before merging a `docs/**` change, read `gh run list --commit <tip> --workflow docs-site.yml --json conclusion`.
- Template: `pages-check.yml` on the scratch branch `claude/coord-pages-check` (still on origin; delete it after copying). Update its action pins.
- Cost: about 164 s per docs push. Runners are free on this public repo.
- Larger alternative: `build_type: workflow`, deploying only when the checks pass (see #32).

### 5. Links that climb out of `docs/`

**Cause**
- `docs/` is both the Pages root and a folder in the repo.
- `docs/X.md -> ../src/...` is correct on GitHub. On the site the browser clamps it at the root, so it becomes `go2cs.net/src/...`, which is never published.
- `jekyll-relative-links` passes these through unchanged.

**Evidence**
- `Architecture.html:59 href="../CLAUDE.md"`, `ConversionStrategies.html:1129 href="../src/core/golib/builtin.cs"`, `index.html:731`.
- Every one climbs exactly one level above the site root, and all 48 targets exist in git.
- `ConversionStrategies.md` had 1 such link at `02b1b58793` and has 39 at `1dae85e093`. The 39 are live now.
- 6 more are hidden inside raw regions (`manual-conversions.md:1442,1487,1555,1675`; `GoCorpusMigration.md:1212,1687`).

**Fix:** replace `docs/_layouts/default.html:50` with the block below. Keep the existing `anchorBody` string unchanged. The `anchorAttrs` from #20 goes on the same include.
```liquid
{%- assign repo_up = page.path | split: "/" | size -%}
{%- capture to_repo_root -%}href="{% for i in (1..repo_up) %}../{% endfor %}{%- endcapture -%}
{%- capture repo_blob -%}href="{{ site.github.repository_url }}/blob/{{ site.github.build_revision | default: 'HEAD' }}/{%- endcapture -%}
{%- assign body = content | replace: to_repo_root, repo_blob -%}
      {% include anchor_headings.html html=body beforeHeading=true anchorAttrs='aria-label="Permalink"' anchorBody="<svg class='octicon' ...unchanged...></svg>" %}
```
- `page.path` depth equals URL depth on all 2163 pages.
- A simulation on `site-b912` rewrote exactly 85 hrefs and left every other count unchanged.
- `build_revision` and `repository_url` both render in this build.

**Guard:** every relative docs link must exist in git.
- URL-decode the target first; `ConversionStrategies.md:9821` links `%D0%B6.cs`.
- Check case-exactly against `git ls-files`, not `os.path.exists`.
- A link that leaves `docs/` must climb exactly to the repo root.
- Assert that the raw count of `href="` + `../`×(depth+1) equals the parsed climb count. This catches the Liquid `replace` hitting a code span; today there are 0 such cases.
- Exempt `JOURNAL-2026-09-12.md`.

### 6. Bare `<T>` and placeholders in prose

**Cause**
- `ж<T>`, `slice<byte>`, `<release date>` and similar tokens sit outside code spans, where they are valid HTML start tags.
- GitHub drops the unknown element, so the token vanishes.
- kramdown opens a span element and prints the rest of the block unparsed.

**Evidence**
- `Roadmap.md:8` becomes `Roadmap.html:65` with literal `**Status (`, backticks and `[roster](...)`. GitHub renders "Status ():".
- `interfaces.md:1273` becomes `interfaces.html:1468-1470`.
- `validation/current/testing.md:98` becomes `testing.html:457`.
- `slice<byte>(s)` shows as `slice(s)` at 10 sites in `strconv.html`.
- `ж<Map>` (44 per page on `internal.runtime.maps`) is stripped by GitHub but fine on the site.
- `<S>` at `phase3/Phase3-Handoff.md:721,723,753` becomes a real strikethrough on the site.

**Fix**
- **Do not hand-edit the 11 `validation/current` pages.** They are generated from `src/core/<pkg>/go2cs_test_disclosures.json` by `src/go2cs/validationProofPages.go`. Either put the tokens in backticks in the JSON reason strings and regenerate, or make `escapeProofCell` (`validationProofPages.go:116-121`) turn `<` into `&lt;` outside backtick spans.
- `Roadmap.md:8`: write the real release date.
- Other visitor pages: put the token in a code span, or write `\<`.
- A code span must never wrap so that `<` starts a line (see #1).

**Guard:** flag every `<Ident` outside fences and code spans except an allowlist of `a` (507 uses) and `br` (3). Exempt the frozen snapshots.

### 7. Tables render as pipe text

**Cause**
- kramdown starts a table only at a block boundary.
- It rejects the whole table when a code span crosses rows, or when the line after the table is not a boundary.

**Evidence**
- `ConversionStrategies.html:3451-3452` shows `<p>| Go | C# |` / `|—|—|`.
- `Roadmap.html:198`, `ValidatedTestPackages.html:2247`, `FINDING-darwin-run-layer.html:591`, `GoCorpusMigration.html:1062`, `c1-relabel/index.html:246`.
- All 5 source tables that lack a blank line before them fail.

**Fix**
- Blank line after `ConversionStrategies.md:2890`.
- Blank line after `Roadmap.md:99`, and also after `:108`. This makes the list loose on GitHub (cosmetic).
- Blank lines before `phase4/h10-evidence/c1-relabel/README.md:64`, `:84` and `:97`.
- Blank line after `GoCorpusMigration.md:801`.
- `ValidatedTestPackages.md:763`: write `` `os/signal`'s ``.
- `phase4/FINDING-darwin-run-layer.md:374`: use a double-backtick span for ``array`1``.
- Leave the two frozen copies alone (`validation/1.24.13.{1,2}`).

**Guard**
- A table header needs a blank line before it.
- The line after the last row must be a blank line, heading, rule, EOF or the next list item.
- Use per-row CommonMark span matching. Do not count backtick parity: parity counting gives 21 false positives.

### 8. `\|` in table code spans

**Cause:** GitHub strips `\` before `|` in table rows, including inside code spans. kramdown keeps it.

**Evidence**
- `GoCorpusMigration.md:841` becomes `GoCorpusMigration.html:1125`, `head -c 200 "$LOG" \| tr ...`. Copied into a shell, `\|` is not a pipe.
- `package-conversion.html:1531` and `ValidatedTestPackages.html:521` show the same problem.

**Fix**
- For the 56 fixable wrong spans (29 on visitor and plan pages, 27 in records), write `<code>head -c 200 "$LOG" &#124; tr ...</code>`.
- Inside `<code>`, also escape `<` and `>`. `GoCorpusMigration.md:846-849` needs `"&lt;stage&gt;/$R/src/core"` and `"$ST/&lt;goos&gt;-amd64"`, or GitHub strips them.
- The 12 grep alternations in `PLAN-linux-operation.md` (lines 1545, 1548, 1549, 1550, 1553, 1557, 1563, 1564, 1565, 1569, 1572, 1574) are right on the site and wrong on GitHub. Write them as `<code>grep -rln 'DllImport&#92;&#124;LibraryImport' ...</code>`.
- Leave the 4 frozen spans alone. Editing the 27 record spans is an owner call.

**Guard:** flag `\|` inside a backtick span on a table row. Accept the `&#92;&#124;` form, and exempt `frozenSnapshot`.

### 9. A wrapped `N. ` line becomes a list

**Cause:** kramdown lets any `N. ` line interrupt a paragraph and drops the number. CommonMark allows only `1.` to interrupt.

**Evidence:** `ValidatedTestPackages.md:184-185` becomes `ValidatedTestPackages.html:251-253`, where "61" becomes "1.". `ConversionStrategies.html:2503-2505` loses "300".

**Fix:** end the previous line with the number:
- `ConversionStrategies.md:2039` "sum is 300."
- `ValidatedTestPackages.md:184` "offers 61."
- `BOARD:16694` "108."
- `CENSUS-h6-handown-package-aliases.md:778` "463."
- `CENSUS-bucket3-darwin.md:208` "line 3323."

Leave the 3 frozen copies alone.

**Guard:** outside fences and comments, after stripping blockquote prefixes, flag `^ *\d+\.[ \t|]` with N ≠ 1 when both hold:
- the previous line is paragraph text;
- the candidate is indented at least as far as that line.

This catches exactly 8 today. The literal predicate in the finding gives 1154 false positives.

### 10. A line containing `|` becomes a one-row table

**Cause:** kramdown makes a table from a block in which every line has an unescaped `|` outside code, with no delimiter row needed.

**Evidence:** `ConversionStrategies-Reference.html:290-293`, where the TOC link is split into 2 cells. `manual-conversions.html:74-78`.

**Fix**
- `ConversionStrategies-Reference.md:179`: `` [The `string | []byte` union](ConversionStrategies-Reference/generic-constraints.md#the-string--byte-union) ``.
- `manual-conversions.md:16` (`address|locked`), `BOARD:17853`, `phase4/CENSUS-h10-eligibility-go124.md:203` and `phase4/hopA-inputs/shard-map-draft.md:77`: write `\|`, or use a code span. Only the code span is verified in this build.
- `phase4/KICKOFF-fleet.md:39`: code span. It is a shell command, and kramdown also typesets it.
- `phase4/FINDING-init-order-tuple-specs.md:262` is a quoted table row. Add `> | Package | Result | Root cause |` and `> |---|---|---|` above it.

**Guard:** flag a block in which every line has an unescaped `|` outside code and no line is a delimiter row. That catches exactly 7 today.

### 11. Fences inside list items

**Cause:** kramdown gathers a list item's lazy lines, and `-`/`+` lines, before it recognises fences.

**Evidence**
- `struct-types.html:769-770` shows literal ```` ```csharp ````.
- Lines 776-798 put the "Guarded by" paragraph and an H2 inside `<pre>`, which breaks the 2 fragments from the legacy map and the index.
- `CENSUS-runtime-g6-full-depth.html:761-778` turns the diff lines into bullets.

**Fix**
- Blank line between `ConversionStrategies-Reference/struct-types.md:648` and `:649`.
- Blank lines between `phase4/CENSUS-runtime-g6-full-depth.md:316/317` and `324/325`.

**Guard (two rules)**
- A fence indented less than the item's content column, following item text with no blank line.
- A content-indented fence, with no blank line before it, whose body has a list-marker line.

These flag exactly the 4 fences.

### 12. Wrapped link text serves raw `.md`

This merges four findings.

**Cause:** `jekyll-relative-links` 0.6.1 `generator.rb:10` has `LINK_TEXT_REGEX = %r!(.*?)!` with no `/m` flag. Link text with a newline is never rewritten, and the link lands on the raw `.md` copy (#25).

**Evidence**
- `NEWS.html:337,360,384` and `ConversionStrategies-Reference/interfaces.html:1084`.
- Live: HEAD on `news/2026-07-26-quarter-of-stdlib-tests-pass.md` returns 200 `text/markdown`.

**Fix**
- Move the line break to just before `[` at `NEWS.md:236`, `:255` and `:275`, `ConversionStrategies-Reference/interfaces.md:948`, and `ConversionStrategies-Reference/manual-conversions.md:3621` (latent inside #1).
- Keep the `.md` target. Do not target `.html`; it 404s on GitHub.

**Guard**
- Flag an inline link to a relative `.md` target whose `[text]` contains a newline. Scan the source, not the built HTML.
- In the site checker, treat a page link that resolves to a `.md` file as broken. Today `site_check.py:57` accepts any file.

### 13. Legacy reference map and README index

This merges the slug, emphasis and pipe findings.

**Cause**
- `.claude/coord-scripts/docs/split_reference.py` built the labels and slugs. It is tracked, committed in `ac5b309125`; the anchors lens said it was not in the repo, and that is wrong.
- Its `strip_inline` (`:109-118`) strips `<...>` before removing backticks, so `IArray<E>` loses `<E>`.
- Line `:381` also strips `[` and `]` from labels, which exposes `*` and `_` to emphasis parsing.
- The script refuses to re-run on a stub (`:194-196`), so the fix is a hand edit.

**Evidence**
- 7 broken fragments each at `ConversionStrategies-Reference.html:141,268,270,272,273,487,563` and `ConversionStrategies-Reference/index.html:170,284,286,288,289,473,541`.
- `:562` shows `[A blank scalar range variable never emits as <em>](...)` with no link.
- `:503-504` lose their `*`.

**Fix**
- In `ConversionStrategies-Reference.md` lines 64, 161, 163, 165, 166, 333 and 400 (stub id and fragment), and in `ConversionStrategies-Reference/README.md` lines 85, 175, 177, 179, 180, 340 and 405 (fragment), use these ids:
  - `the-three-deref-accessors-of-жt--when-each-is-needed-and-how-the-converter-picks`
  - `an-array-core-constraint-ne-lifts-to-iarraye`
  - `a-single-term-pointer-constraint-p-t-erases-the-parameter-to-жt`
  - `a-named-numeric-wrapper-is-icomparablet-as-well-as-ordered-by-operators`
  - `lifted-shift-constraint-uses-the-bcl-shape-ishiftoperatorst-int-t`
  - `reading-a-pointer-and-taking-a-field-pointer-allocate-nothing--the-two-costs-hidden-inside-жt`
  - `for-range-over-a-slice-allocates-nothing--slicetgetenumerator-returns-a-struct`
- Do not keep the wrong ids as extra `<a id>`. They never existed before `ac5b309125`, and nothing links to them.
- In the legacy file, replace damaged labels with the heading text including code spans, copied from `README.md`. Example for line 399: `` [A blank scalar range variable never emits as `_`](...#a-blank-scalar-range-variable-never-emits-as-_) ``. This covers lines 179, 349, 350 and 399, plus 15 labels that lost `[]` or `<T>`.

**Guard:** resolve every `.md#frag` link against the target's GitHub-style heading slugs plus its `<a id>` stubs. The emulator matched GitHub on 524/524 headings, and the simulated guard flags exactly these 14 links today.

### 14. Misrooted links in phase3 and phase4 records

**Cause**
- The phase3 files moved one directory down unchanged (`1b153fa250`, then `d3223d252e`).
- Two phase4 designs were written with repo-root paths.

**Evidence**
- Built `phase3/Phase3-Handoff.html:63,70,77` and siblings.
- `phase4/DESIGN-w3a-wrapper-scaffolding.html:63-64`.
- Live `/phase3/Roadmap.md` returns 404.

**Fix**

| File | Line | Change |
|---|---|---|
| `phase3/DESIGN-recursive-enduser-conversion.md` | 6 | `Roadmap.md` → `../Roadmap.md` |
| `phase3/DESIGN-recursive-enduser-conversion.md` | 10 | `../CLAUDE.md` → `../../CLAUDE.md` |
| `phase3/Phase3-AutonomousLoop.md` | 5 | `Roadmap.md` → `../Roadmap.md` |
| `phase3/Phase3-AutonomousLoop.md` | 9 | `../CLAUDE.md` → `../../CLAUDE.md` |
| `phase3/Phase3-AutonomousLoop.md` | 85, 127 | `Baseline-vs-FullConversion.md` → `../../src/archived/Baseline-vs-FullConversion.md` |
| `phase3/Phase3-Floor.md` | 5 | `Roadmap.md` → `../Roadmap.md` |
| `phase3/Phase3-Handoff.md` | 6 | `Roadmap.md` → `../Roadmap.md` |
| `phase3/Phase3-Handoff.md` | 11 | `../CLAUDE.md` → `../../CLAUDE.md` |
| `phase3/Phase3-Handoff.md` | 18, 1797 | `../src/archived/...` → `../../src/archived/...` |
| `phase4/DESIGN-linkname-push-cycles.md` | 6 | `docs/phase4/CENSUS-runtime-first-contact.md` → `CENSUS-runtime-first-contact.md` |
| `phase4/DESIGN-w3a-wrapper-scaffolding.md` | 7 | `docs/phase4/CENSUS-runtime-first-contact.md` → `CENSUS-runtime-first-contact.md` |
| `phase4/DESIGN-w3a-wrapper-scaffolding.md` | 8 | `docs/phase4/MAILBOX.md` → `MAILBOX.md` |

What changes after the edits:
- On the site, 7 of the 12 start working at once.
- The `CLAUDE.md` and `src/` links need #5.
- `:85` and `:1797` render as links only after the `ж<T>` before them is escaped (#6).

### 15. The journal's repo-root links

**Evidence**
- `doctrine/JOURNAL-2026-09-12.md` has 20 `docs/X.md` links (lines 5, 6, 9, 10, 21, 181, 3511, 3515, 4308, 4583, 4584, 4665, 4845, 4846, 4932, 5346, 5675, 5677, 5679, 5681).
- They resolve to `/doctrine/docs/*` on the site and to `docs/doctrine/docs/*` on GitHub.
- The file's blob must equal `1800b04f8:CLAUDE.md` (`contextBudget_test.go:75,284`).
- No built page links to it. Its only inbound link is `CLAUDE.md:205`.

**Fix** (owner)
- (a) Keep it off the site with the `exclude:` list in the config block at the end of this section. CNAME must be listed, or the gem stops excluding it (`configuration.rb:158-163`).
- (b) Accept the dead links, and exempt the file by name in every link guard.

Neither option fixes GitHub.

### 16. Validation proof links to renamed package paths

**Cause**
- The proof pages link `tree/master/src/core/<pkg>` (`validationProofPages.go:223`, `:337`).
- The 1.24 hop renamed those packages.
- The 10 relocation-anchor pages kept in `validation/current` then point at paths master no longer has.
- `push-nuget.ps1:1063` copied them into the 1.24.13.x snapshots, re-pinned to tags where the paths never existed.

**Evidence**
- `validation/current/crypto.internal.alias.html:68` returns 404 on GitHub; the same path at `tree/nuget-1.23.12.3/...` returns 200.
- All 12 dead paths exist at `nuget-1.23.12.3`.

**Fix** (one change in two parts)
- (a) In the 10 anchor pages, retarget the 10 `tree/master/` links and 2 `blob/master/` links to `nuget-1.23.12.3`.
- (b) In the same commit, change `Update-FrozenProofPage` (`src/_roster.ps1:976-994`) so an already-pinned `tree/nuget-N.N.N.N/` link counts as pinned and is left alone, and add a fixture for that page shape.
- **Without (b) the next release breaks.** It throws at `:990` after `git tag -s` at `push-nuget.ps1:973`.
- Do not prune the pages. That contradicts `GoCorpusMigration.md:3171` and `ValidatedTestPackages.md:272`.
- Add a runbook step to `GoCorpusMigration.md`: after a hop renames packages, re-point anchor pages to the last tag that carried each package.

**Owner decision:** re-pinning the frozen snapshots 1.23.1.3 through 1.23.12.2 to their own tags. That leaves 0 dead links.

### 17. Pages whose first line is not the H1

**Cause:** `jekyll-titles-from-headings` matches a heading only at `\A`. The repo guard skips files with no raw guard (`docsJekyllLiquid_test.go:227` runs before the title check at `:233`).

**Evidence**
- `NEWS.html:10,12,41`: default `<title>`, og:title "go2cs", banner "go2cs".
- Sources:
  - The logo is on line 1 of `NEWS.md`, `StdLibCompileMilestone.md` and the 4 `news/*.md` files.
  - `ConversionStrategies-Reference.md` opens with a blockquote.
  - `Performance.md:1` is a comment prepended by `src/tests/Performance/run-performance.ps1:71-72`.

**Fix**
- Swap lines 1 and 3 in `NEWS.md`, `StdLibCompileMilestone.md` and the 4 news posts.
- Move the 2-line "has moved" blockquote below the H1 in `ConversionStrategies-Reference.md`.
- Make `run-performance.ps1` append the AUTO-COPIED banner at the end of `docs/Performance.md`, then regenerate it.

**Guard**
- Move the title rule above the `continue` and apply it to every published `.md`.
- Match the plugin regex against the whole text with the BOM stripped, since setext headings span two lines.
- Exempt `docs/README.md` (the home page, whose default title is intended) and `frozenSnapshot`.
- With those exemptions exactly these 8 pages fail today.

### 18. Probe folder cited by DESIGN-zh-box-b1 never landed

**Evidence**
- `phase4/DESIGN-zh-box-b1.md:19` links `probes/b1-box-dispatch/`.
- That folder exists only on `origin/claude/g-b1-box-design` (commits `6815eba009` and `f632a942bb`).
- It returns 404 on the live site and on GitHub `tree/master`.

**Fix** (owner)
- (a) `git checkout f632a942bb -- docs/phase4/probes/b1-box-dispatch` on a lane branch; the README becomes its index.
- (b) Point the link at `https://github.com/ritchiecarroll/go2cs/tree/f632a942bb9ca67cbc8412ee9f3f6516fdc281b4/docs/phase4/probes/b1-box-dispatch`.

**Never use `6815eba009`.** Its `README.md:12` holds a real machine hostname that `f632a942bb` scrubbed. `CENSUS-preservation-2026-09-12.md:227-229` already says never to delete the branch.

### 19. `validation/current` has no index

**Evidence:** `validation/1.23.1.2/README.md:3` links `../current`. Live, `/validation/current/` returns 404. The page is a frozen snapshot.

**Fix** (owner): add `docs/validation/current-index.md` with front matter `permalink: /validation/current/`, an H1, and `[Validation proofs](index.md)`.
- It sits outside `current/`, so no tool counts it.
- Never add a README or index inside `current/`. `push-nuget.ps1:337` and `_roster.ps1:638` would count it as a proof page, the release preflight at `push-nuget.ps1:791` would go red, and `:1063` would copy it into every snapshot.
- Alternative, if the snapshot may be edited: link `https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/current`.

### 20. Heading permalinks have no accessible name

**Evidence:** `default.html:50` passes an `aria-hidden` svg as the only content and no `anchorAttrs`. `generic-constraints.html:54` shows the result.

**Fix:** add `anchorAttrs='aria-label="Permalink"'` to the include, as in the #5 snippet.
- Because the anchor sits inside the heading, screen readers will read "Permalink Generic constraints".
- Never use `%heading%` in the attribute: 3,151 headings contain a raw `"`.

### 21. The 3 frozen rosters keep the default title

**Evidence:** `validation/1.24.13.2/ValidatedTestPackages.html:10,12,41`. The raw guard is on line 1 of each frozen source.

**Fix** (owner): add `_config.yml` defaults, one exact-file scope per snapshot. Do not use a glob; it runs `Dir.glob` on every missed lookup and would override future snapshots.
```yaml
defaults:
  - scope: { path: "validation/1.23.12.3/ValidatedTestPackages.md" }
    values: { title: "Validated Test Packages — 1.23.12.3" }
  - scope: { path: "validation/1.24.13.1/ValidatedTestPackages.md" }
    values: { title: "Validated Test Packages — 1.24.13.1" }
  - scope: { path: "validation/1.24.13.2/ValidatedTestPackages.md" }
    values: { title: "Validated Test Packages — 1.24.13.2" }
```
Leave "go2cs" out of the value. seo-tag already appends " | go2cs".

### 22. No custom 404 page

**Evidence:** there is no `404*` file under `docs/`. Live, missing pages return GitHub's generic page (9379 B).

**Fix:** add `docs/404.md` with front matter `permalink: /404.html` and `title: Page not found`, a short message, and root-absolute links only (`[Home](/)`). Fix #2 in the same change; the 404 page is served at any depth.

### 23. Banner repeats the H1; one description for every page

**Evidence**
- `default.html:37-38`. `pointers.html:41` and `:51` are both `<h1>`.
- Description, og:description and tagline are identical on 2163 pages. `og:image` appears on 0.

**Fix** (owner)
- The minimal fix is `{{ site.title }}` in the banner at `:37`.
- `titles_from_headings: {strip_title: true}` would orphan 104 in-site fragment links on 35 pages.
- For descriptions and images, use `defaults:`, not front matter; front matter shows as a table on GitHub.
- `/images/go2cs-large.png` is only 340x247.

### 24. Typographic substitution

**Evidence**
- `phase4/KICKOFF-fleet.html` id is `kickoff--go2cs-fleet`; on GitHub it is `kickoff----go2cs-fleet`.
- `KICKOFF-fleet.html:117` shows `–no-prune`.
- 69 heading ids differ. 12 CLI flags are mangled: 9 written in prose on record pages, 3 inside failed code spans.
- No link targets the 69 ids today.

**Fix** (owner): in `docs/_config.yml`:
```yaml
kramdown:
  gfm_quirks: [paragraph_end, no_auto_typographic]
```
- `paragraph_end` must be restated.
- This makes the 69 ids and the visible text match GitHub and fixes all 12 flags.
- Trade-offs: about 1,853 en dashes on 125 pages and 103 em dashes revert to ASCII. About 10 titles change. External deep links to the current ids break.
- Add `smart_quotes: ["apos", "apos", "quot", "quot"]` only if straight quotes are also wanted.

### 25. Every `.md` source is published raw

**Cause:** `jekyll-optional-front-matter` 0.3.2 and `jekyll-readme-index` 0.3.0 keep the originals unless `remove_originals` is true.

**Fix** (owner, after #12): see the config block below.
- Both keys are needed. The first alone removes 2144 files and leaves the 19 READMEs.
- Nothing in the repo links to a `go2cs.net/*.md` URL.

### 26. DarkReader from a CDN with no SRI

**Evidence**
- `default.html:29` loads `darkreader@4.9.31/darkreader.min.js`. The package has no such file; jsDelivr generates it, and its header says not to use SRI with it.
- 4.9.31 is from 2021; the latest is 4.9.133.

**Fix:** replace line 29:
```html
<script src="https://cdn.jsdelivr.net/npm/darkreader@4.9.31/darkreader.js" integrity="sha256-OwM7P0PRtrDAgiygteRNBoy0f9NWG102emM7kiedZFg=" crossorigin="anonymous"></script>
```
- The hash was computed locally.
- The download is about 40.9 KB brotli against 25.2 KB today.
- Keep the `light_mode` wrapper. It is the theme's documented opt-out.
- Owner decisions: `enable()` vs `auto()`, whether to upgrade, and whether to self-host.
- A pure CSS palette would also need overrides for `rouge-github.scss` (63 hard-coded colours) and the kbd colours.

### 27. Remote theme fetched unpinned at HEAD

**Evidence**
- `_config.yml:2` reads `remote_theme: chris1111/cayman-dark`, and every build downloads `.../zip/HEAD`.
- The fork's `_sass` and `assets` blobs are identical to Cayman 0.2.0, which github-pages v232 bundles.
- The site overrides the fork's layout and `style.scss` anyway.

**Fix:** replace lines 1-2 with `theme: jekyll-theme-cayman`. One check build should confirm that `assets/css/style.css` still hashes to `d091a1023520a79f5eb68385d1a13a682cb334a53480aceb5fff85209acc894c`.

Fallback: `remote_theme: chris1111/cayman-dark@fe3525069a05f3ca4a012cfeccdcc8526c321044`. A pin stops drift only; a rename or deletion still breaks the build.

### 28. Stale shared-content header

**Evidence:** `default.html:1-5` names Gemstone and a sync script that does not exist.

**Fix:** replace it with:
```liquid
{% comment %} go2cs site layout. Derived from gemstone/shared-content (2020), and maintained in this repo since then. No script syncs it. {% endcomment %}
```
Add a sentence that every same-site URL uses `relative_url` only in the same commit as #2.

### 29. Proof page titles carry no version

**Evidence:** `validation/1.23.1.2/bufio.html:10` has the same title as `current/`.

**Fix** (owner): read the snapshot name from `page.dir` in the layout:
```liquid
{% assign vp = page.dir | split: '/' %}{% if vp[1] == 'validation' and vp[2] and vp[2] != 'current' %}{% assign snap = vp[2] %}{% endif %}
```
- Use `snap` in the tagline.
- For the tab title, use `{% seo title=false %}` with a custom `<title>` when `snap` is set.
- Config defaults alone would not change `<title>`.

### 30. Evidence files published with no inbound link

**Fix** (optional): exclude the 3 folders that hold no pages: `phase4/hopA-inputs/recon-evidence`, `phase4/hopA-inputs/provisional` and `phase4/hopA-inputs/recon-tsv` (1.82 MiB). Do not exclude all of `hopA-inputs`; the roster links `shardmap.py` and `population-go1.24.13.txt`.

### 31. Build time growth

**Evidence**
- Jekyll went from 63.892 s (2026-09-20) to 137.201 s (the fix deploy).
- The clean A/B for one snapshot (`4c53b02a0a` to `9a5f63041b`) cost +26.1 s of Jekyll for +5.11 MB of Markdown, about 5.1 s/MB.
- The only 10-minute timeout in the logs is `deploy-pages`, which takes 8 to 34 s.

**Fix:** no action now. Headroom is about 16 to 20 snapshots of today's size.
- Optional guard: a Markdown byte ceiling for `docs/**/*.md` in repoguard, for example 100 MB against today's 36,968,111 B.
- The largest single lever is `validation/*/crypto.cipher.md` (2.5 MB each). It is published 3 times as 7.74 MB of HTML each.

### 32. Every master push rebuilds the site

**Evidence**
- 85 of 299 builds had no docs change.
- The busiest rolling hour held 11 runs, 3 of them cancelled.
- `default.html:26` puts `build_revision` in the stylesheet URL, so every rebuild resets the CSS cache.

**Fix** (owner): switch Pages source to GitHub Actions. Use a workflow with `paths: ['docs/**', '<the workflow file>']`, `workflow_dispatch`, `permissions: pages: write, id-token: write, contents: read`, and `concurrency: group: pages`. It runs `configure-pages`, then `jekyll-build-pages@v1` with `source: ./docs`, then `upload-pages-artifact`, then `deploy-pages`. This pairs with #4.

### 33. Node 20 warnings

**Evidence**
- Pages workflow: `upload-artifact@v4`, pulled in by `upload-pages-artifact@v3`. GitHub owns it.
- Repo workflows: `licensing.yml:35-36`, `dco.yml:15`, `os-matrix.yml:114,270,273,280,1035`, and `cla.yml:30`.

**Fix:** move to the Node-24 majors:
- checkout v5 or later (latest v7.0.1);
- setup-go v6 or later;
- setup-dotnet v5 or later;
- upload-artifact v6 or later (v5 is still node20);
- `upload-pages-artifact@v5` in any new workflow.

`cla.yml` has no Node-24 release yet. `ubuntu-latest` moves to Ubuntu 26 from 2026-10-19.

### Config changes in one place

All of these are optional and are owner decisions. They merge the `exclude` needs of #15 and #30.
```yaml
theme: jekyll-theme-cayman          # replaces lines 1-2 (#27)
kramdown:
  gfm_quirks: [paragraph_end, no_auto_typographic]   # #24
optional_front_matter:
  remove_originals: true            # #25, only after #12
readme_index:
  remove_originals: true            # #25
exclude:
  - CNAME                           # must stay listed once exclude is set
  - doctrine/JOURNAL-2026-09-12.md  # #15 option (a)
  - phase4/hopA-inputs/recon-evidence
  - phase4/hopA-inputs/provisional
  - phase4/hopA-inputs/recon-tsv    # #30
defaults: [...]                     # #21, and #23 if wanted
```

## 4. Already fixed in 4fb6e460c6 (confirmed)

- **The build break is fixed and deployed.** The Liquid break at `ConversionStrategies.md:8760` is gone. Run 36341195472 succeeded with 0 Liquid warnings, 2164 pages, "done in 137.201 seconds". Deploy finished at 18:38:11Z. Live `style.css?v=4fb6e460c6...` and the Glossary title are correct.
- **The 17 silent losses are gone.** There were 17 `{{.GoFiles}}`-style templates published as empty text. `pc-run2.log` and `log-36341195472.txt` have 0 Liquid warnings; each of the 7 earlier green logs had 17.
- **The 19 line-1 raw guards moved below their H1s.** The 3 frozen snapshots were left as they were (#21).
- **`TestDocsSurviveJekyllLiquid` exists and passes** in 0.76 s. It is proven: run on the `1dae85e093` blob, it names line 8760.
- **Nothing in section 3 is addressed by this commit.** The verifiers checked each item.

## 5. Checked and fine

**Build**
- 2164 pages were written: the 2163 `.md` files plus `TargetAtlas.html`, which is standalone by design. None were dropped.
- The failed build failed closed: deploy was skipped, and the old site kept serving.
- The fix added no build time: 137.8 s against 136.7 s before.
- `CNAME` is excluded by the gem default. That is expected.

**Pages settings**
- HTTPS is enforced. The certificate is approved to 2026-12-18.
- http redirects to https with a 301, and www redirects to the apex with a 301.

**Layout**
- The theme stylesheet, favicon, Home and "View on GitHub" links resolve at every depth. The site's own `style.scss` is compiled.
- canonical equals og:url on 2163/2163 pages. JSON-LD and og:type are fine.
- `anchor_headings.html` v1.0.6 shows 0 nested-heading content loss across 10,474 headings.
- jemoji makes 0 substitutions, so it could be dropped.

**Anchors**
- kramdown and GitHub ids agree on 524/524 strategy headings, apart from the dash class (#24).
- No page has a duplicate id, and the -N dedupe scheme is the same on both sides.
- All 10,471 heading self-links resolve. 3,447 of 3,593 authored fragment links resolve.
- All 475 legacy stubs are present as ids.

**Links**
- The 7 `../README.md` links resolve to `/#try-it-yourself--validate-a-converted-test-suite`.
- 3316 of 3384 absolute repo links resolve at their ref.
- 19 README folders became index pages, and 103 of 105 subfolder links resolve.

**Rendering**
- There is no math, IAL, definition list or abbreviation.
- Footnotes, task lists and strikethrough render.
- No block HTML exists in the sources.
- These pages have no structural difference from GitHub's rendering: README, NEWS, news, Glossary, DotNetMigration, Architecture, Performance and the reference README.
- 228 of 233 `validation/current` pages are identical to GitHub's rendering.

**Places where the site is right and GitHub is wrong**
- `ж<Map>` on the maps proof pages.
- The table at `DATA-h10-shardmap-projection-go124.md:321`.
- 3 tables whose rows carry an extra cell.
- 7 `---` lines under multi-line paragraphs.

**Titles**
- No markup leaks into titles.
- `index.html`'s default title is jekyll-seo-tag's normal home-page form.

**Pages workflow warnings**
- All non-Liquid warnings in the Pages log are GitHub-owned.

## 6. UNVERIFIED

**Needs a real build (no Ruby or kramdown on this box)**
- Every proposed Markdown edit's kramdown rendering is inferred from the plugin sources and from analogous lines in this build. One build through `actions/jekyll-build-pages` would confirm them all.
- Content hidden today may surface after #1, for example the inline `ж<T>` at `interfaces.html:1468-1477`.
- The #5 Liquid rewrite: only its string effect was simulated. It is also unknown whether `site.github.source.branch` is populated.
- Whether github-pages v232 honours the `gfm_quirks` override and whether `_config.yml` defaults apply to optional-front-matter pages. The plugin sources say yes.
- Whether `theme: jekyll-theme-cayman` produces a byte-identical `style.css`.

**GitHub-side behaviour**
- The account-level domain verification state; the API cannot read it.
- Whether cancelled runs count toward the 10-builds/hour limit.
- The Jekyll build job's own time limit.
- Per-page render cost; the logs are buffered.
- The date GitHub stops running Node-20 actions, and whether any workflow is sensitive to the Ubuntu 26 image.

**Rendering on github.com, not checked directly**
- `FINDING-init-order-tuple-specs.md:262`, the other pipe-prose lines, and `Charter.md:459` `<class>`.
- How browsers present `text/markdown` (display or download).

**Visitor impact and external links**
- Whether publishing `/CNAME` would cause harm.
- External deep links to `go2cs.net/*.md` or to the current heading ids.
- Search-engine indexing; the site has no sitemap.
- A screen-reader test of #20.

**Counts and history**
- 3 anchor emulator-limit headings: `BOARD:17758`, `BOARD:25777` and `RECON-go1.24-hop.md:485`.
- One fragment-count difference, 145 against 144, was not investigated.
- Whether `claude/coord-docs-strategies` was pushed before its merge.