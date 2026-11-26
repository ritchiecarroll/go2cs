// TwinOptIn is release-smoke's twin arm (src/tests/PackageTests/release-smoke.ps1, arm T): a goroutine program that
// imports nothing. A producer sends on a channel (it parks) and main ranges over it (it parks), so a build with
// GoTwinsFor naming this project carries twins of both and a build without it carries none. It needs only go.lib
// and go.gen, so the arm reads the opt-in itself and nothing else.
package main

func producer(c chan int, n int) {
	for i := 0; i < n; i++ {
		c <- i
	}
	close(c)
}

func main() {
	c := make(chan int)
	go producer(c, 3)
	sum := 0
	for v := range c {
		sum += v
	}
	println("sum", sum)
}
