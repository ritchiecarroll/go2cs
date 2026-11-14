package main

import (
	"fmt"
	"unicode/utf8"
)

// Converting an integer to a string yields the UTF-8 encoding of that code point, and the
// conversion to a NAMED string type does the same. The named type's wrapper converts only from
// a string, so `(literalErr)b` did not compile (CS0030); it now goes through the same code-point
// step as the unnamed form. github.com/ncruces/go-strftime returns `literalErr(b)` for a byte b
// (sqlite-cs row S6).

type literalErr string

func (e literalErr) Error() string { return "strftime: unsupported literal: " + string(e) }

type digit byte

const star = '*'

func check(b byte) error {
	if '0' <= b && b <= '9' {
		return literalErr(b)
	}
	return nil
}

func main() {
	fmt.Println(check('7'), check('x') == nil)

	var r rune = 'λ'
	var n int = 0x4e16
	var u uint16 = 'é'
	var d digit = '3'
	fmt.Println(literalErr(r), literalErr(n), literalErr(u), literalErr(d), literalErr(d+1))
	fmt.Println(literalErr(star), literalErr(utf8.RuneError) == "�", len(literalErr(n)), literalErr('A')[0])
}
