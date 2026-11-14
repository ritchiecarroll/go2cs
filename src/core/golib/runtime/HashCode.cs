// HashCode.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go.golib;

/// <summary>
/// Provides a helper class for generating hash codes.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public class HashCode
{
    // Typed overloads for one to eight values, which a generated struct's GetHashCode binds ahead of the
    // params form: no object[] and no box per field. System.HashCode.Combine<T> hashes value?.GetHashCode(),
    // exactly what the params form computes from the boxed value, so every hash is unchanged.
    public static int Combine<T1>(T1 value1) => System.HashCode.Combine(value1);
    public static int Combine<T1, T2>(T1 value1, T2 value2) => System.HashCode.Combine(value1, value2);
    public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3) => System.HashCode.Combine(value1, value2, value3);
    public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4) => System.HashCode.Combine(value1, value2, value3, value4);
    public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) => System.HashCode.Combine(value1, value2, value3, value4, value5);
    public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) => System.HashCode.Combine(value1, value2, value3, value4, value5, value6);
    public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) => System.HashCode.Combine(value1, value2, value3, value4, value5, value6, value7);
    public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) => System.HashCode.Combine(value1, value2, value3, value4, value5, value6, value7, value8);

    /// <summary>
    /// Combines all values into a single hash code.
    /// </summary>
    /// <param name="objects">Source objects.</param>
    /// <returns>Combined hash code.</returns>
    /// <remarks>
    /// System hash code combine function combines up to 8 objects, this function works with
    /// any number by combing the first 8 then combining the remainder in groups of 8.
    /// </remarks>
    public static int Combine(params object[] objects)
    {
        switch (objects.Length)
        {
            case 0:
                return 0;
            case <= 8:
                // Use the built-in HashCode.Combine for 1-8 objects for optimal performance
                return objects.Length switch
                {
                    1 => System.HashCode.Combine(objects[0]),
                    2 => System.HashCode.Combine(objects[0], objects[1]),
                    3 => System.HashCode.Combine(objects[0], objects[1], objects[2]),
                    4 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3]),
                    5 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3], objects[4]),
                    6 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5]),
                    7 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5], objects[6]),
                    8 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5], objects[6], objects[7]),
                    _ => 0 // Won't reach here due to the condition
                };
        }

        // For more than 8 objects, combine in groups of 8 and then combine the results
        int hashCode = 0;
    
        for (int i = 0; i < objects.Length; i += 8)
        {
            int remaining = Math.Min(8, objects.Length - i);
        
            int groupHash = remaining switch
            {
                1 => System.HashCode.Combine(objects[i]),
                2 => System.HashCode.Combine(objects[i], objects[i + 1]),
                3 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2]),
                4 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3]),
                5 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3], objects[i + 4]),
                6 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3], objects[i + 4], objects[i + 5]),
                7 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3], objects[i + 4], objects[i + 5], objects[i + 6]),
                8 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3], objects[i + 4], objects[i + 5], objects[i + 6], objects[i + 7]),
                _ => 0 // Won't reach here
            };

            hashCode = System.HashCode.Combine(hashCode, groupHash);
        }

        return hashCode;
    }
}
