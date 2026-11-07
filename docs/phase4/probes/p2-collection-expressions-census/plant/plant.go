package plant

import "fmt"

type Names []string

func take(s []int)               {}
func gen[T any](s []T)           {}
func iface(v any)                {}
func ret() []int                 { return []int{1} }               // targeted:return
func ranges(s []int, a [3]int, p *[3]int, m map[int]int, str string) {
	for i, v := range s { _, _ = i, v } // range:slice:pair, key-named
	for _, v := range s { _ = v }       // range:slice:pair, key-blank
	for i := range s { _ = i }          // range:slice:index-only
	for range s {}                      // range:slice:none
	for i, v := range a { _, _ = i, v } // range:array:pair
	for i, v := range p { _, _ = i, v } // range:ptr-array:pair
	for k, v := range m { _, _ = k, v } // range:map:pair
	for i, r := range str { _, _ = i, r } // range:string:pair
	var i, v int
	for i, v = range s {} // assigns-existing-vars
	_, _ = i, v
}
func lits() {
	x := []int{1, 2}             // untargeted:define
	var y []int = []int{3}       // targeted:typed-var
	var z = []int{4}             // untargeted:untyped-var
	x = []int{5}                 // targeted:assign
	take([]int{6})               // targeted:arg-slice
	gen([]int{7})                // targeted:arg-generic-slice
	iface([]int{8})              // untargeted:arg-interface
	fmt.Println([]int{9})        // untargeted:arg-interface (variadic any)
	_ = [][]int{{1}, []int{2}}   // define? no: assign to _ => targeted:assign; inner {1} element, []int{2} element
	_ = []int{10}[0]             // untargeted:operand
	for range []int{11} {}       // untargeted:range-operand
	x = append([]int{12}, 13)    // untargeted:builtin-append
	_ = len([]int{14})           // untargeted:builtin-len
	_ = Names{"a"}               // named: not counted as unnamed
	_ = []int{0: 1}              // keyed: not counted
	_ = [2]int{1, 2}             // array
	_, _, _ = x, y, z
}

var pkgLevel = []int{15} // targeted:package-var-field
