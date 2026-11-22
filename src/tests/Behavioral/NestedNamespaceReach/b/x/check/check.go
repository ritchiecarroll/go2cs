// Package check sits in namespace go.NestedNamespaceReach.b.x. outer is imported aliased, so no using brings its
// overloads in; deep is imported unaliased, so its using puts Info's overloads first. A failed assertion from
// outer.Wide must still name Wide as the source interface.
package check

import (
	"fmt"

	"NestedNamespaceReach/a/deep"
	o "NestedNamespaceReach/b/outer"
)

// other implements outer.Wide too, so asserting a Wide to it is possible and fails at run time.
type other struct{}

func (other) Name() string { return "other" }
func (other) Close() error { return nil }

func Check(w o.Wide) (msg string) {
	defer func() { msg = fmt.Sprint(recover()) }()
	_ = w.(other)
	return "no panic"
}

func Run() string { return deep.Label() + ": " + Check(o.Make()) }
