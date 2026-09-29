// PointerTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class PointerTypeTemplate
{
    public static string Generate(string className, string targetTypeName) =>
        $$"""

                public override bool Equals(object? obj) => obj switch
                {
                    {{className}} other => m_value == other.m_value,
                    // A null reference is the same Go nil pointer as the canonical typed nil instance.
                    null => IsNilPointer,
                    _ => false
                };

                public override int GetHashCode() => m_value?.GetHashCode() ?? 0;

                public ref {{targetTypeName}} Value => ref m_value.Value;

                public bool IsNull => m_value is null || m_value.IsNull;

                // STRUCTURAL nil — pointer identity and the reflection bridge's nil probes; the
                // value-peeking IsNull above remains the dereference guard. See ж<T>.IsNilPointer.
                public bool IsNilPointer => m_value is null || m_value.IsNilPointer;

                // The CANONICAL typed nil instance for this named pointer type (see ж<T>.NilBox):
                // every nil→'{{className}}' conversion yields this shared instance, so a typed nil
                // boxed into an interface keeps its Go type and reference-compares equal across sites.
                internal static {{className}} NilInstance { get; } = new(nil);
                
                public {{PointerPrefix}}<TElem> of<TElem>(FieldRefFunc<TElem> fieldRefFunc) => m_value.of(fieldRefFunc);
                
                public {{PointerPrefix}}<TElem> of<TElem>(FieldRefFunc<{{targetTypeName}}, TElem> fieldRefFunc) => m_value.of(fieldRefFunc);
                
                public {{PointerPrefix}}<TElem> at<TElem>(nint index) => m_value.at<TElem>(index);

                public {{PointerPrefix}}<TElem> at<TElem>(int index) => m_value.at<TElem>(index);

                public {{PointerPrefix}}<TElem> at<TElem>(ulong index) => m_value.at<TElem>(index);

                static {{targetTypeName}} IPointer<{{targetTypeName}}>.operator ~(IPointer<{{targetTypeName}}> value) => value.Value;

                public static unsafe implicit operator {{className}}(uintptr value)
                {
                    return new {{className}}(new {{BoxConstructPrefix}}<{{targetTypeName}}>(*({{targetTypeName}}*)value));
                }
                
                // Both OUTBOUND conversions defer to the box's own operator rather than take
                // `&value.Value`: that is a DEREFERENCE, which a native box over a pointer-order
                // token refuses by design (Q44 §10.3 arm 2a) although the token IS its address, and
                // for managed storage it is an unpinned, unregistered `fixed` address. The box's
                // operator returns a native address unread, 0 for nil, and pins and registers the rest.
                public static unsafe implicit operator uintptr({{className}} value) => (uintptr)value.m_value;
                
                public static unsafe implicit operator {{className}}(void* value)
                {
                    return new {{className}}(new {{BoxConstructPrefix}}<{{targetTypeName}}>(*({{targetTypeName}}*)value));
                }
                
                public static unsafe implicit operator void*({{className}} value) => (void*)value.m_value;
        """;
}
