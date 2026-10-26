package main

import "fmt"

// Slicing a POINTER-TO-ARRAY (`p[lo:hi:max]`, p of type `*[N]T`) auto-derefs in Go. The C#
// box `ж<array<T>>` has no slice/range members — its underlying `array<T>` does — so the
// converter must operate on the dereferenced array: `(~p).slice(…)` / `(~p)[..]`, not
// `p.slice(…)` (CS1929). Mirrors the runtime's select.go `cas1[:ncases:ncases]` and
// mprof.go `stk[:n:n]`, where the operand is a `*[N]T`. A deref-aliased pointer PARAMETER or
// RECEIVER is the EXCEPTION — it is emitted as the array value itself, so it is sliced directly
// (a `~` would deref a non-pointer, CS0023). See firstTwo (receiver) and sumParam (parameter).

type grid [4]int

// firstTwo slices the receiver (`b *grid`), which is emitted as the value `ref grid b` — `b[:2]`
// must be `b[..2]`, not `(~b)[..2]`.
func (b *grid) firstTwo() int {
	total := 0
	for _, v := range b[:2] {
		total += v
	}
	return total
}

// sumParam slices the pointer parameter `p *grid` (deref-aliased), also directly: `p[:]` -> `p[..]`.
func sumParam(p *grid) int {
	total := 0
	for _, v := range p[:] {
		total += v
	}
	return total
}

func main() {
	arr := [5]int{10, 20, 30, 40, 50}
	p := &arr

	full := p[:]            // (~p)[..]
	low := p[2:]            // (~p)[2..]
	high := p[:3]           // (~p)[..3]
	mid := p[1:4]           // (~p)[1..4]
	three := p[1:3:4]       // (~p).slice(1, 3, 4)

	fmt.Println(full)
	fmt.Println(low)
	fmt.Println(high)
	fmt.Println(mid)
	fmt.Println(three, len(three), cap(three))

	// A slice over the array pointer shares the array's backing storage.
	full[0] = 99
	fmt.Println(arr[0])

	// Receiver and parameter forms: sliced directly (no `~`).
	g := grid{2, 4, 6, 8}
	fmt.Println(g.firstTwo(), sumParam(&g))

	rangeIndexOnly()
}

// rangeIndexOnly ranges a `*[N]T` for its indices only. The length of a `*[N]T` is the constant N and an index-only range
// never dereferences the pointer, so a NIL pointer still ranges N times in Go: `range(nilp, 3)`, not a read of `.Value`.
func rangeIndexOnly() {
	var nilp *[3]int
	for i := range nilp {
		fmt.Print(i, " ")
	}
	fmt.Println()

	var nilg *grid
	count := 0
	for range nilg {
		count++
	}
	fmt.Println("nil *grid ranges", count)

	p := &[2]string{"a", "b"}
	for i := range p {
		p[i] += "!"
	}
	fmt.Println(p[0], p[1])

	var g grid
	g.fill()
	fmt.Println("receiver ranges", g)
}

// fill ranges its pointer receiver for indices only: runtime's pageBits.setAll. The receiver is emitted as the value it
// addresses (`ref grid b`), never a box, so it ranges as the array, `range(b)`.
func (b *grid) fill() {
	for i := range b {
		b[i] = i * 10
	}
}
