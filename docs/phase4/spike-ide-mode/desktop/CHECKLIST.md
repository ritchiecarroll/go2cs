# IDE-mode spike: desktop checklist

The hands-on half of [the spike](../../SPIKE-ide-mode.md): about 45 minutes. Each line says what to do, what should
happen, and whether to take a screenshot. Write PASS or FAIL beside each line, with a few words when something
differs. A FAIL is a useful answer; nothing needs fixing during the run.

**Before you start** (about 5 minutes, once):

1. Install Go 1.24, the .NET 10 SDK and Python 3, and in VS Code the "C#" extension from Microsoft.
2. In a terminal at the root of this branch, run
   `pwsh -NoProfile -File docs/phase4/spike-ide-mode/desktop/prepare.ps1`
   (or `powershell` instead of `pwsh`). It ends with "Ready." and a list of paths. Keep that list open.
   Expected: no red text. Screenshot: no.

## E2: VS Code with Microsoft's debugger

3. VS Code: File > Open Folder > the `stepping` folder the script printed. Open `main.go`.
   Expected: the file opens. Screenshot: no.
4. Click in the margin left of line 23 (`a, b := pair()`).
   Expected: a red dot. Screenshot: no.
5. Run and Debug (Ctrl+Shift+D) > choose "go2cs: stepping (Debug, #line)" > Start (F5).
   Expected: the program stops on line 23 of `main.go`, the line is highlighted, and the Call Stack shows `main.go`
   line 23. Screenshot: **yes**.
6. Press Step Over (F10) three times.
   Expected: the highlight moves to lines 24, 25, 26, one line per press. Screenshot: no.
7. Add a red dot on line 16 (`sum += i`) and press Continue (F5).
   Expected: stops on line 16. The Call Stack shows `work` at `main.go` line 16 and, below it, `main.go` line 30.
   Screenshot: **yes**.
8. With the program stopped on line 16, press Step Over (F10) about ten times.
   Expected: the highlight walks lines 14, 15, 16 of the loop. Write down how many presses stay on line 14 (the `for`
   line) before it moves on: the Linux debugger stops there two or three times per turn of the loop.
   Screenshot: no.
9. Remove the red dots on 16 and 23, add one on line 35 (`fmt.Println(`), press Continue (F5), then open
   VARIABLES > Locals.
   Expected: stops on line 35; `a` is 1, `b` is 2. Write down how `results` is shown (a raw C# value is expected, not
   `[4 4 12]`). Screenshot: **yes**.
10. Stop debugging. Add a comment line at the end of `main.go` and save it, but do not run the script. Put a red dot
    on line 23 and press F5.
    Expected: the dot turns hollow or grey, and the program does not stop there, because the file no longer matches
    the one that was built. Screenshot: **yes**. Then undo the edit and save.
11. Open the `caller` folder the script printed, put a red dot on line 15 (`fmt.Println(where())`) and press F5.
    Expected: stops on line 15 of its `main.go`. Screenshot: no.

## E3: Visual Studio

12. Visual Studio: File > Open > Project/Solution > the `stepping` `.slnx` the script printed. Then File > Open >
    File > the `stepping` `main.go`.
    Expected: both open. Screenshot: no.
13. In `main.go`, click on line 23 and press F9.
    Expected: a red dot (it may say "will not currently be hit" until the program runs). If F9 does nothing, write
    that down and try Debug > New Breakpoint > Function Breakpoint with `main.go, line 23`. Screenshot: **yes**.
14. Press F5.
    Expected: Visual Studio builds and stops on line 23 of `main.go`, and the Call Stack window shows `main.go`.
    Screenshot: **yes**.
15. Press F10 three times, then look at the Locals window.
    Expected: lines 24, 25, 26; Locals lists `a` and `b`. Screenshot: no.

## E4: Rider (optional)

16. Rider: open the same `.slnx`, open `main.go`, and click the margin on line 23.
    Expected: a red dot. Screenshot: **yes**.
17. Start debugging.
    Expected: stops on line 23 of `main.go`. Screenshot: **yes**.

## E8: the build inside the IDE

These use the two projects in `hosts/`, which convert the Go code themselves when they build, so they need the
converter the script built. The script's last line names its folder. In the same terminal as before, run
`$env:PATH = "<that folder>;" + $env:PATH`, then start Visual Studio from that terminal (`devenv`).

18. Visual Studio: open `docs/phase4/spike-ide-mode/hosts/consumer/Consumer.csproj` and wait until loading finishes.
    Expected: no `obj/go2cs` folder appears beside `Consumer.csproj` just from opening it (loading must not convert).
    Screenshot: no.
19. Press F5.
    Expected: a console window prints lines ending in `hello v3, dotnet (3 words) cs-a`, and an `obj/go2cs` folder
    now exists. Screenshot: **yes**.
20. Stop. Edit `samples/shared/words/words.go`: change `" v3"` to `" v4"`, save, and press F5 again.
    Expected: Visual Studio rebuilds and the lines now end in `hello v4, ...`. If it starts the old program without
    rebuilding, write that down: it means the up-to-date check ignores `.go` files. Screenshot: **yes**.
21. Open `docs/phase4/spike-ide-mode/hosts/gomain/GoMain.csproj` and press F5.
    Expected: one line, starting `main v`, then the program ends. Screenshot: no.
22. Optional, in Rider: repeat 18 and 19.
    Expected: the same. Screenshot: no.

**Afterwards:** change `" v4"` back to `" v3"` in `words.go`. The `.vscode` folders the script wrote are ignored by
git. Send the list of PASS/FAIL lines and the screenshots to COORD.
