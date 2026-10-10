// Phase5CoverageAPIs guards runtime/coverage in a program built without -cover: every exported
// function returns Go's error rather than reaching a throwing stub. ClearCounters is the one whose
// first call is getCovCounterList, a runtime linkname push the managed model answers with the
// empty list Go's runtime returns for a binary with no counter sections; it runs last so the four
// siblings are measured even if it fails.
package main

import (
	"fmt"
	"io"
	"os"
	"runtime/coverage"
)

func main() {
	dir := os.TempDir()
	fmt.Println("WriteCounters:", coverage.WriteCounters(io.Discard))
	fmt.Println("WriteCountersDir:", coverage.WriteCountersDir(dir))
	fmt.Println("WriteMeta:", coverage.WriteMeta(io.Discard))
	fmt.Println("WriteMetaDir:", coverage.WriteMetaDir(dir))
	fmt.Println("ClearCounters:", coverage.ClearCounters())
}
