package main

import (
	"fmt"
	"math"
	"os"
)

// Go's spec wraps the one overflowing signed quotient, the most negative value divided by -1, to that
// same value, and makes any value modulo -1 zero, with no panic. .NET throws OverflowException for
// both, which recover() cannot see. Each arm runs under recover and prints its class, so a divergence
// names it. MinIntDivideShadow holds a package that declares its own quo. Division by zero must still be a recoverable Go panic. An argument runs only the arm of
// that name, so each class can be read on its own where a failure kills the process.

type word int64

type small int32

var (
	i   int   = math.MinInt
	i32 int32 = math.MinInt32
	i64 int64 = math.MinInt64
	r   rune  = math.MinInt32
	w   word  = math.MinInt64
	s   small = math.MinInt32
	i8  int8  = math.MinInt8
	i16 int16 = math.MinInt16
	m1        = -1
	zero      = 0
)

// localRem declares a local named rem in the statement whose initializer takes a remainder: C#'s
// local is in scope in its own initializer, so the converted remainder must not bind to it.
func localRem() string {
	if rem := i64 % int64(m1); rem == 0 {
		return fmt.Sprint(rem)
	}

	return "nonzero"
}

func arm(class string, f func() string) {
	if len(os.Args) > 1 && os.Args[1] != class {
		return
	}

	defer func() {
		if r := recover(); r != nil {
			fmt.Println(class+": panic:", r)
		}
	}()

	fmt.Println(class+":", f())
}

func main() {
	arm("int", func() string { return fmt.Sprint(i/m1 == math.MinInt, " ", i%m1) })
	arm("int32", func() string { return fmt.Sprint(i32/int32(m1), " ", i32%int32(m1)) })
	arm("int64", func() string { return fmt.Sprint(i64/int64(m1), " ", i64%int64(m1)) })
	arm("rune", func() string { return fmt.Sprint(r/rune(m1), " ", r%rune(m1)) })
	arm("named int64", func() string { return fmt.Sprint(w/word(m1), " ", w%word(m1)) })
	arm("named int32", func() string { return fmt.Sprint(s/small(m1), " ", s%small(m1)) })
	arm("narrow", func() string { return fmt.Sprint(i8/int8(m1), " ", i8%int8(m1), " ", i16/int16(m1), " ", i16%int16(m1)) })
	arm("compound", func() string {
		q, m := i64, i64
		q /= int64(m1)
		m %= int64(m1)
		return fmt.Sprint(q, " ", m)
	})
	arm("constant -1", func() string {
		q, m := i32, i32
		q /= -1
		m %= -1
		return fmt.Sprint(i/-1 == math.MinInt, " ", i%-1, " ", i64/-1, " ", i64%-1, " ", q, " ", m)
	})
	arm("ordinary", func() string { return fmt.Sprint(i64/int64(7), " ", i64%int64(7), " ", -7/m1, " ", 7%(m1-2)) })
	arm("local named rem", localRem)
	arm("divide by zero", func() string { return fmt.Sprint(i64 / int64(zero)) })
	arm("remainder by zero", func() string { return fmt.Sprint(i % zero) })
}
