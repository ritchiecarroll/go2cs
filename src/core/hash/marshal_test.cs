// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Test that the hashes in the standard library implement
// BinaryMarshaler, BinaryUnmarshaler,
// and lock in the current representations.
namespace go;

using bytes = bytes_package;
using md5 = crypto.md5_package;
using sha1 = crypto.sha1_package;
using sha256 = crypto.sha256_package;
using sha512 = crypto.sha512_package;
using Δencoding = encoding_package;
using hex = go.encoding.hex_package;
using Δhash = hash_package;
using adler32 = go.hash.adler32_package;
using crc32 = go.hash.crc32_package;
using crc64 = go.hash.crc64_package;
using fnv = go.hash.fnv_package;
using testing = testing_package;
using crypto;
using go.encoding;
using go.hash;

partial class hash_test_package {

internal static slice<byte> fromHex(@string s) {
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}


partial struct marshalTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal Func<Δhash.Hash> @new;
    internal slice<byte> golden;
}
internal static slice<marshalTestsᴛ1> marshalTests = new marshalTestsᴛ1[]{
    new("adler32"u8, () => new hash_Hash32ᴠHash(adler32.New()), fromHex("61646c01460a789d"u8)),
    new("crc32"u8, () => new hash_Hash32ᴠHash(crc32.NewIEEE()), fromHex("63726301ca87914dc956d3e8"u8)),
    new("crc64"u8, () => new hash_Hash64ᴠHash(crc64.New(crc64.MakeTable(crc64.ISO))), fromHex("6372630273ba8484bbcd5def5d51c83c581695be"u8)),
    new("fnv32"u8, () => new hash_Hash32ᴠHash(fnv.New32()), fromHex("666e760171ba3d77"u8)),
    new("fnv32a"u8, () => new hash_Hash32ᴠHash(fnv.New32a()), fromHex("666e76027439f86f"u8)),
    new("fnv64"u8, () => new hash_Hash64ᴠHash(fnv.New64()), fromHex("666e7603cc64e0e97692c637"u8)),
    new("fnv64a"u8, () => new hash_Hash64ᴠHash(fnv.New64a()), fromHex("666e7604c522af9b0dede66f"u8)),
    new("fnv128"u8, () => fnv.New128(), fromHex("666e760561587a70a0f66d7981dc980e2cabbaf7"u8)),
    new("fnv128a"u8, () => fnv.New128a(), fromHex("666e7606a955802b0136cb67622b461d9f91e6ff"u8)),
    new("md5"u8, md5.New, fromHex("6d643501a91b0023007aa14740a3979210b5f024c0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f80000000000000000000000000000f9"u8)),
    new("sha1"u8, sha1.New, fromHex("736861016dad5acb4dc003952f7a0b352ee5537ec381a228c0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f80000000000000000000000000000f9"u8)),
    new("sha224"u8, sha256.New224, fromHex("73686102f8b92fc047c9b4d82f01a6370841277b7a0d92108440178c83db855a8e66c2d9c0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f80000000000000000000000000000f9"u8)),
    new("sha256"u8, sha256.New, fromHex("736861032bed68b99987cae48183b2b049d393d0050868e4e8ba3730e9112b08765929b7c0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f80000000000000000000000000000f9"u8)),
    new("sha384"u8, sha512.New384, fromHex("736861046f1664d213dd802f7c47bc50637cf93592570a2b8695839148bf38341c6eacd05326452ef1cbe64d90f1ef73bb5ac7d2803565467d0ddb10c5ee3fc050f9f0c1808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9fa0a1a2a3a4a5a6a7a8a9aaabacadaeafb0b1b2b3b4b5b6b7b8b9babbbcbdbebfc0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f80000000000000000000000000000f9"u8)),
    new("sha512_224"u8, sha512.New512_224, fromHex("736861056f1a450ec15af20572d0d1ee6518104d7cbbbe79a038557af5450ed7dbd420b53b7335209e951b4d9aff401f90549b9604fa3d823fbb8581c73582a88aa84022808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9fa0a1a2a3a4a5a6a7a8a9aaabacadaeafb0b1b2b3b4b5b6b7b8b9babbbcbdbebfc0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f80000000000000000000000000000f9"u8)),
    new("sha512_256"u8, sha512.New512_256, fromHex("736861067c541f1d1a72536b1f5dad64026bcc7c508f8a2126b51f46f8b9bff63a26fee70980718031e96832e95547f4fe76160ff84076db53b4549b86354af8e17b5116808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9fa0a1a2a3a4a5a6a7a8a9aaabacadaeafb0b1b2b3b4b5b6b7b8b9babbbcbdbebfc0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f80000000000000000000000000000f9"u8)),
    new("sha512"u8, sha512.New, fromHex("736861078e03953cd57cd6879321270afa70c5827bb5b69be59a8f0130147e94f2aedf7bdc01c56c92343ca8bd837bb7f0208f5a23e155694516b6f147099d491a30b151808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9fa0a1a2a3a4a5a6a7a8a9aaabacadaeafb0b1b2b3b4b5b6b7b8b9babbbcbdbebfc0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f80000000000000000000000000000f9"u8))
}.slice();

public static void TestMarshalHash(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in marshalTests) {
        ref var tt = ref heap(new marshalTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var buf = new slice<byte>(256);
            foreach (var (i, _) in buf) {
                buf[i] = (byte)i;
            }
            var h = ttʗ1.@new();
            h.Write(buf[..256]);
            var sum = h.Sum(default!);
            var h2 = ttʗ1.@new();
            var h3 = ttʗ1.@new();
            const nint split = 249;
            for (nint i = 0; i < split; i++) {
                h2.Write(buf.slice(i, i + 1));
            }
            var (h2m, ok) = h2._<Δencoding.BinaryMarshaler>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("Hash does not implement MarshalBinary"u8);
            }
            var (enc, err) = h2m.MarshalBinary();
            if (err != default!) {
                tΔ1.Fatalf("MarshalBinary: %v"u8, err);
            }
            if (!bytes.Equal(enc, ttʗ1.golden)) {
                tΔ1.Errorf("MarshalBinary = %x, want %x"u8, enc, ttʗ1.golden);
            }
            (var h3u, ok) = h3._<Δencoding.BinaryUnmarshaler>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("Hash does not implement UnmarshalBinary"u8);
            }
            {
                var errΔ1 = h3u.UnmarshalBinary(enc); if (errΔ1 != default!) {
                    tΔ1.Fatalf("UnmarshalBinary: %v"u8, errΔ1);
                }
            }
            h2.Write(buf[(int)(split)..]);
            h3.Write(buf[(int)(split)..]);
            var sum2 = h2.Sum(default!);
            var sum3 = h3.Sum(default!);
            if (!bytes.Equal(sum2, sum)) {
                tΔ1.Fatalf("Sum after MarshalBinary = %x, want %x"u8, sum2, sum);
            }
            if (!bytes.Equal(sum3, sum)) {
                tΔ1.Fatalf("Sum after UnmarshalBinary = %x, want %x"u8, sum3, sum);
            }
        });
    }
}

} // end hash_test_package
