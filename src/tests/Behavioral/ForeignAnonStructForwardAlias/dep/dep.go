package dep

// modernc.org/libc/sys/types' shape (sqlite-cs W2): the anonymous struct, published as an alias and lifted HERE first.
type TagLC_ID = struct {
	FwLanguage uint16
	FwCountry  uint16
}

func Make() TagLC_ID { return TagLC_ID{FwLanguage: 1, FwCountry: 2} }
