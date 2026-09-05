package main

import (
	"fmt"
	"unsafe"
)

// A (*[N]T)(ptr)[:n] pointer-cast slice is emitted as a golib slice<T> (not a bare Span<T>), so it
// ranges as (index, element) and supports element-address Ꮡ(s, i) — mirrors runtime's printDebugLog
// (range + &state[i]) and os_windows (range over an unsafe []byte). This is a COMPILE + target guard.
// Here the source `&arr` is already `*[4]int`, so `(*[4]int)(unsafe.Pointer(&arr))` is a no-op IDENTITY
// that the converter collapses to the array box and slices directly (`(~Ꮡarr)[..4]`) — a real
// slice<T> over the array's own storage, no transient unsafe.Pointer=nuint round-trip. (A NON-identity
// pointer-cast slice, whose source is an unsafe.Pointer VALUE, keeps the Span-based emission; the
// stdlib exercises that shape.)
func main() {
	arr := [4]int{10, 20, 30, 40}
	s := (*[4]int)(unsafe.Pointer(&arr))[:4]

	sum := 0
	for i := range s { // index-only range over a pointer-cast slice
		sum += i
	}

	total := 0
	for _, v := range s { // value range
		total += v
	}

	for i := range s { // element-address through &s[i]
		ps := &s[i]
		*ps += 1
	}

	fmt.Println(sum, total, s[0])

	// census F13: a DOUBLE deref of a double-pointer reinterpret - reflect MapOf's
	// `**(**mapType)(unsafe.Pointer(&imap))` - the outer deref must parenthesize the
	// inner `~`-form deref before appending .Value, or the postfix binds into the cast
	// chain and reads the inner unsafe.Pointer slot (CS0029). The target element type
	// DIFFERS from the source (`**[2]int64` over a `**[4]int`) so it stays a genuine
	// reinterpret — an identical type would collapse to the box. Compile-shape only,
	// and since Q44 (2026-09-05) the `never` guard is what makes that true at run time:
	// the source pointee is a POINTER (reference-bearing storage the collector may move),
	// so the reinterpret is unrepresentable in the managed model -- `[4]int` read as
	// `[2]int64` through array wrappers has no alias -- and its dereference is the
	// design's loud fault (a native box over the pointer's order token) where it used to
	// be a pun through a transient slot address that happened to survive the Clone().
	// The emitted shape is still the golden's; the program never executes it.
	ip := &arr
	if never {
		back := **(**[2]int64)(unsafe.Pointer(&ip))
		_ = back
	}

	// census F14: a literal with an unsafe.Pointer result whose arms return different
	// C#-typed expressions defeats lambda return inference (CS8917, reflect deepEqual
	// ptrval); the emitted lambda states its return type explicitly.
	pick := func(u bool) unsafe.Pointer {
		if u {
			return unsafe.Pointer(uintptr(0))
		}
		return unsafe.Pointer(ip)
	}
	_ = pick(true)

	// A NAMED-over-pointer conversion from a raw address (syscall.Pointer shape,
	// internal/poll WSAMsg.Name CS0030 x6): hops uintptr then the named operator -
	// ((opaque)(zh<nint>)(uintptr)(...)). Compile-shape only.
	op := opaque(unsafe.Pointer(ip))
	_ = op
}

// opaque mirrors syscall.Pointer: a named type whose underlying is a pointer.
type opaque *[4]int

// never gates the compile-shape-only F13 line: a package var (not a constant), so the
// branch is emitted and type-checked but never taken.
var never bool
