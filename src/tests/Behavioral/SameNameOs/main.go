// Two imported packages share the name os: the standard library's, whose FileInfo is an alias of io/fs's
// interface, and a local one with a FileInfo struct of its own. Each reference resolves to its own package's type,
// so the local struct is adapted to the standard interface where it is passed as one.
package main

import (
	"fmt"
	"os"

	los "SameNameOs/hooks/os"
)

func describe(fi os.FileInfo) string {
	return fmt.Sprintf("%s %d %v", fi.Name(), fi.Size(), fi.IsDir())
}

func main() {
	h := los.NewFileInfo("a.txt", 42)
	fmt.Println(describe(h))

	var fi os.FileInfo = los.NewFileInfo("b.txt", 7)
	fmt.Println(fi.Name(), fi.Size())
}
