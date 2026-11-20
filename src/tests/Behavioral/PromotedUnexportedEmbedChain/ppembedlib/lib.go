// Package ppembedlib holds the embedding shapes of PromotedUnexportedEmbedChain: pointer-receiver methods of BaseNode,
// promoted to exported types through chains that pass an UNEXPORTED embed.
package ppembedlib

// BaseNode carries the pointer-receiver methods every chain promotes.
type BaseNode struct{ children []string }

func (n *BaseNode) Add(s string) { n.children = append(n.children, s) }
func (n *BaseNode) Count() int   { return len(n.children) }
func (n *BaseNode) Last() string { return n.children[len(n.children)-1] }

// BaseInline is an exported value embed of BaseNode.
type BaseInline struct{ BaseNode }

// Link is goldmark's ast.Link: its FIRST hop is unexported, and the method is three hops down.
type baseLink struct {
	BaseInline
	Dest string
}

type Link struct{ baseLink }

// Image reaches BaseNode through an exported hop, then an unexported one.
type inner struct{ BaseNode }

type Inline struct{ inner }

type Image struct{ Inline }

// Para is the exported single-hop control. Emph reaches BaseNode through an unexported embed two hops down. Tag is the
// unexported SINGLE-hop control (Bump is tagBase's own), the shape the converter already called directly.
type Para struct{ BaseNode }

type emphBase struct{ BaseNode }

type Emph struct{ emphBase }

type tagBase struct{ bumps int }

func (t *tagBase) Bump() int { t.bumps++; return t.bumps }

type Tag struct{ tagBase }

func NewLink(dest string) *Link { l := &Link{}; l.Dest = dest; return l }
