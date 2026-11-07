// P384CurveBytes prints P-384 results that depend on every field multiply and square: ECDH public keys
// and shared secrets, ECDSA public keys from fixed private keys, verification of a fixed signature and
// of tampered copies, and crypto/elliptic point arithmetic. The converted field arithmetic must give
// Go's bytes exactly.
package main

import (
	"crypto/ecdh"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/sha512"
	"encoding/hex"
	"fmt"
	"math/big"
)

// key derives a 48-byte P-384 scalar below the group order from a label: clearing the top bit puts it
// below 2^383, and the order is above that.
func key(label string) []byte {
	d := sha512.Sum384([]byte(label))
	d[0] &= 0x7f
	return d[:]
}

const fixedSig = "30660231009f9537dcd1b97340cbc064cc9d1f6ef09f142c1d70472b4f9738d58d9d69c35755ab0baca0ae0849b2b845b39bff4c4b023100cceb089f51f1f4b122ca5d7b7a6779459b661539662c1585d6f15598d7e2f361b36622275e1bbe9f88ddbd87ea710e2d"

func main() {
	curve := ecdh.P384()

	for i := 0; i < 4; i++ {
		a, err := curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-384 ECDH a %d", i)))
		if err != nil {
			panic(err)
		}
		b, err := curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-384 ECDH b %d", i)))
		if err != nil {
			panic(err)
		}
		ab, err := a.ECDH(b.PublicKey())
		if err != nil {
			panic(err)
		}
		ba, err := b.ECDH(a.PublicKey())
		if err != nil {
			panic(err)
		}
		fmt.Printf("ecdh %d pub %x\n", i, a.PublicKey().Bytes())
		fmt.Printf("ecdh %d shared %x agree %v\n", i, ab, string(ab) == string(ba))
	}

	priv, err := ecdsa.ParseRawPrivateKey(elliptic.P384(), key("go2cs P-384 behavioral key 0"))
	if err != nil {
		panic(err)
	}
	pub, err := priv.PublicKey.Bytes()
	if err != nil {
		panic(err)
	}
	fmt.Printf("ecdsa pub %x\n", pub)

	h := sha512.Sum384([]byte("go2cs P-384 behavioral message"))
	sig, _ := hex.DecodeString(fixedSig)
	fmt.Println("ecdsa fixed signature verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], sig))

	tampered := append([]byte(nil), sig...)
	tampered[len(tampered)-1] ^= 1
	fmt.Println("ecdsa tampered signature verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], tampered))

	h[0] ^= 1
	fmt.Println("ecdsa other message verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], sig))

	p := elliptic.P384()
	for i, k := range []int64{1, 2, 3, 0x17f, 1 << 40} {
		x, y := p.ScalarBaseMult(big.NewInt(k).Bytes())
		x2, y2 := p.ScalarMult(x, y, key(fmt.Sprintf("go2cs P-384 scalar %d", i)))
		fmt.Printf("elliptic %d base %x %x\n", i, x, y)
		fmt.Printf("elliptic %d mult %x %x on curve %v\n", i, x2, y2, p.IsOnCurve(x2, y2))
	}
}
