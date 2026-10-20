//go:build windows

package main

import "unsafe"

func unsafePointer(p *uint16) unsafe.Pointer { return unsafe.Pointer(p) }
