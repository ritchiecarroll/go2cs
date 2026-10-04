// SortMethodSelfCapture guards a call to a package-level function F whose argument's type declares a method also
// named F in the same package. Go always binds the FUNCTION. The converter emits the method as the extension
// `F(this T x)` in the same package class, and C# overload resolution prefers it over F(Interface) -- an exact
// match beats a boxing conversion -- so the call binds the METHOD instead:
//
//   - sort's own IntSlice/Float64Slice/StringSlice.Sort is `func (x StringSlice) Sort() { Sort(x) }`, which then
//     calls itself: any .Sort() on those types overflowed the stack (pflag's sortFlags, so cobra too);
//   - a class-qualified call from another package, sort.Sort(sort.StringSlice(v)), sees the same extension as an
//     ordinary static member of sort's package class;
//   - a user package with the same pair (words.Sort calling the package's own Sort) captures the same way.
//
// A simple-name call through `using static` never captures: C# does not import extension methods as simple names.
package main

import (
	"fmt"
	"sort"
)

type words []string

func (w words) Len() int           { return len(w) }
func (w words) Less(i, j int) bool { return w[i] < w[j] }
func (w words) Swap(i, j int)      { w[i], w[j] = w[j], w[i] }

// Sort is a package-level function with the same name as the method below -- the user-defined pair.
func Sort(data sort.Interface) { sort.Sort(data) }

// Sort sorts w through the package-level Sort, as sort's own XSlice.Sort methods do.
func (w words) Sort() { Sort(w) }

func main() {
	ints := sort.IntSlice{3, 1, 2}
	ints.Sort()
	fmt.Println("IntSlice.Sort:", ints)

	floats := sort.Float64Slice{2.5, -1, 0.5}
	floats.Sort()
	fmt.Println("Float64Slice.Sort:", floats)

	strs := sort.StringSlice{"pear", "apple", "fig"}
	strs.Sort()
	fmt.Println("StringSlice.Sort:", strs)

	names := []string{"carol", "alice", "bob"}
	sort.Sort(sort.StringSlice(names))
	fmt.Println("sort.Sort(sort.StringSlice(v)):", names)

	w := words{"delta", "alpha", "charlie"}
	w.Sort()
	fmt.Println("user words.Sort:", w)
}
