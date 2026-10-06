// Guards init order against the no-inline carrier. Go runs a package's init functions in file order
// and, within a file, in source order. The converter writes each init as a C# module initializer,
// and C# runs those in declaration order. A no-inline mark written as a partial method would place
// that init's position at its GENERATED declaring part, which sorts after every source file, so a
// marked init would run last. An init therefore never takes the mark as `partial`; it keeps the
// [MethodImpl(MethodImplOptions.NoInlining)] attribute itself. The second init below is marked by
// the directive.
package main

import "fmt"

func init() { fmt.Println("main.go init 1") }

//go:noinline
func init() { fmt.Println("main.go init 2") }

func main() { fmt.Println("main") }
