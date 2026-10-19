package main

import (
	"GenericInterfaceCrossPackage/keylike"
)

type encKey struct{ b []byte }

func (e *encKey) Bytes() []byte { return e.b }

type decKey struct {
	b  []byte
	ek *encKey
}

func (d *decKey) Bytes() []byte { return d.b }

func (d *decKey) EncapsulationKey() *encKey { return d.ek }

func generateKey() (*decKey, error) {
	return &decKey{b: []byte{1, 2}, ek: &encKey{b: []byte{3, 4}}}, nil
}

func newEncapKey(b []byte) (*encKey, error) {
	return &encKey{b: append([]byte(nil), b...)}, nil
}

func main() {
	keylike.RoundTrip("round trip", generateKey, newEncapKey)
}
