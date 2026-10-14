"""tT4-plantreal.py -- kit change T4-19 (the Go 1.26 hop's H0, F-H0-2; ruled COORD 2026-10-14).

usage: python tT4-plantreal.py <emitted file> <seed file, plant appended> <file holding the plant line>
prints PURE (the file's drift is the plant alone) or REAL (the emission differs from the seed with the plant removed),
exit 0 either way; exit 2 when the seed does not end in the plant block (the plant was never appended: not measurable).

The plant line comes from a FILE, never argv: MSYS rewrites an argument beginning '//' (the plant is a C# comment)
into '/' when it starts a native program, so an argv line never matches (measured 2026-10-14 by this file's test).

WHY: tT4-emitcheck.sh plants a line into PLANT_CS (unicode/utf8/utf8.cs) and PLANT_SIB (sync/pool.cs.auto), in the
seeded root AND the seed comparand, so that the conversion's rewrite reads as drift (the positive control). Through
T4 the lists then dropped the plant file BY PATH (`grep -vxF "$PLANT_CS"`), which also dropped any REAL drift in that
file. utf8.cs carried T4's comment parity from TRAIN FL to the hop's H0 unseen by every check. The plant is now
subtracted by its LINE: the seed's appended block ('\\n' + line + '\\n', as `printf '\\n%s\\n'` writes it) is removed,
CR stripped on BOTH sides, and the emission compared with what is left.
"""
import sys


def main(emitted, seed, linefile):
    e = open(emitted, 'rb').read().replace(b'\r', b'')
    s = open(seed, 'rb').read().replace(b'\r', b'')
    line = open(linefile, 'rb').read().replace(b'\r', b'').rstrip(b'\n')
    block = b'\n' + line + b'\n'
    if not line or not s.endswith(block):
        print('UNMEASURABLE (the seed does not end in the plant block)')
        return 2
    print('PURE' if e == s[:-len(block)] else 'REAL')
    return 0


if __name__ == '__main__':
    if len(sys.argv) != 4:
        print(__doc__)
        sys.exit(2)
    sys.exit(main(*sys.argv[1:]))
