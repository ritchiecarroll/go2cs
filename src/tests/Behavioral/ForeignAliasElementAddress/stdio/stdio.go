package stdio

// The shape of modernc.org/libc/stdio: `type FILE = X_iobuf`, an alias of a named struct.
type X_iobuf struct {
	Fptr int64
	Fcnt int32
}

type FILE = X_iobuf
