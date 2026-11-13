package main

import "fmt"

// sqlite-cs S9: modernc.org/libc libc_windows.go `type ( long = int32; ulong = uint32 )`: package-level type ALIASES whose
// names are C# keywords.
type (
	long  = int32
	ulong = uint32
)

func size(n int) long { return long(n) }

func main() {
	var u ulong = 7
	fmt.Println(size(3), u, long(-1)) // Go: 3 7 -1
}
