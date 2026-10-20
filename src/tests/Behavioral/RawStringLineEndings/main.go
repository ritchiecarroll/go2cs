// A Go raw string never holds a carriage return: the compiler drops any CR between the backquotes.
// The converter emits a multi-line raw string as a C# raw string literal, whose line breaks C#
// takes from the source file, and every checkout materializes converted .cs files with CRLF
// line endings. This program prints the length and the quoted bytes of each multi-line raw string
// shape the converter emits, so a build of the CHECKED-OUT .cs (not only a fresh transpile) has to
// agree with Go byte for byte.
package main

import "fmt"

// A package-level constant: emitted as a static readonly field.
const pemBlock = `-----BEGIN TEST-----
QUJDREVGR0g=
-----END TEST-----
`

// A []byte conversion of a raw string, as net/http/internal/testcert spells its certificate.
var rawBytes = []byte(`first
second`)

// A raw string as a format argument, as net/http's pattern conflict message spells it.
func describe(name string) string {
	return fmt.Sprintf(`name: %s
done`, name)
}

const quote = "`"

func main() {
	plain := `line one
line two`
	trailing := `ends with a newline
`
	// A raw string folded with constants into one concatenation, as go/build's tests spell it.
	chain := `head
` + quote + `x` + quote + `
tail`
	oneLine := `C:\no\newline\here`
	interpreted := "keeps\r\nits CR"

	for _, s := range []string{plain, trailing, pemBlock, describe("p"), chain, oneLine, interpreted} {
		fmt.Println(len(s))
		fmt.Printf("%q\n", s)
	}

	fmt.Println(len(rawBytes))
	fmt.Printf("%q\n", rawBytes)

	h := uint32(2166136261)
	for i := 0; i < len(pemBlock); i++ {
		h = (h ^ uint32(pemBlock[i])) * 16777619
	}
	fmt.Printf("%08x\n", h)
}
