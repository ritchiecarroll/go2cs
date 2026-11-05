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
// GOARCH is printed as a fact about the binary, not as a value. The converted standard library is ONE build per
// GOOS, converted for amd64 and shipped under both mac RIDs, so its runtime.GOARCH is "amd64" on an Apple-silicon
// host where Go's is "arm64" (osx-arm64's behavioral leg read a stdout mismatch on exactly this line). What the
// build info must get right is that its GOARCH setting is the binary's own runtime.GOARCH, which holds on both
// sides on every host.
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
			fmt.Println("GOARCH is runtime.GOARCH:", setting.Value == runtime.GOARCH)
		}
	}
}
