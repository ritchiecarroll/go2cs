// Package mapval declares a named MAP type called Value.
package mapval

type Value map[string]int

func (v Value) Count() int { return len(v) }

func (v Value) Get(k string) int { return v[k] }

func (v *Value) Put(k string, x int) { (*v)[k] = x }
