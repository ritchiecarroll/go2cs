#!/usr/bin/env bash
# safe-push.sh -- the push/announce/security COMPOSITION, as one script with its own controls.
#
# WHY THIS IS A SCRIPT AND NOT A RULE. On 2026-09-06 four participants, including the two who had
# written the rule down, each got this composition wrong at least once in one session:
#   * the security census composed into the SAME command as the push it was meant to gate, so its
#     verdict printed after the push had already happened and could gate nothing;
#   * `git push | tail` then `$?` -- a REJECTED push reporting rc=0, because that is tail's status;
#   * a --force-with-lease expected-SHA EXPANDED from a nine-character prefix, 31 characters invented
#     (the lease failed CLOSED, which was the mechanism's luck and nobody's design);
#   * an `echo` after a loop asserting "silence above = clean" against output that falsified it two
#     lines earlier.
# In every case the INSTRUMENT was correct and the composition around it made its verdict inert, and
# in every case the output looked exactly like the healthy case. "Remember to order it correctly" has
# a measured failure rate; this file is the remedy. Coordinator ruling, 2026-09-06.
#
# THE FOUR PROPERTIES, each with its own arm in --self-test:
#   ORDER     the security check runs BEFORE the push and a hit ABORTS -- it cannot be a bystander
#   CAPTURE   every native exit is captured into a variable BEFORE anything touches $? or a pipe
#   READ      every SHA is READ (ls-remote/rev-parse), never typed and never prefix-expanded; a
#             human-supplied SHA is RESOLVED before use
#   IDENTITY  the SHA you ANNOUNCED and the SHA you PUSH are the same object
#
# IDENTITY is the one nothing else covers. Announce-then-push protects a reader from a moving ref;
# nothing stopped anyone announcing X and pushing Y.
#
# usage:
#   safe-push.sh --branch <name> [--ref <local ref>] [--announced <sha>] [--new]
#                [--remote <name|path>] [--dry-run]
#   safe-push.sh --self-test
set -uo pipefail

die() { printf 'ABORT: %s\n' "$*" >&2; exit 1; }
step() { printf '\n== %s\n' "$*"; }

BRANCH=""; REF="HEAD"; ANNOUNCED=""; NEW=0; DRY=0; SELFTEST=0; REMOTE="origin"
while [ $# -gt 0 ]; do
  case "$1" in
    --branch)    BRANCH="${2:?}"; shift 2 ;;
    --ref)       REF="${2:?}"; shift 2 ;;
    --announced) ANNOUNCED="${2:?}"; shift 2 ;;
    --remote)    REMOTE="${2:?}"; shift 2 ;;
    --new)       NEW=1; shift ;;
    --dry-run)   DRY=1; shift ;;
    --self-test) SELFTEST=1; shift ;;
    *) die "unknown argument: $1" ;;
  esac
done

# ---------------------------------------------------------------------------------------------
# resolve_sha -- the READ property. It REFUSES a short hash rather than expanding it, which is the
# whole point: `git rev-parse` would expand a nine-character prefix silently and CORRECTLY, and that
# is worse than an error, because it teaches the habit that produced 31 invented characters.
#
# The coordinator's rule of 2026-09-06 is that a resolution check must FETCH first, since an
# unresolved SHA in a stale clone is not a fabricated one and treating it as such accuses a lane that
# did everything right. THAT AMBIGUITY CANNOT ARISE HERE and the exemption is recorded at the site
# rather than assumed: the announced object is the commit you are about to PUSH, and you cannot push
# an object you do not have, so it is local by construction. Do not lift this resolve into a context
# where the distinction is live.
resolve_sha() {
  local raw="$1" what="$2"
  [ -n "$raw" ] || die "$what: empty"
  case "$raw" in
    *[!0-9a-fA-F]*) die "$what: not a hex object name: $raw" ;;
  esac
  [ ${#raw} -eq 40 ] || die "$what: a SHA is READ, never expanded from a prefix -- got ${#raw} chars, need 40. Read it with 'git ls-remote' or 'git rev-parse'."
  git cat-file -e "${raw}^{commit}" 2>/dev/null || die "$what: $raw does not resolve to a commit here. In a PUSH the announced object is local by construction, so this is a typo or an invention -- not a stale clone."
  printf '%s' "$raw"
}

# ---------------------------------------------------------------------------------------------
# The security gate DELEGATES to the repository's own instrument. It deliberately carries no
# patterns and no denylist of its own: src/go2cs/fleetIdentifierCensus_test.go already implements the
# owner's standing order with two passes (path-anchored structural, and denied-token for identifiers
# used OUTSIDE any path) over a SHA-256 denylist that never spells what it forbids. A second copy
# here would be the silent-duplication shape -- two implementations of one rule, neither aware of the
# other -- and would ship a weaker denylist besides.
#
# ⚠ WHAT THIS CHECKS, STATED PRECISELY BECAUSE THE SMALLER TRUE CLAIM IS WORTH MORE THAN THE LARGER
# ONE: the repository guard enumerates `git ls-files`, so it answers "is the TRACKED TREE clean",
# NOT "is this commit RANGE clean". For an ordinary push those coincide -- what you are pushing is
# committed and therefore tracked. They diverge for a push from a tree whose working state differs
# from the range being pushed. Pointing that guard's scanner at a range is a separate, unsized item.
security_gate() {
  local root rc
  root=$(git rev-parse --show-toplevel 2>/dev/null) || die "not inside a git repository"
  [ -f "$root/src/go2cs/fleetIdentifierCensus_test.go" ] || \
    die "the repository security guard is not at $root/src/go2cs/fleetIdentifierCensus_test.go -- a composition that cannot find its gate does not push"
  command -v go >/dev/null 2>&1 || \
    die "the Go toolchain is required to run the repository security guard, and a push that cannot run it is not gated"

  printf '   delegating to src/go2cs/fleetIdentifierCensus_test.go (the repo guard, not a second denylist)\n'

  # ⚠ `-count=1` IS LOAD-BEARING AND IS ASSERTED, NOT MERELY WRITTEN (coordinator requirement,
  # 2026-09-06). Without it cmd/go's test cache can serve this invocation, and the gate then reports
  # `ok (cached)` while checking NOTHING -- a vacuous green nested inside the instrument built
  # against vacuous greens, and the hardest layer to see, because the outer run is genuinely running
  # and the gate genuinely appears to fire. This repository has already paid that bill once:
  # CLAUDE.md records that cmd/go's cache drops files resolving outside the module root, so a
  # narrowed predicate reported `ok (cached)` and failed only under `-count=1`.
  #
  # So the output is READ rather than discarded, and a cached verdict is refused. The flag can be
  # deleted by a future edit; this check is what notices. The self-test proves the detector can fire
  # by measuring that the same invocation WITHOUT the flag really does cache.
  local guard_out
  guard_out=$( cd "$root/src/go2cs" && go test -count=1 \
      -run 'TestNoFleetIdentifiersInTrackedFiles|TestFleetIdentifierScannerFiresAndRestores|TestFleetIdentifierClearancesAreLive' . 2>&1 )
  rc=$?
  [ $rc -eq 0 ] || die "repository fleet-identifier guard FAILED (exit $rc) -- NOT pushed. Re-run it directly in src/go2cs to see the findings."
  case "$guard_out" in
    *'(cached)'*) die "the security gate's inner run was CACHED, so it checked nothing -- an arm whose inner run was cached is not an arm. Restore -count=1 in security_gate." ;;
  esac
  printf '   guard exit 0, not cached -- checked the TRACKED TREE, not the pushed RANGE (see the note at security_gate)\n'
}

# ---------------------------------------------------------------------------------------------
run_push() {
  local branch="$1" ref="$2" announced="$3" isnew="$4" dry="$5" remote="$6"
  local local_sha remote_sha range rc out ncommits now

  # Input validation FIRST, before anything touches the network. The announced SHA is the one value
  # a human types, and its form and resolvability are decidable with no remote at all. The self-test
  # found out why the ORDER matters: with this check further down, three arms feeding a bad SHA died
  # on an unrelated branch-existence abort, so "it refused" would have read as proof the SHA check
  # worked while that check never executed. An arm asserts the REASON it failed, or it is not a
  # control -- and an arm that cannot reach its target is a script-ordering defect in test clothing.
  if [ -n "$announced" ]; then announced=$(resolve_sha "$announced" "announced SHA"); fi

  step "READ the local SHA (never typed)"
  local_sha=$(git rev-parse "${ref}^{commit}"); rc=$?
  [ $rc -eq 0 ] || die "cannot resolve local ref: $ref"
  printf '   %s = %s\n' "$ref" "$local_sha"

  step "READ the remote SHA (never typed, never expanded)"
  remote_sha=$(git ls-remote "$remote" "refs/heads/$branch" | cut -f1)
  if [ -z "$remote_sha" ]; then
    [ "$isnew" -eq 1 ] || die "refs/heads/$branch does not exist on $remote. Pass --new if that is intended."
    printf '   refs/heads/%s absent on %s -- new branch\n' "$branch" "$remote"
  else
    printf '   refs/heads/%s = %s\n' "$branch" "$remote_sha"
    [ "$isnew" -eq 0 ] || die "--new given but refs/heads/$branch already exists at $remote_sha"
  fi

  step "IDENTITY: the announced SHA is the SHA being pushed"
  if [ -n "$announced" ]; then
    [ "$announced" = "$local_sha" ] || die "you announced $announced but $ref is $local_sha -- announce-then-push protects nobody if the two differ"
    printf '   announced == local: %s\n' "$local_sha"
  elif [ -n "$remote_sha" ]; then
    die "refs/heads/$branch is already public at $remote_sha, so its SHA may have been posted. Announce the new SHA, then re-run with --announced $local_sha."
  else
    printf '   new branch, no prior SHA to have announced -- skipped\n'
  fi

  # ⚠ NOT ONLY A VACUOUS-CENSUS GUARD, and the second reason was MEASURED rather than reasoned:
  # `--force-with-lease` IS NOT EVALUATED WHEN THERE IS NOTHING TO PUSH. Measured 2026-09-06 against
  # a hermetic local origin, one variable between the arms:
  #     stale lease, remote == local (nothing to push)  -> exit 0, "Everything up-to-date", LEASE NEVER CHECKED
  #     stale lease, one commit to push                 -> exit 1, "stale info"
  # So a caller with a fabricated lease AND nothing to push gets a clean exit 0 and reads it as
  # protection. This abort is the only thing between them and that reading. It is also the ordinary
  # vacuity rule: a check asserts its input population is non-empty before its verdict means anything.
  step "RANGE is non-empty (a verdict over no commits is clean by construction)"
  if [ -n "$remote_sha" ]; then range="${remote_sha}..${local_sha}"; else range="$local_sha"; fi
  ncommits=$(git rev-list --count "$range" 2>/dev/null); rc=$?
  [ $rc -eq 0 ] || die "cannot count commits in $range"
  [ "$ncommits" -gt 0 ] || die "range $range is EMPTY -- nothing to push, and both the security verdict and the lease would be vacuous: git short-circuits an up-to-date push WITHOUT evaluating --force-with-lease at all"
  printf '   %s carries %s commit(s)\n' "$range" "$ncommits"

  step "ORDER: the security gate runs BEFORE the push, and a failure ABORTS"
  security_gate

  if [ "$dry" -eq 1 ]; then step "--dry-run: everything above passed; no push issued"; return 0; fi

  step "PUSH (exit CAPTURED before any pipe)"
  if [ -n "$remote_sha" ]; then
    out=$(git push --force-with-lease="refs/heads/$branch:$remote_sha" "$remote" "$local_sha:refs/heads/$branch" 2>&1); rc=$?
  else
    out=$(git push "$remote" "$local_sha:refs/heads/$branch" 2>&1); rc=$?
  fi
  printf '   PUSH_EXIT=%s\n' "$rc"
  printf '%s\n' "$out" | sed 's/^/   | /'
  [ $rc -eq 0 ] || die "push failed (exit $rc). If this says 'stale info' the ref moved: READ the remote SHA again and re-announce. Never reach for plain --force, which protects nothing."

  step "VERIFY the remote moved to exactly what we pushed"
  now=$(git ls-remote "$remote" "refs/heads/$branch" | cut -f1)
  [ "$now" = "$local_sha" ] || die "remote is $now, expected $local_sha"
  printf '   remote == local == %s\n' "$local_sha"
  printf '\nSAFEPUSH OK  %s -> refs/heads/%s on %s\n' "$local_sha" "$branch" "$remote"
  return 0
}

# ---------------------------------------------------------------------------------------------
# --self-test. HERMETIC: it creates a bare repository and uses it as the remote, so no arm touches
# the network and no arm can reach a real branch. Every arm asserts the REASON it aborted rather than
# a nonzero exit -- three arms once aborted for an unrelated reason and a suite checking `exit != 0`
# would have printed three greens over a check that never ran.
self_test() {
  local fails=0 out rc tmp bare
  tmp=$(mktemp -d) || die "mktemp failed"
  bare="$tmp/origin.git"
  git init -q --bare "$bare" || die "cannot create the hermetic origin"
  # Expand the path INTO the trap now rather than referencing $tmp at exit: the variable is local to
  # this function, so a deferred reference is unbound by the time the trap fires and `set -u` reports
  # it as an error on line 1 -- a cleanup failing loudly for a reason that has nothing to do with the
  # thing being tested, which is its own small false signal.
  trap "rm -rf '$tmp'" EXIT

  arm() { # arm <name> <expected-substring> <command...>
    local name="$1" want="$2"; shift 2
    out=$("$@" 2>&1); rc=$?
    if [ $rc -eq 0 ]; then printf '  FAIL %-30s exited 0; expected an abort\n' "$name"; fails=$((fails+1)); return; fi
    case "$out" in
      *"$want"*) printf '  ok   %-30s aborts on: %s\n' "$name" "$want" ;;
      *) printf '  FAIL %-30s aborted for the WRONG reason:\n%s\n' "$name" "$out"; fails=$((fails+1)) ;;
    esac
  }
  pass() { # pass <name> <expected-substring> <command...>
    local name="$1" want="$2"; shift 2
    out=$("$@" 2>&1); rc=$?
    if [ $rc -ne 0 ]; then printf '  FAIL %-30s exited %s; expected success:\n%s\n' "$name" "$rc" "$out"; fails=$((fails+1)); return; fi
    case "$out" in
      *"$want"*) printf '  ok   %-30s succeeds and reports: %s\n' "$name" "$want" ;;
      *) printf '  FAIL %-30s succeeded WITHOUT its evidence:\n%s\n' "$name" "$out"; fails=$((fails+1)) ;;
    esac
  }

  printf 'safe-push self-test -- hermetic origin at %s\n' "$bare"
  printf 'every arm asserts the REASON it aborted, never merely a nonzero exit\n\n'

  # READ -- the three shapes of a SHA that was not read.
  arm 'short SHA refused'    'never expanded from a prefix' \
      bash "$0" --branch none --remote "$bare" --announced 52c01fbb9 --dry-run
  arm 'fabricated SHA'       'does not resolve to a commit' \
      bash "$0" --branch none --remote "$bare" --announced 0123456789abcdef0123456789abcdef01234567 --dry-run
  arm 'non-hex SHA'          'not a hex object name' \
      bash "$0" --branch none --remote "$bare" --announced zzzz --dry-run

  # A branch that does not exist on the remote, without --new.
  arm 'missing branch needs --new' 'Pass --new if that is intended' \
      bash "$0" --branch none --remote "$bare" --dry-run

  # Seed the hermetic origin so the announce and lease arms have a public ref to reason about.
  git push -q "$bare" "HEAD^{commit}:refs/heads/seeded" || die "self-test: cannot seed the hermetic origin"

  arm 'public branch needs announce' 'Announce the new SHA' \
      bash "$0" --branch seeded --remote "$bare" --dry-run
  arm 'announced != pushed'  'announce-then-push protects nobody' \
      bash "$0" --branch seeded --remote "$bare" --announced "$(git rev-parse 'HEAD~1^{commit}')" --ref HEAD --dry-run
  arm 'empty range refused'  'WITHOUT evaluating --force-with-lease' \
      bash "$0" --branch seeded --remote "$bare" --ref HEAD --announced "$(git rev-parse 'HEAD^{commit}')" --dry-run

  # THE POSITIVE CONTROL, and it is a REAL PUSH rather than a --dry-run: the composition exists for
  # the push path, so a suite that never pushes never tests the thing. Pushing HEAD~1 to a new branch
  # on the hermetic origin exercises the whole chain including the remote verification.
  pass 'real push to a new branch' 'SAFEPUSH OK' \
      bash "$0" --branch fresh --remote "$bare" --ref 'HEAD~1' --new

  # CAPTURE -- a FAILING push must abort, not be read as success. This is the arm for the trap that
  # started the whole exercise: `git push | tail` then `$?` reported rc=0 over a REJECTED push. The
  # failure is produced deterministically by pointing the remote at a path that is not a repository.
  #
  # There is deliberately NO "stale lease rejected" arm, and the reason is a property of the design
  # rather than an omission: this script READS the lease from ls-remote immediately before pushing,
  # so the lease is current by construction and can only go stale if the ref moves inside the window
  # between those two commands -- a genuine race a shell test cannot produce on demand. Faking one
  # would test the fake. The lease's own behaviour was measured directly instead, and that
  # measurement is recorded at the range check above, where it is what justifies the abort.
  arm 'failing push aborts'  'push failed' \
      bash "$0" --branch fresh --remote "$tmp/not-a-repo" --ref 'HEAD~1' --new

  # THE CACHE DETECTOR'S POSITIVE CONTROL, and it is a MEASUREMENT rather than a grep of our own
  # source. It runs the gate's exact inner invocation WITHOUT -count=1, twice, and requires the
  # second to report `(cached)`. That establishes three things the detector depends on and that no
  # reading of this file could: caching is real for this invocation, `(cached)` is the string cmd/go
  # actually emits, and therefore -count=1 in security_gate is load-bearing rather than decorative.
  # If cmd/go ever stops caching this shape, or renames the marker, THIS arm fails -- which is the
  # correct place to find out, instead of discovering it from a gate that silently stopped checking.
  local root cache_out
  root=$(git rev-parse --show-toplevel 2>/dev/null)
  if [ -n "$root" ] && [ -f "$root/src/go2cs/fleetIdentifierCensus_test.go" ] && command -v go >/dev/null 2>&1; then
    ( cd "$root/src/go2cs" && go test -run 'TestFleetIdentifierClearancesAreLive' . ) >/dev/null 2>&1
    cache_out=$( cd "$root/src/go2cs" && go test -run 'TestFleetIdentifierClearancesAreLive' . 2>&1 )
    case "$cache_out" in
      *'(cached)'*) printf '  ok   %-30s cmd/go DOES cache this invocation, so -count=1 is load-bearing\n' 'cache detector control' ;;
      *) printf '  FAIL %-30s the same invocation without -count=1 did NOT report (cached):\n%s\n' 'cache detector control' "$cache_out"; fails=$((fails+1)) ;;
    esac
  else
    printf '  FAIL %-30s cannot reach the repository guard to control the cache detector\n' 'cache detector control'; fails=$((fails+1))
  fi

  printf '\n%s\n' "$([ $fails -eq 0 ] && echo 'SELF-TEST CLEAN' || echo "SELF-TEST: $fails arm(s) wrong")"
  return $fails
}

if [ "$SELFTEST" -eq 1 ]; then self_test; exit $?; fi
[ -n "$BRANCH" ] || die "usage: $0 --branch <name> [--ref <ref>] [--announced <sha>] [--new] [--remote <name|path>] [--dry-run]"
run_push "$BRANCH" "$REF" "$ANNOUNCED" "$NEW" "$DRY" "$REMOTE"
