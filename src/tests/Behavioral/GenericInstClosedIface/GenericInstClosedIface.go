package main

import "fmt"

// One instantiation of a generic type declared HERE, converted to a NON-generic interface whose
// method names the type argument. The open definition cannot implement such an interface in C#:
// box<T>.val() returns T where valuer.val() returns point. So the pair is carried by an adapter
// over the closed instantiation, and every site that converts box[point] to valuer wraps.

type point struct{ x, y int }

// box: a value receiver whose method RETURNS the type parameter.
type box[T any] struct{ v T }

func (b box[T]) val() T { return b.v }

type valuer interface{ val() point }

// cell: the same behind a pointer receiver.
type cell[T any] struct{ v T }

func (c *cell[T]) get() T { return c.v }

type getter interface{ get() point }

// sink: the type parameter in PARAMETER position.
type sink[T any] struct{ n int }

func (s sink[T]) take(v T) int { return s.n + 1 }

type taker interface{ take(v point) int }

// bag: no required method names the type parameter, so every instantiation satisfies sizer and
// the open definition implements it directly.
type bag[T any] struct{ items []T }

func (b bag[T]) size() int { return len(b.items) }

type sizer interface{ size() int }

// outer is not generic; it promotes val() from an embedded instantiation.
type outer struct {
	box[point]
	n int
}

type holder struct {
	v valuer
	n int
}

func show(v valuer) {
	p := v.val()
	fmt.Println("argument", p.x, p.y)
}

func build(x, y int) valuer {
	return box[point]{point{x, y}}
}

func main() {
	var v valuer
	v = box[point]{point{1, 2}}
	fmt.Println("assignment", v.val().x, v.val().y)

	var w valuer = box[point]{point{3, 4}}
	fmt.Println("var", w.val().x, w.val().y)

	show(box[point]{point{5, 6}})

	r := build(7, 8)
	fmt.Println("return", r.val().x, r.val().y)

	s := []valuer{box[point]{point{9, 10}}, box[point]{point{11, 12}}}
	fmt.Println("element", s[0].val().x, s[1].val().y)

	h := holder{v: box[point]{point{13, 14}}, n: 1}
	fmt.Println("field", h.v.val().x, h.n)

	m := map[string]valuer{"a": box[point]{point{15, 16}}}
	fmt.Println("map value", m["a"].val().y)

	ch := make(chan valuer, 1)
	ch <- box[point]{point{17, 18}}
	fmt.Println("send", (<-ch).val().x)

	b := box[point]{point{1, 2}}
	fmt.Println("equal to value", v == b, w == b)
	fmt.Println("equal interfaces", v == valuer(b), v == w)

	back, ok := v.(box[point])
	fmt.Println("assert back", back.v.x, back.v.y, ok)

	switch t := w.(type) {
	case box[point]:
		fmt.Println("type switch", t.v.x, t.v.y)
	default:
		fmt.Println("type switch missed")
	}

	fmt.Printf("verbs %v %T\n", v, v)

	var a any = box[point]{point{1, 2}}
	av, ok := a.(valuer)
	fmt.Println("any to valuer", ok, av.val().x, av.val().y)
	fmt.Println("asserted equals converted", av == v)
	keys := map[valuer]string{v: "converted"}
	keys[av] = "asserted"
	fmt.Println("map key", len(keys), keys[v])
	fmt.Printf("asserted verbs %v %v\n", av, v)

	var n any = box[int]{7}
	_, ok = n.(valuer)
	fmt.Println("box[int] is valuer", ok)

	c := &cell[point]{point{21, 22}}
	var g getter = c
	fmt.Println("pointer", g.get().x, g.get().y)
	c.v.x = 23
	fmt.Println("pointer shares", g.get().x)
	gc, ok := g.(*cell[point])
	fmt.Println("pointer back", gc == c, ok)

	var t taker = sink[point]{n: 4}
	fmt.Println("parameter", t.take(point{1, 1}))

	var z sizer = bag[int]{[]int{1, 2, 3}}
	var y sizer = bag[string]{[]string{"a"}}
	fmt.Println("no parameter named", z.size(), y.size())

	var o valuer = outer{box[point]{point{24, 25}}, 1}
	fmt.Println("embedded", o.val().x, o.val().y)
}
