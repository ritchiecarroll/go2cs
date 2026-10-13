# tT4-drift-plants.py -- floor 13 for tT4-drift-tokens.py: every plant must be NAMED as a failure, and every
# unplanted pair it starts from must PASS. Copies only, in <plantdir>; the U roots and the worktree are only read.
#   python -B tT4-drift-plants.py <worktree> <scratch> <plantdir>
import os, shutil, subprocess, sys

HERE = os.path.dirname(os.path.abspath(__file__))
TOOL = os.path.join(HERE, 'tT4-drift-tokens.py')

def blob(wt, p):
    return subprocess.run(['git', '-C', wt, 'show', f'HEAD:{p}'], capture_output=True, check=True).stdout

def run(a, b, kind):
    r = subprocess.run([sys.executable, '-B', TOOL, 'pair', a, b, kind], capture_output=True, text=True, encoding='utf-8')
    return r.returncode, r.stdout.strip()

def main(wt, scratch, pd):
    os.makedirs(pd, exist_ok=True)
    ok = True
    # (name, target, path, kind, old substring on ONE line, new substring, a fragment that must be on that line)
    plants = [
        ('P1-identifier', 'windows', 'src/core/archive/tar/common.cs', 'cs', None, None, None),
        ('P2-raw-string-content', 'windows', 'src/core/internal/trace/traceviewer/mmu.cs', 'cs', None, None, '"""'),
        ('P2b-raw-string-after-slashes', 'windows', 'src/core/internal/trace/traceviewer/mmu.cs', 'cs',
         'src="https://www.gstatic.com/charts/loader.js"', 'src="https://www.gstatic.com/charts/loadeX.js"', None),
        ('P3-slashes-in-string-then-code', 'windows', 'src/core/crypto/rsa/rsa.cs', 'cs',
         'Minimum_key_size)"u8, size);', 'Minimum_key_size)"u8, sizeX);', None),
        ('P4a-pkginfo-adjacent-comment', 'windows', 'src/core/archive/zip/package_info.cs', 'pkginfo',
         '// </GoSourcePositionMaps>', '// </GoSourcePositionMapz>', None),
        ('P4b-pkginfo-inserted-attribute', 'windows', 'src/core/archive/zip/package_info.cs', 'pkginfo',
         '// </GoSourcePositionMaps>', '[assembly: GoImplement<nopCloser, io_package.Reader>]\r\n// </GoSourcePositionMaps>', None),
    ]
    for name, o, p, kind, old, new, frag in plants:
        d = os.path.join(pd, name); os.makedirs(d, exist_ok=True)
        a = os.path.join(d, 'committed_' + p.rsplit('/', 1)[1]); b = os.path.join(d, 'emitted_' + p.rsplit('/', 1)[1])
        with open(a, 'wb') as f: f.write(blob(wt, p))
        src = os.path.join(scratch, f'U-{o}', *p.split('/'))
        shutil.copyfile(src, b)
        rc0, out0 = run(a, b, kind)
        print(f'{name} UNPLANTED {o} {p}: rc={rc0} {out0[:200]}')
        if rc0 != 0: ok = False; print(f'  !! the unplanted pair does not PASS: the plant would prove nothing')
        data = open(b, 'rb').read().decode('utf-8')
        if name == 'P1-identifier':
            # the first code line 'return <ident>' outside any comment: rename the identifier after 'return '
            lines = data.split('\n')
            for i, l in enumerate(lines):
                s = l.lstrip()
                if s.startswith('return ') and s[7:8].isalpha() and '//' not in l and '"' not in l:
                    old, new = l, l.replace('return ', 'return Planted_', 1); break
            planted = data.replace(old, new, 1)
        elif name == 'P2-raw-string-content':
            # change one character INSIDE the first raw string literal's content
            i = data.index('templMMU = """') + len('templMMU = '); j = data.index('"""', i + 3)
            body = data[i + 3:j]
            k = next(x for x, c in enumerate(body) if c.isalpha())
            ch = body[k]; rep = 'Q' if ch != 'Q' else 'Z'
            planted = data[:i + 3] + body[:k] + rep + body[k + 1:] + data[j:]
            old, new = repr(body[max(0, k - 20):k + 20]), f'char {k} {ch!r} -> {rep!r}'
        else:
            assert data.count(old) == 1, f'{name}: anchor {old!r} found {data.count(old)} times'
            planted = data.replace(old, new, 1)
        assert planted != data
        with open(b, 'wb') as f: f.write(planted.encode('utf-8'))
        rc1, out1 = run(a, b, kind)
        named = rc1 == 1 and 'FAIL' in out1
        print(f'{name} PLANTED [{str(old)[:90]!s} -> {str(new)[:90]!s}]: rc={rc1} {"NAMED" if named else "NOT NAMED"} :: {out1[:400]}')
        if not named: ok = False
        # restore: put the original U-root bytes back into the copy and re-read: must be byte-identical and PASS again
        shutil.copyfile(src, b)
        same = open(b, 'rb').read() == open(src, 'rb').read()
        rc2, out2 = run(a, b, kind)
        print(f'{name} RESTORED byte-identical={same} rc={rc2}')
        if not same or rc2 != 0: ok = False
    print(f'DRIFT-PLANTS {"OK" if ok else "FAILED"} ({len(plants)} plants)')
    return 0 if ok else 1

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    except Exception: pass
    sys.exit(main(*sys.argv[1:]))
