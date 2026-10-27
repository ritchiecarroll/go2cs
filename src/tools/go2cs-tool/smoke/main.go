// The go2cs dotnet tool's install smoke (release-binaries.yml) converts this program twice, once with Go
// installed and once with none, and requires the two conversions to be byte-identical. It touches the parts of
// a conversion that run the Go toolchain: package loading (go list) and the stack-append harvest, which
// compiles the package, so a toolchain that cannot compile changes the emission rather than failing.
package main

import (
	"fmt"
	"sort"
	"strings"
)

type shape interface {
	area() float64
}

type rect struct{ w, h float64 }

func (r rect) area() float64 { return r.w * r.h }

func squares(n int) []int {
	var out []int
	for i := 1; i <= n; i++ {
		out = append(out, i*i)
	}
	return out
}

func safeDivide(a, b int) (q int, err error) {
	defer func() {
		if r := recover(); r != nil {
			err = fmt.Errorf("recovered: %v", r)
		}
	}()
	return a / b, nil
}

func main() {
	results := make(chan int)
	go func() {
		defer close(results)
		for _, v := range squares(5) {
			results <- v
		}
	}()

	sum := 0
	for v := range results {
		sum += v
	}

	counts := map[string]int{}
	for _, word := range strings.Fields("go to c sharp and back to go") {
		counts[word]++
	}
	words := make([]string, 0, len(counts))
	for word := range counts {
		words = append(words, word)
	}
	sort.Strings(words)

	var s shape = rect{3, 4}
	_, err := safeDivide(1, 0)

	fmt.Println(sum, words, counts["go"], s.area(), err)
}
