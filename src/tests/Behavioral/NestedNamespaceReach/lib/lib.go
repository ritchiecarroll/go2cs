// Package lib sits in namespace go.NestedNamespaceReach. Its unaliased import of NestedNamespaceReach/x/sync puts
// that package's class, sync_package, in view beside the standard library's, so the embedded stdsync.Mutex must
// name the standard one in full.
package lib

import (
	stdsync "sync"

	"NestedNamespaceReach/x/sync"
)

type Counter struct {
	stdsync.Mutex
	n int
}

func (c *Counter) Inc() int {
	c.Lock()
	defer c.Unlock()
	c.n++
	return c.n
}

func Tag() string { return sync.Name() }
