package main

import (
	"fmt"
	"runtime/debug"
)

// The build info a program reads about itself: its main package path, its main module, and the target it was
// built for. Only settings with an honest converted value are printed -- not CGO_ENABLED or DefaultGODEBUG,
// which can differ by build environment, and not -compiler, -buildmode or vcs.*, which go2cs never reports.
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
		case "GOOS", "GOARCH":
			fmt.Printf("%s: %s\n", setting.Key, setting.Value)
		}
	}
}
