# coord-queue-status.py -- stamp every coord-queue-*.md with a STATUS header line from the coordinator's ledger (2026-09-05).
# Run again whenever a landing changes a status; the header is the FIRST line and is replaced in place.
import io, os, re, glob
SP = 'C:/Projects/go2cs/.claude/coord-scripts/'
STATUS = {
    'composite-literal-elements': 'LANDED train 23 (SUB-Q1 54c7ecb85 / 5a63568df); re-dispatch 2026-09-05 found it landed',
    'printf-comma-paren': 'LANDED (SUB-Q2 46c13d703 / 9d19a59fa: a NEGATIVE result -- no defect, guard PrintfFormatCommaParen); re-dispatch 2026-09-05 found it landed',
    'none-bucket-byte-probe': 'OPEN (instrument note; no slot)',
    'q9-arm64-stdlibinternalabi-goarch': 'LANDED (SUB-Q9)',
    'q10-runner-transpile-ok-over-best-effort': 'LANDED (SUB-Q10)',
    'q11-update-targets-unconditional': 'LANDED (SUB-Q11, train 23)',
    'q13-chain-preserve-failed-records': 'LANDED (coordinator battery scripts preserve records since train 24)',
    'q14-golibtests-tc0-guards': 'LANDED (SUB-Q14)',
    'q15-syscall-usecgroupfd-tty': 'LANDED half 2 (C1 314ce0cb4); half 1 = Q33 (cgroup2 host)',
    'q17-bcache-registercache': 'LANDED (SUB-Q17; bcache banked 2026-09-03)',
    'q18-testing-row-sizing': 'LANDED (SUB-Q18, train 24)',
    'q19-address-take-pin-cost': 'CLOSED on the syscall pair (2026-09-04)',
    'q20-os-alloc-floor-record': 'LANDED (SUB-Q20 / SUB-Q32 records)',
    'q22-elided-named-composite-renderer': 'IN FLIGHT (SUB-Q22 since 2026-09-05 00:15)',
    'q24-q12-windows-arms': 'OPEN (net/http Q12 windows arms; unowned)',
    'q25-implement-record-relocatable': 'LANDED (SUB-Q25)',
    'q26-clean-bin-noninteractive': 'LANDED (SUB-Q26, train 23)',
    'q27-pprof-goroutine-profile-stub': 'LANDED (SUB-Q27, train 25); label half re-enters with Q44',
    'q28-managed-execution-tracer': 'DESIGN LANDED (SUB-Q28); runtime/trace stays unimplemented',
    'q29-testv-tristate-retirement': 'LANDED (SUB-Q29, train 25)',
    'q30-pinned-object-heap-boxes': 'CLOSED unfavourable (ratio census)',
    'q31-osexec-hostshaped-annotation': 'LANDED (C1, train 25)',
    'q32-score-i1-on-q5-ladder': 'LANDED (SUB-Q32, train 25)',
    'q33-cgroup-exec-trampoline': 'OPEN (needs a cgroup2 Linux host: G-LAPTOP WSL)',
    'q34-syscall-linux-six-line-staleness': 'LANDED docs (C1, train 25); regen debt',
    'q35-i1-linux-darwin-footprint': 'LANDED (G, train 26)',
    'q36-rebank-net-http-1345': 'LANDED (SUB-Q36, train 25)',
    'q37-handshake-margin-ab': 'OPEN (needs a quiet box; after a train battery)',
    'q38-wave-population-board-line': 'OPEN (fold into the next docs seat / doctrine batch 11)',
    'q39-external-variant-lift-dedup': 'LANDED (SUB-Q39, train 26)',
    'q40-getg-over-registry': 'DESIGN LANDED (SUB-Q40, train 26); cut = Q47',
    'q41-arm64-mute-death': 'PLACED (C2, 2026-09-05); instrument seat C2Q41F train 28; remedy = darwin increment 6',
    'q42-pinned-box-staleness-witness': 'LANDED (SUB-Q42, train 26)',
    'q43-pprof-hostkiller-gate-census': 'SEATED train 27 (SUB-Q43 5b58d49ea)',
    'q44-reference-bearing-box-token': 'DESIGN LANDED train 26; CUT rehearsed (C2), seat train 28 after rebase',
    'q45-runtime-pinner': 'DESIGN seated train 27 (49ccad282); CUT in flight (SUB-Q45, same branch), seat train 28',
    'q46-panic-systemstack-orphan': 'ROOTED, no cut (C1, 2026-09-05): init-started goroutine vs the CLR type-initializer lock; ruling posted',
    'q47-getg-cut': 'CUT (C1 49ad67e32), seat train 28 as C1RT4',
    'q48-trace-impl-header-reconcile': 'SEATED train 27 (G c5e552949)',
    'q50-unsafe-string-aliasing': 'CUT (SUB-Q50 e8fcb6703), seat train 28',
    'q51-runtime-pprof-bank': 'OPEN (after Q47 + Q44 land)',
    'q52-darwin-signal-delivery-design': 'OPEN (C2 after Q44/Q49; the Linux rt_sigaction twin joins it)',
    'q53-fatalthrow-getcallerpc': 'OPEN (C1 increment-5 candidate)',
    'q54-runtime-lock-release-on-exception': 'OPEN (design; C1 increment-5 candidate)',
    'stack-leakcheck-results': 'OPEN (results note; no slot)',
}
for p in glob.glob(SP + 'coord-queue-*.md'):
    key = os.path.basename(p)[len('coord-queue-'):-3]
    t = io.open(p, encoding='utf-8', newline='').read()
    nl = '\r\n' if '\r\n' in t[:200] else '\n'
    lines = t.split(nl)
    st = STATUS.get(key, 'UNKNOWN -- audit')
    hdr = '# STATUS (2026-09-05): ' + st
    if lines and lines[0].startswith('# STATUS'):
        lines[0] = hdr
    else:
        lines.insert(0, hdr)
    io.open(p, 'w', encoding='utf-8', newline='').write(nl.join(lines))
    print('%-45s %s' % (key, st[:70]))
