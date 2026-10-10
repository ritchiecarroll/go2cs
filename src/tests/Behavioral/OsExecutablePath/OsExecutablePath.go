package main

import (
	"fmt"
	"os"
	"path/filepath"
)

// os.Executable must name a real, absolute file on every target. On darwin Go fills
// os.executablePath from the runtime at startup (a //go:linkname push from runtime.sysargs),
// so a conversion that leaves that field empty answers "cannot find executable path" there
// while linux (/proc/self/exe) and windows (GetModuleFileName) still succeed. The printed
// lines are properties, never the path itself: the Go binary and the converted host have
// different names and locations.
func main() {
	path, err := os.Executable()
	fmt.Println("error is nil:", err == nil)
	if err != nil {
		fmt.Println("error:", err)
		return
	}
	fmt.Println("path is absolute:", filepath.IsAbs(path))

	info, statErr := os.Stat(path)
	fmt.Println("path exists:", statErr == nil)
	if statErr == nil {
		fmt.Println("path is a regular file:", info.Mode().IsRegular())
	}

	again, err2 := os.Executable()
	fmt.Println("second call agrees:", err2 == nil && again == path)
}
