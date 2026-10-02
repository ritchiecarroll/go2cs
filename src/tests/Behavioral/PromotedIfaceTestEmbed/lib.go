package PromotedIfaceTestEmbed

type Base struct {
	N int
}

func (b Base) Name() string { return "base" }

func (b Base) Count() int { return b.N }

type Namer interface {
	Name() string
	Count() int
}

func Show(n Namer) string { return n.Name() }
