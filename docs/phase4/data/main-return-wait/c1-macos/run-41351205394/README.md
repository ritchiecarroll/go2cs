# Main-return wait, macOS leg: run 41351205394

The workflow `.github/workflows/c1-mrw-macos.yml` ran `../run-leg.sh` at branch tip `de94388e27`, on tip `bbf1725aa7`
and base `c5a4637bba`, N=50 per arm. The runners pushed their raw logs to two branches:

- `claude/c1-mrw-macos-raw-osx-arm64` at `577759e570`: macos-15, Apple M1 (Virtual), 3 CPUs, macOS 15.8.2.
- `claude/c1-mrw-macos-raw-osx-x64` at `539d4b7ab7`: macos-15-intel, Intel i7-8700B, 4 CPUs, macOS 15.7.9.

Both runners had .NET SDK 10.0.403 and runtime 10.0.14, and ran under macOS's own bash 3.2.57.

These files are those logs with runner paths rewritten (`<tmp>`, `<checkout>`, `<runner-home>`). Each AOT build log is
cut to its error lines (`*.errors.log`); the full logs, about 6,000 lines of trim warnings each, stay on the raw
branches.

Per run, `<arm>.tsv` holds: exit code, stderr bytes, whether stderr holds the panic report, whether stdout holds
`recovered`, how many of the 32 spinners were alive after the run (`-` idle), wall ms, and whether the program died at
startup (invalid). `<arm>.first-run.*` is each arm's first run, kept whole.

The first run, 41348661504, measured nothing past GolibTests, because the script used bash 4 associative arrays. It has no logs here.
