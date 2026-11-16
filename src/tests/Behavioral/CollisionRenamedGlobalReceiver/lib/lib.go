package lib

import "sync/atomic"

// An EXPORTED package var collision-renamed for a same-named method of its package (P3's review of W13, arm a). An
// importer reaches it as `lib.Tid` and, through a dot import, as the bare `Tid`.
var Tid atomic.Int32

type T struct{ ID int32 }

func (t *T) Tid() int32 { return t.ID }

// Next uses the var from its own package, through the same implicit pointer receiver.
func Next() int32 { return Tid.Add(1) }
