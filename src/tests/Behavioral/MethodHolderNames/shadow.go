package main

import "fmt"

type meter struct{ main int }

// reading holds a local named like the package: inside it the package's own name means the local, so the
// package's declarations are reached another way.
func (m meter) Limit() int {
	main := m.main + 1
	return main + Limit
}

// Scale reads the package's function beside a parameter that carries the package's name.
func (m meter) Scale(main int) int { return Scale(main) + m.main }

// Count reads the package's variable; nothing here is named like the package.
func (m meter) Count() int { return Count + m.main }

func shadows() {
	m := meter{main: 2}
	fmt.Println("shadows", m.Limit(), m.Scale(3), m.Count())
}
