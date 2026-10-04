package main

import (
	"fmt"
	"sync"
)

func pair() (int, int) { return 1, 2 }

func work(n int) (sum int) {
	defer func() {
		sum *= 2
	}()
	for i := 0; i < n; i++ {
		if i%2 == 0 {
			sum += i
		}
	}
	return sum
}

func main() {
	a, b := pair()
	var wg sync.WaitGroup
	results := make([]int, 3)
	for i := range 3 {
		wg.Add(1)
		go func() {
			defer wg.Done()
			results[i] = work(i + a + b)
		}()
	}
	wg.Wait()
	double := func(x int) int { return x * 2 }
	fmt.Println(
		results,
		double(a),
	)
}
