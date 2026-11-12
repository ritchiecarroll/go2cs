package xxh3

import (
	"encoding/binary"
	"math/bits"
)

// Uint128 is a 128 bit value.
// The actual value can be thought of as u.Hi<<64 | u.Lo.
type Uint128 struct {
	Hi, Lo uint64
}

// Bytes returns the uint128 as an array of bytes in canonical form (big-endian encoded).
func (u Uint128) Bytes() [16]byte {
	return [16]byte{
		byte(u.Hi >> 0x38), byte(u.Hi >> 0x30), byte(u.Hi >> 0x28), byte(u.Hi >> 0x20),
		byte(u.Hi >> 0x18), byte(u.Hi >> 0x10), byte(u.Hi >> 0x08), byte(u.Hi),
		byte(u.Lo >> 0x38), byte(u.Lo >> 0x30), byte(u.Lo >> 0x28), byte(u.Lo >> 0x20),
		byte(u.Lo >> 0x18), byte(u.Lo >> 0x10), byte(u.Lo >> 0x08), byte(u.Lo),
	}
}

type (
	ui = uintptr

	u8   = uint8
	u32  = uint32
	u64  = uint64
	u128 = Uint128
)

// go2cs fork: ptr stands in for the original's unsafe.Pointer into a hash input or a secret. It is a byte
// slice and an offset into it, so it steps forward and back as the original's pointer arithmetic does, and
// every read is a bounds-checked read of the same little-endian bytes. def marks the package's default key,
// which the original recognized by pointer identity (secret != key).
type ptr struct {
	b   []byte
	o   ui
	def bool
}

func (p ptr) add(n ui) ptr { return ptr{p.b, p.o + n, p.def} }
func (p ptr) sub(n ui) ptr { return ptr{p.b, p.o - n, p.def} }

type str struct {
	p ptr
	l uint
}

func bytesOf(b []byte) str  { return str{p: ptr{b: b}, l: uint(len(b))} }
func stringOf(s string) str { return str{p: ptr{b: []byte(s)}, l: uint(len(s))} }

func readU8(p ptr, o ui) uint8 {
	return p.b[p.o+o]
}

func readU16(p ptr, o ui) uint16 {
	return binary.LittleEndian.Uint16(p.b[p.o+o:])
}

func readU32(p ptr, o ui) uint32 {
	return binary.LittleEndian.Uint32(p.b[p.o+o:])
}

func readU64(p ptr, o ui) uint64 {
	return binary.LittleEndian.Uint64(p.b[p.o+o:])
}

func writeU64(p ptr, o ui, v u64) {
	binary.LittleEndian.PutUint64(p.b[p.o+o:], v)
}

const secretSize = 192

func initSecret(secret ptr, seed u64) {
	for i := ui(0); i < secretSize/16; i++ {
		lo := readU64(key, 16*i) + seed
		hi := readU64(key, 16*i+8) - seed
		writeU64(secret, 16*i, lo)
		writeU64(secret, 16*i+8, hi)
	}
}

func xxh64AvalancheSmall(x u64) u64 {
	// x ^= x >> 33                    // x must be < 32 bits
	// x ^= u64(key32_000 ^ key32_004) // caller must do this
	x *= prime64_2
	x ^= x >> 29
	x *= prime64_3
	x ^= x >> 32
	return x
}

func xxhAvalancheSmall(x u64) u64 {
	x ^= x >> 33
	x *= prime64_2
	x ^= x >> 29
	x *= prime64_3
	x ^= x >> 32
	return x
}

func xxh64AvalancheFull(x u64) u64 {
	x ^= x >> 33
	x *= prime64_2
	x ^= x >> 29
	x *= prime64_3
	x ^= x >> 32
	return x
}

func xxh3Avalanche(x u64) u64 {
	x ^= x >> 37
	x *= 0x165667919e3779f9
	x ^= x >> 32
	return x
}

func rrmxmx(h64 u64, len u64) u64 {
	h64 ^= bits.RotateLeft64(h64, 49) ^ bits.RotateLeft64(h64, 24)
	h64 *= 0x9fb21c651e98df25
	h64 ^= (h64 >> 35) + len
	h64 *= 0x9fb21c651e98df25
	h64 ^= (h64 >> 28)
	return h64
}

func mulFold64(x, y u64) u64 {
	hi, lo := bits.Mul64(x, y)
	return hi ^ lo
}
