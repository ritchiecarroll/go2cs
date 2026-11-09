package main

import "fmt"

type Tag struct {
	str  string
	pExt uint16
}

var Und Tag

func (t Tag) equalTags(o Tag) bool { return t.str == o.str }

func (t Tag) gen(b []byte) int { return copy(b, "core") }

// RemakeString calls a value method on its own receiver, so it is emitted over the receiver box.
// Shaped like x/text's (*Tag).RemakeString: a stack buffer sliced through a method call.
func (t *Tag) RemakeString() {
	if t.equalTags(Und) {
		return
	}
	var buf [8]byte
	b := buf[:t.gen(buf[:])]
	t.pExt = uint16(len(b))
	t.str = string(b) + "!"
}

func next(t Tag) (Tag, error) {
	t.str += "+"
	return t, nil
}

// A value receiver calling the pointer method directly.
func (t Tag) minimize() (Tag, error) {
	t.RemakeString()
	return t, nil
}

// The receiver redefined by `:=` before the call (x/text's Tag.minimize).
func (t Tag) minimizeDefine() (Tag, error) {
	t, err := next(t)
	if err != nil {
		return t, err
	}
	t.RemakeString()
	return t, nil
}

// The receiver reassigned by `=` before the call.
func (t Tag) minimizeAssign() (Tag, error) {
	var err error
	t, err = next(t)
	if err != nil {
		return t, err
	}
	t.RemakeString()
	return t, nil
}

// A method value of the pointer method on the value receiver.
func (t Tag) minimizeMethodValue() Tag {
	remake := t.RemakeString
	remake()
	return t
}

type wrapper struct {
	tag Tag
	n   int
}

// The pointer method called through a field of the value receiver.
func (w wrapper) remakeField() wrapper {
	w.tag.RemakeString()
	w.n++
	return w
}

// handle embeds a pointer: a call promoted through it binds that pointer and takes no address of
// the receiver, so the writes land in the shared Tag.
type handle struct{ *Tag }

func (h handle) remake() { h.RemakeString() }

func (h handle) remakeMethodValue() {
	remake := h.RemakeString
	remake()
}

func remakeHandleParam(h handle) { h.RemakeString() }

// Controls: a value parameter and a local.
func remakeParam(t Tag) Tag {
	t.RemakeString()
	return t
}

func remakeLocal() Tag {
	t := Tag{str: "local"}
	t.RemakeString()
	return t
}

func main() {
	orig := Tag{str: "a"}

	t, _ := orig.minimize()
	fmt.Println(t.str, t.pExt, orig.str, orig.pExt)

	t, _ = orig.minimizeDefine()
	fmt.Println(t.str, t.pExt, orig.str, orig.pExt)

	t, _ = orig.minimizeAssign()
	fmt.Println(t.str, t.pExt, orig.str, orig.pExt)

	t = orig.minimizeMethodValue()
	fmt.Println(t.str, t.pExt, orig.str, orig.pExt)

	w := wrapper{tag: orig}
	w2 := w.remakeField()
	fmt.Println(w2.tag.str, w2.tag.pExt, w2.n, w.tag.str, w.n)

	t = remakeParam(orig)
	fmt.Println(t.str, t.pExt, orig.str)

	t = remakeLocal()
	fmt.Println(t.str, t.pExt)

	p1, p2, p3 := &Tag{str: "p1"}, &Tag{str: "p2"}, &Tag{str: "p3"}
	handle{p1}.remake()
	handle{p2}.remakeMethodValue()
	remakeHandleParam(handle{p3})
	fmt.Println(p1.str, p1.pExt, p2.str, p2.pExt, p3.str, p3.pExt)

	p4, p5 := &Tag{str: "p4"}, &Tag{str: "p5"}
	h4, h5 := handle{p4}, handle{p5}
	h4.RemakeString()
	remake := h5.RemakeString
	remake()
	fmt.Println(p4.str, p4.pExt, p5.str, p5.pExt)

	// The empty tag equals Und, so RemakeString returns early and leaves it unchanged.
	var empty Tag
	t, _ = empty.minimize()
	fmt.Printf("%q %d\n", t.str, t.pExt)
}
