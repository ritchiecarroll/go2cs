"""coffstr.py OBJ : read an ILC .obj's COFF (regular or /bigobj) header, its symbol-table offset and count, and the
string table: total bytes, stored names, and the bytes and count of names matching each probe."""
import struct, sys

PROBES = ["DisplayClass", "IGoMethodSet", "MethodSet", "<>c", "b__"]

def main(path):
    with open(path, "rb") as f:
        head = f.read(56)
        sig1, sig2 = struct.unpack_from("<HH", head, 0)
        if sig1 == 0 and sig2 == 0xFFFF:  # ANON_OBJECT_HEADER_BIGOBJ
            nsect, symptr, nsyms = struct.unpack_from("<III", head, 44)
            symsize = 20
            kind = "bigobj"
        else:
            nsect, = struct.unpack_from("<H", head, 2)
            symptr, nsyms = struct.unpack_from("<II", head, 8)
            symsize = 18
            kind = "coff"
        strpos = symptr + nsyms * symsize
        f.seek(strpos)
        size, = struct.unpack("<I", f.read(4))
        table = f.read(size - 4)
    names = table.split(b"\0")
    if names and names[-1] == b"":
        names.pop()
    print(f"format={kind} sections={nsect} symtab_offset={symptr} symbols={nsyms}")
    print(f"string_table_bytes={size} names={len(names)}")
    for p in PROBES:
        pb = p.encode()
        hit = [n for n in names if pb in n]
        print(f"probe {p!r}: names={len(hit)} bytes={sum(len(n) + 1 for n in hit)}")

if __name__ == "__main__":
    main(sys.argv[1])
