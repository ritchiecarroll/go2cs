// witnessprobe (Go reference): the narrow shape-witness population in miniature. lsconv's MappedSpan + FromLSPRange +
// lspRangeToVirtual, its masked non-generic caller (converters.go:136's shape), and gc/internal/gen's Ptr / Array /
// Deref / ConstArray. The C# probe prints the same lines; equal output is the bar.
package main

import "fmt"

// ---- lsconv's shapes ----

type Script interface {
	FileName() string
	Text() string
}

type SourceFile struct{ name, text string }

func (f *SourceFile) FileName() string { return f.name }
func (f *SourceFile) Text() string     { return f.text }

type MappedSpan[T Script] struct {
	Script T
	Start  int
}

func FromLSPRange[T Script](script T, start int) []MappedSpan[T] {
	return lspRangeToVirtual([]T{script}, start)
}

func lspRangeToVirtual[T Script](scripts []T, start int) []MappedSpan[T] {
	var out []MappedSpan[T]
	for i, s := range scripts {
		out = append(out, MappedSpan[T]{s, start + i + len(s.Text())})
	}
	return out
}

// converters.go:136's shape: a NON-generic caller that instantiates the generic with a pointer (masked today).
func FromLSPRangeForSourceFile(files []*SourceFile, start int) []MappedSpan[*SourceFile] {
	return lspRangeToVirtual(files, start)
}

// A callee that writes its []T parameter: Go shares the caller's backing array.
func renameFirst[T Script](scripts []T, other T) { scripts[0] = other }

// The same write from a callee that DOES call a method (so it takes the witness): still the caller's slice.
func renameFirstNamed[T Script](scripts []T, other T) string {
	scripts[0] = other
	return other.FileName()
}

// ---- gc/internal/gen's shapes ----

type Value interface{ kind() string }

type u8 struct{ v byte }

func (u8) kind() string { return "u8" }

type wrap[T Value] interface {
	Value
	wrap(x int) T
}

type Ptr[T Value] struct{ off int }

func (Ptr[T]) kind() string      { var x T; return "Ptr[" + x.kind() + "]" }
func (Ptr[T]) wrap(x int) Ptr[T] { return Ptr[T]{x} }

func Deref[W wrap[T], T Value](ptr Ptr[W]) T {
	var w W
	return w.wrap(ptr.off + 1)
}

type Array[T Value] struct{ vals []T }

func (Array[T]) kind() string { var x T; return "Array[" + x.kind() + "]" }

func ConstArray[T Value](vals []T, name string) (y Array[T]) {
	y.vals = vals
	return y
}

func main() {
	a := &SourceFile{"a.ts", "let x"}
	spans := FromLSPRange(a, 10)
	fmt.Println("FromLSPRange:", len(spans), spans[0].Start, spans[0].Script == a)
	spans[0].Script.text = "changed through the result"
	fmt.Println("write through the result:", a.text)

	files := []*SourceFile{a, {"b.ts", "yz"}}
	masked := FromLSPRangeForSourceFile(files, 0)
	fmt.Println("masked site:", len(masked), masked[1].Start, masked[1].Script == files[1], fmt.Sprintf("%T", masked))

	other := &SourceFile{"c.ts", ""}
	renameFirst(files, other)
	fmt.Println("callee wrote the caller's slice:", files[0] == other)
	third := &SourceFile{"d.ts", ""}
	fmt.Println("witness callee wrote the caller's slice:", renameFirstNamed(files, third), files[0] == third)

	p := Ptr[u8]{3}
	fmt.Println("gen Ptr:", p.kind(), fmt.Sprintf("%T", p))
	arr := ConstArray([]u8{{1}, {2}}, "k")
	arr.vals[0].v = 9
	fmt.Println("gen ConstArray:", arr.kind(), len(arr.vals), fmt.Sprintf("%T", arr))
}
