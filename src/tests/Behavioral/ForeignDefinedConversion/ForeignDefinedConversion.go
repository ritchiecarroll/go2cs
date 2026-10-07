// Conversion into a type defined over ANOTHER package's named type (logrus' hooks/slog:
// `type Level logrus.Level; var _ slog.Leveler = Level(0)`).
//
// Such a type keeps the foreign base in its [GoType], so its operators convert only from that base.
// A constant or a basic value converted into it was cast directly, `((Level)0)`, which needs two
// user-defined conversions (int -> levellib.Level -> Level) and is CS0030. It now hops through the
// base. The stdlib's only instances are typed const declarations (windows registry's
// `Key(syscall.HKEY_*)`), which take a different path that already hopped.
package main

import (
	"fmt"
	"time"

	"ForeignDefinedConversion/levellib"
)

// Level is written over another package's named integer.
type Level levellib.Level

func (l Level) String() string { return fmt.Sprintf("Level(%d)", uint32(levellib.Level(l))) }

// Dur is written over a standard-library named integer.
type Dur time.Duration

// A package-level conversion of a constant, logrus' exact shape.
var _ fmt.Stringer = Level(0)

func main() {
	fmt.Println(Level(0), Level(3))

	var u uint32 = 5
	fmt.Println(Level(u))

	n := 7
	fmt.Println(Level(n), Level(n+1))

	fmt.Println(time.Duration(Dur(5)), time.Duration(Dur(2)*Dur(time.Second)))

	fmt.Println(levellib.Level(Level(4)).Double())
}
