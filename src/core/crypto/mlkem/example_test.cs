// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using Δmlkem = go.crypto.mlkem_package;
using log = log_package;
using go.crypto;

partial class mlkem_test_package {

public static void Example() {
    // Alice generates a new key pair and sends the encapsulation key to Bob.
    var (dk, err) = Δmlkem.GenerateKey768();
    if (err != default!) {
        log.Fatal(err);
    }
    var encapsulationKey = dk.EncapsulationKey().Bytes();

    // Bob uses the encapsulation key to encapsulate a shared secret, and sends
    // back the ciphertext to Alice.
    var ciphertext = Bob(encapsulationKey);

    // Alice decapsulates the shared secret from the ciphertext.
    (var sharedSecret, err) = dk.Decapsulate(ciphertext);
    if (err != default!) {
        log.Fatal(err);
    }

    // Alice and Bob now share a secret.
    _ = sharedSecret;
}

public static slice<byte> /*ciphertext*/ Bob(slice<byte> encapsulationKey) {
    slice<byte> ciphertext = default!;

    // Bob encapsulates a shared secret using the encapsulation key.
    var (ek, err) = Δmlkem.NewEncapsulationKey768(encapsulationKey);
    if (err != default!) {
        log.Fatal(err);
    }
    (var sharedSecret, ciphertext) = ek.Encapsulate();

    // Alice and Bob now share a secret.
    _ = sharedSecret;

    // Bob sends the ciphertext to Alice.
    return ciphertext;
}

} // end mlkem_test_package
