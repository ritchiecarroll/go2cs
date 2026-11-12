package main

import (
	"fmt"

	. "MethodHolderAlias/geom"
	"MethodHolderAlias/geom/part"
	html "MethodHolderAlias/html/plate"
	"MethodHolderAlias/tone"
)

type field struct{ s Shape }

// Area reads the dot-imported function Area, which shares its name, and the dot-imported constant.
func (f field) Area() int { return Area(f.s.W.Len, f.s.H.Len) + Unit }

func main() {
	s := Shape{W: part.Side{Len: 4}, H: part.Side{Len: 5}}
	fmt.Println("child namespace", s.Area(), s.Unit(), Area(2, 3), Unit)

	area := Shape.Area
	unit := (*Shape).Unit
	fmt.Println("expressions", area(s), unit(&s), part.Side.Double(part.Side{Len: 7}))

	fmt.Println("dot import", field{s}.Area())

	pitch, base := tone.Play(2)
	fmt.Println("type named like its package", pitch, base, tone.Pitch(1), tone.Base)

	fmt.Println("import of the same name", html.New("page").Render(), html.Render("x"))
}
