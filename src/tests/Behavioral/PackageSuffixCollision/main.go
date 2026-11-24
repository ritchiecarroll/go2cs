package main

// A converted package's class is <name>_package. Each row names a Go identifier so that it could collide with such a
// class, or capture the emitted references to one: its own package's (in the libraries), an imported package's (here,
// at package level, as a local and as a parameter), a member's (field and method, the near-miss), and an import alias's.

import (
	"fmt"
	"strconv"
	"strings"

	"PackageSuffixCollision/ownc"
	"PackageSuffixCollision/ownf"
	"PackageSuffixCollision/ownt"
	"PackageSuffixCollision/ownv"
	"PackageSuffixCollision/shadow"
)

// main_package is the main package's own class name, as a type.
type main_package struct {
	fmt_package int // a field named like an imported package's class
}

// holder carries a method named exactly like an imported package's class (a field of that name sits on main_package).
type holder struct{ n int }

// fmt_package is a method named like an imported package's class.
func (h holder) fmt_package() int { return h.n + 1 }

// Fmt_package is an exported method with the suffix.
func (m main_package) Fmt_package() int { return m.fmt_package * 3 }

// fmt_package at package level, while the file also calls fmt.Println.
var fmt_package = 1

func local() string {
	strings_package := "local" // a local named like an imported package's class
	return strings.ToUpper(strings_package)
}

func param(strconv_package int) string { // a parameter named like an imported package's class
	return strconv.Itoa(strconv_package + 1)
}

func main() {
	fmt.Println("own type:", ownt.Get(21))
	fmt.Println("own var:", ownv.Get(), ownv.Get())
	fmt.Println("own const:", ownc.Get())
	fmt.Println("own func:", ownf.Get())
	fmt.Println("pkg var:", fmt_package)
	fmt_package += 10
	fmt.Println("pkg var after:", fmt_package)
	fmt.Println("local:", local())
	fmt.Println("param:", param(41))
	m := main_package{fmt_package: 7}
	fmt.Println("field/method:", m.fmt_package, holder{n: 7}.fmt_package(), m.Fmt_package())
	fmt.Println("alias shadow:", shadow.Get())
}
