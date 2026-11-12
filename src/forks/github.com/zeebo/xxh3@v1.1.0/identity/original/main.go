// Prints every xxh3 result over a fixed input set, one line each. Run once against the original module and once
// against the fork (go.mod replace); the two outputs must be byte-identical.
package main

import (
	"bufio"
	"fmt"
	"os"

	"github.com/zeebo/xxh3"
)

func main() {
	w := bufio.NewWriter(os.Stdout)
	defer w.Flush()

	// A fixed pseudo-random input (xorshift64), long enough for every length below.
	const max = 1<<20 + 3
	data := make([]byte, max)
	x := uint64(0x9E3779B97F4A7C15)
	for i := range data {
		x ^= x << 13
		x ^= x >> 7
		x ^= x << 17
		data[i] = byte(x)
	}

	var lengths []int
	for l := 0; l <= 2600; l++ { // 0, 1-3, 4-8, 9-16, 17-128, 129-240, >240 and the first block boundaries
		lengths = append(lengths, l)
	}
	lengths = append(lengths, 4095, 4096, 4097, 10000, 65536+7, max)
	seeds := []uint64{0, 1, 0xdeadbeef, 0x9E3779B97F4A7C15, ^uint64(0)}

	for _, l := range lengths {
		b := data[:l]
		s := string(b)
		fmt.Fprintf(w, "L%d Hash %x HashString %x Hash128 %x HashString128 %x\n", l, xxh3.Hash(b), xxh3.HashString(s), xxh3.Hash128(b).Bytes(), xxh3.HashString128(s).Bytes())
		for _, seed := range seeds {
			fmt.Fprintf(w, "L%d S%x HashSeed %x HashStringSeed %x Hash128Seed %x HashString128Seed %x\n", l, seed,
				xxh3.HashSeed(b, seed), xxh3.HashStringSeed(s, seed), xxh3.Hash128Seed(b, seed).Bytes(), xxh3.HashString128Seed(s, seed).Bytes())
		}
	}

	// The streaming Hasher and Hasher128: several chunk patterns, a reused hasher, a zero value, and Sum appends.
	chunks := []int{1, 7, 63, 64, 65, 1000, 1024, 1025, 3000}
	reused := xxh3.New()
	reused128 := xxh3.New128()
	for _, l := range []int{0, 1, 3, 8, 16, 17, 128, 129, 240, 241, 1023, 1024, 1025, 1088, 2048, 4097, 10000, 65543} {
		for _, c := range chunks {
			for _, seed := range seeds {
				h := xxh3.NewSeed(seed)
				h128 := xxh3.NewSeed128(seed)
				for o := 0; o < l; o += c {
					e := o + c
					if e > l {
						e = l
					}
					if (o/c)%2 == 0 {
						h.Write(data[o:e])
						h128.Write(data[o:e])
					} else {
						h.WriteString(string(data[o:e]))
						h128.WriteString(string(data[o:e]))
					}
				}
				fmt.Fprintf(w, "STREAM L%d C%d S%x Sum64 %x Sum128 %x H128 %x Sum %x Sum128b %x\n", l, c, seed,
					h.Sum64(), h.Sum128().Bytes(), h128.Sum128().Bytes(), h.Sum([]byte{0xAA}), h128.Sum(nil))
			}
			reused.Reset()
			reused128.ResetSeed(uint64(l))
			reused.Write(data[:l])
			reused128.Write(data[:l])
			fmt.Fprintf(w, "REUSED L%d C%d %x %x\n", l, c, reused.Sum64(), reused128.Sum128().Bytes())
		}
		var zero xxh3.Hasher
		var zero128 xxh3.Hasher128
		zero.Write(data[:l])
		zero128.Write(data[:l])
		fmt.Fprintf(w, "ZERO L%d %x %x %d %d %d %d\n", l, zero.Sum64(), zero128.Sum128().Bytes(), zero.Size(), zero.BlockSize(), zero128.Size(), zero128.BlockSize())
	}
}
