// Package outer sits in namespace go.NestedNamespaceReach.b. Its Wide embeds deep.Info, an interface of another
// nested namespace.
package outer

import "NestedNamespaceReach/a/deep"

type Wide interface {
	deep.Info
	Close() error
}

type impl struct{}

func (impl) Name() string { return "impl" }
func (impl) Close() error { return nil }
func Make() Wide          { return impl{} }
