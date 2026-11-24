// Package ualias declares an alias of unsafe.Pointer for another package to spell by its qualified name.
package ualias

import "unsafe"

type Ptr = unsafe.Pointer
