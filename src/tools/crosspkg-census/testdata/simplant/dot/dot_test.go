package dot_test

import (
	"bytes"
	. "io"
	"testing"
)

// dot-import clash: bytes.Buffer's WriteString forwarder would shadow io.WriteString (io_test's shape).
type Buf struct{ bytes.Buffer }

func TestX(t *testing.T) { var b Buf; WriteString(&b, "x") }
