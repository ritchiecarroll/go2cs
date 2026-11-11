// PromotedValueEmbedLib is the library half of a cross-package guard for a PLAIN (non-box)
// pointer-receiver method that is EXPORTED and promoted through an UNEXPORTED VALUE embed — the
// testing.T.common.Name() shape (x/net/nettest's timeoutWrapper reads t.Name()). The method returns
// a BUILTIN (string / @string): a PUBLIC C# type whose Go-lowercase name the go2cs-gen scope
// heuristic wrongly read as unexported, so the promoted `Name(this ж<Widget>)` forwarder was emitted
// `internal` and was therefore INVISIBLE across assemblies. Cross-package the converter emits a bare
// `Ꮡw.Name()` (b8764925a dropped the inaccessible `.of(T.Ꮡcommon)` descent for a foreign unexported
// value embed), which then bound a SAME-NAMED foreign extension (Gadget.Name, mirroring flag.Name)
// → CS1929. See docs/ConversionStrategies.md "Promoted box-receiver method through an unexported
// value embed" (Plain-return-type addendum).
package PromotedValueEmbedLib

// common is embedded by VALUE (and is unexported) in Widget. Name is a PLAIN pointer-receiver method:
// it READS a field, it does NOT take the address of one, so the converter emits it as an ordinary
// `Name(this ref common)` extension — NOT a direct-ж (box-receiver) primary like testing's
// Errorf/Helper/Logf. Its return type is a builtin (string), the case the b8764925a box-shim family
// did not reach.
type common struct {
	label string
}

func (c *common) Name() string {
	return c.label
}

// Widget value-embeds the unexported common and promotes its Name() to other packages.
type Widget struct {
	common
}

// New returns a heap-allocated *Widget so the promoted method reads through the same box.
func New(label string) *Widget {
	return &Widget{common: common{label: label}}
}

// Gadget is an UNRELATED exported type whose OWN pointer-receiver Name() produces a same-named
// `Name(this ref Gadget)` extension (public, like flag.FlagSet.Name). Cross-package that foreign
// candidate is what `Ꮡw.Name()` mis-bound to (CS1929) while Widget's promoted forwarder was internal.
type Gadget struct {
	tag string
}

func (g *Gadget) Name() string {
	return g.tag
}

// NewGadget returns a heap-allocated *Gadget.
func NewGadget(tag string) *Gadget {
	return &Gadget{tag: tag}
}

// Meta is an EXPORTED struct embedded by VALUE in the exported Badge, and its exported methods return
// builtins. The promoted forwarders' scope was read off the SPELLING of the return type, so `@string`,
// `bool` and every tuple that ends in one made the forwarder `internal`, and a call from another
// package could not bind it (CS1929). Go's rule is that the method's NAME decides: an exported method
// is exported whatever it returns.
type Meta struct {
	text string
	ok   bool
}

func (m Meta) Text() string { return m.text }

func (m Meta) Ok() bool { return m.ok }

func (m Meta) Both() (string, bool) { return m.text, m.ok }

// Self is a pointer-receiver method that returns a type of THIS package beside a builtin, the
// `(*T, error)` shape. Another package reaches it through the embed's box, so it is the control.
func (m *Meta) Self() (*Meta, string) { return m, m.text }

// Badge promotes Meta's four methods to other packages.
type Badge struct {
	Meta
	kind int
}

// NewBadge returns a Badge by value: the promoted value methods are called on a local of the caller.
func NewBadge(text string) Badge {
	return Badge{Meta: Meta{text: text, ok: true}, kind: 1}
}
