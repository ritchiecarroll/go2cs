package main

import "fmt"

// Package-level functions that share a name with an intrinsic golib supplies to every converted
// file. Go's own calls never reach them by accident; neither may the converter's.
func tmpstring(b []byte) string { return "" }

func LiteralByteAt(s string) byte { return '?' }

func trySelect() {}

func wrapperRecv() {}

type I interface{ M() int }

type T struct{ n int }

func (t T) M() int { return t.n }

var m = map[string]int{"key": 1}

// lookup indexes a map by a []byte conversion, which converts without copying the bytes.
func lookup(b []byte) int { return m[string(b)] }

// sumIndexes declares a local named len and ranges a slice by index only.
func sumIndexes(s []int) int {
	len := func(x []int) int { return 1000 }
	total := 0

	for i := range s {
		total += i * 100
	}

	return total + len(s)/1000
}

// byteAt indexes a string literal with a variable.
func byteAt(i int) byte { return "golib"[i] }

// poll selects with a default case.
func poll(c chan int) string {
	select {
	case v := <-c:
		return fmt.Sprint("received ", v)
	default:
		return "nothing ready"
	}
}

func main() {
	trySelect()
	wrapperRecv()

	fmt.Println("lookup:", lookup([]byte("key")))
	fmt.Println("sum:", sumIndexes([]int{1, 2, 3, 4, 5, 6, 7, 8}))
	fmt.Printf("byte: %c %c\n", byteAt(2), LiteralByteAt("x"))

	c := make(chan int, 1)
	fmt.Println("poll:", poll(c))
	c <- 5
	fmt.Println("poll:", poll(c))

	method := I.M
	fmt.Println("method:", method(T{n: 7}))
}
