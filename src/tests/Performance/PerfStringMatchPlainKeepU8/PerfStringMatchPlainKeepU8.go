package main

import (
	"fmt"
	"strings"
	"time"
)

// Literal-string hot paths: switch-on-string dispatch, literal-prefix scanning,
// literal returns, and literal map keys -- the shapes the tiered @string literal
// optimizations target in converted C# (span comparison operators, u8 casts,
// hoisted literals). Go allocates nothing at any of these sites.

var lines = []string{
	"//go:build linux && amd64",
	"// regular comment line",
	"package main",
	"import \"fmt\"",
	"func main() {",
	"\tx := true",
	"}",
	"//go:build windows",
	"var y = false",
	"// another comment",
}

var words = []string{"true", "false", "linux", "windows", "amd64", "arm64", "ignore", "main"}

func classify(w string) int {
	switch w {
	case "true":
		return 1
	case "false":
		return 2
	case "linux", "windows", "darwin":
		return 3
	case "amd64", "arm64":
		return 4
	}
	return 0
}

func kindName(k int) string {
	switch k {
	case 1, 2:
		return "bool"
	case 3:
		return "goos"
	case 4:
		return "goarch"
	}
	return "word"
}

func run(n int) int {
	total := 0
	counts := map[string]int{}

	for i := 0; i < n; i++ {
		line := lines[i%len(lines)]
		if strings.HasPrefix(line, "//go:build") {
			counts["build"]++
			total += len(line)
		} else if strings.HasPrefix(line, "// ") {
			counts["comment"]++
		}

		w := words[i%len(words)]
		k := classify(w)
		total += k
		total += len(kindName(k))
	}

	total += counts["build"]*3 + counts["comment"]
	return total
}

func main() {
	start := time.Now().UnixNano()

	total := run(20000000)

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", total)
	fmt.Println("elapsed_ns:", elapsed)
}
