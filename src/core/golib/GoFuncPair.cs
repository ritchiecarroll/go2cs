// GoFuncPair.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go.golib;

// ---------------------------------------------------------------------------------------------
// M:N PROTOTYPE (stage (b), N3, docs/phase4/DESIGN-mn-func-values.md sections 4, 9.3 and 10.4).
// Prototype code: nothing from this branch lands on master as it is.
//
// A func value created in a twin body whose literal can park is a PAIR: its plain body and its
// twin, made together. The value handed out is the PLAIN delegate type bound to the pair's
// Invoke, so plain code calls it exactly as before; a twin calling through it finds the pair as
// the delegate's target and awaits the twin instead.
//
// THE ONE UNWRAP RULE. A pair-bound delegate's Method is the pair's Invoke and its Target is the
// pair, so every golib reader of Delegate.Method or Delegate.Target reads through Underlying: a
// pair stands for its plain delegate, and a delegate over another delegate's Invoke (`new F(g)`,
// a rebind) stands for the inner one. There is no second chase anywhere in golib.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// The base of every generated func-value pair: a func value's plain body and its twin, made together in a twin body.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class GoFuncPair
{
    /// <summary>The plain delegate the pair stands for: what every reader of the func value sees.</summary>
    public abstract Delegate Plain { get; }

    /// <summary>The twin, typed by the canonical twin delegate of the func value's Go signature.</summary>
    public abstract Delegate Twin { get; }

    /// <summary>
    /// The delegate <paramref name="fn"/> stands for: through a pair to its plain delegate, and through a delegate over
    /// another delegate's <c>Invoke</c> to the inner one. golib's one unwrap rule.
    /// </summary>
    public static Delegate Underlying(Delegate fn)
    {
        for (int hops = 0; hops < 8; hops++)
        {
            if (fn.Target is GoFuncPair pair)
            {
                fn = pair.Plain;
                continue;
            }

            if (fn.Target is Delegate inner && fn.Method.Name == "Invoke" && fn.Method.DeclaringType == inner.GetType())
            {
                fn = inner;
                continue;
            }

            break;
        }

        return fn;
    }
}
