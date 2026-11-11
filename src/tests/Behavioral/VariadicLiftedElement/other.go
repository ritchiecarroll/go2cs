package main

// A second file whose variadic element is the same anonymous interface: it reuses main.go's lift.
func leastEnd(end int, nodes ...interface{ End() int }) int {
	for _, n := range nodes {
		if n.End() < end {
			end = n.End()
		}
	}

	return end
}
