// Package lib uses one anonymous interface type and one anonymous struct type at several package-level
// sites. Go calls each one type, so the C# conversion must declare one lift for each and use it at every site.
package lib

// Option and Other are the interfaces the anonymous interface embeds.
type Option interface{ A() int }

// Other is the second embedded interface.
type Other interface{ B() int }

type both struct{ n int }

func (b *both) A() int { return b.n }
func (b *both) B() int { return b.n * 2 }

// With returns the anonymous interface; its lift is the first one in the package.
func With(n int) interface {
	Option
	Other
} {
	return &both{n}
}

// Holder's exported field has the same anonymous interface type, so it reuses With's lift.
type Holder struct {
	V interface {
		Option
		Other
	}
}

// Mk stores With's result in the field: the same Go type on both sides.
func Mk(n int) Holder { return Holder{V: With(n)} }

// Pair returns an anonymous struct; its lift is the first one for that type.
func Pair(n int) struct{ X, Y int } { return struct{ X, Y int }{n, n + 1} }

// Box's exported field has the same anonymous struct type.
type Box struct{ P struct{ X, Y int } }

// MkBox stores Pair's result in the field.
func MkBox(n int) Box { return Box{P: Pair(n)} }
