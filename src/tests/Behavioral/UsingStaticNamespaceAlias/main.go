// Guards the using-static alias rule (visitFile.go, importsStaticMembers): a file that reaches Go
// names bare through a `using static` never imports a whole .NET namespace, and binds by alias
// exactly the .NET types its emission names.
//
// The two on-demand namespaces are both demanded here: a function that executes a `go` keeps its
// frame with [MethodImpl(MethodImplOptions.NoInlining)] (System.Runtime.CompilerServices), and a
// struct with a zero-size field is laid out at Go's offsets with [StructLayout] / [FieldOffset]
// (System.Runtime.InteropServices). The dot-import below makes Unsafe, Closure and Marshal bare
// names -- and each of them is also a type in one of those namespaces. Imported as namespaces, every
// bare use below would be CS0229 (ambiguous), which is how go/types' test host broke in TRAIN M.
//
// The dot-import is deliberately NOT the first import, the shape of both real sites.
package main

import (
	"fmt"

	. "UsingStaticNamespaceAlias/aliaslib"
)

type noCopy struct{}

// counter carries a zero-size field, so it takes Go's own explicit layout.
type counter struct {
	_ noCopy
	v int32
}

// launch executes a go, so its frame is kept (NoInlining).
func launch(done chan string) {
	go func() {
		done <- Marshal(Unsafe)
	}()
}

func main() {
	done := make(chan string)
	launch(done)
	fmt.Println(<-done)

	c := counter{v: 7}
	fmt.Println(Closure(int(c.v)))
	fmt.Println(Unsafe)
}
