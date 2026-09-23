# AZ1 brief: crypto/tls at Go 1.24.13, full-host state, standard 600 s BoGo wall

You are lane **AZ1**, a temporary Azure Windows Server 2025 VM. Refer to it by that nickname only. You have one job: run
**one row**, `crypto/tls`, then report. If the row reaches the **full-host state** (section E.4), bank it; anything
short of that is evidence only. COORD makes every ruling. The owner performs every act on the VM itself, holds the
GitHub credentials, and owns the VM's lifetime.

## 0. Placeholders

| Placeholder | Meaning | Value |
|:--|:--|:--|
| `<STAMP>` | full 40-hex tip of `claude/version-go1.24.13` named in COORD's dispatch (the batch-8b STAMP, or 8c's if that lands first). Needed before section C | from the dispatch |
| `<NOTES>` | COORD's ruling on crypto/tls's stale manifest `notes` and roster prose (section C.5): the replacement text, or `hold`. Needed before C.5 | from the dispatch |
| `<SIZE>` | the VM size class as the owner stated it, for example "F-series v7, 48 vCPU". Never read it from the VM | from the dispatch |
| `<TIP>` | the commit the row runs at: `<STAMP>`, or the C.5 notes commit | derived |
| `<REPO>` | main clone (on `master`; the PARENT of both worktrees) | `C:\src\go2cs` |
| `<TREE>` | run and bank worktree | `C:\src\az1-tls` |
| `<EVTREE>` | evidence worktree | `C:\src\az1-ev` |
| `<GOROOT>` | go1.24.13, spelled exactly as `go env GOROOT` prints it | `C:\go124` |
| `<DOTNET>` | .NET SDK 10.0.400 root | `C:\dotnet10` |
| `<AZ>` | scratch root, outside every git tree | `C:\az1` |

`setup-az1.ps1` creates `<REPO>`, `<GOROOT>`, `<DOTNET>` and `<AZ>` (`C:\az1`, with its `dl` download folder);
you create the two worktrees. No path here contains a username.

**Security (owner order).** Never print, commit, push or send the VM's computer name, DNS host name or DNS suffix,
admin username, profile path, IP address, resource group, subscription or tenant. Use fleet nicknames only: i7, i9,
G-LAPTOP, R-LAPTOP, C1, C2, AZ1. A VM *size* such as "F-series v7, 48 vCPU" is fine to state; a resource *name* is
not. **Never query Azure's instance metadata service** (IMDS, the link-local metadata endpoint) by any route or
tool: one print of it carries the subscription, resource group, VM name and admin username. The size class comes
from `<SIZE>`.

**Which shell.** Every block below names its tool. **[PowerShell]** means the PowerShell tool (pwsh 7); **[Git Bash]**
means the Bash tool. Never run a [PowerShell] block through Git Bash: there `*>` becomes a glob and `C:\src\go2cs`
collapses to `C:srcgo2cs`.

## 1. Why this row, and what "standard wall" means

- **The goal is crypto/tls's FULL-host state.** It needs two things:
  - the pinned boringssl module downloads, so the BoGo runner exists;
  - the C# `TestBogoSuite` finishes inside **600 s**.

  Before this, only the i9 reached that state. The owner has barred the i9 from this row because of its CPU fault.
- **Where the 600 s comes from.** BoringSSL's runner starts a nested `go test .` with no `-timeout`, so that child
  gets Go's default 10-minute wall. **Only `GOFLAGS` reaches that child. `-TestTimeout` does not.** So an empty
  GOFLAGS at every scope is the point of this run. The comparison JSON does not record GOFLAGS, so your evidence
  must record it.
- **Control reading: R-LAPTOP at `d095fe8108`**, with `GOFLAGS=-timeout=40m` on 16 logical CPUs
  (`claude/r-tls-bogo-evidence` `7462befde0`). Result: VALIDATED, 4759 + 1. The C# BoGo runner took **1788.4 s**;
  Go's took 30.4 s.
- **The margin is thin.** Scaling R's time linearly to 48 logical CPUs gives about 596 s. That is an estimate,
  not a measurement, and both outcomes are plausible. More workers can also bring back the i9's 11 probable load
  flakes.
- **This is the first crypto/tls reading over batch 8b's delta** (`43d149b87d..<STAMP>`; 8c's too, if `<STAMP>` is
  8c's). The 8b battery did not run crypto/tls. If this row diverges, R's reading is the control, and the code-axis
  suspects are every code ref in that delta, not only golib's:
  - golib ref 10 (a typed nil func asserted to an interface adapts) and ref 11 (a native-rooted field ref's
    identity is its address);
  - runtime ref 11 (Windows `sysAllocOS` / `sysFreeOS` by direct P/Invoke, `mem_windows`) and ref 12 (`cpuprof`);
  - syscall ref 7 (`zsyscall_windows.cs` re-emitted, 265 lines changed) and `internal/runtime/atomic` ref 6;
  - the converter refs 4, 8 and 9: `testConversion.go` (+118 lines: the `-tests` comparison instrument itself),
    `visitFuncDecl.go`, `linknameOperations.go` and `manualTypeOperations.go`.

  The list of record is
  `git -C C:\src\go2cs diff --stat 43d149b87d <STAMP> -- src/go2cs src/core/golib src/core/runtime src/core/syscall src/core/internal/runtime`.

## 2. Safety floors in play (CLAUDE.md numbering)

- **1** Run one conversion on this box at a time, and nothing else heavy while it runs. Time the Go side AFTER the
  row, never alongside it.
- **3** The wrapper passes the output directory as the second positional. Never call `go2cs` by hand for this row.
- **4** While the row runs, the tree's source is frozen. The wrapper you run is a per-run copy outside the tree.
  The owner must not re-run `setup-az1.ps1` while a lane tree exists (it refuses its fetch and ownership steps if
  one does).
- **6** Pass `-GoRoot` exactly as `go env GOROOT` prints it, with backslashes. A forward-slash spelling misroutes
  the emission and still exits reporting success.
- **7** Read `$LASTEXITCODE` on the line right after each native call, never through a pipe. Gate every step on
  the one before it.
- **8** Never `git add -A` or `git add .`. Stage by name. After anything that cleans, the unfiltered
  `git status --porcelain` must show no ` D` line.
- **9** Announce, then push, **fast-forwards included**: for any branch whose SHA you have already reported, send
  COORD the new SHA first, then push. Never force-push; a correction lands as a new commit on top.
- **12** Have at least 25 GB free before the row. Never remove `<REPO>`: it is the PARENT of both worktrees.
- **13** Before relying on a gate, make it fail once: the census self-test and the per-name token plants in A.5,
  and the page-count control in G.
- **16** Only an unfiltered command answers "is it clean". Never `| head` or `Select -First` a cleanliness check.

Also: **floor 5**. Never `Get-Process <name> | Stop-Process`.

## A. Preconditions

1. The owner has run `setup-az1.ps1`, and its readback is clean: `go env GOROOT` reads `C:\go124 (OK...)`, the
   SDK is 10.0.400, `lane trees` is 0, `user.useConfigOnly` is `true`, `commit identity` matches the fleet
   identity, and there are no WARNING lines.
2. **Owner acts** (you do not do these):
   - Install Claude Code and sign in. **Connect Remote Control on the AZ1 session**, so COORD's i7 session appears
     in this session's ListAgents.
   - Set `git config --global user.name` / `user.email` to the fleet's commit identity (the one on `7462befde0`).
     The setup already set `user.useConfigOnly true`, so git never synthesizes an identity from the account and
     the VM's DNS name.
   - **Prime the push credential at the RDP console, once:**
     `git -C C:\src\go2cs push --dry-run origin HEAD:refs/heads/claude/az1-auth-probe` (a dry run pushes nothing).
     In the Git Credential Manager dialog, prefer **Token** with a **fine-grained PAT for this repository only**,
     `Contents: read and write`, expiring in 1 to 2 days. A browser sign-in grants an OAuth token over all of the
     owner's repositories, on an internet-reachable VM.
   - **Names.** The admin username, and the computer name unless it is `AZ1` itself, should be distinctive:
     6 or more characters and not a dictionary word. The census refuses every occurrence of each (A.5), so a common
     word turns ordinary text into refusals. If a name is already short or common, tell COORD that fact, never
     the name.

   Never put a token on a command line or into a file.
3. **Recommended owner acts:** pause Windows Update for the day; make sure no Azure auto-shutdown falls within two
   hours of the run; limit the RDP inbound rule to the owner's own source address, or use JIT access or Bastion.
4. **Defender:** leave it as it is. Record `(Get-MpComputerStatus).RealTimeProtectionEnabled` [PowerShell]; a
   non-elevated read works. A non-elevated session cannot read exclusions (`Get-MpPreference` answers "Must be an
   administrator to view exclusions"), so the exclusion COUNTS come from the setup readback's `Defender
   exclusions` line, as COORD's dispatch quotes it; if it does not, record "exclusions: not read (non-elevated)".
   Never record exclusion paths. Scan drag on 3,418 shim spawns is an unmeasured variable, so note it; do not
   change it.
5. **Lane checks, in this order, before section C.** Each one gates the next.
   1. **COORD route.** ListAgents must show `coord -- Go corpus migration to 1.24.13 coordination`. If it does not,
      tell the owner in this session to connect Remote Control (they are at the console when the session starts),
      and continue only once it shows.
   2. **Commit identity [Git Bash].** Print nothing but these results:

      ```bash
      git config --global user.useConfigOnly                              # must print: true
      git -C /c/src/go2cs log -1 --format='%an%n%ae' 7462befde0 > /c/az1/ident-fleet.txt
      { git config --global user.name; git config --global user.email; } > /c/az1/ident-cfg.txt
      cmp -s /c/az1/ident-fleet.txt /c/az1/ident-cfg.txt; echo "identity rc=$?"   # must be 0
      ```

      On any other result, send COORD an OWNER-HAND line (section J) and stop.
   3. **Push route [PowerShell].** Write `C:\az1\push-env.ps1` with the single line
      `$env:GCM_INTERACTIVE = 'Never'; $env:GIT_TERMINAL_PROMPT = '0'`. Dot-source it in EVERY call that pushes,
      probes or runs `ls-remote`, and never in the row. A missing credential then fails at once instead of
      raising a desktop dialog that no one sees at the end of the run. Probe the route now:

      ```powershell
      . C:\az1\push-env.ps1
      git -C C:\src\go2cs push --dry-run origin HEAD:refs/heads/claude/az1-auth-probe
      $rc = $LASTEXITCODE; "auth probe rc=$rc"                           # must be 0; a dry run pushes nothing
      ```

      If rc is not 0: OWNER-HAND (the push credential, A.2) and stop. Never answer a credential prompt yourself.
   4. **The branch names are free [PowerShell].**

      ```powershell
      . C:\az1\push-env.ps1
      git -C C:\src\go2cs ls-remote --exit-code origin refs/heads/claude/az1-tls-bank refs/heads/claude/az1-tls-evidence
      $rc = $LASTEXITCODE; "ls-remote rc=$rc"                            # must be 2: neither exists
      ```

      rc 0 means one exists: stop and ask COORD. Any other rc: stop and ask.
   5. **The never-push token file [PowerShell].** The census has a local never-push token file for names it cannot
      know, which here means this VM's own names. A name listed there turns the census's `RUNTIME_ACCOUNT` and
      `RUNTIME_MACHINE` arms live, and its `TOKENFILE` arm refuses that name in every mode, even on upstream fixtures,
      with masked output. The census reads `$HOME/.claude/coord-identifier-tokens` on its own. Do NOT use
      `.claude/coord-scripts/coord-identifier-tokens.local`: the i7 ignores that name only through `.git/info/exclude`,
      so in this clone it would show as untracked.

      ```powershell
      $tf = Join-Path $env:USERPROFILE '.claude\coord-identifier-tokens'
      $sfx = @(@(Get-DnsClient).ConnectionSpecificSuffix | Where-Object { $_ } | ForEach-Object { $_.Split('.')[0] })
      $names = @(@($env:COMPUTERNAME, $env:USERNAME, [Net.Dns]::GetHostName(), (Split-Path $env:USERPROFILE -Leaf)) + $sfx |
          Where-Object { $_ -and $_.Length -ge 4 -and $_ -ine 'AZ1' })
      New-Item -ItemType Directory -Force (Split-Path $tf) | Out-Null
      [IO.File]::WriteAllLines($tf, [string[]] $names)
      "token lines: $($names.Count) (DNS-suffix labels: $($sfx.Count))"   # COUNTS; never print the file
      ```

   6. **Census readiness [Git Bash].** `C=/c/src/go2cs/.claude/coord-scripts/coord-identifier-census.sh`, run from
      master. Tool calls keep no shell state, so set `C=` at the top of EVERY [Git Bash] call that uses it. It is
      the fleet's one census, and the rule is that no tool carries a private copy of any arm, so there is no
      separate own-name check.
      - `bash "$C" selftest` must print `SELF-TEST PASSED`. Its denied-set control fires only on a denied name this
        box can derive, and here that is a piece of the fleet identity from step 2, which is why step 2 comes first.
      - **The header.** The census must see the token file, with both host arms live:

        ```bash
        printf 'an ordinary line\n' > /c/az1/probe.txt
        bash "$C" entry /c/az1/probe.txt < /dev/null > /c/az1/probe.out 2>&1; echo "rc=$?"
        grep -E 'token file:|arm inert|SKIPPED' /c/az1/probe.out
        ```

        Require rc 0, `token file: yes`, `RUNTIME_ACCOUNT=` of 1 or more, `RUNTIME_MACHINE=1`, and no
        `RUNTIME_ACCOUNT:` or `RUNTIME_MACHINE:` line that says `arm inert`. A `RUNTIME_OWNERNAME: ... arm inert` line
        is expected, because one piece of the fleet identity is not in the denied set. One exception is accepted: if
        the computer is named `AZ1` itself, `RUNTIME_MACHINE` reads `SKIPPED` (under 4 characters).
      - **Plant every name through the census (floor 13).** Each plant must read `REFUSED by TOKENFILE`, and the
        number of plants must equal step 5's token-line count:

        ```bash
        TF="$HOME/.claude/coord-identifier-tokens"; n=0
        while IFS= read -r t || [ -n "$t" ]; do t="${t%$'\r'}"; [ -n "$t" ] || continue; n=$((n + 1))
          printf 'owner column reads %s here\n' "$t" > /c/az1/plant.txt
          bash "$C" entry /c/az1/plant.txt < /dev/null > /c/az1/plant.out 2>&1; rc=$?
          if [ "$rc" -eq 1 ] && grep -Eq '^ +refuse +TOKENFILE +occ=[0-9]+ +hits=[1-9]' /c/az1/plant.out
          then echo "plant $n: REFUSED by TOKENFILE"; else echo "plant $n: NOT REFUSED (rc $rc) -- STOP"; fi
        done < "$TF"
        rm -f /c/az1/plant.txt /c/az1/plant.out /c/az1/probe.txt /c/az1/probe.out
        ```

   7. Send COORD the **ready** message (section J): the blob SHA of the brief you read, the step results above as
      counts and rcs, and the section B readbacks once you have them.

## B. The shell

Tool calls do not keep environment variables between calls. Write `<AZ>\env.ps1` once, then **dot-source it at
the top of every [PowerShell] call** that touches the row:

```powershell
$env:GOROOT = 'C:\go124'; $env:GOTOOLCHAIN = 'local'; $env:CGO_ENABLED = '0'
$env:DOTNET_ROOT = 'C:\dotnet10'; $env:PATH = "C:\go124\bin;C:\dotnet10;$env:PATH"
$env:DOTNET_NOLOGO = '1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'; $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
Remove-Item Env:GOFLAGS, Env:GODEBUG, Env:GOPROXY, Env:GOSUMDB, Env:GONOSUMDB, Env:GOPRIVATE, Env:GONOPROXY -ErrorAction SilentlyContinue
```

The wrapper sets the Go half of this itself but NOT the .NET half, which is why this file exports it. Leave
`GODEBUG` unset: the tree's winsymlink host seat applies `winsymlink=0` only on a junction fallback, and it
defers to any preset value. Leave `GOPROXY` and `GOSUMDB` at their defaults: both sides fetch boringssl through
the public module proxy. `TEMP`, the module cache, `GOCACHE` and the NuGet cache stay under the profile, as on R;
section F.6 says how a profile path in failure text is handled.

**Read these back BY OUTPUT and quote them verbatim in the evidence.** If any value is wrong, stop and report.

| Check | Expected |
|:--|:--|
| `go version` | `go1.24.13 windows/amd64` |
| `go env GOROOT` | `C:\go124` |
| `(Get-Command go).Source` | under `C:\go124\bin` |
| `go env GOFLAGS` | empty |
| `GOFLAGS` at User and Machine scope (`[Environment]::GetEnvironmentVariable('GOFLAGS','User')`, then `'Machine'`) | empty |
| `GODEBUG` at process, User and Machine scope | empty |
| `go env GOPROXY` | `https://proxy.golang.org,direct` |
| `go env GOSUMDB` | `sum.golang.org` |
| `dotnet --version` | `10.0.400` |
| `(Get-Command dotnet).Source` | `C:\dotnet10\dotnet.exe` |

Also record these values, with no expected value: `dotnet --list-runtimes`, `$PSVersionTable.PSVersion`
(expect 7.x), `[Environment]::ProcessorCount`, the CPU model, RAM, and free GB on C:.

## C. Refs and the tree

1. **[PowerShell]** Refresh the refs, and fast-forward the main checkout so it stays on `master` (it carries
   CLAUDE.md, the rules, the skills and the census):

   ```powershell
   git -C C:\src\go2cs fetch origin --prune;           $rc = $LASTEXITCODE; "fetch rc=$rc"
   git -C C:\src\go2cs merge --ff-only origin/master;  $rc = $LASTEXITCODE; "ff rc=$rc"
   ```
2. **[PowerShell] Assert the stamp.**
   - `git ls-remote origin refs/heads/claude/version-go1.24.13` (after `. C:\az1\push-env.ps1`) must print
     `<STAMP>`, all 40 hex.
   - The last `STAMP` line of `git show origin/claude/mailbox:docs/phase4/LEDGER.md` carries a 9- or 10-character
     short SHA (for example `2026-09-22 23:14 · STAMP · bb54ff0920 · ...`). That hash must be a **prefix** of
     `<STAMP>`. Match the line with `'^\d{4}-\d\d-\d\d \d\d:\d\d \S+ STAMP \S+ ([0-9a-f]{7,40})\b'`, which holds
     even if the console mangles the middle dot, then test `'<STAMP>'.StartsWith($Matches[1])`.
   - If they disagree, stop and ask COORD. `-ExpectTip` is an exact string compare, so always pass the full
     40-hex SHA.
3. **[Git Bash] Pick the wrapper and make a per-run copy.** Run
   `git -C /c/src/go2cs merge-base --is-ancestor d095fe8108 <STAMP>; echo "rc=$?"`.
   - rc 0 (8c carries it): `<SRC>` = `<STAMP>`.
   - rc 1 (the 8b stamp): `<SRC>` = `d095fe8108`. Its wrapper blob starts `f17cc5b437`.
   - Any other rc (128 is a missing object): stop and ask COORD.

   **Do not merge or cherry-pick `d095fe8108` into the tree.** For crypto/tls the argv is identical either way
   (no execution pin, 30m floor). This way the page's converter SHA stays one that origin has, and 8c boards
   `d095fe8108` by its own merge. The wrapper derives its floors, roster and functions from `-Tree`, so a copy
   works. Copy it so the bytes are exact, then prove the copy matches:

   ```bash
   git -C /c/src/go2cs show <SRC>:src/run-h10-recon.ps1 > /c/az1/run-h10-recon.ps1
   git hash-object --no-filters /c/az1/run-h10-recon.ps1        # must equal:
   git -C /c/src/go2cs rev-parse <SRC>:src/run-h10-recon.ps1
   ```
4. **[PowerShell] Create the tree.** A banking tree sits ON A BRANCH, which is why the row passes `-AllowBranch`.
   A.5 step 4 has already proved both branch names free on origin.
   `git -C C:\src\go2cs worktree add -b claude/az1-tls-bank C:\src\az1-tls <STAMP>`. Then the unfiltered
   `git -C C:\src\az1-tls status --porcelain` must be empty.
5. **Apply the notes ruling `<NOTES>`.** The hand-owned `src/core/crypto/tls/go2cs_test_disclosures.json`
   `notes[0]` still says four expired-fixture tests fail. At 1.24.13 all four pass on both sides, and the proof
   page prints `notes` word for word.
   - **If `<NOTES>` is text:** edit that one string with the Edit tool; never re-serialize the JSON. Prove it
     still parses with `Get-Content -Raw ... | ConvertFrom-Json` [PowerShell]. Commit it alone, unsigned, after
     the checks in section F. `<TIP>` = that commit.
   - **If `<NOTES>` is `hold`:** make no commit. `<TIP>` = `<STAMP>`, and your report says the page carries the
     stale note.
   - **If the dispatch carries no ruling:** ask COORD before this step. Do not write prose of your own.

   In every case, never touch the `TestBogoSuite` host-limit pin or its reason text. It stays (ledger 15:59), and
   refreshing its 1.23.12 figures is C1's re-sign item.
6. **[PowerShell] Build the converter under the pins.** In `C:\src\az1-tls\src\go2cs`, dot-source `env.ps1`, run
   `go build -o go2cs.exe .`, and require rc 0. The build needs proxy.golang.org, and it seeds the module cache
   that the BoGo fetch reuses.
7. **[PowerShell] Take the residue census, unfiltered.** `git -C C:\src\az1-tls status --porcelain --ignored=matching`
   must print exactly one line: `!! src/go2cs/go2cs.exe`. If anything else appears, stop.
8. **Check disk:** at least 25 GB free on C:. The wrapper also refuses below that; check first anyway.

## D. The row (all [PowerShell])

1. **Inputs.** Write `C:\az1\tls.txt` with the single line `crypto/tls`.
   - Out file: `C:\az1\rows.tsv`.
   - Scratch: `C:\az1\scratch`. The wrapper proves it sits outside every git tree.
2. **Write `C:\az1\row.ps1`.** It deletes its own rc file first and writes it in `finally`, so a stale value can
   never be read: an absent `row.rc` means the run never reached its end.

   ```powershell
   param([switch] $SelfTest)
   $rcFile = if ($SelfTest) { 'C:\az1\selftest.rc' } else { 'C:\az1\row.rc' }
   Remove-Item -LiteralPath $rcFile -ErrorAction SilentlyContinue
   $rc = 'THREW'
   try {
       . C:\az1\env.ps1
       if ($env:GOFLAGS -or (go env GOFLAGS)) { Write-Host 'GOFLAGS is set -- refusing: the standard wall is the point'; $rc = 3 }
       else {
           $global:LASTEXITCODE = $null
           & C:\az1\run-h10-recon.ps1 -NameList C:\az1\tls.txt -Tree C:\src\az1-tls -GoRoot C:\go124 `
               -Out C:\az1\rows.tsv -ExpectTip <TIP> -Scratch C:\az1\scratch -TestConfig Release -TestTimeout 90m -AllowBranch -SelfTest:$SelfTest
           $rc = if ($null -eq $LASTEXITCODE) { 'NO-EXIT-CODE' } else { $LASTEXITCODE }
       }
   } catch {
       $rc = "THREW: $($_.Exception.Message)"; Write-Host $rc
   } finally {
       Set-Content -LiteralPath $rcFile -Value $rc
   }
   if ($rc -is [int]) { exit $rc } else { exit 97 }
   ```

   About the two arguments:
   - **`-TestTimeout 90m`** matches R's ask. The wrapper keeps the longer of the ask and the tree's derived 30m
     floor, and prints both; never pass less than the floor. This value sets the outer oracle and C# host
     deadlines. It does NOT move the BoGo wall.
   - **`-AllowBranch`** is needed because this tree is a banking tree on a branch.
3. **Self-test first:** `pwsh -NoProfile -File C:\az1\row.ps1 -SelfTest` must print `SELF-TEST PASSED`, and
   `C:\az1\selftest.rc` must read `0`. The self-test never touches `row.rc`.
4. **Nothing else running.** Take one process listing: no `go`, `go2cs`, `dotnet`, `MSBuild`, `VBCSCompiler` or
   test host may be alive. Note it for the evidence.
5. **Launch in the background, from the PowerShell tool.** Use the tool's `run_in_background`; the harness wakes
   you when the row exits, so no sleep-polling. Run exactly:
   `Remove-Item C:\az1\row.rc -ErrorAction SilentlyContinue; pwsh -NoProfile -File C:\az1\row.ps1 *> C:\az1\run.log`.
   On R this took about 37 minutes; here it should take less.
6. **Copy the artifacts out the moment it exits, before anything else touches the tree.** Copy
   `C:\src\az1-tls\src\core\crypto\tls\go2cs_test_results.json` and `go2cs_test_comparison.json` to
   `C:\az1\keep\`, and record each file's SHA-256 and size. The wrapper keeps only a bounded tail of the results
   file, which is one line of about 1.7 MB. The C# BoGo wall is recorded only in the full file.

## E. Read, in this order (all [PowerShell])

1. `C:\az1\row.rc`, the background task's own exit code, then the last 60 lines of `run.log` (read them; quote
   nothing from them before the census).
   - `row.rc` absent: row.ps1 never reached its end (a reboot or a kill). That is NOVERDICT; report it.
   - `THREW: ...` or `NO-EXIT-CODE`: a terminating error escaped the wrapper. The task's exit code reads 97.
   - Otherwise the task's exit code must equal `row.rc`. If they disagree, report both.
2. **The results tail FIRST (floor 14).** Read the last ~8 KB of the results file, plus
   `C:\az1\scratch\crypto__tls\results-tail.txt`. A deadline kill states itself, for example
   `test timed out after 10m0s`. Name it if it is there.
3. The row in `rows.tsv`: `word`, `verdicts`, `sweep_s`, `wall_s`.
4. **The comparison JSON and the FULL-HOST gate.** Its members are `package`, `status`, `go`, `csharp`, `matched`
   (a boolean), `skipped`, `disclosed` (strings of the form `Name (class): reason`), `excluded`, `errors`,
   `orphanedDisclosures`, `environment`, and `withdrawn` when the host withdrew Go-side leaves. There is no
   divergences member, so the block derives the diverging names from `go` against `csharp`, minus the disclosed
   names, and gates on that derivation too (a second reading beside `status`). Run it as ONE [PowerShell] call.

   ```powershell
   $j  = Get-Content -Raw C:\az1\keep\go2cs_test_comparison.json | ConvertFrom-Json
   $gn = @($j.go.PSObject.Properties.Name); $cn = @($j.csharp.PSObject.Properties.Name)
   $dis = @($j.disclosed | Where-Object { $_ }); $od = @($j.orphanedDisclosures | Where-Object { $_ })
   $wd = @($j.withdrawn | Where-Object { $_ })
   $gB = @($gn -cmatch '^TestBogoSuite(/|$)').Count; $cB = @($cn -cmatch '^TestBogoSuite(/|$)').Count
   $sum = Get-Content -Raw C:\az1\scratch\crypto__tls\summary.txt -ErrorAction SilentlyContinue
   $dn  = @($dis | ForEach-Object { ("$_" -split ' ', 2)[0] })
   $all = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal); foreach ($n in $gn + $cn) { [void] $all.Add($n) }
   $div = @($all | Where-Object { $j.go.$_ -cne $j.csharp.$_ -and $dn -cnotcontains $_ } | Sort-Object)
   $full = [ordered]@{
     'no undisclosed divergence (derived)'    = ($div.Count -eq 0)
     'status validated, matched true'         = ($j.status -eq 'validated' -and $j.matched -eq $true)
     'go 4760 / C# 4760'                      = ($gn.Count -eq 4760 -and $cn.Count -eq 4760)
     'BoGo fan-out 3419 / 3419'               = ($gB -eq 3419 -and $cB -eq 3419)
     'TestBogoSuite pass / pass'              = ($j.go.TestBogoSuite -eq 'pass' -and $j.csharp.TestBogoSuite -eq 'pass')
     'disclosed = TestCertCache only'         = ($dis.Count -eq 1 -and "$($dis[0])".StartsWith('TestCertCache ('))
     '4759 matched (summary line)'            = ("$sum" -match '^Validated 4759 tests ')
     'withdrawn absent'                       = ($wd.Count -eq 0)
     'errors empty'                           = (@($j.errors | Where-Object { $_ }).Count -eq 0)
     'orphan = TestBogoSuite host-limit p/p'  = ($od.Count -eq 1 -and $od[0].name -eq 'TestBogoSuite' -and $od[0].class -eq 'host-limit' -and $od[0].go -eq 'pass' -and $od[0].csharp -eq 'pass')
   }
   $full.GetEnumerator() | ForEach-Object { '{0} {1}' -f $(if ($_.Value) { 'PASS' } else { 'FAIL' }), $_.Key }
   "go $($gn.Count) / C# $($cn.Count); BoGo fan-out go $gB / C# $cB; disclosed $($dis.Count); withdrawn $($wd.Count)"
   "diverging (disclosed removed): $($div.Count)"; $div | ForEach-Object { '{0}  go={1}  cs={2}' -f $_, $j.go.$_, $j.csharp.$_ }
   "FULL-HOST STATE: $(if (@($full.Values | Where-Object { -not $_ }).Count -eq 0) { 'YES' } else { 'NO' })"
   ```

   The gate was proved on the i7 before this brief went out (2026-09-23): R's `7462befde0` record reads 10/10 and
   YES; a planted third state (the BoGo leaves moved to `withdrawn`, C#'s root failed) reads NO on six checks; a
   planted single flip reads NO on the derived-divergence check alone.

   **Name the state.** The wrapper word alone cannot, because two short states also come back as `PASS` with
   status `validated`:
   - **VALIDATED:** `FULL-HOST STATE: YES`. Only this state banks (section G).
   - **THIRD-STATE, the wall finding:** the runner exists but C# missed the 600 s wall. Go's `TestBogoSuite`
     passes and fans out; C#'s fails and the host-limit disclosure absorbs it; `withdrawn` holds the Go-side
     BoGo leaves (about 3418); the count reads about 1340 matched + 2 disclosed. G-LAPTOP read exactly this as
     `PASS 1340` (`c660a17d8c`).
   - **NO-RUNNER, the network finding:** `TestBogoSuite` fail/fail with no `TestBogoSuite/` names on either side.
     The boringssl fetch failed (`failed to download boringssl`), and the host-conditional signature admits it,
     so this state can also read validated. It is network, not a verdict.
   - **DIVERGED, NOVERDICT, TIMEOUT:** the wrapper's own words.
   - Any other `NO`: name the failing checks and treat it as short of the full-host state.
5. **The C# `TestBogoSuite` wall.** In `go2cs_test_results.json`, find the `events[]` entry with
   `test == 'TestBogoSuite'` and `action` of pass, fail or skip. Its `elapsed` is in seconds. Compare it with
   600 s and state the margin.
6. **Go's wall, AFTER the row.** In `C:\go124\src\crypto\tls`, with the same `env.ps1`, run
   `go test -count=1 -timeout 30m -tags purego,math_big_pure_go -run '^TestBogoSuite$' -json .`. Read the
   `TestBogoSuite` pass event's `Elapsed` (R measured 30.4 s). `-count=1` keeps a cached result from reading as
   a wall.
7. **The i9's 11 cases, by name.** Read
   `git show 7462befde0:docs/phase4/h10-evidence/r-tls-bogo/i9-vs-r.tsv`. For each case, look up AZ1's verdict on
   both sides, and write `az1-vs-i9-vs-r.tsv` with the columns
   `case i9_go i9_cs R_go R_cs AZ1_go AZ1_cs`.

**Score these predictions exactly as written:**
- status `validated`; go 4760 / C# 4760;
- 4759 matched + 1 disclosed (`TestCertCache`, codegen-liveness);
- block 3419 on both sides; `errors` `[]`; no `withdrawn`;
- `orphanedDisclosures` = `[TestBogoSuite host-limit, pass/pass]`;
- wrapper word `PASS verdicts=4759`;
- C# `TestBogoSuite` under 600 s. This one is a coin-flip-thin prediction.

## F. Census and hygiene before ANY commit, push or message

The census is `C` from A.5 (set it at the top of each call), and its token file makes it the own-name check as
well. Every census call below runs in **[Git Bash]**.

1. **Converted test artifacts:** `*_test.cs`, `package_test_info.cs`, `go2cs_test_host.cs`, `*.tests.csproj`, and
   `docs/validation/current/*.md`. From inside the tree, run `cd /c/src/az1-tls && bash "$C" converted <files...>`
   and require exit 0.
2. **Everything else, one file per call:** the roster, the manifest, `README.md`, testdata,
   `run-validated-sweep.ps1`, each evidence file, each commit-message body file, and each report (section J).
   Run `bash "$C" entry <file>` and require `CLEAN`. Check each commit subject with `bash "$C" subject "<subject>"`.
   Always commit with `git commit -F <censused file>`.
3. **Commit identity, after EVERY commit and before EVERY push.** Author and committer headers sit outside every
   diff, message and subject the census reads.

   ```bash
   T=/c/src/az1-tls        # /c/src/az1-ev for the evidence commit
   git -C "$T" log -1 --format='%an%n%ae%n%cn%n%ce' > /c/az1/ident.txt
   git -C /c/src/go2cs log -1 --format='%an%n%ae%n%cn%n%ce' 7462befde0 > /c/az1/ident-fleet4.txt
   cmp -s /c/az1/ident.txt /c/az1/ident-fleet4.txt; echo "identity rc=$?"      # must be 0
   bash "$C" entry /c/az1/ident.txt
   ```

   Byte equality with the fleet identity is the gate: those bytes are already public on every fleet commit. The
   census on `ident.txt` is a second reading, and it REFUSES by design on `RUNTIME_OWNERNAME`. The name lines carry
   the owner's name, which the denied set lists; the email line clears as the ruled public handle (measured on the
   i7, 2026-09-22). So read it by arm: `TOKENFILE`, `RUNTIME_ACCOUNT` and `RUNTIME_MACHINE` must each read `hits=0`.
   If `cmp` fails, do not push. Stop and ask COORD: a pushed identity cannot be recalled.
4. **Optional second derivation, count only [PowerShell].** If the census and this disagree, stop and report both
   readings; the census is the gate.

   ```powershell
   $T = 'C:\src\az1-tls'   # the tree whose index you are about to commit: C:\src\az1-ev for the evidence commit
   $terms = @(Get-Content (Join-Path $env:USERPROFILE '.claude\coord-identifier-tokens') | Where-Object { $_ })
   $d = @(git -C $T diff --cached); if ($LASTEXITCODE) { throw "git diff --cached failed: $LASTEXITCODE" }
   $added = @($d | Where-Object { $_.StartsWith('+') -and -not $_.StartsWith('+++') })
   "own-name hits (added lines): $(@($added | Select-String -SimpleMatch -Pattern $terms).Count)"
   ```

5. **A census refusal on upstream test data** (bytes that came from Go, not from this host): do not edit those
   bytes. Hold that file, and ask COORD.
6. **A refusal on a file you authored**: EVIDENCE.md, failures.txt, `az1-vs-i9-vs-r.tsv`, a message, or a report.
   Name the FILE and the ARM, never the value. Redact in place with the census's admitted placeholders: a profile
   segment becomes `Users\<user>` (so `C:\Users\<user>\AppData\Local\Temp\...`), and any other VM name becomes
   `<host>`. Then re-run the census. **Never edit a file the wrapper, the converter or Go wrote**: the comparison and
   results JSON, `summary.txt`, `output-no-summary.txt`, `results-tail.txt`, `rows.tsv`. If one refuses, keep it in
   `C:\az1\keep\`, record its SHA-256 and size in EVIDENCE.md, and quote from it only in redacted form.

## G. FULL-HOST STATE ONLY: bank on `claude/az1-tls-bank`

**The gate.** Bank ONLY IF section E.4 printed `FULL-HOST STATE: YES`. All ten of its checks must hold:
- status `validated`, `matched` true, and no undisclosed divergence by E.4's own derivation;
- go 4760 / C# 4760;
- BoGo fan-out 3419 / 3419 on both sides;
- `TestBogoSuite` pass / pass;
- 4759 matched + 1 disclosed, with `TestCertCache` the only disclosure;
- no `withdrawn`, and `errors` empty;
- `orphanedDisclosures` = exactly the `TestBogoSuite` host-limit entry, pass / pass.

**Anything else is evidence only.** That includes THIRD-STATE, NO-RUNNER, DIVERGED, NOVERDICT and TIMEOUT. Go to
section I, make no commit on this branch, and never push it.

Load the **validation-bank** skill first. The model is G's `c660a17d8c`: test artifacts, the README badge, the
proof page and one roster row cell. No index, no header. All lane commits are unsigned.

**Commit 1.** The notes commit from C.5, if `<NOTES>` was text.

**Commit 2: the artifacts.** Stage every path by name:

- **Under `src/core/crypto/tls/`, excluding `src/core/crypto/tls/internal/`:**
  - the regenerated test sources, including the NEW `fips_test.cs`;
  - `package_test_info.cs`, `go2cs_test_host.cs` and `crypto.tls.tests.csproj`;
  - `README.md`, which carries the badge;
  - the **eight testdata files that changed at 1.24.13**, each proved BYTE-equal to its GOROOT source:

    ```powershell
    $names = 'Client-TLSv10-ClientCert-ECDSA-ECDSA', 'Client-TLSv10-ClientCert-ECDSA-RSA',
             'Client-TLSv12-ClientCert-ECDSA-ECDSA', 'Client-TLSv12-ClientCert-ECDSA-RSA',
             'Client-TLSv13-ClientCert-ECDSA-RSA', 'Server-TLSv10-ECDHE-ECDSA-AES',
             'Server-TLSv12-ECDHE-ECDSA-AES', 'Server-TLSv13-ECDHE-ECDSA-AES'
    foreach ($n in $names) {
        $a = git -C C:\src\az1-tls hash-object --no-filters "src/core/crypto/tls/testdata/$n"; $ra = $LASTEXITCODE
        $b = git hash-object --no-filters "C:\go124\src\crypto\tls\testdata\$n"; $rb = $LASTEXITCODE
        '{0,-40} {1}' -f $n, $(if ($ra -eq 0 -and $rb -eq 0 -and $a -and $a -eq $b) { 'BYTE-EQUAL' } else { "DIFFERS (rc $ra/$rb)" })
    }
    ```

    All eight must read `BYTE-EQUAL`. This repository marks testdata `-text` (`git check-attr`: text unset), so it is
    never CRLF-converted. **Any other testdata file that shows as modified is therefore not a phantom: HOLD it and
    ask COORD.**
- **`docs/validation/current/crypto.tls.md`:** the page the run wrote.
- **`docs/ValidatedTestPackages.md`: the crypto/tls ROW only.**
  - Tests goes from 3643 to **4759** (the page's matched count), and Disclosed stays **1**.
  - Update the prose per `<NOTES>`. On `hold`, change the cells only and name the stale prose in your report.
  - Write no `|` character in any prose you add. The roster check's section 3 requires exactly five unescaped
    pipes per row.
  - Keep the `linux: 401 + 1` annotation.
  - **Never** touch the header, `docs/validation/index.md` (the run rewrote it; leave it unstaged) or the README
    NEWS block. COORD derives those.
- **Production `.cs` files the run re-emitted.** Classify each one by the skill's shapes: CRLF phantom, `-tests`
  closure, or one-way hook.
  - Bank what the skill says to bank.
  - Leave unstaged what it says to restore.
  - HOLD anything you cannot classify, and ask COORD.

  List every unstaged modified file, with its class, in your report.

**Then check:**
- `git diff --cached --stat` matches the list above exactly.
- The unfiltered `git status --porcelain` shows no ` D` line.
- Run `pwsh -NoProfile -File C:\src\az1-tls\src\check-roster-format.ps1` [PowerShell]. The prediction: red ONLY at
  §2 (header arithmetic) and §2e (NEWS), both of which COORD derives. Green at §2b, 2b2, 2b3, 2c, 2d, 2f and 3
  (rendered-table integrity), with 2f's checked count up by one. Any other red: stop and report.

**Commit 3: BlockSize, in a commit of its own [PowerShell].** Count the committed page's `TestBogoSuite` verdict
rows with the sweep's own predicate:

```powershell
function Count-Block($rev, [switch] $All) {
  $page = @(git -C C:\src\az1-tls show "${rev}:docs/validation/current/crypto.tls.md")
  if ($LASTEXITCODE) { throw "git show ${rev} failed: $LASTEXITCODE" }
  $in = $false; $n = 0
  foreach ($l in $page) {
    if ($l -match '^##\s') { $in = [bool]($l -match '^##\s+Verdicts\b'); continue }
    if ($in -and $l -match '^\|\s*`([^`]+)`\s*\|') { $x = $Matches[1]
      if ($All -or $x -eq 'TestBogoSuite' -or $x.StartsWith('TestBogoSuite/')) { $n++ } } }
  $n }
Count-Block '<STAMP>'       # CONTROL: 3243, the 1.23.12 page the current literal was derived from
Count-Block 'HEAD'          # 3419
Count-Block 'HEAD' -All     # 4760 verdict rows in total
```

- The control must read 3243, HEAD must read 3419, and the page must list 4760 verdict rows in total.
- If all three hold, change ONLY the literal in `src/run-validated-sweep.ps1`, from
  `'crypto/tls' = @{ Test = 'TestBogoSuite'; BlockSize = 3243 }` to `3419`.
- If any one fails, make no commit 3, and do not push the bank branch. Report the three readings and hold for
  COORD.
- Its real gate belongs to the merge result's sweep on a reduced host: red at 3243 by the named reason, green at
  3419.
- The 1.24.13 comment refresh and the crypto/tls floor re-check are COORD's seat. The rule there is to raise the
  floor if the row's wall is at least 1,350 s, so report `wall_s`.

**Push: only if all three Count-Block assertions held and commit 3 exists,** and only after F.3's identity check on
HEAD [PowerShell]:

```powershell
. C:\az1\push-env.ps1
git -C C:\src\az1-tls push -u origin claude/az1-tls-bank;                  $rc = $LASTEXITCODE; "push rc=$rc"
git -C C:\src\az1-tls ls-remote origin refs/heads/claude/az1-tls-bank;     $rc = $LASTEXITCODE; "ls-remote rc=$rc"
```

The ls-remote SHA must EQUAL local HEAD. Any later commit on this branch follows floor 9: announce, then push.

## H. Evidence branch `claude/az1-tls-evidence` (push it in EVERY outcome)

This VM is not on the fleet LAN and it is temporary. Evidence reaches COORD only by git.

1. **Create the evidence tree** at the commit the row ran on [PowerShell]:
   `git -C C:\src\go2cs worktree add -b claude/az1-tls-evidence C:\src\az1-ev <TIP>`.
2. **Add these files** under `docs/phase4/h10-evidence/az1-tls-bogo/`:
   - `EVIDENCE.md`, in the shape of R's: the run table, the word and counts, the E.4 gate lines, the fan-out and
     walls table, the 11 cases, and your class;
   - `go2cs_test_comparison.json`, `results-tail.txt`, `rows.tsv` and `az1-vs-i9-vs-r.tsv`;
   - `summary.txt`, or `output-no-summary.txt` when that is what the wrapper wrote. The wrapper writes the second
     only when no summary line printed (the DIVERGED and no-verdict shapes), and it is the whole converter stdout,
     so it will likely carry profile and NuGet-cache paths. It goes through the census like every other file, and
     if it refuses, quote the lines you need, redacted, instead of committing it;
   - when the state is not VALIDATED, also `failures.txt`: each diverging name from E.4 (and `TestBogoSuite` itself
     on THIRD-STATE or NO-RUNNER, where the disclosure absorbs it), its go/cs verdicts, and its C# failure text
     (the `output` events for that test in the results JSON), redacted per F.6.
3. **The full `go2cs_test_results.json`:** commit it ONLY if the entry census is CLEAN on it. Otherwise keep it in
   `C:\az1\keep\`, and record its SHA-256 and size in EVIDENCE.md.
4. **Never commit `run.log`.** `t.TempDir` paths can carry the profile. Quote the lines you need in EVIDENCE.md,
   after the census.
5. **EVIDENCE.md must state:**
   - the verbatim readbacks: GOFLAGS empty at process, User and Machine scope, and `go env GOFLAGS` empty;
   - the `-TestTimeout` ask and floor;
   - SDK 10.0.400 and the runtime; the pwsh version;
   - logical CPUs, the CPU model, RAM and `<SIZE>`;
   - the Defender real-time state and the exclusion counts (A.4); free disk at the start; "nothing else running";
   - both `TestBogoSuite` walls, and the SHA-256 of both kept JSON files.
6. **Commit and push.** Run the census on every file (section F), commit unsigned, run F.3's identity check with
   `T=/c/src/az1-ev`, then push after `. C:\az1\push-env.ps1` and read the branch back EQUAL. Any later commit on
   this branch follows floor 9: announce, then push.

## I. ANYTHING SHORT OF THE FULL-HOST STATE: evidence only

- **Bank nothing.** Make no commit on `claude/az1-tls-bank`, and never push it.
- **THIRD-STATE (the wall finding):** report the C# `TestBogoSuite` elapsed at the kill, how many leaves C#
  reported, and the `withdrawn` count.
- **NO-RUNNER (the network finding):** report the fetch failure lines, redacted, and the time; nothing about the
  verdicts follows from it.
- **If only BoGo cases diverged,** set them against the i9's 11 by name.
- **Class the divergence with two arms.** Run NEITHER unless COORD asks:
  - **(a) The wall axis:** the same tree with `GOFLAGS=-timeout=40m`, R's setting.
  - **(b) The code axis:** R's `d095fe8108` reading is the control. The suspects are batch 8b's delta
    (`43d149b87d..<STAMP>`) as section 1 lists them: golib refs 10 and 11, runtime refs 11 and 12, syscall ref 7,
    `internal/runtime/atomic` ref 6, and the converter refs 4, 8 and 9. The `git diff --stat` in section 1 is the
    list of record.

## J. Report: one message per event

**Route.** Send each message to COORD with SendMessage, addressed to
`coord -- Go corpus migration to 1.24.13 coordination` (A.5 step 1 proved the route).
- **If the route drops later:** finish the step you are on, push the evidence branch, and stop. The owner then sends
  COORD one line naming the branch. Never route a report through the owner's attention in this session: the owner
  watches the COORD session only.
- If COORD's dispatch says `fleet-msg.sh` admits `AZ1` (its LANES list does not today), that inbox is a second
  route, used as the dispatch says.
- **Anything you need from the owner** is one line to COORD: `OWNER-HAND: <what> -- <why> -- <exact command or click>`.
  Never wait silently at a prompt.

**Every message is censused, OWNER-HAND lines and the ready message included.** Write it to
`C:\az1\report-<n>.txt` first. It goes out only after `bash "$C" entry /c/az1/report-<n>.txt` [Git Bash] reads
CLEAN, and then its text goes out verbatim. The one exception is before A.5 step 6 has passed, when the census is
not yet ready: then the only message allowed is an OWNER-HAND line built from this brief's own words (for example
`OWNER-HAND: commit identity -- A.5 step 2 rc 1 -- set user.name/user.email per brief A.2`), carrying no value
read from the VM. Quote failure text only
from the censused `failures.txt`, and run.log lines only after the census. Stamp times with
`Get-Date -Format 'yyyy-MM-dd HH:mm'` in the same command that writes the line.

```
AZ1 crypto/tls @ <TIP short> (<STAMP short>, 8b|8c): <VALIDATED|THIRD-STATE|NO-RUNNER|DIVERGED|NOVERDICT|TIMEOUT> (wrapper word <w>, rc <n>)
FULL-HOST gate: <10/10 PASS | the failing checks by name>
go <n> / C# <n>; <m> matched + <d> disclosed; withdrawn <k>; BoGo fan-out go <n> / C# <n> (expect 3419/3419)
C# TestBogoSuite <s> s vs 600 s (margin <+/-s>); Go TestBogoSuite <s> s; row wall_s <s>
GOFLAGS empty (process/User/Machine/go env); -TestTimeout 90m (floor 30m); SDK 10.0.400 / runtime <x>; pwsh <v>; <N> logical, <R> GiB, <SIZE>
i9's 11 cases: <all agree | the ones that do not, by name>
divergences: <name: go/cs: first line of failure text, from failures.txt>, ...
branches: claude/az1-tls-bank <sha | not pushed>; claude/az1-tls-evidence <sha>; census CLEAN (converted <k> files, entry <m>), token plants <p>/<p>, identity EQUAL
held for COORD: <notes/prose | unclassified files | census refusals | stale 1.23.12 comment sites>; on a bank: the two core-refs table rows retire (RETIRED, SWEEP IT)
```

About the last line: banking the re-emitted `crypto.tls.tests.csproj` retires two declared rows in
`src/go2cs/internal/repoguard/coreReferencesResolve_test.go` (`crypto/tls -> crypto/internal/mlkem768` and
`crypto/tls -> runtime/internal/math`). The guard only logs `RETIRED, SWEEP IT` and stays green, so the table
sweep must land in the batch that boards the bank.

**Afterwards:** leave both worktrees and `C:\az1` exactly as they are until COORD releases the lane.

## K. Teardown (owner acts, when COORD releases the lane)

Deallocating the VM keeps its disk, and every stored credential with it. Before deallocating or deleting:
1. Revoke the GitHub credential at its source: delete the fine-grained PAT, or revoke Git Credential Manager's
   authorization under the account's Applications settings.
2. Sign out of Claude Code on the VM.
3. Then deallocate or delete the VM. Deleting it is the owner's act alone.
