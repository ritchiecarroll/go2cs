// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

partial class main_package {

public static UntypedInt N => /* 11 + 1 */ 12;

internal static slice<rune> board = slice<rune>(
    (@string)"""
...........
...........
....●●●....
....●●●....
..●●●●●●●..
..●●●○●●●..
..●●●●●●●..
....●●●....
....●●●....
...........
...........

""");

internal static nint center;

[GoInit] internal static void init() {
    println((@string)"init fn 1"u8);
    nint n = 0;
    foreach (var (pos, field) in board) {
        if (field == (rune)'○') {
            center = pos;
            n++;
        }
    }
    if (n != 1) {
        center = -1;
    }
}

[GoInit] internal static void initΔ1() {
    println((@string)"init fn 2"u8);
}

internal static nint moves;

internal static bool move(nint pos, nint dir) {
    moves++;
    if (board[pos] == (rune)'●' && board[pos + dir] == (rune)'●' && board[pos + 2 * dir] == (rune)'○') {
        board[pos] = (rune)'○';
        board[pos + dir] = (rune)'○';
        board[pos + 2 * dir] = (rune)'●';
        return true;
    }
    return false;
}

internal static void unmove(nint pos, nint dir) {
    board[pos] = (rune)'●';
    board[pos + dir] = (rune)'●';
    board[pos + 2 * dir] = (rune)'○';
}

internal static bool solve() {
    nint last = default!;
    nint n = default!;
    foreach (var (pos, field) in board) {
        if (field == (rune)'●') {
            foreach (var (_, dir) in new nint[]{-1, -N, +1, +N}.array()) {
                if (move(pos, dir)) {
                    if (solve()) {
                        unmove(pos, dir);
                        println(((@string)board));
                        return true;
                    }
                    unmove(pos, dir);
                }
            }
            last = pos;
            n++;
        }
    }
    if (n == 1 && (center < 0 || last == center)) {
        println(((@string)board));
        return true;
    }
    return false;
}

internal static void Main() {
    if (!solve()) {
        println((@string)"no solution found"u8);
    }
    println(moves, (@string)"moves tried"u8);
}

} // end main_package
