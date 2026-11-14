package main

import (
	"fmt"
	"unsafe"

	"ForeignAnonStructForwardAlias/dep"
)

// sqlite-cs W2 (S15's shape in ONE file, over another package's lift): modernc.org/sqlite/lib's `type LC_ID = TLC_ID`
// is declared BEFORE `type TLC_ID = struct{...}` in the same file, and that anonymous struct is identical to one libc
// already lifted (sys/types' tagLC_ID). Go calls them all one type. The forward alias rendered a deferred marker the
// post-barrier pass could not resolve (the reused lift is another package's, never in this package's registry): raw Go
// text, `global using LC_ID = go.main_package.struct{FwLanguage uint16; FwCountry uint16};`.
type LC_ID = TLC_ID

type TLC_ID = struct {
	FwLanguage uint16
	FwCountry  uint16
}

func main() {
	var t TLC_ID = dep.Make()
	l := LC_ID{FwLanguage: 9, FwCountry: 3}
	var shared dep.TagLC_ID = l
	fmt.Println(t.FwLanguage, t.FwCountry, l.FwLanguage, shared.FwCountry, unsafe.Sizeof(l))
}
