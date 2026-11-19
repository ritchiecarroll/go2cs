package stdio

// modernc.org/libc/stdio's shape (ccgo, sqlite-cs W5a): X_iobuf is an alias of an ANONYMOUS struct, holding a padding
// array, and FILE an alias of that alias. Lifted HERE, and published as an alias record importers load.
const X_IOB_ENTRIES = 3

type X_iobuf = struct {
	F_ptr        uintptr
	F_cnt        int32
	F__ccgo_pad1 [4]byte
	F_flag       int32
}

type FILE = X_iobuf
