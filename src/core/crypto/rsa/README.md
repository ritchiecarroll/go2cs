# go.crypto.rsa

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-568%2F569_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.24.13.4/crypto.rsa.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/crypto/rsa@go1.24.13)\
[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/crypto/rsa) [![Source](https://img.shields.io/badge/Source-@1.24.13.4-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.4/src/core/crypto/rsa)

Package rsa implements RSA encryption as specified in PKCS #1 and RFC 8017.

RSA is a single, fundamental operation that is used in this package to implement either public-key encryption or public-key signatures.

The original specification for encryption and signatures with RSA is PKCS #1 and the terms "RSA encryption" and "RSA signatures" by default refer to PKCS #1 version 1.5. However, that specification has flaws and new designs should use version 2, usually called by just OAEP and PSS, where possible.

Two sets of interfaces are included in this package. When a more abstract interface isn't necessary, there are functions for encrypting/decrypting with v1.5/OAEP and signing/verifying with v1.5/PSS. If one needs to abstract over the public key primitive, the PrivateKey type implements the Decrypter and Signer interfaces from the crypto package.

Operations involving private keys are implemented using constant-time algorithms, except for \[GenerateKey] and for some operations involving deprecated multi-prime keys.

### Minimum key size

\[GenerateKey] returns an error if a key of less than 1024 bits is requested, and all Sign, Verify, Encrypt, and Decrypt methods return an error if used with a key smaller than 1024 bits. Such keys are insecure and should not be used.

The \`rsa1024min=0\` GODEBUG setting suppresses this error, but we recommend doing so only in tests, if necessary. Tests can use [testing.T.Setenv](https://pkg.go.dev/testing@go1.24.13#T.Setenv) or include \`//go:debug rsa1024min=0\` in a \`\_test.go\` source file to set it.

Alternatively, see the \[GenerateKey (TestKey)] example for a pregenerated test-only 2048-bit key.

\[GenerateKey (TestKey)]: #example-GenerateKey-TestKey

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
