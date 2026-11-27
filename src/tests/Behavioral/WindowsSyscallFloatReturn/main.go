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
}
