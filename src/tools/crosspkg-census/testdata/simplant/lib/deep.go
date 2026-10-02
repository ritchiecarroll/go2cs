package lib

import "bufio"

// C4 plant: RW embeds bufio.ReadWriter (another package), so a type embedding RW reaches ReadString THROUGH RW's
// own promotion -- and ReadString returns a tuple.
type RW struct{ bufio.ReadWriter }

type hidden struct{}

// C3 plant: an exported method whose signature names an unexported type of this package.
func (L) Leak() hidden { return hidden{} }
