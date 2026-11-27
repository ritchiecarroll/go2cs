package main

import (
	"fmt"
	"runtime"
	"runtime/debug"
)

// The build info a program reads about itself: its main package path, its main module, and the target it was
// built for. Only settings with an honest converted value are printed -- not CGO_ENABLED or DefaultGODEBUG,
// which can differ by build environment, and not -compiler, -buildmode or vcs.*, which go2cs never reports.
//
// GOARCH is printed as a value, and as a fact about the binary. darwin has two flavours of the converted standard
// library: darwin/arm64 compiles Go's own constant, and darwin/amd64, which also runs in arm64 processes, reads its
// GOARCH from the process. So on an Apple-silicon host both sides print "arm64" here, and on an amd64 host both print
// "amd64". This is the gate on that: osx-arm64's leg compares the value itself. The build info's GOARCH setting must
// also be the binary's own runtime.GOARCH, on both sides, on every host.
func main() {
	bi, ok := debug.ReadBuildInfo()
	fmt.Println("ok:", ok)

	if !ok {
		return
	}

	fmt.Println("path:", bi.Path)
	fmt.Println("main.path:", bi.Main.Path)
	fmt.Println("main.version:", bi.Main.Version)

	for _, setting := range bi.Settings {
		switch setting.Key {
		case "GOOS":
			fmt.Printf("%s: %s\n", setting.Key, setting.Value)
		case "GOARCH":
			fmt.Printf("%s: %s\n", setting.Key, setting.Value)
			fmt.Println("GOARCH is runtime.GOARCH:", setting.Value == runtime.GOARCH)
		}
	}
}
