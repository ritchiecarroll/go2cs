# _nugetpush.ps1 - the release's push to a NuGet feed: one package at a time, resumable and rate-aware.
#
# Dot-sourced by push-nuget.ps1 (-Push) and release-nuget.ps1 (Phase 3). Written for the 1.26.10.1 publish, whose
# Phase 3 died on push 351 of 362: nuget.org caps pushes per API key per window and answered the 351st with
# 403 Quota Exceeded and a Retry-After of 2489 s, and `dotnet nuget push *.nupkg` stopped there. Finishing the
# release took a hand-made list of the 12 IDs the flat container did not have.
#
# What this does instead, per package, in the caller's order:
#   1. ASK THE FEED WHETHER THIS EXACT VERSION IS PUBLISHED, from the flat container:
#      <PackageBaseAddress>/<id>/<version>/<id>.nuspec answers 200 only for a version that exists. It is the only
#      truthful check: the v2 download URL redirects (302) for ANY version, and a gallery page answers 200 for one
#      that was never pushed. A published package is skipped, which is what makes a re-run a resume.
#   2. PUSH IT, as the NuGet client does: a PUT of the .nupkg as multipart form data to the PackagePublish endpoint
#      the feed's service index names, with the trailing slash the client appends (measured: `dotnet nuget push`
#      PUTs <@id>/). Pushing directly, rather than through `dotnet nuget push`, is what makes the Retry-After header
#      readable: the client reports the status line and nothing else.
#   3. ON A REFUSAL THAT NAMES A RETRY-AFTER (403 Quota Exceeded, 429, 503), WAIT IT OUT, printing the time the push
#      resumes, then push the same package again. A 403 WITHOUT one (a bad or scoped-out API key) is final.
#   4. A 409 means the version exists: pushed by an earlier run and not yet in the flat container (nuget.org indexes
#      asynchronously). It counts as published, the same as --skip-duplicate.
# A source that is not http(s) is a folder feed, which has no index, no quota and no flat container; it keeps the
# plain `dotnet nuget push --skip-duplicate`.

Add-Type -AssemblyName System.Net.Http
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

# Windows PowerShell's .NET Framework may not offer TLS 1.2 by default, and nuget.org accepts nothing older.
if ($PSVersionTable.PSEdition -ne 'Core') {
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12
}

# go.lib and go.gen first: every other package depends on them. Last, the go2cs dotnet tool's go2cs package
# (go2cs.<version>.nupkg), after the go2cs.<rid> packages it names. nuget.org indexes asynchronously, so the order is
# a courtesy to anyone restoring or installing mid-publish, not a requirement.
function Get-GoNuGetPushOrder([System.IO.FileInfo[]]$Packages) {
    $first = '^go\.(lib|gen)\.'
    $last = '^go2cs\.\d'
    @($Packages | Where-Object { $_.Name -match $first }) +
        @($Packages | Where-Object { $_.Name -notmatch $first -and $_.Name -notmatch $last }) +
        @($Packages | Where-Object { $_.Name -match $last })
}

# The id and version a .nupkg declares, read from the .nuspec at its root (the file name is a convention, the
# nuspec is the package).
function Get-GoNuGetPackageIdentity([string]$Path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)

    try {
        $entry = @($zip.Entries | Where-Object { $_.FullName -notmatch '/' -and $_.Name -like '*.nuspec' }) | Select-Object -First 1
        if (-not $entry) { throw "No .nuspec at the root of $Path" }

        $reader = New-Object System.IO.StreamReader($entry.Open())
        try { $xml = [xml]$reader.ReadToEnd() } finally { $reader.Dispose() }
    }
    finally {
        $zip.Dispose()
    }

    $meta = $xml.package.metadata
    [pscustomobject]@{ Id = "$($meta.id)".Trim(); Version = "$($meta.version)".Trim() }
}

# Is the package a -Cgo pack (src/tools/nugetgo/nugetgo-pack.ps1 -Cgo)? Two keys, either one enough (P3's INC 2b read,
# NIT 4), so a change to one cannot let a -Cgo pack through:
#   - its build/ or buildTransitive/<id>.targets carries the RID guard's error code, NUGETGOCGO001 (NugetgoCgo.psm1);
#   - its lib/ holds a REFERENCE assembly while it carries runtimes/ assemblies: the -Cgo layout, where the go.* and
#     every other nugetgo RID-specific package put an implementation in lib/. Read as the attribute's name in the
#     assembly's metadata strings, since Windows PowerShell 5.1 (the release's push) has no System.Reflection.Metadata.
function Test-GoNuGetCgoPackage([string]$Path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)

    try {
        foreach ($entry in @($zip.Entries | Where-Object { $_.FullName -match '^build(Transitive)?/[^/]+\.targets$' })) {
            $reader = New-Object System.IO.StreamReader($entry.Open())
            try { if ($reader.ReadToEnd().Contains('NUGETGOCGO001')) { return $true } } finally { $reader.Dispose() }
        }

        if (@($zip.Entries | Where-Object { $_.FullName -match '^runtimes/[^/]+/lib/[^/]+/[^/]+\.dll$' }).Count) {
            foreach ($entry in @($zip.Entries | Where-Object { $_.FullName -match '^lib/[^/]+/[^/]+\.dll$' })) {
                $buffer = New-Object System.IO.MemoryStream
                $stream = $entry.Open()
                try { $stream.CopyTo($buffer) } finally { $stream.Dispose() }
                if ([System.Text.Encoding]::ASCII.GetString($buffer.ToArray()).Contains('ReferenceAssemblyAttribute')) { return $true }
            }
        }
    }
    finally {
        $zip.Dispose()
    }

    $false
}

# Does the package's go2cs/source-metadata.txt record A6 ("# a6: ", written by nugetgo-pack.ps1 -Cgo over a go.lib that
# declares GoCgoRunTimeTargetCheck)?
function Test-GoNuGetCgoA6Note([string]$Path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)

    try {
        $entry = $zip.GetEntry('go2cs/source-metadata.txt')
        if (-not $entry) { return $false }
        $reader = New-Object System.IO.StreamReader($entry.Open())
        try { return ("`n" + $reader.ReadToEnd()).Contains("`n# a6: ") } finally { $reader.Dispose() }
    }
    finally {
        $zip.Dispose()
    }
}

# NuGet's normalized version, the form the flat container keys a version by: build metadata dropped, numeric parts
# without leading zeros, at least three of them, a fourth part of 0 dropped, and lower case. 1.26.10.0 is 1.26.10.
function ConvertTo-GoNuGetNormalizedVersion([string]$Version) {
    $release, $prerelease = (($Version -split '\+', 2)[0]) -split '-', 2
    $parts = @($release.Split('.') | ForEach-Object { [string][long]$_ })

    while ($parts.Count -lt 3) { $parts += '0' }
    if ($parts.Count -eq 4 -and $parts[3] -eq '0') { $parts = $parts[0..2] }

    $normalized = $parts -join '.'
    if ($prerelease) { $normalized += "-$prerelease" }

    $normalized.ToLowerInvariant()
}

# The two endpoints a push needs, from the feed's v3 service index.
function Get-GoNuGetServiceEndpoints([System.Net.Http.HttpClient]$Client, [string]$Source) {
    $index = $Client.GetStringAsync($Source).GetAwaiter().GetResult() | ConvertFrom-Json

    $publish = @($index.resources | Where-Object { "$($_.'@type')" -like 'PackagePublish/*' }) | Select-Object -First 1
    $base = @($index.resources | Where-Object { "$($_.'@type')" -eq 'PackageBaseAddress/3.0.0' }) | Select-Object -First 1

    if (-not $publish) { throw "The service index at $Source names no PackagePublish endpoint" }
    if (-not $base) { throw "The service index at $Source names no PackageBaseAddress (flat container), so published versions cannot be checked" }

    [pscustomobject]@{ Publish = "$($publish.'@id')".TrimEnd('/') + '/'; FlatContainer = "$($base.'@id')".TrimEnd('/') + '/' }
}

# Whether this exact id and version is on the feed: the flat container's .nuspec answers 200 or 404, and anything
# else is not an answer.
function Test-GoNuGetPackagePublished([System.Net.Http.HttpClient]$Client, [string]$FlatContainer, [string]$Id, [string]$Version) {
    $id = $Id.ToLowerInvariant()
    $url = "$FlatContainer$id/$(ConvertTo-GoNuGetNormalizedVersion $Version)/$id.nuspec"
    $response = $Client.GetAsync($url).GetAwaiter().GetResult()

    try {
        $status = [int]$response.StatusCode
    }
    finally {
        $response.Dispose()
    }

    if ($status -eq 200) { return $true }
    if ($status -eq 404) { return $false }

    throw "Cannot tell whether $Id $Version is published: $url answered $status"
}

# One PUT of one package. Returns the status, the reason phrase, the body, and the Retry-After in seconds when the
# response carried one (as a header, or failing that as a "retry after N" in the body).
function Send-GoNuGetPackage([System.Net.Http.HttpClient]$Client, [string]$PublishUrl, [string]$ApiKey, [string]$Path) {
    $stream = [System.IO.File]::OpenRead($Path)
    $content = New-Object System.Net.Http.MultipartFormDataContent

    try {
        $file = New-Object System.Net.Http.StreamContent($stream)
        $file.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue('application/octet-stream')
        $content.Add($file, 'package', 'package.nupkg')

        $request = New-Object System.Net.Http.HttpRequestMessage([System.Net.Http.HttpMethod]::Put, $PublishUrl)
        $request.Headers.Add('X-NuGet-ApiKey', $ApiKey)
        $request.Headers.Add('X-NuGet-Protocol-Version', '4.1.0')
        $request.Content = $content

        $response = $Client.SendAsync($request).GetAwaiter().GetResult()

        try {
            $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $retryAfter = $null

            if ($response.Headers.RetryAfter) {
                $delta = $response.Headers.RetryAfter.Delta
                $date = $response.Headers.RetryAfter.Date

                if ($null -ne $delta) { $retryAfter = [math]::Ceiling($delta.TotalSeconds) }
                elseif ($null -ne $date) { $retryAfter = [math]::Max(0, [math]::Ceiling(($date - [DateTimeOffset]::UtcNow).TotalSeconds)) }
            }

            if ($null -eq $retryAfter -and $body -match '(?i)retry[- ]after\D{0,16}(\d+)') { $retryAfter = [int]$Matches[1] }

            [pscustomobject]@{ Status = [int]$response.StatusCode; Reason = "$($response.ReasonPhrase)"; Body = $body; RetryAfterSeconds = $retryAfter }
        }
        finally {
            $response.Dispose()
        }
    }
    finally {
        $content.Dispose()
        $stream.Dispose()
    }
}

# THE CALL, for any script that publishes go.* packages (release-nuget.ps1 Phase 3, push-nuget.ps1 -Push, an owner's
# sheet). Dot-source this file, then:
#
#   $result = Invoke-GoNuGetPush -Packages <FileInfo[]> -Source <v3 index URL or folder> -ApiKey <key> [-StopOnRateLimit]
#
# It pushes $Packages in the order given (Get-GoNuGetPushOrder puts go.lib and go.gen first) and returns
# [pscustomobject]@{ Pushed; AlreadyPublished; Waits }. THE RESUME ENTRY POINT IS THE SAME CALL: a re-run skips every
# package the feed already has, so it carries on where the last run stopped.
#
# A refusal with a Retry-After is waited out by default. With -StopOnRateLimit it is not: the call throws at once,
# naming the package, the counts so far and the UTC time to re-run, for a caller that would rather stop than block.
# Every other refusal throws the same way, naming the package and how many were pushed before it.
# $Sleep is how a wait is spent; the selftest passes one that records instead of sleeping.
function Invoke-GoNuGetPush {
    param(
        [Parameter(Mandatory)] [System.IO.FileInfo[]] $Packages,
        [Parameter(Mandatory)] [string] $Source,
        [Parameter(Mandatory)] [string] $ApiKey,
        [scriptblock] $Sleep = { param([int]$Seconds) Start-Sleep -Seconds $Seconds },
        [int] $MaxWaitsPerPackage = 6,
        [switch] $StopOnRateLimit
    )

    # No -Cgo pack is pushed to any feed until the run-time RID check (A6, cgo-against-go.* INC 3) lands (COORD's ruling
    # (a), condition 3). The whole set is refused before the first push, so a refusal never leaves a half-pushed release.
    # A -Cgo pack is pushed only when its self-description records that its go.lib floor carries the run-time target check
    # (A6; COORD's push-lift ruling (a)): nugetgo-pack.ps1 -Cgo writes "# a6: " only over a go.lib that declares it.
    $cgoPacks = @($Packages | Where-Object { (Test-GoNuGetCgoPackage $_.FullName) -and -not (Test-GoNuGetCgoA6Note $_.FullName) } | ForEach-Object { $_.Name })
    if ($cgoPacks.Count) { throw "REFUSED: $($cgoPacks -join ', ') $(if ($cgoPacks.Count -eq 1) { 'is a -Cgo pack' } else { 'are -Cgo packs' }) (its build/<id>.targets carries NUGETGOCGO001, or its lib/ holds reference assemblies beside runtimes/) whose go2cs/source-metadata.txt does not record the run-time target check (A6: no `"# a6: `" line), so its go.lib floor may not refuse its bindings at load; nothing was pushed" }

    $pushed = 0
    $published = 0
    $waits = 0
    $total = $Packages.Count

    if ($Source -notmatch '^(?i)https?://') {
        foreach ($package in $Packages) {
            & dotnet nuget push $package.FullName --source $Source --api-key $ApiKey --skip-duplicate
            if ($LASTEXITCODE -ne 0) { throw "push failed for $($package.Name) ($LASTEXITCODE) after $pushed of $total pushed" }
            $pushed++
        }

        return [pscustomobject]@{ Pushed = $pushed; AlreadyPublished = 0; Waits = 0 }
    }

    $client = New-Object System.Net.Http.HttpClient
    $client.Timeout = [TimeSpan]::FromMinutes(10)

    try {
        $endpoints = Get-GoNuGetServiceEndpoints $client $Source
        $n = 0

        foreach ($package in $Packages) {
            $n++
            $identity = Get-GoNuGetPackageIdentity $package.FullName
            $label = "[$n/$total] $($identity.Id) $($identity.Version)"

            if (Test-GoNuGetPackagePublished $client $endpoints.FlatContainer $identity.Id $identity.Version) {
                Write-Host "  $label  already published -- skipped"
                $published++
                continue
            }

            $waited = 0

            while ($true) {
                $result = Send-GoNuGetPackage $client $endpoints.Publish $ApiKey $package.FullName

                if ($result.Status -ge 200 -and $result.Status -lt 300) {
                    Write-Host "  $label  pushed"
                    $pushed++
                    break
                }

                if ($result.Status -eq 409) {
                    Write-Host "  $label  already on the feed (409) -- skipped"
                    $published++
                    break
                }

                if (($result.Status -in @(403, 429, 503)) -and ($null -ne $result.RetryAfterSeconds) -and ($waited -lt $MaxWaitsPerPackage)) {
                    $seconds = [int][math]::Max(1, $result.RetryAfterSeconds)
                    $resume = [DateTime]::UtcNow.AddSeconds($seconds).ToString('yyyy-MM-dd HH:mm:ss')

                    if ($StopOnRateLimit) {
                        throw ("push of $($identity.Id) $($identity.Version) rate-limited ($($result.Status) $($result.Reason), Retry-After $seconds s) " +
                            "after $pushed pushed and $published already published of $total; stopped. Resume at $resume UTC by re-running " +
                            "the same push (published packages are skipped)")
                    }

                    Write-Host ("  $label  refused: $($result.Status) $($result.Reason), Retry-After $seconds s, after $pushed pushed this run. " +
                        "Waiting; the push resumes at $resume UTC. (Stopping now is safe: a re-run skips what is published.)") -ForegroundColor Yellow
                    & $Sleep $seconds
                    $waits++
                    $waited++
                    continue
                }

                $detail = "$($result.Status) $($result.Reason)"
                if ($null -ne $result.RetryAfterSeconds) { $detail += ", Retry-After $($result.RetryAfterSeconds) s after $waited wait(s) on this package" }
                if ($result.Body) { $detail += ": $($result.Body.Trim())" }

                throw "push of $($identity.Id) $($identity.Version) refused ($detail) after $pushed pushed and $published already published of $total; re-run to resume (published packages are skipped)"
            }
        }
    }
    finally {
        $client.Dispose()
    }

    [pscustomobject]@{ Pushed = $pushed; AlreadyPublished = $published; Waits = $waits }
}
