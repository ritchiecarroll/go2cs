Test-SheetPaste.ps1's fixtures (COORD seat #39, 2026-10-28):
  good.md  two PowerShell blocks, seven submissions, no finding
  bad.md   blocks 1 and 6 clean; blocks 2 to 5 and 7 each carry one shape the check refuses, at the line the suite
           names (block 7: a package push with no Assert-NugetgoPushable.ps1 before it, COORD's INC 4 review)
The suite pins every finding by block, line and kind, so a line added above a bad block moves the expected line too.
