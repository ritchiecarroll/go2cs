// The sample program for golang.org/x/sync v0.19.0: errgroup, semaphore and singleflight. Goroutines write results
// into fixed slots and the program prints after Wait, so its output does not depend on scheduling.
package main

import (
	"context"
	"errors"
	"fmt"

	"golang.org/x/sync/errgroup"
	"golang.org/x/sync/semaphore"
	"golang.org/x/sync/singleflight"
)

func main() {
	var g errgroup.Group
	squares := make([]int, 5)
	for i := range squares {
		g.Go(func() error { squares[i] = i * i; return nil })
	}
	fmt.Println("errgroup:", g.Wait(), squares)

	g2, ctx := errgroup.WithContext(context.Background())
	g2.Go(func() error { return errors.New("first failure") })
	g2.Go(func() error { <-ctx.Done(); return ctx.Err() })
	fmt.Println("errgroup with context:", g2.Wait(), ctx.Err())

	sem := semaphore.NewWeighted(2)
	fmt.Println("semaphore:", sem.TryAcquire(2), sem.TryAcquire(1))
	sem.Release(2)
	fmt.Println("after release:", sem.TryAcquire(1))

	var sf singleflight.Group
	v, err, shared := sf.Do("key", func() (any, error) { return "computed once", nil })
	fmt.Println("singleflight:", v, err, shared)
}
