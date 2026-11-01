#!/bin/bash
# The main-return wait seat's discriminating arm: a TRACE build of golib, patched into a CHECKOUT (never committed),
# that writes timestamped events to the file named by GO2CS_MRW_TRACE (nothing when it is unset, nothing on stdout).
# It answers which release lets a late print through: when main's tail starts, why the wait released, when the tail
# ended, and when a goroutine parked or entered a system call (the print's WriteFile is one).
#
#   make-trace.sh <checkout> on        patch MainExit.cs and Goroutine.cs (refuses a dirty file)
#   make-trace.sh <checkout> restore   put both files back to HEAD, only if they hold nothing but the trace
#
# Each line: <microseconds, Stopwatch> TAB <managed thread id> TAB <event>.
set -euo pipefail
[ $# -eq 2 ] || { sed -n '2,10p' "$0"; exit 2; }
TREE=$1; ARM=$2
ME=src/core/golib/runtime/MainExit.cs
GO=src/core/golib/runtime/Goroutine.cs
cd "$TREE"

# trace_patch <MainExit.cs> <Goroutine.cs>: apply the trace to these two files.
trace_patch() {
    local ME=$1 GO=$2
    # The writer, appended to MainExit.cs. File.AppendAllText goes through .NET's own I/O, never golib's syscall
    # trampoline, so tracing a system call cannot recurse.
    printf '%s\r\n' '' \
      'internal static class MainExitTrace // TRACE' \
      '{' \
      '    private static readonly string? s_path = Environment.GetEnvironmentVariable("GO2CS_MRW_TRACE");' \
      '    private static readonly object s_gate = new();' \
      '    internal static bool On => s_path is not null;' \
      '    internal static void Write(string evt)' \
      '    {' \
      '        if (s_path is null) return;' \
      '        long us = System.Diagnostics.Stopwatch.GetTimestamp() * 1_000_000 / System.Diagnostics.Stopwatch.Frequency;' \
      '        lock (s_gate) System.IO.File.AppendAllText(s_path, us + "\t" + Environment.CurrentManagedThreadId + "\t" + evt + "\n");' \
      '    }' \
      '}' >> "$ME"
    # main's tail: its start, and the wait's end.
    perl -0pi -e 's/(    private static void OnProcessExit\(object\? sender, EventArgs e\)\r?\n    \{\r?\n)/$1        MainExitTrace.Write("tail-start count=" + RunningPanicDefers + " exitRequested=" + s_exitRequested); \/\/ TRACE\r\n/' "$ME"
    perl -0pi -e 's/(\r?\n)(        AwaitPanickingGoroutines\(\);\r?\n)/$1$2        MainExitTrace.Write("tail-wait-done count=" + RunningPanicDefers + " panicking=" + s_panicking); \/\/ TRACE\r\n/' "$ME"
    # The wait: the last sample of each counted goroutine, and why it released.
    perl -0pi -e 's/(                    holds \|= Holds\(ref entry\.Watch, new PanickerSample\(blocked, sequence, cpu\), elapsed\);\r?\n)/$1                    lastSample = "blocked=" + blocked + " seq=" + sequence + " cpuMs=" + (cpu is { } c ? (c - (entry.Watch.CpuStart ?? c)).TotalMilliseconds.ToString("F2") : "none") + " confirmedFor=" + (entry.Watch.Blocked ? (elapsed - entry.Watch.BlockedSince).TotalMilliseconds.ToString("F2") : "-"); \/\/ TRACE\r\n/' "$ME"
    perl -0pi -e 's/(                if \(!holds \|\| elapsed >= HangGuard\)\r?\n)(                    return;)/                if (!holds || elapsed >= HangGuard) { MainExitTrace.Write("wait-release holds=" + holds + " elapsedMs=" + elapsed.TotalMilliseconds.ToString("F2") + " " + lastSample); return; } \/\/ TRACE\r\n                if (false)\r\n$2/' "$ME"
    perl -0pi -e 's/(        SpinWait spin = default;\r?\n\r?\n        try\r?\n        \{\r?\n)/        string lastSample = ""; \/\/ TRACE\r\n$1/' "$ME"
    perl -0pi -e 's/(            \}\r?\n        \}\r?\n        finally\r?\n        \{\r?\n            foreach \(\(ThreadCpuReader\? reader, _\) in watches\.Values\))/            }\r\n            MainExitTrace.Write("wait-exit count=" + RunningPanicDefers + " panicking=" + s_panicking); \/\/ TRACE\r\n        }\r\n        finally\r\n        {\r\n            foreach ((ThreadCpuReader? reader, _) in watches.Values)/' "$ME"
    # A goroutine's system calls (the print is one) and a counted goroutine's parks.
    perl -0pi -e 's/(    public static void EnterBlocked\(\)\r?\n    \{\r?\n)/$1        if (MainExitTrace.On \&\& t_current is { IsMain: false } traced) MainExitTrace.Write("syscall goroutine=" + traced.Id + " counted=" + traced.m_panicDefersCounted); \/\/ TRACE\r\n/' "$GO"
    perl -0pi -e 's/(        int previous = Volatile\.Read\(ref goroutine\.m_waitReason\);\r?\n)/$1        if (MainExitTrace.On \&\& goroutine.m_panicDefersCounted != 0) MainExitTrace.Write("park goroutine=" + goroutine.Id + " reason=" + reason); \/\/ TRACE\r\n/' "$GO"
    local n
    n=$(cat "$ME" "$GO" | grep -c '// TRACE')
    [ "$n" -eq 9 ] || { echo "expected 9 TRACE lines (the writer and 8 events), found $n: restore and fix the patch"; exit 3; }
}

case "$ARM" in
  on)
    for f in $ME $GO; do
      git diff --quiet -- "$f" || { echo "refusing: $f differs from HEAD"; exit 2; }
    done
    trace_patch "$ME" "$GO"
    echo "trace patched. Rebuild golib and the program; set GO2CS_MRW_TRACE=<file> for each run."
    ;;
  restore)
    # Restore only a file that is exactly HEAD plus the trace: patch HEAD's copies in a scratch directory and compare
    # (line endings aside: the blob is LF, the checkout CRLF). Anything else in the file is someone's work.
    tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
    git show "HEAD:$ME" > "$tmp/MainExit.cs"; git show "HEAD:$GO" > "$tmp/Goroutine.cs"
    trace_patch "$tmp/MainExit.cs" "$tmp/Goroutine.cs"
    for pair in "$ME:$tmp/MainExit.cs" "$GO:$tmp/Goroutine.cs"; do
      f=${pair%%:*}; want=${pair#*:}
      cmp -s <(tr -d '\r' < "$f") <(tr -d '\r' < "$want") \
        || { echo "refusing: $f is not exactly HEAD plus the trace; restore it by hand"; exit 2; }
    done
    for f in $ME $GO; do
      # Through git, not `git show > file`: a raw LF write leaves a CRLF checkout modified.
      git checkout HEAD -- "$f"
      [ -z "$(git status --porcelain -- "$f")" ] && echo "restored: $f matches HEAD"
    done
    ;;
  *) echo "unknown arm: $ARM"; exit 2 ;;
esac
