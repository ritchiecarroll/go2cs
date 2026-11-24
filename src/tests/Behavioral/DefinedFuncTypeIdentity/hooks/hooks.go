package hooks

// Handler is a methodless defined func type in another package.
type Handler func(int) int

// Apply takes the named type; Raw takes the unnamed one.
func Apply(h Handler, x int) int { return h(x) }

func Raw(f func(int) int, x int) int { return f(x) }

// Make returns the named type.
func Make(k int) Handler { return func(x int) int { return x + k } }
