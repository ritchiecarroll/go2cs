# OsGetpagesize: the windows emission of record

The behavioral project `src/tests/Behavioral/OsGetpagesize`'s `package_info.cs` as the converter emits it on
`windows/amd64`, for C1 to apply to `claude/c1-darwin-exepath` as one commit (COORD ruling, ledger 2026-10-10 08:54,
option (a): goldens are the windows emission of record). This branch holds only that file, at its repository path,
and this README.

- Converter: built from `claude/c1-darwin-exepath` cd95f1e5958234bbb30962ff27af43a5bfc6bdc5 with `go build -o go2cs.exe`
  in `src/go2cs` (go1.24.13, GOTOOLCHAIN=local), as check-no-regression.ps1 builds it. The binary's sha256 on the i9:
  1bc34391fba329dad8d699a3b6bab7de8f4e7c80673ce3001530315ec1e065d2 (that host's build; the same hash on another host is
  not claimed).
- Command, the one check-no-regression.ps1 runs: `go2cs -go2cspath <tree>\src <tree>\src\tests\Behavioral\OsGetpagesize`,
  on the i9 (windows/amd64).
- Diff against the committed golden (blob 9973c62b4e -> 374e5bd55a): exactly two lines added, none removed, and no other
  file of the project changes:
  - `global using syscallꓸHandle = go.syscall_package.ΔHandle;`
  - `global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;`
- The file is stored as the repository stores it (text with eol=crlf: LF in the object).
- Controls on windows, each with its own seat's converter: `OsExecutablePath` at cd95f1e595 and `ReexecArgv0Token` at
  `claude/c1-argv0-reexec` 76af080f11 both emit their committed goldens unchanged (0 files changed): neither needs this.
