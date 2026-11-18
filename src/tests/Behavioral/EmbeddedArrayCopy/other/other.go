package other

import "EmbeddedArrayCopy/lib"

// Emb has lib.Emb's fields, so either converts to the other.
type Emb struct {
	lib.Inner
	N int
}
