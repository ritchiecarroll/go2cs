package main

import (
	"encoding/binary"
	"fmt"

	"github.com/google/uuid"
)

// encode reproduces NewV6's two writes for a given 100ns timestamp and returns what Time() decodes.
func encode(now uint64) uuid.Time {
	var u uuid.UUID
	binary.BigEndian.PutUint64(u[0:], now)
	u[6] = 0x60 | (u[6] & 0x0F)
	u[8] = 0x80
	return u.Time()
}

func main() {
	base := uint64(0x01f0_0000_0000_0000) // a timestamp in the 2020s range, low 16 bits zero
	for _, pair := range [][2]uint64{{base + 0x5ffe, base + 0x5fff}, {base + 0x5fff, base + 0x6000}, {base + 0x6fff, base + 0x7000}} {
		t1, t2 := encode(pair[0]), encode(pair[1])
		fmt.Printf("now %#x -> %#x : Time %#x -> %#x : reversed=%v\n", pair[0], pair[1], uint64(t1), uint64(t2), t1 > t2)
	}
}
