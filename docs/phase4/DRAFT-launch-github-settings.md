# DRAFT: GitHub settings for the launch (for the owner to apply)

> **State: DRAFT, 2026-10-11, for the owner.** Everything on this page is a repository **setting**, which
> only the owner can change, or text for him to paste. Nothing here is applied. It accompanies the issue
> forms under `.github/ISSUE_TEMPLATE/` on the same branch, and it serves the launch plan's readiness
> items 5, 6 and 9 (`launch/PLAN-launch.md` on `claude/launch`). Every "today" below was read from the
> GitHub API on 2026-10-11.

## 1. Labels

The issue forms apply `triage` plus one kind label. **GitHub drops a label a form names if the label does
not exist**, without saying so, so create these before the forms land.

Today the repository has GitHub's defaults (`bug`, `duplicate`, `enhancement`, `good first issue`,
`help wanted`, `invalid`, `question`, `wontfix`) and `challenge`.

| Label | Meaning | Applied by |
|:--|:--|:--|
| `triage` | New, not yet read by a maintainer | every form |
| `conversion` | go2cs failed to convert, or its C# does not compile | the conversion bug form |
| `behavior-difference` | The converted program runs and differs from Go | the behavior difference form |
| `nugetgo` | A published `go.*` or `nugetgo.*` package | the package form |
| `docs` | The documentation is wrong, missing or unclear | by hand |
| `platform: windows` | Shows only on Windows | by hand |
| `platform: linux` | Shows only on Linux | by hand |
| `platform: macos` | Shows only on macOS | by hand |
| `good first issue` | Exists already; keep it | by hand |

Two more are worth having once reports arrive: `needs-repro` (no smallest Go yet) and `known-limitation`
(answered by a link to Limitations, then closed).

Commands, if you prefer the CLI to the web page:

```shell
gh label create triage --description "New, not yet read by a maintainer" --color FBCA04
gh label create conversion --description "go2cs failed to convert, or its C# does not compile" --color D93F0B
gh label create behavior-difference --description "The converted program runs and differs from Go" --color B60205
gh label create nugetgo --description "A published go.* or nugetgo.* package" --color 0E8A16
gh label create docs --description "The documentation is wrong, missing or unclear" --color 0075CA
gh label create "platform: windows" --description "Shows only on Windows" --color C5DEF5
gh label create "platform: linux" --description "Shows only on Linux" --color C5DEF5
gh label create "platform: macos" --description "Shows only on macOS" --color C5DEF5
```

## 2. Discussions

Discussions is **already on**. Today it has the categories General, Ideas, Polls, Q&A and Show and tell,
one discussion (GitHub's own welcome post) and nothing pinned.

**Proposed categories:** Q&A, Show and tell, Ideas, Announcements.

- **Add Announcements**, with the format "Announcement", so only maintainers can post there. Release notes
  and the launch posts go in it.
- **Keep** Q&A (answerable), Show and tell, and Ideas.
- **Remove Polls**, and fold General into Q&A, so a newcomer has four clear places and not six.
- Delete or replace GitHub's welcome post with the pinned post below.

**A pinned post, "Start here"**, in Announcements. Text to paste:

```markdown
# Start here

Welcome. go2cs converts Go source code into C# that reads like the Go it came from and behaves the same.

**New to it?**
- The [README](https://go2cs.net/) is a ten-minute read: what it is, how to install it, how to convert a module.
- The [FAQ](https://go2cs.net/FAQ.html) answers the questions most people ask first.
- [Limitations](https://go2cs.net/Limitations.html) says where a conversion will fail or fight you.
  Please read it before you spend an evening.
- [Known issues](https://go2cs.net/KnownIssues.html) lists specific bugs and their workarounds.

**Where to post**
- **A question** about using go2cs or about converted code: [Q&A](../discussions/categories/q-a).
- **Something you converted**, or a Go module you would like to see on NuGet: [Show and tell](../discussions/categories/show-and-tell).
- **A feature or a change** you would like: [Ideas](../discussions/categories/ideas).
- **A defect**: [open an issue](../issues/new/choose). The forms ask for the output of `go2cs -version`,
  your operating system, and the smallest Go that shows the problem.
- **A security vulnerability**: privately, as the [security policy](../security/policy) describes.

[The owner's expectation line goes here: launch plan decision D9.]
```

## 3. The security policy

`docs/SECURITY.md` is where GitHub looks, so the Security tab finds it. Two findings:

1. **Private vulnerability reporting is switched off**, and the policy tells reporters to use it. The
   link in the policy, `…/security/advisories/new`, does not offer a private report while the setting is
   off. **Switch it on**: Settings, then Code security, then "Private vulnerability reporting". Until it
   is on, the policy points at a door that is closed.
2. **The policy did not say which releases receive fixes.** This branch adds a short "Supported versions"
   section: the latest published release only, with a plain statement that the Go release it tracks may
   be out of upstream support. It names no version, so it does not go stale at the next release.

## 4. The README expectation line

The wording is the owner's (launch plan decision D9), so it is not written here. A hidden placeholder
comment marks the proposed place in `docs/README.md`, under Contributing, and the same line closes the
pinned post above. What it needs to say: this is one person's project, replies are best-effort, and
where to ask.

## 5. The About box

Today: the description is "Golang to C# Converter", the website is `https://go2cs.net/`, and there are
**no topics**. Topics are how GitHub's search and topic pages find a repository. Proposed:

- **Description:** "Convert Go source code into C# that reads like Go and behaves the same. The converted
  Go standard library passes Go's own tests."
- **Topics:** `go`, `golang`, `csharp`, `dotnet`, `transpiler`, `source-to-source`, `compiler`, `roslyn`,
  `nuget`, `interop`.

## 6. The Releases panel

Today GitHub lists one release: `v0.1.2`, a pre-release from March 2022. The NuGet releases are git tags
(`nuget-1.24.13.5` and earlier), and a tag is not a Release, so the panel on the repository's front page
shows a project that last shipped in 2022.

**The cleanest fix:** publish a GitHub Release for each `nuget-<version>` tag from now on, starting with
1.24.13.6, with notes taken from that release's NEWS entry and a link to it. One command after the tag is
pushed:

```shell
gh release create nuget-1.24.13.6 --verify-tag --latest --title "go2cs 1.24.13.6" --notes-file <the NEWS entry>
```

Leave `v0.1.2` as it is: it is history, and a newer release marked latest moves it down the list. The
release binaries of the install proposal (`claude/r-install-scope`) attach to the same Release.

## 7. License detection

**Measured:** GitHub's API reports the root `LICENSE` as key `other`, SPDX `NOASSERTION`. The file is
found, so the repository page shows a "View license" link, but no license name. The cause is the file's
own shape: twelve lines explaining the three licenses, then the MIT text. GitHub's detector matches a
file against known license texts and the preamble defeats the match.

**The options**, all of which change a legal file and so are the owner's to decide, not committed here:

| Option | What GitHub would show | Cost |
|:--|:--|:--|
| **A. Leave it** | "View license" | None. It is honest: no single license covers the repository |
| **B. MIT text alone in `LICENSE`**, the explanation moved to `LICENSING.md` | "MIT license" | Misleading at a glance: the converter, the part most visitors come for, is AGPL |
| **C. One file per license at the root** (`LICENSE-AGPL-3.0`, `LICENSE-BSD-3-Clause`, `LICENSE-MIT`, each the unmodified text), with `LICENSE` either removed or reduced to a pointer | GitHub documents that it lists each detected license when a repository has several license files | Three root files; the texts already exist under `LICENSES/`, where GitHub does not look. Not measured here: try it on a fork before deciding |

**Recommendation: C if a license name on the front page matters to you, otherwise A.** Not B. The
predictable launch question is "AGPL?", and a front page that says "MIT" would start that conversation
with a correction.
