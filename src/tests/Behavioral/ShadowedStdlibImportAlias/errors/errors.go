// Package errors has the same name as the standard-library package it imports under an alias, the shape of
// github.com/pkg/errors (its go113.go: `import stderrors "errors"` and `func Is(err, target error) bool { return
// stderrors.Is(err, target) }`). The alias must reach the STANDARD LIBRARY's package, never this one.
package errors

import stderrors "errors"

func Is(err, target error) bool { return stderrors.Is(err, target) }

func Unwrap(err error) error { return stderrors.Unwrap(err) }
