<#
.SYNOPSIS
    What a nugetgo module pack's self-description lists (docs/PLAN-nugetgo.md section 5, stage S2).

.DESCRIPTION
    Two questions nugetgo-pack.ps1 asks of a `go2cs -recurse=nuget` output root before it packs a module:

      Get-NugetgoPackedPackages      each packed library's Go import path and assembly name -- the self-description's
                                     `package` lines.
      Get-NugetgoThirdPartyRequires  the third-party MODULES the packed libraries reference -- each becomes a
                                     `require` line (module, the version go2cs.modules.lock says was converted, and
                                     its NuGet ID) and a NuGet dependency on the package version the caller names
                                     (owner ruling B4: the first revision built for the same corpus).

    A reference the tool cannot account for is REFUSED by name rather than dropped: a packed module whose
    dependency vanished from its nuspec restores, then fails at the first type it cannot find, on the user's
    machine. The writer and the format live in Go (internal/sourcemeta, through internal/gensourcemeta); this
    module only gathers their arguments.
#>

# The import path a project directory converts: its directory under <RecurseRoot>/<tree>/, forward slashes.
function Get-RelativeImportPath([string]$TreeRoot, [string]$ProjectDirectory) {
    $tree = [System.IO.Path]::GetFullPath($TreeRoot).TrimEnd('\', '/')
    $dir = [System.IO.Path]::GetFullPath($ProjectDirectory).TrimEnd('\', '/')
    if (-not $dir.StartsWith($tree + [System.IO.Path]::DirectorySeparatorChar, [StringComparison]::Ordinal)) { return $null }
    return $dir.Substring($tree.Length + 1).Replace('\', '/')
}

function Get-NugetgoPackedPackages {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RecurseRoot, [Parameter(Mandatory)][object[]]$Libraries)

    $srcRoot = Join-Path $RecurseRoot 'src'

    foreach ($library in $Libraries) {
        $importPath = Get-RelativeImportPath $srcRoot $library.DirectoryName
        if (-not $importPath) { throw "REFUSED: $($library.FullName) is not under $srcRoot" }
        [xml]$project = Get-Content -Raw -LiteralPath $library.FullName
        $assembly = @($project.Project.PropertyGroup | ForEach-Object { $_.AssemblyName } | Where-Object { $_ })[0]
        if (-not $assembly) { $assembly = [System.IO.Path]::GetFileNameWithoutExtension($library.Name) }
        [pscustomobject]@{ ImportPath = $importPath; Assembly = [string]$assembly }
    }
}

function Get-NugetgoThirdPartyRequires {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RecurseRoot, [Parameter(Mandatory)][object[]]$Libraries, [string[]]$ThirdPartyPackage = @())

    # -ThirdPartyPackage '<module>=<nuget-id>@<package-version>', one per dependency module.
    $named = @{}
    foreach ($entry in $ThirdPartyPackage) {
        if ($entry -notmatch '^(?<module>[^=@\s]+)=(?<id>[^=@\s]+)@(?<version>[^=@\s]+)$') {
            throw "REFUSED: -ThirdPartyPackage '$entry': want <module>=<nuget-id>@<package-version>"
        }
        # "go." is the converted Go standard library, in any letter case (owner ruling, 2026-10-02): the registry's lint
        # exactly (nugetgo cmd/sitegen/parse.go at 90cc7d7409) -- the prefix only, the dot included, after the shape
        # check above -- in the same words as the converter's -nuget-map lint and sourcemeta.CheckModuleNuGetID.
        if ($Matches['id'].ToLowerInvariant().StartsWith('go.', [StringComparison]::Ordinal)) {
            throw "REFUSED: -ThirdPartyPackage '$entry': nuget-id `"$($Matches['id'])`" uses the `"go.`" prefix, which is the converted Go standard library; the ID of a converted module starts with `"nugetgo.`""
        }
        $named[$Matches['module']] = [pscustomobject]@{ NuGetId = $Matches['id']; PackageVersion = $Matches['version'] }
    }

    # The converted dependency modules and their versions, as the conversion recorded them.
    $locked = @{}
    $lockFile = Join-Path $RecurseRoot 'go2cs.modules.lock'
    if (Test-Path -LiteralPath $lockFile) {
        foreach ($line in [System.IO.File]::ReadAllLines($lockFile)) {
            if ($line.Trim() -eq '' -or $line.StartsWith('#')) { continue }
            $fields = $line -split '\s+'
            $locked[$fields[0]] = $fields[1]
        }
    }

    $pkgRoot = Join-Path $RecurseRoot 'pkg'
    $modules = New-Object 'System.Collections.Generic.SortedSet[string]' ([StringComparer]::Ordinal)

    foreach ($library in $Libraries) {
        [xml]$project = Get-Content -Raw -LiteralPath $library.FullName
        foreach ($reference in @($project.Project.ItemGroup | ForEach-Object { $_.ProjectReference } | Where-Object { $_ })) {
            $include = ([string]$reference.Include).Replace('\', [System.IO.Path]::DirectorySeparatorChar).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
            $target = [System.IO.Path]::GetFullPath((Join-Path $library.DirectoryName $include))
            $importPath = Get-RelativeImportPath $pkgRoot (Split-Path $target)
            if (-not $importPath) { continue }   # the module's own packages (src/), or the runtime

            # The longest locked module path covering the import path.
            $module = @($locked.Keys | Where-Object { $importPath -ceq $_ -or $importPath.StartsWith("$_/", [StringComparison]::Ordinal) } |
                Sort-Object Length -Descending)[0]
            if (-not $module) { throw "REFUSED: $($library.Name) references the third-party package $importPath, which no go2cs.modules.lock entry covers ($lockFile)" }
            [void]$modules.Add($module)
        }
    }

    foreach ($module in $modules) {
        if (-not $named.ContainsKey($module)) {
            throw ("REFUSED: the packed packages reference the third-party module $module (converted locally into this root); a packed " +
                "module carries it as a NuGet dependency instead -- pass -ThirdPartyPackage '$module=<nuget-id>@<package-version>' naming its " +
                "published package (owner ruling B4: the first revision built for the same corpus)")
        }
    }

    foreach ($module in $named.Keys) {
        if (-not $modules.Contains($module)) { throw "REFUSED: -ThirdPartyPackage names $module, which no packed package references" }
    }

    foreach ($module in $modules) {
        [pscustomobject]@{ Module = $module; Version = $locked[$module]; NuGetId = $named[$module].NuGetId; PackageVersion = $named[$module].PackageVersion }
    }
}

Export-ModuleMember -Function Get-NugetgoPackedPackages, Get-NugetgoThirdPartyRequires
