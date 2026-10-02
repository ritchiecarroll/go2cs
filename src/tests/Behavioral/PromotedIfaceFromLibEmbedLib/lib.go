package promolib

// Base's value-receiver methods are promoted into any struct that embeds it, in any package.
type Base struct {
	N int
}

func (b Base) Name() string { return "base" }

func (b Base) Count() int { return b.N }

// Namer is satisfied by Base and, through promotion, by a struct that embeds Base.
type Namer interface {
	Name() string
	Count() int
}
