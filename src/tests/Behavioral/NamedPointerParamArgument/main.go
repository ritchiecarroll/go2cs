// Named-pointer parameter guard: a parameter whose type is a NAMED pointer (`type Features *flagSet`)
// is emitted as the generated wrapper class and gets no deref alias, unlike a plain `*flagSet`
// parameter (`ж<flagSet> Ꮡo` plus `ref var o`). Its uses must name the parameter itself: passing it
// to a `*flagSet` parameter or comparing it with nil used to emit `Ꮡf`, a box that does not exist
// (CS0103), and `*f = v` assigned the parameter instead of storing through it. The call shape is
// klauspost/cpuid/v2's (*CPUInfo).HasAll.
package main

import "fmt"

type flagSet [2]uint8

type Features *flagSet

func (s *flagSet) hasSetP(o *flagSet) bool { return s[0]&o[0] == o[0] }

type CPU struct{ fs flagSet }

func (c *CPU) HasAll(f Features) bool { return c.fs.hasSetP(f) }

func describe(f Features) string {
	if f == nil {
		return "nil"
	}

	return fmt.Sprint((*f)[0], (*f)[1])
}

// reset stores through the parameter: Go writes the caller's flagSet, not a local copy.
func reset(f Features) { *f = flagSet{9, 9} }

func main() {
	c := &CPU{fs: flagSet{3, 0}}
	fmt.Println(c.HasAll(&flagSet{1, 0}), c.HasAll(&flagSet{4, 0}))
	fmt.Println(describe(&flagSet{5, 6}), describe(nil))

	fs := &flagSet{1, 2}
	reset(fs)
	fmt.Println(fs[0], fs[1])
}
