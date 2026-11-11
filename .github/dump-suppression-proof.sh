#!/usr/bin/env bash
# dump-suppression-proof.sh - the per-job proof that a linux leg writes no crash dumps (S7-b).
#
# The fleet rule (COORD, 2026-11-09, after six aborts wrote 211 GB of dumps on a lane box): no many-thread linux run
# without dump suppression PROVEN on that machine first, and a failed proof stops the run. A hosted job is a fresh
# machine whose image can change its dump handler without notice, so the proof is a step of every job that sweeps
# (docs/phase4/DATA-la64-s7-bank-plan.md section 3). Every line this script prints starts DUMPPROOF; a failure
# prints one line naming what failed, and exits non-zero so no later step runs.
#
#   record   STATE        the settings, a census of the dump locations and the free bytes per filesystem
#   control  STATE        POSITIVE control: with a file core_pattern and no core limit, a one-thread process killed by
#                         SIGSEGV must leave one new file the census sees, and a 128 MiB write must show in the free
#                         space. A detector that sees neither proves nothing.
#   apply    STATE        core_pattern = |/bin/false, the core limit 0:0 on the runner's worker (every later step's
#                         parent), and DOTNET_DbgEnableMiniDump=0 for every later step; each read back
#   prove    STATE [red]  NEGATIVE proof, in a NEW step: this shell's own core limit must read 0 (inherited); a .NET
#                         probe of 2,000 threads crashed from outside and by Environment.FailFast must leave 0 new
#                         files, no new systemd-coredump entry, and free space within 64 MiB on every filesystem.
#                         `red` runs the same probe with suppression OFF (the control's settings) and must FAIL here.
#
# The outside crash is SIGQUIT, not SIGSEGV. Measured on .NET 10 (linux x64, 2026-11-11): a process that receives
# SIGSEGV, SIGABRT or SIGBUS from kill(2) keeps running (one was still alive ten minutes after kill -SEGV), because
# the runtime's handlers treat a signal it did not raise as nothing to act on. SIGQUIT keeps its default action, which
# is to terminate with a core dump (status 131), so it reaches the kernel's dump path from outside. FailFast ends in
# abort(): status 134, SIGABRT raised by the runtime itself.
#   recheck  STATE        after the sweep: any new dump file since the proof holds the shard's evidence
set -uo pipefail
export LC_ALL=C

readonly MIB=$((1024 * 1024))
readonly DELTA_LIMIT=$((64 * MIB))
readonly CONTROL_BYTES=$((128 * MIB))
readonly PROBE_THREADS=2000

say()  { echo "DUMPPROOF $*"; }
fail() { echo "DUMPPROOF FAIL: $*"; echo "::error title=dump-suppression proof::$*"; exit 1; }

cmd="${1:-}"; state="${2:-}"; mode="${3:-}"
[ -n "$cmd" ] && [ -n "$state" ] || fail "usage: $0 record|control|apply|prove|recheck STATE [red]"
mkdir -p "$state" || fail "cannot create the state folder $state"
scratch="$state/cores"
mkdir -p "$scratch"

# The filesystems whose free space is watched: the root, the workspace, the runner's temp folder, /tmp and /var
# (each path is read through df, so two paths on one filesystem read the same number).
free_paths() { printf '%s\n' / "${GITHUB_WORKSPACE:-$PWD}" "${RUNNER_TEMP:-/tmp}" /tmp /var; }

free_bytes() {
  local p avail
  while IFS= read -r p; do
    avail="$(df -B1 --output=avail "$p" 2>/dev/null | tail -1 | tr -d ' ')"
    case "$avail" in ''|*[!0-9]*) continue ;; esac
    printf '%s\t%s\n' "$p" "$avail"
  done < <(free_paths)
}

# The dump census: every file in the system dump folders and the scratch folder, and every dump-NAMED file
# (core, core.N, core.<name>.<pid>, coredump*, *.dmp) in the workspace, the runner's temp folder and /tmp. One
# "size path" line each, sorted. Names, not all files, outside the dump folders: a sweep writes thousands of
# ordinary files there, and a dump is what is counted.
census() {
  {
    sudo find /var/lib/systemd/coredump /var/crash "$scratch" -xdev -type f -printf '%s %p\n' 2>/dev/null
    for root in "${GITHUB_WORKSPACE:-$PWD}" "${RUNNER_TEMP:-/tmp}" /tmp; do
      sudo find "$root" -xdev \( -name .git -prune \) -o -type f \
        \( -name core -o -name 'core.[0-9]*' -o -regex '.*/core\.[^/]*\.[0-9]+' -o -name 'coredump*' -o -name '*.dmp' \) \
        -printf '%s %p\n' 2>/dev/null
    done
  } | sort -u
}

coredump_entries() {
  if command -v coredumpctl > /dev/null 2>&1; then
    sudo coredumpctl --no-pager --no-legend list 2>/dev/null | wc -l
  else
    echo 0
  fi
}

# The runner's worker process: every later step's shell is its child, so a core limit set on it reaches every later
# step and every process those steps start. A limit set in this shell would die with this step.
worker_pid() {
  local p=$$ comm
  while [ "${p:-0}" -gt 1 ]; do
    comm="$(cat "/proc/$p/comm" 2>/dev/null)"
    if [ "$comm" = "Runner.Worker" ]; then echo "$p"; return 0; fi
    p="$(awk '/^PPid:/ { print $2 }' "/proc/$p/status" 2>/dev/null)"
  done
  return 1
}

# The new census lines since a baseline file, and their count.
new_files() { comm -13 "$1" <(census); }

# Each new file's apparent size and its size on disk: a core of a many-thread process is mostly holes, so the two differ.
describe() {
  local size path
  while read -r size path; do
    [ -n "$path" ] || continue
    say "$1 saw: $path apparent=$size on-disk=$(sudo du -B1 "$path" 2>/dev/null | cut -f1)"
  done
}

# Fails when any watched filesystem lost more than DELTA_LIMIT bytes since a free-space record.
check_free() {
  local baseline="$1" label="$2" path before now delta
  while IFS=$'\t' read -r path before; do
    now="$(df -B1 --output=avail "$path" 2>/dev/null | tail -1 | tr -d ' ')"
    delta=$((before - now))
    say "free $label: $path before=$before now=$now used=$delta"
    if [ "$delta" -gt "$DELTA_LIMIT" ]; then
      fail "$label: $path lost $delta bytes, more than the 64 MiB the proof allows"
    fi
  done < "$baseline"
}

case "$cmd" in
record)
  {
    echo "ulimit -c soft : $(ulimit -S -c)"
    echo "ulimit -c hard : $(ulimit -H -c)"
    echo "core_pattern   : $(cat /proc/sys/kernel/core_pattern)"
    echo "core_uses_pid  : $(cat /proc/sys/kernel/core_uses_pid)"
    unit() { local state; state="$(systemctl is-active "$1" 2>/dev/null)"; echo "${state:-absent}"; }
    echo "apport         : $(unit apport.service)"
    echo "systemd-coredump socket: $(unit systemd-coredump.socket)"
    echo "coredumpctl entries: $(coredump_entries)"
  } | tee "$state/settings.txt" | sed 's/^/DUMPPROOF record: /'
  free_bytes | tee "$state/free-record.tsv" | sed 's/^/DUMPPROOF record free: /'
  census > "$state/census-record.txt"
  say "record census: $(wc -l < "$state/census-record.txt") dump file(s) before the proof"
  sed 's/^/DUMPPROOF record census: /' "$state/census-record.txt"
  ;;

control)
  [ -f "$state/census-record.txt" ] || fail "control: no record (run record first)"
  sudo sysctl -q -w "kernel.core_pattern=$scratch/core.%e.%p" > /dev/null || fail "control: cannot set a file core_pattern"
  say "control: core_pattern = $(cat /proc/sys/kernel/core_pattern)"
  before="$state/census-control.txt"; census > "$before"
  # sudo prlimit, not ulimit: a non-root shell cannot raise a hard limit the image set to 0.
  bash -c 'sudo prlimit --pid $$ --core=unlimited:unlimited; sleep 60 & pid=$!; sleep 0.5; kill -SEGV "$pid"; wait "$pid"; exit 0'
  sleep 1
  seen="$(new_files "$before")"
  count="$(printf '%s' "$seen" | grep -c . || true)"
  say "control: SIGSEGV of a one-thread process left $count new file(s)"
  [ "$count" -ge 1 ] || fail "control: the census saw no dump with suppression OFF, so it is blind and proves nothing"
  printf '%s\n' "$seen" | describe control
  free_bytes > "$state/free-control.tsv"
  dd if=/dev/zero of="$scratch/control-128MiB" bs=1M count=128 conv=fsync status=none || fail "control: cannot write 128 MiB"
  while IFS=$'\t' read -r path before_bytes; do
    case "$path" in "${RUNNER_TEMP:-/tmp}") ;; *) continue ;; esac
    now="$(df -B1 --output=avail "$path" | tail -1 | tr -d ' ')"
    used=$((before_bytes - now))
    say "control: a 128 MiB write moved $path's free space by $used bytes"
    [ "$used" -ge $((CONTROL_BYTES - 8 * MIB)) ] || fail "control: the free-space check saw $used bytes of a 128 MiB write, so it is blind"
  done < "$state/free-control.tsv"
  sudo rm -f -- "$scratch"/core.* "$scratch/control-128MiB"
  say "control: PASS (the detector sees a dump file and a 128 MiB write)"
  ;;

apply)
  sudo sysctl -q -w 'kernel.core_pattern=|/bin/false' > /dev/null || fail "apply: cannot set core_pattern"
  got="$(cat /proc/sys/kernel/core_pattern)"
  [ "$got" = '|/bin/false' ] || fail "apply: core_pattern reads back '$got', not '|/bin/false'"
  say "apply: core_pattern = $got"
  w="$(worker_pid)" || fail "apply: no Runner.Worker above this step, so a core limit cannot reach the later steps"
  sudo prlimit --pid "$w" --core=0:0 || fail "apply: cannot set the core limit on the worker ($w)"
  limits="$(awk '/^Max core file size/ { print $5, $6 }' "/proc/$w/limits")"
  [ "$limits" = '0 0' ] || fail "apply: the worker's core limit reads back '$limits', not '0 0'"
  say "apply: core limit 0:0 on Runner.Worker $w"
  echo 'DOTNET_DbgEnableMiniDump=0' >> "$GITHUB_ENV"
  grep -qx 'DOTNET_DbgEnableMiniDump=0' "$GITHUB_ENV" || fail "apply: DOTNET_DbgEnableMiniDump=0 did not reach GITHUB_ENV"
  say "apply: DOTNET_DbgEnableMiniDump=0 exported to the later steps"
  ;;

prove)
  if [ "$mode" = red ]; then
    say "prove: RED ARM -- suppression NOT applied; this step must fail"
    sudo sysctl -q -w "kernel.core_pattern=$scratch/core.%e.%p" > /dev/null || fail "prove: red arm cannot set a file core_pattern"
    sudo prlimit --pid $$ --core=unlimited:unlimited || fail "prove: red arm cannot lift this step's core limit"
    say "prove: red arm core_pattern $(cat /proc/sys/kernel/core_pattern), ulimit -c $(ulimit -S -c):$(ulimit -H -c)"
  else
    soft="$(ulimit -S -c)"; hard="$(ulimit -H -c)"; pattern="$(cat /proc/sys/kernel/core_pattern)"
    say "prove: this step inherits ulimit -c $soft:$hard, core_pattern $pattern, DOTNET_DbgEnableMiniDump=${DOTNET_DbgEnableMiniDump:-unset}"
    [ "$soft" = 0 ] && [ "$hard" = 0 ] || fail "prove: this step's core limit is $soft:$hard, so the later steps do not inherit 0"
    [ "$pattern" = '|/bin/false' ] || fail "prove: core_pattern reads '$pattern', not '|/bin/false'"
    [ "${DOTNET_DbgEnableMiniDump:-}" = 0 ] || fail "prove: DOTNET_DbgEnableMiniDump is '${DOTNET_DbgEnableMiniDump:-unset}' in this step, not 0"
  fi

  probe="$state/probe"
  mkdir -p "$probe"
  cat > "$probe/probe.csproj" <<'XML'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
XML
  cat > "$probe/Program.cs" <<'CS'
using System;
using System.Threading;

// Starts N threads that wait forever, reports READY, then either waits to be killed or calls FailFast.
int count = int.Parse(args[0]);
ManualResetEventSlim gate = new(false);

for (int i = 0; i < count; i++)
    new Thread(() => gate.Wait()) { IsBackground = true }.Start();

Console.WriteLine($"READY {count}");
Console.Out.Flush();

if (args[1] == "failfast")
    Environment.FailFast("dump-suppression proof: FailFast with many threads");

Thread.Sleep(Timeout.Infinite);
CS
  dotnet build "$probe/probe.csproj" -c Release -nologo -v q -o "$probe/bin" > "$state/probe-build.log" 2>&1 || { tail -20 "$state/probe-build.log"; fail "prove: the .NET probe did not build"; }

  baseline="$state/census-prove.txt"; census > "$baseline"
  free_bytes > "$state/free-prove.tsv"
  entries_before="$(coredump_entries)"

  for how in sigquit failfast; do
    out="$state/probe-$how.out"
    # env --default-signal=QUIT: the runner starts every step shell with SIGQUIT IGNORED (its SigIgn reads 0x1004 on
    # both linux legs, run 42938068113), and an ignored disposition survives exec. The x64 .NET runtime keeps an
    # inherited ignore, so a probe started without this outlived SIGQUIT by 120 s and left no dump (runs 42936721919
    # and 42936743025); the arm64 runtime installs its own handler anyway, which is why only x64 showed it. With the
    # default restored the probe's SigIgn reads 0x1002 and SIGQUIT ends it with status 131 on both legs. The masks
    # printed below keep that visible.
    (cd "$scratch" && exec env --default-signal=QUIT dotnet "$probe/bin/probe.dll" "$PROBE_THREADS" "$how") > "$out" 2>&1 &
    pid=$!
    for _ in $(seq 1 120); do grep -q '^READY' "$out" && break; sleep 0.5; done
    grep -q "^READY $PROBE_THREADS" "$out" || { kill -KILL "$pid" 2>/dev/null; fail "prove: the probe ($how) never reported $PROBE_THREADS threads"; }
    masks() { grep -E '^(SigPnd|ShdPnd|SigBlk|SigIgn|SigCgt)' "/proc/$1/status" 2>/dev/null | tr '\t\n' '  '; }
    say "prove: probe $how pid $pid before: $(masks "$pid")"
    say "prove: this step's shell $$: $(masks $$)"
    if [ "$how" = sigquit ]; then
      kill -QUIT "$pid"
      sleep 1
      say "prove: probe $how pid $pid 1 s after SIGQUIT: $(masks "$pid")"
    fi
    late=0
    if ! timeout 120 tail --pid="$pid" -f /dev/null; then
      # A process the kernel is still dumping has not died yet: kill it, then count the files BEFORE any verdict, so a
      # dump in progress reads as a dump and not as a probe that never crashed.
      say "prove: the probe ($how) is still alive 120 s after the crash; killing it"
      late=1
      kill -KILL "$pid" 2>/dev/null
    fi
    wait "$pid"; status=$?
    say "prove: probe $how with $PROBE_THREADS threads ended with status $status"
    sleep 2
    seen="$(new_files "$baseline")"
    count="$(printf '%s' "$seen" | grep -c . || true)"
    say "prove: after the $how crash, $count new dump file(s)"
    if [ "$count" -ne 0 ]; then
      printf '%s\n' "$seen" | describe prove
      sudo rm -f -- "$scratch"/core.*
      fail "prove: the $how crash of a $PROBE_THREADS-thread .NET probe left $count dump file(s); the sweep does not run"
    fi
    [ "$late" = 0 ] || fail "prove: the probe ($how) did not die within 120 s and left no dump file"
    [ "$status" -ge 128 ] || fail "prove: the probe ($how) exited $status, not by a signal, so it did not crash"
    check_free "$state/free-prove.tsv" "prove $how"
    entries_now="$(coredump_entries)"
    [ "$entries_now" -eq "$entries_before" ] || fail "prove: systemd-coredump gained $((entries_now - entries_before)) entr(ies) after the $how crash"
  done

  if [ "$mode" = red ]; then
    fail "prove: RED ARM -- the probe crashed twice with suppression OFF and the census saw nothing, so the proof is blind"
  fi
  census > "$state/census-proven.txt"
  say "prove: PASS (2 crashes of a $PROBE_THREADS-thread .NET probe, 0 new files, under 64 MiB on every filesystem)"
  ;;

recheck)
  [ -f "$state/census-proven.txt" ] || fail "recheck: no proven census (the proof did not pass)"
  seen="$(new_files "$state/census-proven.txt")"
  count="$(printf '%s' "$seen" | grep -c . || true)"
  say "recheck: $count new dump file(s) since the proof"
  if [ "$count" -ne 0 ]; then
    printf '%s\n' "$seen" | describe recheck
    fail "recheck: $count dump file(s) appeared during the run; HOLD this shard's evidence and dispatch no further shard until the cause is named"
  fi
  say "recheck: PASS"
  ;;

*)
  fail "unknown command '$cmd'"
  ;;
esac
