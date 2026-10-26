# _nugetpush.ps1 - RED: the release's push as it ships at d269ef477b, extracted from push-nuget.ps1's -Push loop
# (release-nuget.ps1's Phase 3 is the same `dotnet nuget push --skip-duplicate`, over a glob): one push per
# package, stopping at the first refusal. src/tests/nuget-push-selftest.ps1 runs it against a mock feed and
# fails: it PUTs packages already published, a 403 with Retry-After ends the run, and the error does not say
# how far it got. The next commit replaces this file with the resumable, rate-aware push.
# MEASUREMENT ONLY: a temporary NuGet.Config marks the mock's http source allowInsecureConnections, which
# today's SDK requires before it will push to http at all; nothing else differs from the shipped loop.
function Invoke-GoNuGetPush {
    param([System.IO.FileInfo[]] $Packages, [string] $Source, [string] $ApiKey, [scriptblock] $Sleep)
    $config = Join-Path ([System.IO.Path]::GetTempPath()) ("nuget-red-" + [guid]::NewGuid().ToString('N') + '.config')
    Set-Content -Path $config -Value "<?xml version=`"1.0`" encoding=`"utf-8`"?><configuration><packageSources><add key=`"mock`" value=`"$Source`" allowInsecureConnections=`"true`" /></packageSources></configuration>"
    $pushed = 0
    try {
        foreach ($p in $Packages) {
            & dotnet nuget push $p.FullName --source $Source --api-key $ApiKey --skip-duplicate --configfile $config 2>&1 | ForEach-Object { [Console]::Error.WriteLine("DOTNET: $_") }
            if ($LASTEXITCODE -ne 0) { throw "push failed for $($p.Name) ($LASTEXITCODE)" }
            $pushed++
        }
    } finally { Remove-Item $config -ErrorAction SilentlyContinue }
}
