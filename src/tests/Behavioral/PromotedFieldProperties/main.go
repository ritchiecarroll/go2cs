// Parity seat #31b: a PROMOTED field, read or written through a pointer, reads `p.f` in the converted C#, the property
// go2cs-gen walks over the embeds the selection walks. Each shape reads and writes through a real box (a slice of
// pointers, a range variable), so the Output phase compares behavior and the Target phase pins the spelling:
//   - one embed down, through value embeds, reading and compound-assigning;
//   - through a POINTER embed, whose field lives in another allocation: a write reaches the embedded struct itself;
//   - two embeds down, the second a pointer;
//   - a nil pointer embed panics on the promoted read, as Go does.
package main

import "fmt"

type Point struct{ X, Y int }

type Named struct{ Name string }

type Shape struct {
	Point
	Named
	sides int
}

type Labeled struct {
	*Shape
	label string
}

type Deep struct {
	Labeled
	depth int
}

func main() {
	shapes := []*Shape{{Point: Point{1, 2}, Named: Named{"tri"}, sides: 3}, {Point: Point{4, 5}, Named: Named{"sq"}, sides: 4}}

	for _, s := range shapes {
		s.X += 10
		s.Y *= 2
		s.Name += "!"
		fmt.Println(s.X, s.Y, s.Name, s.sides)
	}

	labeled := []*Labeled{{Shape: shapes[0], label: "first"}}
	labeled[0].X = 100
	fmt.Println(shapes[0].X, labeled[0].Name, labeled[0].label)

	deep := []*Deep{{Labeled: Labeled{Shape: shapes[1], label: "second"}, depth: 2}}
	deep[0].Y++
	deep[0].label += "?"
	fmt.Println(shapes[1].Y, deep[0].label, deep[0].Name, deep[0].depth)

	readNil()
}

func readNil() {
	defer func() { fmt.Println("recovered:", recover() != nil) }()

	broken := []*Labeled{{label: "nil shape"}}
	fmt.Println(broken[0].X)
}
