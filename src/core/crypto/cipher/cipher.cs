// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cipher implements standard block cipher modes that can be wrapped
// around low-level block cipher implementations.
// See https://csrc.nist.gov/groups/ST/toolkit/BCM/current_modes.html
// and NIST Special Publication 800-38A.
namespace go.crypto;

partial class cipher_package {

// A Block represents an implementation of block cipher
// using a given key. It provides the capability to encrypt
// or decrypt individual blocks. The mode implementations
// extend that capability to streams of blocks.
partial interface Block {
    // BlockSize returns the cipher's block size.
    nint BlockSize();
    // Encrypt encrypts the first block in src into dst.
    // Dst and src must overlap entirely or not at all.
    void Encrypt(slice<byte> dst, slice<byte> src);
    // Decrypt decrypts the first block in src into dst.
    // Dst and src must overlap entirely or not at all.
    void Decrypt(slice<byte> dst, slice<byte> src);
}

// A Stream represents a stream cipher.
partial interface Stream {
    // XORKeyStream XORs each byte in the given slice with a byte from the
    // cipher's key stream. Dst and src must overlap entirely or not at all.
    //
    // If len(dst) < len(src), XORKeyStream should panic. It is acceptable
    // to pass a dst bigger than src, and in that case, XORKeyStream will
    // only update dst[:len(src)] and will not touch the rest of dst.
    //
    // Multiple calls to XORKeyStream behave as if the concatenation of
    // the src buffers was passed in a single run. That is, Stream
    // maintains state and does not reset at each XORKeyStream call.
    void XORKeyStream(slice<byte> dst, slice<byte> src);
}

// A BlockMode represents a block cipher running in a block-based mode (CBC,
// ECB etc).
partial interface BlockMode {
    // BlockSize returns the mode's block size.
    nint BlockSize();
    // CryptBlocks encrypts or decrypts a number of blocks. The length of
    // src must be a multiple of the block size. Dst and src must overlap
    // entirely or not at all.
    //
    // If len(dst) < len(src), CryptBlocks should panic. It is acceptable
    // to pass a dst bigger than src, and in that case, CryptBlocks will
    // only update dst[:len(src)] and will not touch the rest of dst.
    //
    // Multiple calls to CryptBlocks behave as if the concatenation of
    // the src buffers was passed in a single run. That is, BlockMode
    // maintains state and does not reset at each CryptBlocks call.
    void CryptBlocks(slice<byte> dst, slice<byte> src);
}

// AEAD is a cipher mode providing authenticated encryption with associated
// data. For a description of the methodology, see
// https://en.wikipedia.org/wiki/Authenticated_encryption.
partial interface AEAD {
    // NonceSize returns the size of the nonce that must be passed to Seal
    // and Open.
    nint NonceSize();
    // Overhead returns the maximum difference between the lengths of a
    // plaintext and its ciphertext.
    nint Overhead();
    // Seal encrypts and authenticates plaintext, authenticates the
    // additional data and appends the result to dst, returning the updated
    // slice. The nonce must be NonceSize() bytes long and unique for all
    // time, for a given key.
    //
    // To reuse plaintext's storage for the encrypted output, use plaintext[:0]
    // as dst. Otherwise, the remaining capacity of dst must not overlap plaintext.
    // dst and additionalData may not overlap.
    slice<byte> Seal(slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData);
    // Open decrypts and authenticates ciphertext, authenticates the
    // additional data and, if successful, appends the resulting plaintext
    // to dst, returning the updated slice. The nonce must be NonceSize()
    // bytes long and both it and the additional data must match the
    // value passed to Seal.
    //
    // To reuse ciphertext's storage for the decrypted output, use ciphertext[:0]
    // as dst. Otherwise, the remaining capacity of dst must not overlap ciphertext.
    // dst and additionalData may not overlap.
    //
    // Even if the function fails, the contents of dst, up to its capacity,
    // may be overwritten.
    (slice<byte>, error) Open(slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData);
}

} // end cipher_package
