// Guards the frame names of package initialization (runtime/managed_impl.cs, goFrameName): a
// package-level var initializer runs in Go's `init` frame, a func literal inside one is
// `init.funcN`, and the package's init functions are `init.0`, `init.1`, ... in the order the
// compiler sees them -- files in filename order, then declaration order within each (a.go, this
// file, z.go). Measured against Go 1.24.13; output-compared with go run.
package main

import (
	"fmt"
	"runtime"
)

func here() string {
	pc, _, line, _ := runtime.Caller(1)
	return fmt.Sprintf("%s:%d", runtime.FuncForPC(pc).Name(), line)
}

var direct = here()

var viaClosure = func() string { return here() }()

var viaClosure2 = func() string { return here() }()

func init() { fmt.Println("main.go init 1:", here()) }

func init() {
	literal := func() string { return here() }
	fmt.Println("main.go init 2:", here(), "literal:", literal())
}

func main() {
	fmt.Println("direct:", direct)
	fmt.Println("viaClosure:", viaClosure)
	fmt.Println("viaClosure2:", viaClosure2)
}
