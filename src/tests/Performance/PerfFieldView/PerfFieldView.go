package main

import (
	"fmt"
	"time"
)

// Field addresses taken through a pointer, in the shape of crypto/internal/fiat's P-256 point
// arithmetic: a point of three field elements, each element operation taking *element, every
// operand an `&p.f`. In the converted code each `&p.f` site is its own accessor delegate, and a
// box answers every site from one cached view per field (golib ж.Views.cs), so the lookup that
// matches a site to its field's view runs once per operand. `&pts[i]` mints a fresh element box per
// expression, so the first `&p.f` on each asks the element box for its views from nothing.
// Deterministic: pure integer mixing over fixed rounds, one checksum line.

type element struct {
	v [4]uint64
}

type point struct {
	x, y, z element
}

// Add and Mul have fiat's method shape: a pointer receiver, pointer operands, the receiver returned.
func (e *element) Add(a, b *element) *element {
	for i := 0; i < 4; i++ {
		e.v[i] = a.v[i] + b.v[i]
	}
	return e
}

func (e *element) Mul(a, b *element) *element {
	for i := 0; i < 4; i++ {
		e.v[i] = a.v[i]*(b.v[i]|1) ^ e.v[i]>>7
	}
	return e
}

// step is the point-arithmetic hot path: twelve operands across three fields, each site its own
// accessor, so the z sites look past the x and y views first.
func (p *point) step() *point {
	p.x.Add(&p.y, &p.z)
	p.y.Mul(&p.z, &p.x)
	p.z.Add(&p.x, &p.y)
	p.z.Mul(&p.z, &p.y)
	return p
}

func main() {
	start := time.Now().UnixNano()

	var p point
	p.x.v = [4]uint64{0x9e3779b97f4a7c15, 0xbf58476d1ce4e5b9, 0x94d049bb133111eb, 0x2545f4914f6cdd1d}
	p.y.v = [4]uint64{1, 2, 3, 4}
	p.z.v = [4]uint64{5, 6, 7, 8}
	for r := 0; r < 2000000; r++ {
		p.step()
	}

	pts := make([]point, 64)
	for i := range pts {
		pts[i].x.v[0] = uint64(i)
		pts[i].y.v[1] = uint64(i) * 3
	}
	for r := 0; r < 30000; r++ {
		for i := range pts {
			pts[i].step()
		}
	}

	sum := p.x.v[0] ^ p.y.v[1] ^ p.z.v[2] ^ p.z.v[3]
	for i := range pts {
		sum ^= pts[i].z.v[i&3]
	}

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", sum)
	fmt.Println("elapsed_ns:", elapsed)
}
