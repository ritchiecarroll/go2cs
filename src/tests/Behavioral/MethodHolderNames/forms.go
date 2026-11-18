package main

import "fmt"

// Tag and Seal are struct types, and Chest declares a method named like each. Inside Chest's methods
// every spelling of either type stands beside a method of its name.
type Tag struct {
	id    int
	Value int
	inner Seal
}

type Seal struct{ n int }

type Chest struct {
	tag  Tag
	tags []Tag
}

func (c *Chest) Tag() Tag { return c.tag }

// Seal takes the address of a field through two struct fields whose types carry the methods' names.
func (c *Chest) Seal() *int { return &c.tag.inner.n }

// Forms writes the type Tag in each position Go allows.
func (c *Chest) Forms(v any) string {
	lit := Tag{id: 1, Value: 2}
	ptr := &Tag{id: 3}

	var zero Tag

	list := []Tag{lit, *ptr, zero}
	table := map[Tag]int{lit: 1}
	made := make([]Tag, 2)

	fresh := new(Tag)
	fresh.Value = 9

	valueAddr := &ptr.Value
	*valueAddr += 4

	idAddr := &c.tag.id
	*idAddr = 7

	_, isTag := v.(Tag)
	_, isPtr := v.(*Tag)

	kind := "other"

	switch v.(type) {
	case Tag:
		kind = "tag"
	case *Tag, []Tag:
		kind = "tags"
	}

	count := len(list) + len(table) + len(made) + pick[Tag](list, 0).id

	return fmt.Sprint(lit.id, ptr.Value, zero.id, count, fresh.Value, c.tag.id, isTag, isPtr, kind, *c.Seal())
}

func pick[T any](s []T, i int) T { return s[i] }

func forms() {
	c := &Chest{tags: []Tag{{id: 2}}}
	fmt.Println("forms", c.Forms(Tag{}), c.Forms(&Tag{}), c.Forms(1), c.Tag().id, len(c.tags))
}
