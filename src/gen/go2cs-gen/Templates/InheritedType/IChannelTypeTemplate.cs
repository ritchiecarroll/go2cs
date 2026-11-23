// IChannelTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class IChannelTypeTemplate
{
    // The members this wrapper declares for its own use under an ASCII name a Go method can also declare (R7; see
    // ISliceTypeTemplate.ClaimableMembers). GenTests' NamedChanMapNumericClaimedMemberTests.
    internal static readonly string[] ClaimableMembers = ["Capacity", "GetEnumerator", "IsClosed", "IsUnbuffered", "Length", "Make", "Receive", "Received", "Receiving", "Send", "Sending", "Sent"];

    // constructorName is structName without a generic type's parameters (a C# constructor never carries them).
    //
    // goMethods are the Go methods declared on the type, and its own name (InheritedTypeTemplate.GoMethodNames). C# binds
    // an applicable instance member before the extension method a Go method becomes, so a member a Go method claims moves
    // out of the way (R7): to its explicit IChannel / IChannel<T> / ISupportMake / IEnumerable<T> implementation where an
    // interface declares it, or else to the same member under Δ, where a C# consumer still reaches it. In its default
    // operator mode the converter spells every channel operation with a glyph no Go identifier can spell (ChannelLeftOp,
    // ChannelRightOp); in its named-method mode (-uco=false) it falls back to those glyphs for a channel whose Go type
    // claims the name it would spell (chanOperatorSpelling, visitChanType.go).
    public static string Generate(string structName, string constructorName, string typeName, string targetTypeName, ICollection<string> goMethods)
    {
        string Claimed(string name, string own, string claimed) => goMethods.Contains(name) ? claimed : own;

        // A member no interface declares: `public ... Name`, or `public ... ΔName` when claimed.
        string Name(string name) => goMethods.Contains(name) ? $"Δ{name}" : name;

        string Member(string name, string returnType, string body) =>
            goMethods.Contains(name) ?
                $"{returnType} IChannel<{targetTypeName}>.{name}(in {targetTypeName} value) => {body};" :
                $"public {returnType} {name}(in {targetTypeName} value) => {body};";

        string enumerable = $"global::System.Collections.Generic.IEnumerable<{targetTypeName}>";

        return $$"""

                {{Claimed("Capacity", "public nint Capacity => m_value.Capacity;", "nint IChannel.Capacity => m_value.Capacity;")}}

                {{Claimed("Length", "public nint Length => m_value.Length;", "nint IChannel.Length => m_value.Length;")}}

                public bool {{Name("IsUnbuffered")}} => m_value.IsUnbuffered;

                public bool {{Name("IsClosed")}} => m_value.IsClosed;

                public global::go.SelectOp {{Name("Receiving")}} => m_value.Receiving;

                // Explicit-only: Go code commonly defines its OWN Close() method on a named
                // channel type (net/http's closeWaiter) — a public instance Close here would
                // shadow that method's extension form at every call site. `close(ch)` routes
                // through the golib free function, so no public surface is lost.
                void IChannel.Close() => ((IChannel)m_value).Close();

                {{Member("Send", "void", "m_value.Send(value)")}}

                public void {{ChannelLeftOp}}(in {{targetTypeName}} value) => m_value.Send(value);

                public global::go.SelectOp {{Name("Sending")}}(in {{targetTypeName}} value) => m_value.Sending(value);

                public global::go.SelectOp {{ChannelLeftOp}}(in {{targetTypeName}} value, NilType _) => m_value.Sending(value);

                {{Member("Sent", "bool", "m_value.Sent(value)")}}

                public bool {{ChannelLeftOp}}(in {{targetTypeName}} value, bool _) => m_value.Sent(value);

                public {{targetTypeName}} {{Name("Receive")}}() => m_value.Receive();

                public ({{targetTypeName}} val, bool ok) {{Name("Receive")}}(bool _) => m_value.Receive(_);

                public bool {{Name("Received")}}(out {{targetTypeName}} value) => m_value.Received(out value);

                public bool {{Name("Received")}}(out {{targetTypeName}} value, out bool ok) => m_value.Received(out value, out ok);{{Claimed("Received", "", $"\r\n\r\n        bool IChannel<{targetTypeName}>.Received(out {targetTypeName} value) => m_value.Received(out value);")}}

                public bool {{ChannelRightOp}}(out {{targetTypeName}} value) => m_value.Received(out value);

                public bool {{ChannelRightOp}}(out {{targetTypeName}} value, out bool ok) => m_value.Received(out value, out ok);

                void IChannel.Send(object value) => ((IChannel)m_value).Send(value);

                object IChannel.Receive() => ((IChannel)m_value).Receive();

                bool IChannel.Sent(object value) => ((IChannel)m_value).Sent(value);

                bool IChannel.Received(out object value) => ((IChannel)m_value).Received(out value);

                bool IChannel.ChanRecv(out object value, out bool ok, bool block) => ((IChannel)m_value).ChanRecv(out value, out ok, block);

                bool IChannel.ChanSend(object value, bool block) => ((IChannel)m_value).ChanSend(value, block);

                // A claimed GetEnumerator leaves `foreach` (Go's range over the channel) to IEnumerable<T>, which C# binds before
                // an extension GetEnumerator; InheritedTypeTemplate then declares the interface (ChannelInterfaces).
                {{Claimed("GetEnumerator",
                    $"public global::System.Collections.Generic.IEnumerator<{targetTypeName}> GetEnumerator() => m_value.GetEnumerator();",
                    $"global::System.Collections.Generic.IEnumerator<{targetTypeName}> {enumerable}.GetEnumerator() => m_value.GetEnumerator();")}}

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable)m_value).GetEnumerator();

                {{Claimed("Make",
                    $"public static {structName} Make(nint p1 = 0, nint p2 = -1) => new {structName}(p1);",
                    $"static {structName} ISupportMake<{structName}>.Make(nint p1, nint p2) => new {structName}(p1);")}}

                public {{constructorName}}(nint size) => m_value = new {{typeName}}(size);
        """;
    }
}
