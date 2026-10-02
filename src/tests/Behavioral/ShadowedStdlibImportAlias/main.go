package main

import (
	stderrors "errors"
	"fmt"

	"go2cs/ShadowedStdlibImportAlias/errors"
)

func main() {
	base := stderrors.New("base")
	wrapped := fmt.Errorf("wrapped: %w", base)
	fmt.Println("is:", errors.Is(wrapped, base))
	fmt.Println("unwrap:", errors.Unwrap(wrapped) == base)
}
