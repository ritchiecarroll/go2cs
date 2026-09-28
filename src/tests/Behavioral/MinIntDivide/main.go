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
	calls     = 0
)

type cell struct {
	x int64
	n int
}

type holder struct {
	*cell
}

func (h holder) String() string { return fmt.Sprint(h.x) }

// counted returns i and counts the call, so an arm can show a target or dividend is evaluated once.
func counted(i int) int {
	calls++
	return i
}

func key() string {
	calls++
	return "k"
}

func fresh() *cell {
	calls++
	return &cell{x: math.MinInt64}
}

func ints() []int {
	calls++
	return []int{math.MinInt, 7}
}

// div and rem over a type parameter whose type set holds a governed type: an unnamed instantiation
// takes the division itself, where .NET would throw.
func div[T ~int64](a, b T) T { return a / b }

func mod[T ~int8 | ~int16 | ~int32](a, b T) T { return a % b }

func quoAny[T ~int | ~int64 | ~uint8](a, b T) T { return a / b }

// localRem declares a local named rem in the statement whose initializer takes a remainder: C#'s
// local is in scope in its own initializer, so the converted remainder must not bind to it.
func localRem() string {
	if rem := i64 % int64(m1); rem == 0 {
		return fmt.Sprint(rem)
	}

	return "nonzero"
}

// A local constant and a type-switch binding named quo or rem shadow golib's helpers in C# as a
// local variable does.
func localConst(a, b int64) int64 {
	const quo = 3
	return a/b + quo
}

func laterConst(a, b int32) int32 {
	r := a % b
	const rem = 10
	return r + rem
}

func typeSwitch(a any, b int) int {
	switch quo := a.(type) {
	case int:
		return quo / b
	}
	return 0
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
	arm("compound pointer", func() string {
		p := &i64
		v := *p
		p = &v
		*p /= int64(m1)
		return fmt.Sprint(v)
	})
	arm("compound index expression", func() string {
		sl := []int32{0, math.MinInt32, math.MinInt32}
		j := 0
		sl[j+1] /= int32(m1)
		sl[len(sl)-1] %= int32(m1)
		return fmt.Sprint(sl[1], " ", sl[2])
	})
	arm("compound call index", func() string {
		calls = 0
		sl := []int{0, math.MinInt}
		sl[counted(1)] /= m1
		return fmt.Sprint(sl[1], " calls ", calls)
	})
	arm("compound map key", func() string {
		calls = 0
		m := map[string]int64{"k": math.MinInt64}
		m[key()] /= int64(m1)
		m[key()] %= int64(m1)
		return fmt.Sprint(m["k"], " calls ", calls)
	})
	arm("compound call selector", func() string {
		calls = 0
		c := fresh()
		p := &c
		(*p).x %= int64(m1)
		fresh().x /= int64(m1)
		return fmt.Sprint(c.x, " calls ", calls)
	})
	arm("compound promoted", func() string {
		h := holder{&cell{x: math.MinInt64}}
		h.x /= int64(m1)
		return fmt.Sprint(h)
	})
	arm("generic", func() string {
		return fmt.Sprint(div(i64, int64(m1)), " ", div(w, word(m1)), " ", mod(i32, int32(m1)), " ", mod(i8, int8(m1)),
			" ", quoAny(i, m1), " ", quoAny(uint8(200), uint8(3)))
	})
	arm("constant -1 remainder of a nil selector", func() string {
		var c *cell
		r := c.x % -1
		return fmt.Sprint(r)
	})
	arm("constant -1 remainder of a promoted nil selector", func() string {
		var h holder
		return fmt.Sprint(h.n%-1 == 0)
	})
	arm("constant -1 of an effectful dividend", func() string {
		calls = 0
		sl := []int{math.MinInt}
		r := ints()[counted(0)] % -1
		q := ints()[counted(0)] / -1
		return fmt.Sprint(r, " ", q == math.MinInt, " ", sl[0]%-1, " calls ", calls)
	})
	arm("constant -1 remainder out of range", func() string {
		sl := []int{1}
		j := 3
		return fmt.Sprint(sl[j] % -1)
	})
	arm("compound constant -1", func() string {
		sl := []int64{math.MinInt64, 5}
		j := 0
		sl[j+0] /= -1
		sl[j+1] %= -1
		return fmt.Sprint(sl[0], " ", sl[1])
	})
	arm("constant dividend", func() string {
		const big = math.MaxInt64
		var d64 int64 = math.MinInt64
		b := []byte("abc")
		return fmt.Sprint(5000/int64(m1), " ", len(b)%m1, " ", cap(b)/m1, " ", big/int64(m1), " ", math.MinInt64/int64(m1), " ",
			d64/int64(m1))
	})
	arm("local named rem", localRem)
	arm("local constant or type switch named quo", func() string {
		return fmt.Sprint(localConst(i64, int64(m1)), " ", laterConst(i32, int32(m1)), " ", typeSwitch(i, m1))
	})
	arm("divide by zero", func() string { return fmt.Sprint(i64 / int64(zero)) })
	arm("remainder by zero", func() string { return fmt.Sprint(i % zero) })
}
