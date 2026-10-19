// WindowsDnsTxtPayloadTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

using syscall = go.syscall_package;
using windows = go.@internal.syscall.windows_package;

namespace GolibTests;

/// <summary>
/// The positive pin for net's TXT read: a record whose <c>DNSTXTData</c> carries TWO strings, transcribed
/// by syscall's DnsQuery hand-own and read back the way the hand-owned lookup_windows.cs reads it --
/// <c>d.of(DNSTXTData.ᏑStringArray).ElementRef(i)</c> for every <c>i &lt; d.StringCount</c>.
/// </summary>
/// <remarks>
/// <para>
/// Go declares <c>StringArray [1]*uint16</c>, the union's minimum, and reads past it through an unsafe cast
/// bounded by <c>StringCount</c>. The transcription allocates <c>StringCount</c> elements, so on the managed
/// payload the field view IS that bound. One string would not tell the declared length from the real one;
/// two do, which is why this pins <c>StringCount = 2</c> and reads index 1.
/// </para>
/// <para>
/// The image is shaped as dnsapi shapes a DNS_RECORDW on x64 (header 32 bytes, then DNS_TXT_DATAW:
/// <c>dwStringCount</c> at 0, <c>pStringArray[]</c> at 8), with the name and the strings in the same block,
/// so the entries carry absolute addresses of the block's own interior.
/// </para>
/// <para>
/// Windows-only by nature (the syscall package's windows flavour declares these members at all) and gated
/// on the HOST, as WindowsNetDbTranscriptionTests is. Measured on a linux host (2026-10-19): the windows
/// flavour dies in syscall's module initializer (kernel32 does not load) before this arm's gate is reached,
/// so this arm runs only on Windows, and a linux build of the flavour proves only that it compiles.
/// </para>
/// </remarks>
[TestClass]
public unsafe class WindowsDnsTxtPayloadTests
{
    private static bool OnWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private const int RecordDataOffset = 32;
    private const int TxtStringArrayOffset = 8;
    private const int NameOffset = 64;
    private const int FirstOffset = 128;
    private const int SecondOffset = 192;
    private const int BlockSize = 256;

    private const ushort DnsTypeText = 0x0010;

    private static void writeUtf16(nint block, int offset, string text)
    {
        ushort* target = (ushort*)(block + offset);

        for (int i = 0; i < text.Length; i++)
        {
            target[i] = text[i];
        }

        target[text.Length] = 0;
    }

    // One DNS_RECORDW of type TEXT, no Next, carrying the given strings.
    private static nint txtRecordImage(string name, string first, string second)
    {
        nint block = (nint)NativeMemory.AllocZeroed(BlockSize);

        writeUtf16(block, NameOffset, name);
        writeUtf16(block, FirstOffset, first);
        writeUtf16(block, SecondOffset, second);

        *(nint*)(block + 0) = 0;                    // pNext
        *(nint*)(block + 8) = block + NameOffset;   // pName
        *(ushort*)(block + 16) = DnsTypeText;       // wType
        *(ushort*)(block + 18) = 24;                // wDataLength: count, pad, two pointers

        nint data = block + RecordDataOffset;
        *(uint*)data = 2;                                           // dwStringCount
        *(nint*)(data + TxtStringArrayOffset) = block + FirstOffset;
        *(nint*)(data + TxtStringArrayOffset + 8) = block + SecondOffset;

        return block;
    }

    [TestMethod]
    public void ATxtPayloadWithTwoStringsReadsBothThroughTheFieldView()
    {
        if (!OnWindows)
        {
            Assert.Inconclusive("NOT MEASURED: the syscall package's windows flavour runs only on a Windows host.");
            return;
        }

        nint block = txtRecordImage("txt.example", "first", "second");

        try
        {
            ж<syscall.DNSRecord> record = syscall.transcribeDnsRecords((nuint)block);
            Assert.IsFalse(record.IsNilPointer, "the transcription answered no record");

            ж<syscall.DNSTXTData> d = syscall.DnsRecordPayload<syscall.DNSTXTData>(record);
            Assert.IsFalse(d.IsNilPointer, "no TXT payload was registered for the record");
            Assert.AreEqual((ushort)2, (ushort)d.Value.StringCount);

            // lookup_windows.cs's own loop, verbatim in shape.
            string[] read = new string[d.Value.StringCount];
            string joined = "";

            for (nint i = 0; i < d.Value.StringCount; i++)
            {
                read[i] = windows.UTF16PtrToString(d.of(syscall.DNSTXTData.ᏑStringArray).ElementRef(i));
                joined += read[i];
            }

            CollectionAssert.AreEqual(new[] { "first", "second" }, read);
            Assert.AreEqual("firstsecond", joined);
        }
        finally
        {
            NativeMemory.Free((void*)block);
        }
    }
}
