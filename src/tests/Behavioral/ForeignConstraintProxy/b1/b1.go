// Package b1 instantiates lib's generic types at *elem.Node and returns them: a value of each crosses
// into main, which instantiates the same types itself.
package b1

import (
	"ForeignConstraintProxy/elem"
	"ForeignConstraintProxy/lib"
)

func Make(k int) lib.Ord2[*elem.Node] { return lib.Ord2[*elem.Node]{V: &elem.Node{K: k}} }

func MakeInline(k int) lib.Ord[*elem.Node] { return lib.Ord[*elem.Node]{V: &elem.Node{K: k}} }
