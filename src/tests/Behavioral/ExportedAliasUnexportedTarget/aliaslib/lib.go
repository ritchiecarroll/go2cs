// Package aliaslib exports aliases of its own unexported types, as logrus (`type MutexWrap =
// mutexWrap`) and testify (`type CompareType = compareResult`) do.
package aliaslib

import "sync"

type mutexWrap struct {
	lock     sync.Mutex
	disabled bool
}

func (mw *mutexWrap) Lock() {
	if !mw.disabled {
		mw.lock.Lock()
	}
}

func (mw *mutexWrap) Unlock() {
	if !mw.disabled {
		mw.lock.Unlock()
	}
}

func (mw *mutexWrap) Disable() {
	mw.disabled = true
}

// MutexWrap is USED by the importer.
type MutexWrap = mutexWrap

type compareResult int

const (
	compareLess compareResult = iota - 1
	compareEqual
	compareGreater
)

// CompareType is NOT used by the importer; its `global using` is still declared there.
type CompareType = compareResult

// Compare reports how a orders against b.
func Compare(a, b int) int {
	switch {
	case a < b:
		return int(compareLess)
	case a > b:
		return int(compareGreater)
	}

	return int(compareEqual)
}
