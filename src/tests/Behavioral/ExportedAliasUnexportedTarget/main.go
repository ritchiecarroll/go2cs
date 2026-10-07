// Exported-alias-of-an-unexported-type guard (logrus's MutexWrap, testify's CompareType).
//
// aliaslib exports `type MutexWrap = mutexWrap` and `type CompareType = compareResult`. Every
// importer declares each exported alias of an import as a `global using` over its target, so while
// the target stayed internal the importing assembly failed with CS0122, for the used alias AND for
// the unused one. The SUBPACKAGE is what makes this a guard: in one assembly an internal target is
// visible and the defect cannot show.
package main

import (
	"fmt"

	"ExportedAliasUnexportedTarget/aliaslib"
)

// Logger holds the aliased type the way logrus's Logger holds its MutexWrap.
type Logger struct {
	mu    aliaslib.MutexWrap
	lines []string
}

func (l *Logger) Log(line string) {
	l.mu.Lock()
	defer l.mu.Unlock()

	l.lines = append(l.lines, line)
}

func main() {
	var l Logger
	l.Log("first")
	l.mu.Disable()
	l.Log("second")
	fmt.Println(len(l.lines), l.lines[0], l.lines[1])

	var w aliaslib.MutexWrap
	w.Lock()
	w.Unlock()
	fmt.Println("lock round trip ok")

	fmt.Println(aliaslib.Compare(1, 2), aliaslib.Compare(2, 2), aliaslib.Compare(3, 2))
}
