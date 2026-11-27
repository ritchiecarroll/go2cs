package main

import (
	"fmt"
	"go/build"
	"runtime"
)

// The target a program believes it runs on, read two ways: runtime.GOARCH, the constant the runtime package
// declares, and go/build's default context, which takes its GOARCH from internal/buildcfg's default. Go compiles
// both as constants of the target the binary was built for.
//
// The converted standard library has two darwin flavours. darwin/arm64 compiles Go's own constants. darwin/amd64
// keeps a stand-in for both (runtime's GOARCH and buildcfg's defaultGOARCH) that reads the PROCESS architecture,
// because that flavour also runs in arm64 processes: a local build on Apple silicon with GoTargetArch unset, and any
// package built before the osx-arm64 asset carries arm64 bytes. So on an Apple-silicon host every side here prints
// arm64, Go and both flavours alike, and on an Intel Mac every side prints amd64. A stand-in that stopped reading the
// process would print amd64 on Apple silicon, and this program would fail there.
//
// build.Default.GOARCH honours a GOARCH variable in the environment, as Go's does; both sides read the same one.
func main() {
	fmt.Println("runtime.GOOS:", runtime.GOOS)
	fmt.Println("runtime.GOARCH:", runtime.GOARCH)
	fmt.Println("build.Default.GOOS:", build.Default.GOOS)
	fmt.Println("build.Default.GOARCH:", build.Default.GOARCH)
	fmt.Println("the two agree:", build.Default.GOARCH == runtime.GOARCH)
}
