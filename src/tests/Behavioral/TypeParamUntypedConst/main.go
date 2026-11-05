package main

import (
	"fmt"
	"os"
	"runtime"
)

// Go gives an untyped constant the type of the type parameter it meets: `return 1` in a func returning T,
// `var x T = 1 << n`, and `1 << n` itself, whose left operand takes the shift's type. C# has no implicit
// conversion from an int literal to a type parameter, so each one needs golib's ConvertToType<T>, and a
// shift of it follows Go's rule for a count at or past the type argument's width. Each arm runs under recover
// and prints its class, so a divergence names it; an argument runs only the arm of that name.

type integer interface {
	~uint8 | ~uint32 | ~int8 | ~int64
}

const five = 5

var (
	three uint = 3
	nine  uint = 9
	forty uint = 40
	neg   int  = -1
)

func arm(class string, f func() string) {
	if len(os.Args) > 1 && os.Args[1] != class {
		return
	}

	defer func() {
		if r := recover(); r != nil {
			_, isRuntimeError := r.(runtime.Error)
			fmt.Println(class+": panic:", r, isRuntimeError)
		}
	}()

	fmt.Println(class+":", f())
}

func one[T integer]() T { return 1 }

func named[T integer]() T { return five }

func constShift[T integer]() T { return 1 << 3 }

func bit[T integer](n uint) T { return 1 << n }

func bitSigned[T integer](n int) T { return 1 << n }

func mask[T integer](n uint) T { return 1<<n - 1 }

func varBit[T integer](n uint) T {
	var x T = 1 << n
	return x
}

func orBit[T integer](x T, n uint) T {
	x |= 1 << n
	return x
}

func assign[T integer]() T {
	var x T
	x = 7
	return x
}

func pair[T integer](a, b T) T { return a + b }

func passConst[T integer](x T) T { return pair(x, 2) }

func sum[T integer]() T {
	var total T

	for _, v := range []T{1, 2, 3} {
		total += v
	}

	return total
}

func isZero[T integer](x T) bool { return x == 0 }

func convert[T integer]() T { return T(4) }

func main() {
	arm("return constant", func() string { return fmt.Sprint(one[uint8](), " ", one[int64]()) })
	arm("named constant", func() string { return fmt.Sprint(named[uint32](), " ", named[int8]()) })
	arm("constant shift", func() string { return fmt.Sprint(constShift[uint8](), " ", constShift[int64]()) })
	arm("shift in range", func() string { return fmt.Sprint(bit[uint8](three), " ", bit[uint32](three), " ", bit[int8](three)) })
	arm("shift past width", func() string { return fmt.Sprint(bit[uint8](nine), " ", bit[uint32](forty), " ", bit[int8](nine)) })
	arm("shift wide type", func() string { return fmt.Sprint(bit[int64](forty), " ", bit[uint32](nine)) })
	arm("negative count", func() string { return fmt.Sprint(bitSigned[uint8](neg)) })
	arm("mask", func() string { return fmt.Sprint(mask[uint8](three), " ", mask[uint8](nine), " ", mask[uint32](forty)) })
	arm("var init", func() string { return fmt.Sprint(varBit[uint8](three), " ", varBit[uint8](nine)) })
	arm("compound or", func() string { return fmt.Sprint(orBit[uint8](1, three), " ", orBit[uint8](1, nine)) })
	arm("assign", func() string { return fmt.Sprint(assign[uint8](), " ", assign[int64]()) })
	arm("call argument", func() string { return fmt.Sprint(passConst[uint8](40), " ", passConst[int8](-3)) })
	arm("composite elements", func() string { return fmt.Sprint(sum[uint8](), " ", sum[int64]()) })
	arm("comparison", func() string { return fmt.Sprint(isZero[uint8](0), " ", isZero[int64](5)) })
	arm("conversion", func() string { return fmt.Sprint(convert[uint32](), " ", convert[int8]()) })
}
