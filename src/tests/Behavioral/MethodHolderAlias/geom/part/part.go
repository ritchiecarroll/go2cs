// Package part is a child of package geom: its namespace carries geom's name.
package part

type Side struct{ Len int }

func (s Side) Double() int { return Double(s.Len) }

func Double(n int) int { return n * 2 }
