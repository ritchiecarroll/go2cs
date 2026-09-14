"""coord-board-merge.py <branch> <message-file> -- merge a docs-only board branch whose only conflict is an
append-append hunk in docs/phase4/BOARD-next-validation-candidates.md: keep BOTH sides in order (ours, then
theirs), assert no markers remain, assert the raw guard is the final line, then commit with the given message.
Aborts (git merge --abort) on any other conflict. Run from the coordinator worktree root."""
import io, re, subprocess, sys

branch, msgfile = sys.argv[1], sys.argv[2]
BOARD = 'docs/phase4/BOARD-next-validation-candidates.md'

def run(*a, check=True):
    return subprocess.run(a, capture_output=True, text=True, check=check)

r = run('git', 'merge', '--no-ff', '-S', '--no-commit', branch, check=False)
if r.returncode == 0:
    # merged clean but not committed
    run('git', 'commit', '-S', '-q', '-F', msgfile)
    print('merged clean ->', run('git', 'rev-parse', '--short', 'HEAD').stdout.strip())
    sys.exit(0)

unmerged = run('git', 'diff', '--name-only', '--diff-filter=U').stdout.split()
if unmerged != [BOARD]:
    run('git', 'merge', '--abort', check=False)
    print('ABORT: conflicts outside the board file:', unmerged)
    sys.exit(1)

s = io.open(BOARD, encoding='utf-8', newline='').read()
n = s.count('<<<<<<<')
pat = re.compile(r'<<<<<<< [^\r\n]*\r?\n(.*?)(?:\|\|\|\|\|\|\| [^\r\n]*\r?\n.*?)?=======\r?\n(.*?)>>>>>>> [^\r\n]*\r?\n', re.S)
s2, k = pat.subn(lambda m: m.group(1) + m.group(2), s)
assert k == n and '<<<<<<<' not in s2 and '>>>>>>>' not in s2 and '=======\r\n' not in s2, ('markers', n, k)
lines = [l for l in s2.splitlines() if l.strip()]
assert '{% endraw %}' in lines[-1], ('guard not last', lines[-1][:60])
io.open(BOARD, 'w', encoding='utf-8', newline='').write(s2)
run('git', 'add', BOARD)
run('git', 'commit', '-S', '-q', '-F', msgfile)
head = run('git', 'rev-parse', '--short', 'HEAD').stdout.strip()
clean = run('git', 'status', '--porcelain').stdout.strip()
assert clean == '', ('dirty after commit', clean[:200])
print('merged with', n, 'append-append block(s) kept both sides ->', head)
