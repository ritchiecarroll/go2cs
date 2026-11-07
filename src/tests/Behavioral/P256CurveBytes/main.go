// P256CurveBytes prints P-256 results that depend on every field multiply and square: ECDH public keys
// and shared secrets, ECDSA public keys from fixed private keys, verification of a fixed signature and
// of tampered copies, and crypto/elliptic point arithmetic. The converted field arithmetic must give
// Go's bytes exactly.
package main

import (
	"crypto/ecdh"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/sha256"
	"encoding/hex"
	"fmt"
	"math/big"
)

// key derives a 32-byte P-256 scalar below the group order from a label: clearing the top bit puts it
// below 2^255, and the order is above that.
func key(label string) []byte {
	d := sha256.Sum256([]byte(label))
	d[0] &= 0x7f
	return d[:]
}

const fixedSig = "3045022039ecb7479d32b4b4bdc7d3848a53a59a60a16ebbca393045a3c3424c9a6a494a022100d7c2d0d24c24092e004677a0426a29fba901d520cf22dd5abf43e04de892fefd"

func main() {
	curve := ecdh.P256()

	for i := 0; i < 4; i++ {
		a, err := curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-256 ECDH a %d", i)))
		if err != nil {
			panic(err)
		}
		b, err := curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-256 ECDH b %d", i)))
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

	priv, err := ecdsa.ParseRawPrivateKey(elliptic.P256(), key("go2cs P-256 behavioral key 0"))
	if err != nil {
		panic(err)
	}
	pub, err := priv.PublicKey.Bytes()
	if err != nil {
		panic(err)
	}
	fmt.Printf("ecdsa pub %x\n", pub)

	h := sha256.Sum256([]byte("go2cs P-256 behavioral message"))
	sig, _ := hex.DecodeString(fixedSig)
	fmt.Println("ecdsa fixed signature verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], sig))

	tampered := append([]byte(nil), sig...)
	tampered[len(tampered)-1] ^= 1
	fmt.Println("ecdsa tampered signature verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], tampered))

	h[0] ^= 1
	fmt.Println("ecdsa other message verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], sig))

	p := elliptic.P256()
	for i, k := range []int64{1, 2, 3, 0xff, 1 << 40} {
		x, y := p.ScalarBaseMult(big.NewInt(k).Bytes())
		x2, y2 := p.ScalarMult(x, y, key(fmt.Sprintf("go2cs P-256 scalar %d", i)))
		fmt.Printf("elliptic %d base %x %x\n", i, x, y)
		fmt.Printf("elliptic %d mult %x %x on curve %v\n", i, x2, y2, p.IsOnCurve(x2, y2))
	}
}
