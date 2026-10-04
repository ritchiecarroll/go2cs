// Package consumer imports a `go.`-hosted module beside standard packages whose aliases the converter
// writes root-qualified (unicode/utf8 beside unicode, runtime/debug beside runtime) -- the shape of
// testify's assert package, which imports go.yaml.in/yaml/v3.
package consumer

import (
	"runtime"
	"runtime/debug"
	"unicode"
	"unicode/utf8"

	"go.shadowlib.in/lib"
)

// Describe uses every import, so each alias is emitted and must bind.
func Describe(s string) string {
	upper := 0
	for _, r := range s {
		if unicode.IsUpper(r) {
			upper++
		}
	}
	_ = debug.SetGCPercent(debug.SetGCPercent(100))
	return lib.Greeting(s) + " runes=" + itoa(utf8.RuneCountInString(s)) + " upper=" + itoa(upper) + " gomaxprocs>0=" + bool2s(runtime.GOMAXPROCS(0) > 0)
}

func itoa(n int) string {
	if n == 0 {
		return "0"
	}
	digits := ""
	for ; n > 0; n /= 10 {
		digits = string(rune('0'+n%10)) + digits
	}
	return digits
}

func bool2s(b bool) string {
	if b {
		return "true"
	}
	return "false"
}
