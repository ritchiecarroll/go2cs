package main

import (
	"fmt"
	"os"
	"runtime"
)

// Go panics with the runtime.Error "negative shift amount" when a shift's count is a signed value
// below zero at run time. Each arm runs under recover and prints its class, so a divergence names
// it; an argument runs only the arm of that name.

type count int8

var (
	one   int    = 1
	big   int64  = 1 << 40
	u32   uint32 = 0xF0
	i8    int8   = -128
	neg   int    = -1
	neg8  int8   = -2
	neg64 int64  = -3
	three int    = 3
	wide  int    = 70
	y     int    = -3
	c     count  = -1
	huge  uint   = 1 << 63
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

func main() {
	arm("left int", func() string { return fmt.Sprint(one << neg) })
	arm("right int64 by int8", func() string { return fmt.Sprint(big >> neg8) })
	arm("left uint32 by int64", func() string { return fmt.Sprint(u32 << neg64) })
	arm("right int8", func() string { return fmt.Sprint(i8 >> neg) })
	arm("named count", func() string { return fmt.Sprint(big << c) })
	arm("compound left", func() string {
		x := big
		x <<= neg
		return fmt.Sprint(x)
	})
	arm("compound right", func() string {
		x := u32
		x >>= neg8
		return fmt.Sprint(x)
	})
	arm("modulo count", func() string { return fmt.Sprint(big << (y % 8)) })
	arm("non-negative signed", func() string { return fmt.Sprint(one<<three, " ", big>>three, " ", u32<<wide, " ", i8>>wide) })
	arm("unsigned huge", func() string { return fmt.Sprint(big<<huge, " ", i8>>huge) })
	arm("masked signed", func() string { return fmt.Sprint(big << (y & 7)) })
}
