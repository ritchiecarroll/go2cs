// MnBlockedIO is the M:N spike's blocked-I/O fixture (phase 2, B3). K goroutines sit in a read that cannot
// finish until main writes: first on OS pipes (the read blocks its thread inside a native call), then on
// io.Pipe (the read parks in the io package). While they are blocked, channel work must still run to its end;
// only then does main feed the readers. Sizes come from the environment so one build serves every arm.
package main

import (
	"fmt"
	"io"
	"os"
	"strconv"
)

func envInt(name string, def int) int {
	if v, err := strconv.Atoi(os.Getenv(name)); err == nil && v > 0 {
		return v
	}

	return def
}

func osReader(r *os.File, rounds int, out chan<- int) {
	buf := make([]byte, 1)
	sum := 0

	for i := 0; i < rounds; i++ {
		n, err := r.Read(buf)

		if err != nil || n != 1 {
			out <- -1
			return
		}

		sum += int(buf[0])
	}

	out <- sum
}

func ioReader(r *io.PipeReader, rounds int, out chan<- int) {
	buf := make([]byte, 1)
	sum := 0

	for i := 0; i < rounds; i++ {
		n, err := r.Read(buf)

		if err != nil || n != 1 {
			out <- -1
			return
		}

		sum += int(buf[0])
	}

	out <- sum
}

func ponger(in <-chan int, out chan<- int) {
	for v := range in {
		out <- v + 1
	}

	close(out)
}

// channelWork runs pairs of goroutines that hand a counter back and forth, and returns the grand total.
func channelWork(pairs, rounds int) int {
	results := make(chan int, pairs)

	for p := 0; p < pairs; p++ {
		go func() {
			ping := make(chan int)
			pong := make(chan int)

			go ponger(ping, pong)

			v := 0

			for i := 0; i < rounds; i++ {
				ping <- v
				v = <-pong
			}

			close(ping)
			<-pong
			results <- v
		}()
	}

	total := 0

	for p := 0; p < pairs; p++ {
		total += <-results
	}

	return total
}

func collect(out <-chan int, k int) (sum int, bad int) {
	for i := 0; i < k; i++ {
		v := <-out

		if v < 0 {
			bad++
		} else {
			sum += v
		}
	}

	return
}

func main() {
	kind := os.Getenv("MN_IO_KIND")
	k := envInt("MN_IO_K", 8)
	rounds := envInt("MN_IO_ROUNDS", 3)
	pairs := envInt("MN_IO_PAIRS", 50)

	if kind == "" || kind == "os" {
		out := make(chan int, k)
		writers := make([]*os.File, k)

		for i := 0; i < k; i++ {
			r, w, err := os.Pipe()

			if err != nil {
				fmt.Println("os.Pipe failed at", i)
				return
			}

			writers[i] = w

			go osReader(r, rounds, out)
		}

		fmt.Println("os readers started:", k)
		fmt.Println("channel work beside os readers:", channelWork(pairs, 100))

		for round := 0; round < rounds; round++ {
			for i := 0; i < k; i++ {
				writers[i].Write([]byte{byte(round + 1)})
			}
		}

		sum, bad := collect(out, k)

		fmt.Println("os readers done: sum", sum, "bad", bad)
	}

	if kind == "" || kind == "io" {
		out := make(chan int, k)
		writers := make([]*io.PipeWriter, k)

		for i := 0; i < k; i++ {
			r, w := io.Pipe()

			writers[i] = w

			go ioReader(r, rounds, out)
		}

		fmt.Println("io readers started:", k)
		fmt.Println("channel work beside io readers:", channelWork(pairs, 100))

		for round := 0; round < rounds; round++ {
			for i := 0; i < k; i++ {
				writers[i].Write([]byte{byte(round + 1)})
			}
		}

		sum, bad := collect(out, k)

		fmt.Println("io readers done: sum", sum, "bad", bad)
	}
}
