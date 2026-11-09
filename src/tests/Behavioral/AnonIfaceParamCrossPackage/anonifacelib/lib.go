// Package anonifacedbg is the library half of the AnonIfaceParamCrossPackage guard: exported funcs whose parameter
// is an ANONYMOUS interface, so an importer's call site must name the interface THIS package lifted for it.
package anonifacedbg

import "fmt"

// Kind is a package-local named type that an anonymous interface's method returns.
type Kind int

// Fail is the TypeScript debug.FailBadSyntaxKind shape: a one-method anonymous interface plus a variadic tail.
func Fail(n interface{ KindString() string }, message ...any) string {
	return "fail:" + n.KindString() + fmt.Sprint(message...)
}

// Describe takes a two-method anonymous interface whose methods name a package-local type.
func Describe(d interface {
	Name() string
	Kind() Kind
}) string {
	return fmt.Sprintf("%s/%d", d.Name(), d.Kind())
}
