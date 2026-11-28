// WindowsSyscallFloatReturn guards the floating-point half of syscall's windows/amd64 call contract.
// Go's trampoline (internal/runtime/syscall/windows/asm_windows_amd64.s, asmstdcall) loads the first
// four argument words into BOTH the integer registers (RCX, RDX, R8, R9) and XMM0-XMM3, since the
// Windows x64 convention passes a floating-point argument in the XMM register of its position, and it
// returns XMM0, where a native function leaves a double or float result, as r2. So a Go program calls
// a C function that takes or returns a double through Proc.Call or SyscallN by passing
// math.Float64bits words and reading math.Float64frombits(r2). modernc.org/libc's windows strtod does
// exactly that.
//
// Each line prints the value it read, not a boolean, so the golden holds what go run prints.
//   1. strtod("2.5") returns a double: r2 carries XMM0.
//   2. sqrt(2.25): a double argument goes in XMM0, and the result comes back in XMM0.
//   3. pow(2, 10): two double arguments, XMM0 and XMM1.
//   4. ldexp(1.5, 3): a double in XMM0 and an int in RDX, the argument's position deciding each register.
//   5. labs(-7): an integer result still comes back in r1, unchanged.
//   6. _snprintf with six arguments, doubles at positions 4, 5 and 6: the words past the fourth go on the
//      stack, and a variadic callee reads a double from its integer register or its stack slot.
//   7. _snprintf with 19 arguments, the first count a C# call with one parameter per word could not pass.
//   8. _snprintf with 42 arguments, Go's MaxArgs (internal/runtime/syscall/windows): every word reaches the callee.
//   9. 43 arguments: Go panics before the call ("runtime: SyscallN has too many arguments"), and a caller's
//      recover() sees Go's text.
//  10. GetFileAttributesW on a missing file: the call's last-error comes back as err (ERROR_FILE_NOT_FOUND, 2).
//
// windows/arm64 is not this contract: Go's arm64 trampoline passes integer registers only and never
// writes r2, so a double argument arrives as whatever its register held there. The row reads amd64 only.

//go:build windows

package main

import (
	"fmt"
	"math"
	"runtime"
	"syscall"
	"unsafe"
)

func main() {
	if runtime.GOARCH != "amd64" {
		fmt.Println("amd64 only:", runtime.GOARCH, "passes no floating-point register")
		return
	}

	msvcrt := syscall.NewLazyDLL("msvcrt.dll")

	s := []byte("2.5\x00")
	_, r2, _ := msvcrt.NewProc("strtod").Call(uintptr(unsafe.Pointer(&s[0])), 0)
	fmt.Println("1 strtod(\"2.5\") =", math.Float64frombits(uint64(r2)))

	_, r2, _ = msvcrt.NewProc("sqrt").Call(uintptr(math.Float64bits(2.25)))
	fmt.Println("2 sqrt(2.25) =", math.Float64frombits(uint64(r2)))

	_, r2, _ = msvcrt.NewProc("pow").Call(uintptr(math.Float64bits(2)), uintptr(math.Float64bits(10)))
	fmt.Println("3 pow(2, 10) =", math.Float64frombits(uint64(r2)))

	_, r2, _ = msvcrt.NewProc("ldexp").Call(uintptr(math.Float64bits(1.5)), 3)
	fmt.Println("4 ldexp(1.5, 3) =", math.Float64frombits(uint64(r2)))

	minus7 := int32(-7)
	r1, _, _ := msvcrt.NewProc("labs").Call(uintptr(minus7))
	fmt.Println("5 labs(-7) =", int32(r1))

	buf := make([]byte, 64)
	format := []byte("%.2f %.2f %.2f\x00")
	r1, _, _ = msvcrt.NewProc("_snprintf").Call(uintptr(unsafe.Pointer(&buf[0])), uintptr(len(buf)),
		uintptr(unsafe.Pointer(&format[0])), uintptr(math.Float64bits(1.25)), uintptr(math.Float64bits(2.5)),
		uintptr(math.Float64bits(3.75)))
	fmt.Println("6 _snprintf, three doubles (1.25, 2.5, 3.75) =", string(buf[:int32(r1)]))

	fmt.Println("7 _snprintf, 19 arguments =", snprintfInts(msvcrt, 16))
	fmt.Println("8 _snprintf, 42 arguments =", snprintfInts(msvcrt, 39))
	fmt.Println("9 43 arguments:", tooMany(msvcrt))

	name := []uint16{'n', 'o', '-', 's', 'u', 'c', 'h', '.', 'f', 'i', 'l', 'e', 0}
	_, _, err := syscall.NewLazyDLL("kernel32.dll").NewProc("GetFileAttributesW").Call(uintptr(unsafe.Pointer(&name[0])))
	fmt.Println("10 GetFileAttributesW(\"no-such.file\") err =", uintptr(err.(syscall.Errno)))
}

// snprintfInts formats the integers 1 to n with _snprintf, three words before them, and returns what it wrote.
func snprintfInts(msvcrt *syscall.LazyDLL, n int) string {
	buf := make([]byte, 256)
	format := []byte{}

	for i := 0; i < n; i++ {
		format = append(format, " %d"...)
	}

	format = append(format, 0)
	args := []uintptr{uintptr(unsafe.Pointer(&buf[0])), uintptr(len(buf)), uintptr(unsafe.Pointer(&format[0]))}

	for i := 1; i <= n; i++ {
		args = append(args, uintptr(i))
	}

	r1, _, _ := msvcrt.NewProc("_snprintf").Call(args...)
	return string(buf[1:int32(r1)])
}

// tooMany calls with 43 words and returns what recover() sees.
func tooMany(msvcrt *syscall.LazyDLL) (text any) {
	defer func() { text = recover() }()

	msvcrt.NewProc("labs").Call(make([]uintptr, 43)...)
	return "returned"
}
