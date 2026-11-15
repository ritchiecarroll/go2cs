// AliasedImportRenamedMember guards a member that go2cs collision-renames in its own package, reached
// through an EXPLICITLY-aliased import. time declares `ΔLocation`, `ΔMonth`, `ΔUTC`, `ΔLocal` and `ΔSecond`
// (each Go name collides with a Time method); Millisecond, March and RFC3339 are not renamed.
// modernc.org/libc imports time as `gotime "time"` and writes `*gotime.Location`, `gotime.UTC`, `gotime.Local`,
// `gotime.Second` and `gotime.Month(…)` (sqlite-cs S10). go2cs's alias table is keyed by the DECLARED package
// name, so before the fix the aliased spellings kept their Go names: CS0426 for the types, and UTC, Local and
// Second named the Time methods' groups (CS1503 for gotime.UTC as an argument, CS0019 for gotime.Local != nil).
package main

import (
	"fmt"
	gotime "time"
)

type tzLocation struct {
	tz  string
	loc *gotime.Location
}

func fixed(name string, offset int) *gotime.Location {
	return gotime.FixedZone(name, offset)
}

func timeout(n int) gotime.Duration {
	return gotime.Duration(n)*gotime.Second + 250*gotime.Millisecond
}

func main() {
	z := tzLocation{tz: "UTC", loc: gotime.UTC}
	fmt.Println(z.tz, z.loc.String())

	var locs []*gotime.Location
	locs = append(locs, gotime.UTC, fixed("EST", -5*3600))
	for _, l := range locs {
		fmt.Println(l)
	}

	fmt.Println(gotime.Local != nil, gotime.Local == gotime.UTC || gotime.Local != gotime.UTC)

	fmt.Println(timeout(3), gotime.Second*2, gotime.Millisecond < gotime.Second)

	t := gotime.Date(2024, gotime.March, 5, 6, 7, 8, 0, gotime.UTC)
	fmt.Println(t.In(fixed("EST", -5*3600)).Format(gotime.RFC3339), t.Location() == gotime.UTC)

	m := gotime.Month(2 + 1)
	fmt.Println(m, t.Month() == m, gotime.Month(12))
}
