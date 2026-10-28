# crash.py <run-id> <artifact-dir>
#
# For each package in relay/crash.txt, per RID: the sweep's preserved console output of that row (failing-rows/.../
# <pkg with / as .>/runfinal/row-output.txt), printed as annotations around every 'fatal error:' line -- the tests
# running just before it, the line itself and the dump head after it -- or its last 40 lines when it has none.
import os, re, sys

run, root = sys.argv[1], sys.argv[2]
RIDS = ('osx-arm64', 'osx-x64')
here = os.path.dirname(os.path.abspath(__file__))
crash_file = os.path.join(here, '..', '..', 'relay', 'crash.txt')
if not os.path.exists(crash_file):
    sys.exit(0)

def esc(text):
    return text.replace('%', '%25').replace('\r', '%0D').replace('\n', '%0A')

def esc_prop(text):
    return esc(text).replace(':', '%3A').replace(',', '%2C')

probe_file = os.path.join(here, '..', '..', 'relay', 'probe.txt')
probe_names = [l.rstrip('\n').split('\t', 1)[1] for l in open(probe_file) if '\t' in l] if os.path.exists(probe_file) else []

RUN_RE = re.compile(r'=== RUN|"Action":"run"|"action":"run"')

for pkg in [l.strip() for l in open(crash_file) if l.strip()]:
    leaf = pkg.replace('/', '.')
    for rid in RIDS:
        files = []
        for d, _, fs in os.walk(os.path.join(root, rid)):
            if 'row-output.txt' in fs and os.path.basename(d) == 'runfinal' and os.path.basename(os.path.dirname(d)) == leaf:
                files.append(os.path.join(d, 'row-output.txt'))
        out = [f'{pkg} {rid}: {len(files)} preserved row output(s)']
        for path in sorted(files):
            lines = open(path, encoding='utf-8', errors='replace').read().splitlines()
            fatal = [i for i, l in enumerate(lines) if 'fatal error' in l]
            # FIRST, so the chunk cap never drops it: Go's own output for each probed test (go test -json lines): the comparison record holds verdicts only.
            for name in probe_names:
                key = '"Test":"' + name + '"'
                gl = [l for l in lines if key in l and '"Action":"output"' in l]
                out.append(f'-- Go output for {name}: {len(gl)} line(s)')
                for l in gl[:8]:
                    m = re.search(r'"Output":"((?:[^"\\]|\\.)*)"', l)
                    out.append('   ' + (m.group(1) if m else l)[:240])
            out.append(f'== {os.path.relpath(path, root)}: {len(lines)} lines, fatal-error lines at {fatal[:10]}')
            # One line per match, with the test it sits in: a match inside a test's EXPECTED output (Go's own
            # TestRuntimePanic prints a dump on purpose) is not a crash of the run.
            for i in fatal[:12]:
                m = re.search(r'"Test":"([^"]*)"', lines[i])
                out.append(f'   line {i} test {m.group(1) if m else "?"}: {lines[i][-160:]}')
            if fatal:
                last = fatal[-1]
                out.append(f'-- the LAST match, lines {max(0, last - 3)}..{last + 25}:')
                out.extend('   ' + l[:240] for l in lines[max(0, last - 3):last + 25])
            out.append('-- the last 30 lines:')
            out.extend('   ' + l[:240] for l in lines[-30:])
        chunks, cur = [], ''
        for line in out:
            if cur and len(cur) + len(line) + 1 > 3000:
                chunks.append(cur)
                cur = ''
            cur += line + '\n'
        if cur:
            chunks.append(cur)
        chunks = chunks[:6]
        for i, chunk in enumerate(chunks, 1):
            print(f'::warning title={esc_prop(f"relay {run} crash {pkg} {rid} {i}/{len(chunks)}")}::{esc(chunk.rstrip())}')
