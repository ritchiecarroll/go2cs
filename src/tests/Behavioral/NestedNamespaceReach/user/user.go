// Package user sits in namespace go.NestedNamespaceReach, where its import of child/x puts the namespace
// go.NestedNamespaceReach.child in view. It names child.T only through an inferred type and never imports child,
// so the package alias go2cs adds for child must be renamed away from the namespace.
package user

import "NestedNamespaceReach/child/x"

func use[T any](v T) T { return v }

var made = use(x.Make())

func Value() int { return made.V + 1 }
