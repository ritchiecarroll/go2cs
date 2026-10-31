#!/bin/bash
# The main-return wait census (golib MainExit, Goroutine's main-return marks): where can a goroutine block, in .NET or
# native code, without a mark a returning main reads? Run it from a checkout after any golib or hand-owned change:
#
#   docs/phase4/data/main-return-wait/census.sh [checkout]      (default: the current directory's repository root)
#
# It reads the HAND-OWNED src/core files (golib, [module: GoManualConversion] files, *_impl.cs; testing/ excluded),
# the only route from converted Go to .NET. For every unbounded wait primitive it asks whether its method carries a
# mark (Goroutine.Park, a ParkScope, the entersyscall family, Goroutine.EnterBlocked). An unmarked site must have a
# row in classified.tsv beside this script. It also checks that both syscall funnels carry the mark. Exit 0 only with
# no UNCLASSIFIED site and both funnels marked.
set -u
ROOT=$(cd "${1:-$(git rev-parse --show-toplevel)}" && pwd)
HERE=$(cd "$(dirname "$0")" && pwd)
CLASSIFIED=$HERE/classified.tsv
cd "$ROOT" || exit 2

WAIT='Monitor\.Wait\(|\.WaitOne\(|WaitHandle\.Wait(All|Any)\(|\.Wait\((timeout|millisecondsTimeout|ct|cancellationToken|[A-Za-z_]*[Tt]imeout|\))|Thread\.Sleep\(|SpinWait\.SpinUntil\(|\.GetAwaiter\(\)\.GetResult\(\)|\.Join\((\)|[0-9A-Za-z_]+\))|Console\.(Read|ReadLine|ReadKey)\(|\.(Accept|Receive|ReceiveFrom|Connect|Send|SendTo)\(|WaitForExit\(|\b(kevent|epoll_wait|epoll_pwait|poll|ppoll|select|nanosleep|WaitForSingleObject(Ex)?|WaitForMultipleObjects(Ex)?|GetQueuedCompletionStatus(Ex)?|SleepEx)\('
# Bracket classes, not backslashes: awk reads this pattern too, and rejects \( there.
MARK='Park[(]|ParkScope|entersyscall|GoEntersyscall|EnterBlocked|BlockedWait[(]'

# Pathspecs without glob magic: '*' crosses '/', so 'src/core/golib/*.cs' is every depth ('**/' would demand a
# subdirectory and miss golib's own top level).
{
  git -c core.quotepath=off grep -l -E '^\[module: *(go\.)?GoManualConversion' -- 'src/core/*.cs'
  git -c core.quotepath=off ls-files -- 'src/core/golib/*.cs' 'src/core/unsafe/*.cs' 'src/core/*_impl.cs'
} | grep -v '^src/core/testing/' | grep -v '/Generated/' | sort -u > /tmp/mrw-census-files.$$

sites=0; marked=0; unclassified=0
declare -A buckets
while read -r f; do
  tr -d '\r' < "$f" > /tmp/mrw-census-cur.$$
  while IFS=: read -r ln rest; do
    sites=$((sites + 1))
    start=$(awk -v L=$ln 'NR<=L && /^( {4}| {8})(public|private|internal|protected|static)[^;=]*\(/ {s=NR} END{print s+0}' /tmp/mrw-census-cur.$$)
    if awk -v S=$start -v L=$ln -v M="$MARK" 'NR>=S && NR<=L && $0 ~ M {m=1} END{exit m?0:1}' /tmp/mrw-census-cur.$$; then
      marked=$((marked + 1)); continue
    fi
    rel=${f#src/core/}
    bucket=$(awk -F'\t' -v P="$rel" -v T="$rest" '!/^#/ && $1==P && index(T, $2) {print $3; exit}' "$CLASSIFIED")
    if [ -z "$bucket" ]; then
      unclassified=$((unclassified + 1))
      echo "UNCLASSIFIED  $rel:$ln  $(echo "$rest" | sed 's/^\s*//' | cut -c1-100)"
    else
      buckets[$bucket]=$(( ${buckets[$bucket]:-0} + 1 ))
    fi
  done < <(grep -n -E "$WAIT" /tmp/mrw-census-cur.$$ | grep -v -E '^[0-9]+:\s*(//|\*|///)')
done < /tmp/mrw-census-files.$$
files=$(wc -l < /tmp/mrw-census-files.$$)
rm -f /tmp/mrw-census-files.$$ /tmp/mrw-census-cur.$$

echo "hand-owned files: $files; wait sites: $sites; marked in-method: $marked; unmarked: $((sites - marked))"
for b in "${!buckets[@]}"; do echo "  $b: ${buckets[$b]}"; done | sort
echo "  UNCLASSIFIED: $unclassified"

# The two places a converted program enters the OS must carry the mark.
funnels=0
grep -q 'Goroutine.EnterBlocked();' src/core/syscall/windows/dll_windows.cs || { echo "FUNNEL UNMARKED: syscall/windows/dll_windows.cs callNative"; funnels=1; }
grep -q 'Goroutine.EnterBlocked();' src/core/runtime/syscall_managed_impl.cs || { echo "FUNNEL UNMARKED: runtime/syscall_managed_impl.cs reentersyscall"; funnels=1; }
grep -q 'Goroutine.ExitBlocked();' src/core/runtime/syscall_managed_impl.cs || { echo "FUNNEL UNMARKED: runtime/syscall_managed_impl.cs exitsyscall"; funnels=1; }
[ $funnels -eq 0 ] && echo "syscall funnels: marked (windows callNative; runtime entersyscall/exitsyscall)"

[ $unclassified -eq 0 ] && [ $funnels -eq 0 ]
