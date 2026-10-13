# tT4-drift-classify.py -- a READING of tT4-drift-tokens.py's FAIL pairs: what the code-token hunks ARE.
#   python -B tT4-drift-classify.py <worktree> <scratch> <report-in> <classes-out>
# Every non-equal token hunk (difflib over the comment-stripped token streams) is put in one class:
#   DEREF-ALIAS-DROPPED   committed holds 'ref var X = ref ᏑX . DerefOrNull ( ) ;' (one or more), the emission does not
#   DEREF-ALIAS-ADDED     the reverse
#   LAMBDA-EXPR-BODY      committed '=> { return E ; }', emitted '=> E' (the two hunks '{ return' and '; }' of one lambda)
#   OTHER                 anything else (printed in full)
import difflib, importlib.util, os, re, subprocess, sys
from collections import Counter

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('dt', os.path.join(HERE, 'tT4-drift-tokens.py'))
dt = importlib.util.module_from_spec(spec); spec.loader.exec_module(dt)

def deref_runs(seq):
    i = 0
    while i < len(seq):
        if len(seq) - i >= 11 and seq[i] == 'ref' and seq[i + 1] == 'var' and seq[i + 3] == '=' and seq[i + 4] == 'ref' \
           and seq[i + 5] == 'Ꮡ' + seq[i + 2] and seq[i + 6:i + 11] == ['.', 'DerefOrNull', '(', ')', ';']:
            i += 11; continue
        return False
    return len(seq) > 0

def main(wt, scratch, rep, outp):
    fails = [l.split(' ', 4) for l in open(rep, encoding='utf-8') if l.startswith('FAIL ')]
    head = subprocess.run(['git', '-C', wt, 'rev-parse', 'HEAD'], capture_output=True, text=True).stdout.strip()
    total = Counter(); perpair = []
    with open(outp, 'w', encoding='utf-8', newline='\n') as out:
        for f in fails:
            o, k, p = f[1], f[2], f[3]
            a = subprocess.run(['git', '-C', wt, 'show', f'{head}:{p}'], capture_output=True).stdout.replace(b'\r', b'').decode('utf-8', 'surrogateescape')
            b = dt.rd(os.path.join(scratch, f'U-{o}', *p.split('/')))
            la, ta = dt.tokens(a); lb, tb = dt.tokens(b)
            # v2: the deref-alias statements are removed from BOTH streams first (difflib rotates the cut points inside a
            # run of consecutive aliases, which v1 read as OTHER); the per-side counts say how many were dropped.
            def strip(toks):
                out, n, i = [], 0, 0
                while i < len(toks):
                    if deref_runs([t for t, _ in toks[i:i + 11]]) and len(toks) - i >= 11: n += 1; i += 11; continue
                    out.append(toks[i]); i += 1
                return out, n
            ta, na = strip(ta); tb, nb = strip(tb)
            sa = [t for t, _ in ta]; sb = [t for t, _ in tb]
            ops = [x for x in difflib.SequenceMatcher(None, sa, sb, autojunk=False).get_opcodes() if x[0] != 'equal']
            cls = Counter(); other = []
            if na - nb > 0: cls['DEREF-ALIAS-DROPPED'] += na - nb
            if nb - na > 0: cls['DEREF-ALIAS-ADDED'] += nb - na
            skip = set()
            for n, (tag, i1, i2, j1, j2) in enumerate(ops):
                if n in skip: continue
                da, db = sa[i1:i2], sb[j1:j2]
                if tag == 'delete' and deref_runs(da): cls['DEREF-ALIAS-DROPPED'] += da.count('DerefOrNull'); continue
                if tag == 'insert' and deref_runs(db): cls['DEREF-ALIAS-ADDED'] += db.count('DerefOrNull'); continue
                # '=> {' 'return' E ';' '}'  ->  '=>' E : two deletes, '{ return' right after '=>' and '; }' later
                if tag == 'delete' and da == ['{', 'return'] and i1 > 0 and sa[i1 - 1] == '>' and sa[i1 - 2] == '=' and n + 1 < len(ops):
                    t2, a1, a2, b1, b2 = ops[n + 1]
                    if t2 == 'delete' and sa[a1:a2] == [';', '}']:
                        cls['LAMBDA-EXPR-BODY'] += 1; skip.add(n + 1); continue
                cls['OTHER'] += 1
                other.append(f'{tag} committed line {la.line(ta[i1][1]) if i1 < len(ta) else "EOF"} [{" ".join(da)[:160]}] -> emitted line {lb.line(tb[j1][1]) if j1 < len(tb) else "EOF"} [{" ".join(db)[:160]}]')
            total.update(cls)
            out.write(f'{o} {p} :: ' + ' '.join(f'{c}={v}' for c, v in sorted(cls.items())) + '\n')
            for x in other: out.write(f'    OTHER {x}\n')
            perpair.append((o, p, cls))
        line = f'DRIFT-CLASSES pairs={len(fails)} hunks: ' + ' '.join(f'{c}={v}' for c, v in sorted(total.items())) + \
               f' pairs-with-OTHER={sum(1 for x in perpair if x[2]["OTHER"])}'
        out.write(line + '\n')
    print(line)

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    except Exception: pass
    main(*sys.argv[1:])
