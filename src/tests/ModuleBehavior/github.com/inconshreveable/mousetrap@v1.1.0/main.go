// The behavioral proof sample for github.com/inconshreveable/mousetrap v1.1.0, a module with no Go tests.
//
// StartedByExplorer is the module's whole API. On Windows it walks the process snapshot (CreateToolhelp32Snapshot,
// Process32First, Process32Next) to find the parent process and compares its name with explorer.exe; elsewhere it
// returns false. A program started by `go run`, by `dotnet` or by a runner's shell was not started by Explorer, so
// every host prints false, and on Windows the converted syscall path has run to reach it.
package main

import (
	"fmt"

	"github.com/inconshreveable/mousetrap"
)

func main() {
	fmt.Println("StartedByExplorer:", mousetrap.StartedByExplorer())
}
