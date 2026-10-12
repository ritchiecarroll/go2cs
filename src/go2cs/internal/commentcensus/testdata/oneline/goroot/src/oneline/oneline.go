package oneline

import "context"

// timeoutError reports a timeout.
type timeoutError struct{ err string }

func (e *timeoutError) Error() string     { return e.err }
func (e *timeoutError) Timeout() bool     { return true }
func (e *timeoutError) Is(err error) bool { return err == context.DeadlineExceeded }
func (*timeoutError) marker()             {}

// wrap returns a timeout error.
func wrap(s string) error {
	return &timeoutError{s}
}
