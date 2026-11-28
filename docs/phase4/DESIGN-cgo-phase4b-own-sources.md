# DESIGN — cgo Phase 4b: a package's own C files, and preamble classification by every object the package builds

> **STATUS: DESIGN, posted before code (lane H; branch `claude/h-cgo-p4b-own-sources`, cut from the Phase 4 seat's
> tip).**
>
> **Ruling:** the owner, 2026-11-27: "Let's keep moving, by rule here, what will fit while parallel with M:N". 4b is GO.
> It joins .5 only if it is fully gated before the M:N seat freeze (around 12-05), and otherwise .6.
>
> **Builds on:** Phase 4, the preamble-only slice ([`DESIGN-cgo-phase4-sidebuild.md`](DESIGN-cgo-phase4-sidebuild.md)).
> Its mechanism is unchanged: the per-file address table, `GoCgoLibraries.SideTable` and `SideVariable`, and the
> `delegate* unmanaged[Cdecl]` call through a slot.
>
> **What 4b changes:** WHICH names are side-built, and WHAT the side library links.

## 1. The rows

Row 1 is first because it is a parity gap Phase 4 ships with, loud but not Go-faithful.

| Row | Today (Phase 4) | 4b |
|---|---|---|
| **1. A preamble definition colliding with a linked library's export** (COORD, 2026-11-27; BOARD) | The probe is asked first. It binds the name (`gamma` in darwin/amd64's libSystem, `isatty` in libc), and the cmd/cgo-imports check refuses it. Go compiles and calls the preamble's own. | **Classification order:** a name the file's own unit DEFINES, static or not, is side-built, and the probe is never asked for it. Nothing binds silently to the library; nothing Go accepts is refused. TestCgoPhase4F becomes a parity row: it matches go run with the preamble's own function on every OS. |
| **2. A non-static function one file's preamble defines and another file calls** (COORD's ruling (b); BOARD) | Classified by the hosting unit's object alone, where the name is only declared, so it is refused, loud and by name. go-sqlite3: callback.go calls `_sqlite3_result_text`, which sqlite3.go defines. | A name is side-built when ANY of the package's objects (every preamble unit, and row 3's own objects) defines it as a GLOBAL symbol. A local (static) symbol in another unit is not reachable, as in C. The table entry in the hosting unit resolves at the side link. |
| **3. The package's own C files** | Refused by name (§2.7: "C source files of its own"). | The package's `CFiles` (and gcc-assembled `SFiles`) are compiled with the package's cgo flags and the side build's mapping, and linked into the side library. One copy of each C global, as in the Go binary. |
| **4. C files that preprocess to nothing** (COORD; go-sqlite3 under `libsqlite3`) | Refused with row 3's text. | Compiled like any C file. An empty object contributes nothing. go-sqlite3's `sqlite3-binding.c` is wrapped in `#ifndef USE_LIBSQLITE3`, and go list still lists it as a CFile under the tag. |

## 2. Classification, in order

For each name Go uses (the C functions and variables the route collects):

1. **Compile.** Every preamble unit is compiled with every candidate name it hosts, and every own C file is compiled.
   Each object is read with debug/elf, debug/macho or debug/pe.
2. **Own definition.** If the hosting unit's object defines the name, locally or globally, it is side-built (row 1).
   The probe is not asked.
3. **Package definition.** Else, if any other object of the package defines it GLOBALLY, it is side-built (row 2, and
   row 3 for an own C file).
4. **Library export.** Else the probe is asked. A name it binds to a shared library is Phase 2's: an extern held to
   cmd/cgo's imports, as today.
5. **Nowhere.** Else today's refusal stands, word for word, statically resolved C runtime functions included.

A name that two of the package's objects define globally is refused by name. The side link would fail on it anyway;
this names it first.

The probe now links only the names steps 2 and 3 did not claim. The cmd/cgo-imports check applies to probe-bound
names only, as today.

## 3. What stays refused (each by name)

| Shape | Why |
|---|---|
| C++ (`.cc` `.cpp` `.cxx`; CXXFiles) | Needs CXX for each target and a C++ runtime in the side library. Later (sketch OQ-1). |
| Objective-C (`.m`), Fortran, `.syso` | The ObjC runtime, a Fortran compiler, prebuilt objects of unknown provenance. |
| A static archive named by `-l` that a name resolves to | The "resolved statically" half. Sketch OQ-2: admit a package-named archive and refuse C-runtime archives, decided on a measured case. |
| `#cgo pkg-config` | Phase 4's refusal stands. |
| `//export` | Phase 5's (`claude/h-cgo-p5-design`). |

cgoRecurse_test.go's route-refusal fixture (bad.c) moves to `.cc`, which stays refused, and keeps holding the
refusal text (COORD's condition).

## 4. Toolchains (the owner's question, 2026-11-27)

Phase 4's readings used the runners' default compilers:
- MinGW-Builds gcc 15.2.0 on windows
- Ubuntu gcc 15.2.0 on linux
- Apple clang 17 on both darwin RIDs

The pinned toolchains (`.github/cgo-toolchain.ps1`: zig 0.17.0, llvm-mingw 20261020) were measured in the Phase 4
seat's round 8, at 823978ff1a. 4b's gates run on BOTH families:
- converter-cgo and cgo-e2e on the default compiler, and with `;cc=zig` (linux, darwin) and `;cc=llvm-mingw`
  (windows);
- the go-sqlite3 amalgamation compile under zig and under gcc. Its wall time is a reading in its own right, since the
  conversion pays it.

## 5. Red first, and the gates

1. **Reds**, one commit, tests only, each red today:
   - **Row 1:** TestCgoPhase4F rewritten as parity. The preamble's `isatty` (`_isatty` on windows) matches go run.
     Today it is refused.
   - **Row 2:** a package of two files. b.go calls `C.helper`, which a.go's preamble defines non-statically and b.go's
     only declares. Today it is refused (the cross-file gap).
   - **Row 3:** `TestCgoPhase4COwnCFileConvertsAndRunsAsGoDoes`. A function and a global defined in the package's own
     `own.c` and declared in the preamble, called and read from Go. Today it gets §2.7's refusal.
   - **Row 4:** a C file whose whole body is under `#ifdef GO2CS_NEVER` beside a preamble that defines what Go calls.
     Today it gets §2.7's refusal.
2. **Gates**, Phase 4's:
   - unfiltered go test, offline goldens for the classification order (synthesized symbol tables), and repoguard;
   - hosted converter-cgo and cgo-e2e, ×4 on the default compilers and ×4 on the pins, with READING lines.
3. **The go-sqlite3 reading, after the gates:** convert go-sqlite3 v1.14.53 (the kept module cache) on the hosted legs
   and record exactly where it stops. Expected: at callback.go's `//export`, which is Phase 5's. Any earlier stop is a
   4b defect or a new row.

## 6. Open questions

- **OQ-1:** a name defined both in the hosting unit and globally in another object. C would reject the duplicate at
  the link, and Go's link would too. Refuse by name before the link (§2, the closing rule)? Recommendation: yes.
- **OQ-2:** the amalgamation's compile cost per conversion. Measured, not assumed. A cache by content hash stays out
  of the first cut (sketch OQ-4).
