# DESIGN: seat Z — auto-provision a pinned zig as the C compiler for `-cgo`

C1, 2026-11-10. COORD's seat Z, carrying the owner's direction (PLAN-cgo-interop §5.1, the 2026-11-09 amendment: "a
default that will auto-provision a C toolchain on demand"). It builds on H's scout,
`docs/phase4/SCOUT-cgo-c-toolchain.md`, which measured the candidates, recommended zig and laid out the CLI. H is the
second reader. Branch `claude/c1-cgo-zig-provision`, cut from master `38caf3558a`.

This note adds three things to the scout: where the change lands in the converter, how the pins are produced, and what
tests the seat. Open questions for H are at the end.

## 1. What the seat delivers

When a conversion runs with `-cgo` and no C compiler resolves, go2cs downloads a pinned zig, checks its SHA-256 against
a hash built into the converter, unpacks it into the user cache, and hands it to the `-cgo` load as `CC="<zig> cc"`. It
uses the same machinery as the pinned Go toolchain (`toolchainProvision.go`). Nothing changes for a conversion without
`-cgo`, and `-version` and `-help` never download.

Scope, as §5.1 states it: Phase 1 and 2 need a compiler only on the CONVERTING machine, for the host target. Per-target
`zig cc -target …` for Phase 4 is out of scope here, and so is llvm-mingw (§6, Q2).

## 2. Resolution order

The first that applies wins (the scout's §6):

1. **`-cc CMD`**, used as given. Like `CC`, it may carry arguments. It never downloads. Two reserved values:
   - `-cc pinned` forces step 4 even when a host compiler exists. This is the reproducible choice for CI and any
     byte-comparing gate.
   - `-cc host` forces steps 2 to 3 and refuses step 4.
2. **`CC`** from the environment, as `go` itself reads it.
3. **The platform's default compiler on `PATH`**: `gcc`, or `clang` on darwin, the name Go's `cmd/go/internal/cfg`
   defaults to.
4. **The pinned zig** in the user cache, downloaded on first use.

**`-cc-download=false`** turns step 4 into a loud refusal. The refusal names steps 1 to 3 and
`go2cs toolchain install -cc -from FILE`, and never falls back to a silently cgo-off load (PLAN §2, Finding 1).

The resolved value is printed once with the toolchain provenance line, in the form `C compiler: <cmd> (<which step>)`.
That keeps a conversion's compiler visible in its log, as the Go toolchain's already is.

## 3. Where it lands in the converter

- `conversionDriver.go:148` already appends `CGO_ENABLED=1` to the `-cgo` load's `cfg.Env`. The resolved compiler is
  appended beside it, as `CC=<cmd>` plus, for a pinned zig, `ZIG_GLOBAL_CACHE_DIR` and `ZIG_LOCAL_CACHE_DIR`. Those go
  under go2cs's own cache, beside the toolchain, as the scout's instrument did (`os-matrix.yml:1150`), so zig never
  writes to the user's home cache. Only this load sees them, as only it sees `CGO_ENABLED=1`.
- Resolution runs once per process, before the first `-cgo` load, so a download happens at most once. A conversion
  without `-cgo` never calls it.
- A new `ccToolchain.go` holds the resolver and the zig provisioner, reusing `toolchainProvision.go`'s primitives:
  `verifySHA256`, the staging directory and `renameIntoPlace`, `safeJoin`, `unpackZip`, and the per-user cache root. A
  `.tar.xz` unpacker is new. Go's stdlib has no xz, and the module has no xz dependency, so §6 Q3 asks.

## 4. Pins

`ccToolchainPins.go` is generated, as `toolchainPins.go` is, by `internal/genzigpins`. It reads ziglang.org's
`download/index.json` for the pinned version and writes, per host:

| host | archive | size |
|:--|:--|--:|
| windows-amd64 | `zig-x86_64-windows-0.17.0.zip` | 100.3 MB |
| linux-amd64 | `zig-x86_64-linux-0.17.0.tar.xz` | 57.3 MB |
| darwin-amd64 | `zig-x86_64-macos-0.17.0.tar.xz` | 59.3 MB |
| darwin-arm64 | `zig-aarch64-macos-0.17.0.tar.xz` | 54.0 MB |
| linux-arm64, windows-arm64 | from the same index | not measured |

The four measured hashes are already in the scout's instrument (`os-matrix.yml:1101-1104`), each verified on a hosted
runner. The generator must reproduce exactly those four from the index: that is its positive control. The pin is a
release property. It moves when the converter does, by regeneration, never per run.

## 5. Download sources and integrity

ziglang.org asks automated downloads to use its community mirrors. The order is the mirror list
(`download/community-mirrors.txt`) in random order, then ziglang.org itself. **Every byte from any source is checked
against the pinned SHA-256 before it is unpacked**, and a mismatch deletes the file and tries the next source, as the Go
provisioner does. Because the hash is built into the converter binary, a hostile mirror can only fail the download.
It cannot substitute an archive. §6 Q1 asks whether the minisign check the scout recommends is still owed under a
built-in hash.

`go2cs toolchain install -cc -from FILE` installs from a copied archive against the same hash, for an offline machine.

## 6. Open questions for H, the second reader

- **Q1, minisign.** The scout says a mirror download MUST verify the minisign signature. That holds when the hash is read
  from the index at run time. Here the hash is compiled into the converter, so a signature adds no integrity the pin
  lacks, and checking it would add a BLAKE2b dependency (`golang.org/x/crypto`). My proposal is pinned SHA-256 only, and
  the generator verifies the minisign signature of each archive it pins, at GENERATION time, on a networked machine.
- **Q2, llvm-mingw.** It is out of this seat, being a second, windows-only, unsigned pin, unless you want it here.
- **Q3, `.tar.xz`.** Three of the four measured hosts ship `.tar.xz`. The options are a vendored pure-Go xz reader (for
  example `github.com/ulikunitz/xz`, BSD-3-Clause) or shelling out to the system `tar`, which reads xz on linux and macOS,
  and which the scout's instrument used. Which do you prefer? I lean to the system `tar` on non-windows hosts, where it
  is always present, with zip on windows: no new dependency.
- **Q4, the index snapshot.** This box's network policy refuses ziglang.org, so the generator's testdata snapshot of
  `index.json` (and the arm64 pins) has to come from a networked machine. Do you have the 0.17.0 index you read, or
  should a hosted leg fetch it?
- **Q5, the APSL note.** zig bundles Apple's darwin headers under APSL 2.0 (scout §3). go2cs downloads zig from its
  publishers and redistributes nothing. The seat adds a short paragraph to `docs/DotnetTool.md` saying exactly that, and
  that whether it settles the matter is the owner's call. Is that the wording you want?

### Answers (2026-11-10, H through COORD, binding)

- **The export:** `resolveCToolchain() (cc string, env []string, version string, err error)`, as proposed. cc
  survives cmd/go's quoted.Split when the cache path holds spaces (a unit test, and a Windows CI arm owed). The
  sync.Once has a test-only reset. It is callable from anywhere in the process: increment 4's load-name capture links
  with the same compiler and environment.
- **checkCCFlags:** H calls it from the -cgo flag path, in increment 5 or in a one-line seat of H's if Z lands first.
  Z never edits the driver.
- **Order:** as §2. A host compiler is not reproducible (PLAN-cgo-interop §5.2), so every byte-comparing gate
  (converter-cgo, G1/G2) passes `-cc pinned`.
- **Q1:** SHA-256 at run time, minisign at generation time. The scout's §3 "MUST verify" sentence carries a dated
  note.
- **Q2:** llvm-mingw stays out.
- **Q3:** the system tar on linux and macOS, archive/zip on windows, with a named pre-download check on linux. GNU
  tar execs an `xz` binary that slim images lack, so a host without one is refused before downloading.
- **Q4:** H's verbatim index (2026-11-09, 90,260 bytes, SHA-256 81080b1daf59c569e04ebd90c3a9d2f87bbfd25716be61042c9602d3dbd44a3b)
  is copied byte for byte to `src/go2cs/testdata/cctoolchain/`. The generator test asserts the file's hash as well as
  the four instrument hashes.
- **Q5:** the APSL wording in docs/DotnetTool.md, with H's added sentence.

### Status at increment 2

Done: the resolver, the provisioner, the export, genzigpins and its pins, `toolchain install -cc [-from FILE]`, the
`-cc` and `-cc-download` flags, and the docs. Owed:
- **The generation-time minisign check.** It needs the Zig Software Foundation's public key verbatim, read from
  ziglang.org/download on a networked machine. This box cannot reach that page, so the key is asked of H. Until it
  lands, the pins are the index's hashes alone, which is what the instrument verified on hosted runners.
- **The hosted `converter-cgo` mode with `;cc=go2cs`, and its wrong-hash red.** It runs once H's increment 5 wires
  resolveCToolchain into the -cgo load.
- **A Windows CI arm** of the spaces-in-the-cache-path test.

### Owner ruling: the default is per OS (2026-11-11, COORD relayed; PLAN-cgo-interop §5.1, b77b2e78a4)

The pinned default is **zig on linux and darwin, llvm-mingw on windows**. This reverses Q2. Under zig, windows links
cos and strlen statically, leaving no DLL to P/Invoke. llvm-mingw's UCRT build resolves all four of H's probes to UCRT
or KERNEL32 DLLs, and a cold capture took 0.31 s against zig's 140 s. Increment 3 applies it:
- The provisioner is generic over a pinned toolchain (ccPinnedToolchain.go): a name, a release, an archive pin, the
  compiler's path under the install, its CC arguments (` cc` for zig, none for llvm-mingw), and its sources (zig's
  mirrors then ziglang.org; llvm-mingw's GitHub release, which has no mirrors). zig's caches are set for zig only.
- `defaultCCToolchain(goos)` picks llvm-mingw on windows and zig elsewhere. `-cc pinned` takes that default, and
  `-cc pinned:zig` / `-cc pinned:llvm-mingw` name one, so zig stays reachable on windows by an explicit choice. A name
  with no pin for the platform refuses by name (llvm-mingw is pinned for windows-amd64 only, so windows-arm64 has no
  default pin).
- genzigpins became genccpins. zig's pins still come from the saved index. llvm-mingw 20261020 ucrt-x86_64
  (sha256 fd5462b7f5d93b2ff981fcd3f9c7e427e47f3203c64ea91c27bcc444eebd74cd, 190,612,575 bytes) is held as constants,
  because it has no index and NO publisher signature: the pin is the GitHub asset digest alone. Its test reads the
  converter-cgo instrument's pin table in os-matrix.yml and fails if the two disagree.
- llvm-mingw's archive is a zip, so it takes archive/zip and needs no xz check.
- `go2cs toolchain install -cc [-pin zig|llvm-mingw] [-from FILE]`.
- docs/DotnetTool.md carries the per-OS table, says llvm-mingw is pinned by digest alone, and keeps the APSL paragraph
  with one line on windows.
- A correction the provisioner's header owed: genzigpins never checked minisign signatures. That generation-time check
  is still owed (Status at increment 2), for zig only, since llvm-mingw has no signature to check.

## 7. Tests and gates

- **Unit, local** (no network: an in-process HTTP server serves fake archives):
  - the resolution order, each step;
  - `-cc pinned`, `-cc host`, `-cc-download=false` refusing by name;
  - a wrong-hash archive deleted before unpacking and the next source tried;
  - every source failing reported by name;
  - `install -from` accepting the right archive and refusing a wrong one;
  - a conversion without `-cgo` never resolving;
  - the zig env vars set only on the `-cgo` load.
- **Generator:** reproduces the four instrument hashes from the snapshot (the positive control). A one-byte edit to the
  snapshot changes its output (the negative).
- **Hosted, end to end:** the `converter-cgo` stage gains `;cc=go2cs`, under which the leg installs NO compiler and the
  converter provisions zig itself.
  - On the four hosted systems: H's Phase 1 fixture must read the scout's two sets exactly (§4 there): set 1 on
    linux and darwin, set 2 on windows.
  - Red: `;cc=go2cs;sha=` with a wrong hash planted through a test-only override must refuse before unpacking, as the
    instrument's red did.
- `go test ./...` with TestLicensing, repoguard, and `docs/DotnetTool.md` updated (the CLI and the cache table).
- Package compatibility: converter only, no API change.
