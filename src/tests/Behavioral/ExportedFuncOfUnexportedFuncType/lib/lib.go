package lib

// An exported function whose parameter is an unexported func type: the type becomes a C# delegate, which must be
// public, or the public method that takes it is less accessible than its parameter.

type lessFunc func(a, b int) bool

type sorter struct {
	less []lessFunc
}

// OrderedBy takes a variadic of the unexported func type (sort's example_multi_test shape).
func OrderedBy(less ...lessFunc) *sorter {
	return &sorter{less: less}
}

// Less reports whether a sorts before b, trying each less function in order.
func (s *sorter) Less(a, b int) bool {
	for _, f := range s.less {
		if f(a, b) {
			return true
		}

		if f(b, a) {
			return false
		}
	}

	return false
}

// Apply takes one value of the unexported func type.
func Apply(f lessFunc, a, b int) bool {
	return f(a, b)
}

// Reverse returns a value of the unexported func type.
func Reverse(f lessFunc) lessFunc {
	return func(a, b int) bool { return f(b, a) }
}

// Holder exports a field of the unexported func type.
type Holder struct {
	Less lessFunc
}
