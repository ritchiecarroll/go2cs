// Package deep sits in namespace go.NestedNamespaceReach.a.
package deep

type Info interface{ Name() string }

func Label() string { return "deep" }
