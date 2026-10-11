# DRAFT: a one-line install for go2cs (scope and prototype)

> **State: DRAFT, 2026-10-11, for the owner.** It scopes the launch plan's decision D3
> (`launch/PLAN-launch.md` on `claude/launch`). Measured where it says measured; everything else is an
> estimate or a statement of how an outside service documents itself, and is labelled so. Nothing here
> publishes anything. The prototype is `.github/workflows/release-binaries.yml` on this branch.

## The short answer

- **For the soft launch: GitHub release binaries**, one archive per platform, with a checksum file. The
  install is one line on Linux and macOS and three short lines on Windows.
- **For the wide launch: add a `dotnet tool`**, so a C# developer types `dotnet tool install -g go2cs`,
  and add a Scoop bucket and a Homebrew tap, which cost one small file each and need nobody's review.
- **Go is still required to convert**, whichever way the converter is installed. The converter loads and
  type-checks Go source with the Go toolchain. No install method removes that, and the README must keep
  saying so.

## What is measured

**The converter cross-compiles to all six platforms from one machine.** It is pure Go: `CGO_ENABLED=0`
builds every target with no C toolchain. Built locally on Windows at master `4c9c68a32f` with go1.24.13
and `-trimpath`:

| Platform | RID | Binary size |
|:--|:--|--:|
| Windows x64 | `win-x64` | 21.5 MB |
| Windows Arm64 | `win-arm64` | 20.0 MB |
| Linux x64 | `linux-x64` | 21.2 MB |
| Linux Arm64 | `linux-arm64` | 19.9 MB |
| macOS Intel | `osx-x64` | 21.3 MB |
| macOS Apple silicon | `osx-arm64` | 20.0 MB |

**The prototype workflow's run** is recorded in [The prototype run](#the-prototype-run) below: the archive
sizes, and whether `go2cs -version` ran on each platform's own runner.

**The release string is compiled in.** `go2cs -version` prints `go2cs.release` from an asset embedded at
build time, so a binary built at a release tag names that release with no build flags. A binary built by
CI from an ordinary checkout also carries `vcs.revision`, the commit it was built from.

**The converter's own module path is `go2cs`**, so `go install <url>@<version>` cannot install it. Making
that work means renaming the module to its full import path, which touches every internal import in the
converter. That is a converter change with the full gate battery, and it is not proposed for the launch.

## The options

| Option | What a newcomer types | Cost to us per release | Review or account outside the project |
|:--|:--|:--|:--|
| **GitHub release binaries** | Linux and macOS: one `curl … \| tar` line. Windows: download, expand, add to `PATH` | none once the workflow exists: a tag push builds and attaches them | none |
| **A `dotnet tool`** | `dotnet tool install -g go2cs` | one more package in the set the release ritual already packs, signs and pushes | none new: nuget.org, the same account and signature |
| **Scoop** (Windows) | `scoop bucket add go2cs <bucket repo>`, then `scoop install go2cs` | none: Scoop's own `checkver` and `autoupdate` read the latest GitHub Release | none: the bucket is a repository the owner controls |
| **Homebrew** (macOS, Linux) | `brew install ritchiecarroll/go2cs/go2cs` | a formula edit per release (two URLs, two checksums), which a workflow step can write | none for a tap the owner controls. `homebrew-core` is a different matter: it wants a notable project and builds from source |
| **winget** (Windows) | `winget install go2cs` | a manifest pull request to Microsoft's repository per release; `wingetcreate` automates writing it | **yes**: each manifest is validated and reviewed by Microsoft's pipeline, hours to days |

Scoop, Homebrew and winget all **point at the release binaries**. They are layers on the first option,
not alternatives to it. The `dotnet tool` is the only one that carries its own copy.

### What each install looks like

**Release binaries**, with archive names that carry no version, so `releases/latest/download/<name>` is a
stable URL:

```bash
curl -sSfL https://github.com/ritchiecarroll/go2cs/releases/latest/download/go2cs-linux-x64.tar.gz | tar -xz -C ~/.local/bin go2cs
```

```powershell
irm https://github.com/ritchiecarroll/go2cs/releases/latest/download/go2cs-win-x64.zip -OutFile go2cs.zip
Expand-Archive go2cs.zip -DestinationPath "$HOME\go2cs"
# then add $HOME\go2cs to PATH
```

Deliberately **not** `curl … | sh`: a script piped into a shell is the install pattern security-minded
readers object to first, and this audience includes them.

**The `dotnet tool`** needs the most design. A .NET tool is a managed program; the converter is a native
Go binary. Two shapes:

- **One package holding all six binaries** and a small managed launcher that picks the one for the
  machine and runs it. Simple, and about 120 MB unpacked (six binaries of about 20 MB), well under
  nuget.org's package size limit once compressed: the six
  release archives together are 68 MB (measured, the prototype run). *Not measured: the size of an actual
  tool package.*
- **One package per platform**, which .NET 10 supports for tools, with a small pointer package. Each
  download is about a sixth of the size. *Not prototyped; the packaging is new to this repository.*

Either way the tool needs the .NET SDK, which every user of converted code has anyway.

## Signing

The project's signing card signs NuGet packages, on one machine, with a PIN. That covers the
`dotnet tool` package exactly as it covers every other package. It does not by itself cover the binaries.

| Platform | What an unsigned download meets | What the binaries can get |
|:--|:--|:--|
| **Windows** | A browser download of an unsigned `.exe` is marked as from the internet, and SmartScreen may warn on first run. A download by `irm`, Scoop or winget is not warned about in the same way | **Authenticode**, if the card's certificate permits code signing: `signtool` on the two Windows binaries, on the signing machine, in the same PIN session as the packages. That means the Windows archives are attached by the owner, not by CI. *Whether the certificate allows it is not measured here.* |
| **macOS** | A file downloaded by a browser is quarantined, and Gatekeeper refuses an un-notarized binary with "cannot be opened because the developer cannot be verified". A file fetched by `curl` or Homebrew is not quarantined and runs | Go's linker already gives the Apple silicon binary the ad-hoc signature macOS requires. **Notarization** needs an Apple Developer ID, a paid yearly membership: an owner decision, and not needed if the documented install is `curl` or Homebrew |
| **Linux** | Nothing at the operating-system level | A checksum file, and proof of origin (below) |
| **All three** | | **A build provenance attestation** from the workflow (`actions/attest-build-provenance`): GitHub signs a statement that this exact file was built by this workflow from this commit, and anyone checks it with `gh attestation verify`. It needs no key and no card. Plus `SHA256SUMS`, which the owner can also sign with the GPG key that already signs the commits |

**Recommendation:** attestation and `SHA256SUMS` on every platform from the first release with binaries.
Authenticode on the Windows binaries before the wide launch, if the certificate allows it. No Apple
notarization unless browser downloads on macOS turn out to be how people actually install it.

## How it fits the release ritual

Today `release-nuget.ps1` packs, signs and pushes the packages, and the ritual ends with
`git push origin nuget-<version>`.

- **The tag push is the trigger.** A real version of the workflow runs on `push: tags: ['nuget-*']`, so
  the binaries are built from exactly the tagged commit, after the packages are public.
- **It checks itself against the tag:** `go2cs -version` on each platform must print the tag's version
  as `go2cs.release`, or the run fails.
- **It creates a DRAFT GitHub Release** holding the six archives, `SHA256SUMS` and the attestation, with
  notes from that release's NEWS entry. The owner publishes the draft. That also repairs the Releases
  panel, which shows a 2022 pre-release today.
- **If the Windows binaries are Authenticode-signed**, the owner replaces those two archives in the draft
  before publishing, and the checksum file is regenerated. `-trimpath` builds are reproducible, so the
  signed binary can be checked against the CI build apart from its signature.
- **The `dotnet tool` package**, when it exists, joins the package set and needs no new step.
- **A Scoop bucket** updates itself from the published Release. **A Homebrew tap** takes one formula
  commit, which the same workflow can write.

## Recommendation

| Launch | Install | Why |
|:--|:--|:--|
| **Soft** | **Release binaries**, with checksums and an attestation, unsigned | Cheapest by far; no new account, feed or review; fixes the Releases panel; everything else is built on it. Effort estimate: 1 to 2 lane-days from this prototype to a tag-triggered workflow with a draft Release |
| **Wide** | Add the **`dotnet tool`**, a **Scoop bucket** and a **Homebrew tap**; Authenticode on Windows if the certificate allows | The `dotnet tool` is the command a C# developer already knows, and the C# audience is the one without a Go habit. The bucket and the tap are a file each. Effort estimate: 3 to 5 lane-days for the tool, 1 for the bucket and tap together |
| **Later, on demand** | winget; `go install` by URL; Apple notarization | winget's review is outside the project's control; `go install` is a converter-wide rename; notarization costs a yearly membership |

## The prototype run

`.github/workflows/release-binaries.yml` ran twice on this branch. It has no release step and read-only
permissions, and its archives are workflow artifacts that expire.

**Run 38168481887, at `291b9afca2`: all seven jobs green.** One Linux runner built all six; each archive's
checksum was then verified, and `go2cs -version` run, on a runner of its own platform:

| RID | Runner | Archive | Archive size | `go2cs -version` |
|:--|:--|:--|--:|:--|
| `win-x64` | `windows-latest` | `go2cs-win-x64.zip` | 11.9 MB | exit 0, release 1.24.13.5 |
| `win-arm64` | `windows-11-arm` | `go2cs-win-arm64.zip` | 10.9 MB | exit 0, release 1.24.13.5 |
| `linux-x64` | `ubuntu-latest` | `go2cs-linux-x64.tar.gz` | 11.8 MB | exit 0, release 1.24.13.5 |
| `linux-arm64` | `ubuntu-24.04-arm` | `go2cs-linux-arm64.tar.gz` | 10.9 MB | exit 0, release 1.24.13.5 |
| `osx-x64` | `macos-15-intel` | `go2cs-osx-x64.tar.gz` | 11.7 MB | exit 0, release 1.24.13.5 |
| `osx-arm64` | `macos-15` | `go2cs-osx-arm64.tar.gz` | 11.0 MB | exit 0, release 1.24.13.5 |

All six together are 68 MB. Each binary also printed the commit it was built from as `vcs.revision`.
What this proves is narrow: the binary starts and reports itself on each platform. It converts nothing.

**Run 38168160233, at `16b4b7cab6`: the two macOS jobs failed, and that is the finding.** The four Windows
and Linux jobs passed. Both macOS jobs printed `Failed to resolve GOROOT path` and exited 1.

The cause is in the converter. `src/go2cs/main.go:216-222` resolves `GOROOT`, and exits if it cannot,
before any flag is read; `-version` is handled later, at `main.go:298`. A `-trimpath` binary carries no
`GOROOT` of its own. The macOS runner images do not put Go on `PATH`; the Windows and Linux images do.
The second run installs Go in each smoke job first, and passes everywhere.

**What it means for a release binary:** a newcomer who downloads `go2cs` before installing Go cannot even
ask it for its version, and the message does not say what is missing. Two converter changes would fix
the first impression, and neither is made on this branch, which changes no converter source:

1. Handle `-version` before `GOROOT` is resolved, printing `unknown` for what cannot be read, as the
   option's own description already promises.
2. Replace the six-word fatal line with one that says Go is required and names the release.

**A note on how the runs were started.** A workflow that is not yet on the default branch cannot be
started by hand. For these two runs only, the file carried a push trigger limited to this branch and to
the workflow file itself. The branch's last commit removes it, so the file as proposed runs on
`workflow_dispatch` alone.
