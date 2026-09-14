"""Security census for a handover/doc file before it is pushed.  Every pattern is built at run time
from chr(92) so no backslash passes through a shell; every arm is proven live on a planted line first.
Usage: python coord-handover-census.py <file>   -> exit 0 only on 0 hits AND 0 dead arms."""
import io, os, re, sys

BS = chr(92)
path = sys.argv[1]
text = io.open(path, 'r', encoding='utf-8').read()
lines = text.splitlines()
user = os.environ.get('USERNAME') or os.environ.get('USER') or ''

arms = [
    ('profile-root-win', re.compile('[A-Za-z]:' + BS + BS + '+Users' + BS + BS + '+', re.I), 'C:' + BS + 'Users' + BS + 'x'),
    ('profile-root-posix', re.compile('/(c/)?Users/', re.I), '/c/Users/x'),
    ('home-prefix', re.compile('/home/', re.I), '/home/x'),
    ('unc-prefix', re.compile(BS * 4 + '[A-Za-z0-9]'), BS * 4 + 'box'),
    ('scratchpad-marker', re.compile('AppData|Local' + BS + BS + '+Temp|scratchpad' + BS + BS + '+', re.I), 'AppData' + BS + 'Local'),
]
if user:
    arms.append(('account-name', re.compile(re.escape(user), re.I), user))
else:
    print('account-name arm SKIPPED: no username in the environment (counts as a dead arm)')

dead = 0
hits = 0
for name, rx, plant in arms:
    if not rx.search(plant):
        print('arm %-18s DEAD on its planted line' % name)
        dead += 1
        continue
    found = [i + 1 for i, l in enumerate(lines) if rx.search(l)]
    print('arm %-18s live; hits=%d %s' % (name, len(found), found[:6]))
    hits += len(found)
if not user:
    dead += 1
print('CENSUS hits=%d deadArms=%d arms=%d' % (hits, dead, len(arms)))
sys.exit(0 if hits == 0 and dead == 0 else 3)
