// Package keylike declares a generic interface whose type argument is itself an interface, the
// shape of crypto/mlkem's test-side decapsulationKey[E encapsulationKey], and a generic helper that
// takes constructor functions the way mlkem's testRoundTrip does.
package keylike

import "fmt"

type EncapKey interface {
	Bytes() []byte
}

type DecapKey[E EncapKey] interface {
	Bytes() []byte
	EncapsulationKey() E
}

func RoundTrip[E EncapKey, D DecapKey[E]](name string, generate func() (D, error), newEncap func([]byte) (E, error)) {
	dk, err := generate()
	if err != nil {
		fmt.Println(name, "generate:", err)
		return
	}
	ek := dk.EncapsulationKey()
	ek1, err := newEncap(ek.Bytes())
	if err != nil {
		fmt.Println(name, "newEncap:", err)
		return
	}
	fmt.Printf("%s: dk=%v ek=%v ek1=%v\n", name, dk.Bytes(), ek.Bytes(), ek1.Bytes())
}
