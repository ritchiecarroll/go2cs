package main

import (
	"fmt"
	"reflect"

	"RootedUsingCapture/lib"
)

func main() {
	t := reflect.TypeOf(lib.Guarded{})
	f := t.Field(0)
	fmt.Println("embed:", f.Name, f.Anonymous)
	fmt.Println("type:", f.Type.String())
	fmt.Println("pkgpath:", f.Type.PkgPath())
	fmt.Println("pointer methods:", reflect.PointerTo(f.Type).NumMethod())
	_, tryLock := reflect.PointerTo(t).MethodByName("TryLock")
	fmt.Println("promoted TryLock:", tryLock)

	g := lib.New(lib.Base())
	fmt.Println("add:", g.Add(4))
	fmt.Printf("%T\n", &g.Mutex)
}
