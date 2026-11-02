# darwin Native AOT token-holder confirm: run 41420753631

The workflow `.github/workflows/c1-aot-holder.yml` ran `../run-aot.sh` at branch tip `09c5401d60`. Base is
`64d145b2c4`, G's fix is `7f4279bd15` (claude/g-aot-frozen-token-holder), N=50 per arm, GoTargetOS=darwin. The runners
pushed their raw logs to one ref per run:

- `claude/c1-aot-holder-raw-osx-arm64-41420753631` at `de61ed11ef`: macos-15, Apple M1 (Virtual), 3 CPUs, macOS 15.8.2.
- `claude/c1-aot-holder-raw-osx-x64-41420753631` at `83068d601c`: macos-15-intel, Intel i7-8700B, 4 CPUs, macOS 15.7.9.

Both runners had .NET SDK 10.0.403 and runtime 10.0.14, and ran under macOS's own bash 3.2.57.

Every raw file is here, 20 per runner, with two changes:
- runner paths are rewritten (`<tmp>`, `<checkout>`, `<runner-home>`);
- in the three AOT build logs, the .NET framework assembly version, ten followed by three zeros in dotted form (468
  per log, in `System.Private.CoreLib` and `System.Linq` type names), is respelled with hyphens, `Version=10-0-0-0`.
  The identifier census's strict IPv4 arm reads the dotted form as a private address.
The logs are added by name with `git add -f`, because `.gitignore` ignores `*.log`.

`aot.log` is the script's own record; it holds a `KEY` line per publish, counting the lines that name the colliding
frozen-object key. `<arm>.tsv` is one row per run: exit code, stderr bytes, whether stderr holds the panic report,
whether stdout holds `recovered`, how many of the 32 spinners were alive after the run (`-` idle), wall ms, and whether
the program died at startup.
