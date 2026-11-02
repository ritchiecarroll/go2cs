# The linux re-bank kit, 1.26.10.2

The kit runs the linux act of `docs/PLAN-rebank-go126.md` §12 and §3.2: one `-tests` invocation per row, with each
row's argv derived by AST from the sweep's own functions (`derive-argv.ps1`), never typed. It is frozen. These three
files are byte-identical to the copy smoked on R-WSL:

| File | sha256 |
|:--|:--|
| `rebank-linux.sh` | `99cdef2e8d0ba16a81e23eff0ebc6508a5d8f9af0edd4285fdb9c919c008a1c1` |
| `classify_row.py` | `81e543608d9cabce35e38ce0f2e6013cfcc3189fdc0642771c1c51cfc03d7fad` |
| `derive-argv.ps1` | `0ea6f0b90f915fc717853de71c7b9edbbeb895d11cfac044c1d5a18319f0a6d9` |

## Run it from a copy outside the clone, pins checked first

The kit checks the clone out at the tip and restores it after every row. So it must never run from inside the
clone it drives.

```
mkdir -p /root/rebank-kit
cp docs/phase4/data/rebank-1.26.10.2/linux-kit/{rebank-linux.sh,classify_row.py,derive-argv.ps1} /root/rebank-kit/
( cd /root/rebank-kit && sha256sum rebank-linux.sh classify_row.py derive-argv.ps1 )   # must equal the table above
TIP=<full tip sha> SHARD=<shard> LIST=<row list file> bash /root/rebank-kit/rebank-linux.sh
```

- SHARD is any name; LIST is any file with one row per line, in the order to run.
  - The evidence lands in `$R/<SHARD>.tsv`, with its ledger beside it.
  - `annotate_linux.py` reads only shard names of the declared wave shapes, such as `RWSL-1`, `P1-1` or `P2-2`.
- `$R` (default `/root/h10-<SHARD>`) is the evidence directory, outside the clone.
- Defaults that can be overridden from the environment:
  - `W=/root/go2cs`, the clone;
  - `GOROOT=/root/sdk/go1.26.10`;
  - `KIT=/root/rebank-kit`;
  - `FLOOR_GB=25`.
- Hard-coded: .NET at `/root/dotnet10`, which is both `DOTNET_ROOT` and on `PATH`. A box with .NET elsewhere needs
  that path, for example as a symlink.
- Needs on the box: bash, git, python3, pwsh 7, nslookup and df.
- The kit refuses unless all of these hold: `TMPDIR` is at most 10 bytes, `GOROOT` carries `test/`, and
  `nslookup invalid.invalid` answers NXDOMAIN.

## What it guards

- **The clone:** clean before the checkout, restored by status after every row, and the row's build output purged.
- **Shard end:** `git clean -fdXq -- src`, then an ABORT on any tracked deletion (safety floor 8).
- **The row word** (`classify_row.py`): PASS, DIVERGED, TIMEOUT, NOVERDICT, CONVERT or BUILD. A PASS whose invocation
  exited non-zero reads `PASS?rc`, which does not bank.
- **The derived argv** is `argv.txt`. The evidence directory's only `*.tsv` files are the shard's and its ledger, which
  `annotate_linux.py --evidence` reads.
