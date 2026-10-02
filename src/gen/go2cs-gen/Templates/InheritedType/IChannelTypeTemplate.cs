// IChannelTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class IChannelTypeTemplate
{
    // constructorName is structName without a generic type's parameters (a C# constructor never carries them).
    public static string Generate(string structName, string constructorName, string typeName, string targetTypeName) =>
        $$"""

                public nint Capacity => m_value.Capacity;

                public nint Length => m_value.Length;

                public bool IsUnbuffered => m_value.IsUnbuffered;

                public bool IsClosed => m_value.IsClosed;

                public global::go.SelectOp Receiving => m_value.Receiving;

                // Explicit-only: Go code commonly defines its OWN Close() method on a named
                // channel type (net/http's closeWaiter) — a public instance Close here would
                // shadow that method's extension form at every call site. `close(ch)` routes
                // through the golib free function, so no public surface is lost.
                void IChannel.Close() => ((IChannel)m_value).Close();

                public void Send(in {{targetTypeName}} value) => m_value.Send(value);

                public void {{ChannelLeftOp}}(in {{targetTypeName}} value) => m_value.Send(value);

                public global::go.SelectOp Sending(in {{targetTypeName}} value) => m_value.Sending(value);

                public global::go.SelectOp {{ChannelLeftOp}}(in {{targetTypeName}} value, NilType _) => m_value.Sending(value);

                public bool Sent(in {{targetTypeName}} value) => m_value.Sent(value);

                public bool {{ChannelLeftOp}}(in {{targetTypeName}} value, bool _) => m_value.Sent(value);

                public {{targetTypeName}} Receive() => m_value.Receive();

                public ({{targetTypeName}} val, bool ok) Receive(bool _) => m_value.Receive(_);

                public bool Received(out {{targetTypeName}} value) => m_value.Received(out value);

                public bool Received(out {{targetTypeName}} value, out bool ok) => m_value.Received(out value, out ok);

                public bool {{ChannelRightOp}}(out {{targetTypeName}} value) => m_value.Received(out value);

                public bool {{ChannelRightOp}}(out {{targetTypeName}} value, out bool ok) => m_value.Received(out value, out ok);

                void IChannel.Send(object value) => ((IChannel)m_value).Send(value);

                object IChannel.Receive() => ((IChannel)m_value).Receive();

                bool IChannel.Sent(object value) => ((IChannel)m_value).Sent(value);

                bool IChannel.Received(out object value) => ((IChannel)m_value).Received(out value);

                bool IChannel.ChanRecv(out object value, out bool ok, bool block) => ((IChannel)m_value).ChanRecv(out value, out ok, block);

                bool IChannel.ChanSend(object value, bool block) => ((IChannel)m_value).ChanSend(value, block);

                public global::System.Collections.Generic.IEnumerator<{{targetTypeName}}> GetEnumerator() => m_value.GetEnumerator();

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable)m_value).GetEnumerator();

                public static {{structName}} Make(nint p1 = 0, nint p2 = -1) => new {{structName}}(p1);

                public {{constructorName}}(nint size) => m_value = new {{typeName}}(size);
        """;
}
