// Package anonifaceembed is the library half of the AnonIfaceEmbedCrossPackage guard: exported funcs whose result or
// parameter is an ANONYMOUS interface that embeds named interfaces, so an importer must name the lift THIS package
// published for it.
package anonifaceembed

// Option is a named interface the anonymous interfaces below embed.
type Option interface{ Apply() int }

// Other is a second named interface, so an anonymous interface can embed two.
type Other interface{ Name() string }

type both struct{ n int }

func (b *both) Apply() int   { return b.n }
func (b *both) Name() string { return "both" }
func (b *both) Extra() int   { return b.n * 10 }

// With returns an interface made ONLY of embedded named interfaces, the goldmark html.WithWriter shape.
func With(n int) interface {
	Option
	Other
} {
	return &both{n}
}

// WithMixed returns an interface that embeds a named interface beside an explicit method.
func WithMixed(n int) interface {
	Option
	Extra() int
} {
	return &both{n}
}

// Take accepts the embedded-only interface as a parameter.
func Take(v interface {
	Option
	Other
}) string {
	return v.Name()
}

type runner struct{ n int }

func (r runner) run() int { return r.n }

// Runner returns an interface whose only method is unexported. An importer can spell its own interface the same way,
// with its own run, which is a different Go type: the importer must not take this lift for it.
func Runner(n int) interface{ run() int } { return runner{n} }
