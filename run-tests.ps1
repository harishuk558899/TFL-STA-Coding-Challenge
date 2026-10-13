param(
    [string]$TestFilter = '',
    [string]$ReportDirectory = 'TestResults'
)

$ErrorActionPreference = 'Stop'

$scriptRoot = $PSScriptRoot
$playwrightScript = Join-Path $scriptRoot 'bin/Debug/net8.0/playwright.ps1'
$reportPath = [System.IO.Path]::GetFullPath((Join-Path $scriptRoot $ReportDirectory))
$trxFileName = 'journey-tests.trx'
$trxPath = Join-Path $reportPath $trxFileName
$htmlPath = Join-Path $reportPath 'journey-tests.html'
$dotnetExecutable = Get-Command dotnet -ErrorAction Stop
$testExitCode = 1

function Get-PreferredPowerShell {
    $candidates = @(
        (Get-Command pwsh -ErrorAction SilentlyContinue),
        (Get-Command powershell -ErrorAction SilentlyContinue),
        (Get-Item '/usr/local/microsoft/powershell/7-preview/pwsh' -ErrorAction SilentlyContinue),
        (Get-Item '/opt/homebrew/bin/pwsh' -ErrorAction SilentlyContinue),
        (Get-Item '/usr/local/bin/pwsh' -ErrorAction SilentlyContinue),
        (Get-Item '/opt/homebrew/bin/powershell' -ErrorAction SilentlyContinue),
        (Get-Item '/usr/local/bin/powershell' -ErrorAction SilentlyContinue)
    ) | Where-Object { $_ -ne $null }

    if (-not $candidates) {
        throw 'PowerShell is required to install the Chromium browser for Playwright.'
    }

    $first = $candidates[0]
    if ($first -is [System.Management.Automation.CommandInfo]) {
        return $first.Source
    }

    return $first.FullName
}

function ConvertTo-HtmlText([string]$Value) {
    return [System.Net.WebUtility]::HtmlEncode($Value)
}

Push-Location $scriptRoot
try {
    $env:HEADLESS = 'true'
    New-Item -ItemType Directory -Path $reportPath -Force | Out-Null

    Write-Host '[setup] Restoring packages'
    & $dotnetExecutable.Source restore -p:NuGetAudit=false
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }

    Write-Host '[setup] Building test project'
    & $dotnetExecutable.Source build --no-restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }

    if (Test-Path $playwrightScript) {
        Write-Host '[setup] Ensuring Playwright Chromium is installed'
        $psExecutable = Get-PreferredPowerShell
        & $psExecutable $playwrightScript install chromium
        if ($LASTEXITCODE -ne 0) { throw "Playwright browser installation failed with exit code $LASTEXITCODE" }
    }

    $testArguments = @(
        'test',
        '--no-build',
        '--no-restore',
        '--logger', "trx;LogFileName=$trxFileName",
        '--verbosity', 'normal',
        '--results-directory', $reportPath
    )
    if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
        $testArguments += @('--filter', $TestFilter)
    }

    Write-Host "[test] Running headless tests. Filter: $(if ($TestFilter) { $TestFilter } else { 'all tests' })"
    & $dotnetExecutable.Source @testArguments
    $testExitCode = $LASTEXITCODE

    if (-not (Test-Path $trxPath)) {
        throw "The test run did not produce the expected TRX file: $trxPath"
    }

    [xml]$trx = Get-Content -Path $trxPath -Raw
    $testRows = @(
        foreach ($result in $trx.SelectNodes("//*[local-name()='UnitTestResult']")) {
            $duration = $result.GetAttribute('duration')
            try { $duration = ([TimeSpan]::Parse($duration)).ToString('c') } catch { }
            $message = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='Message']")
            $stackTrace = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='StackTrace']")
            $failure = @($message.InnerText, $stackTrace.InnerText | Where-Object { $_ }) -join "`n"
            [pscustomobject]@{
                Name = $result.GetAttribute('testName')
                Outcome = $result.GetAttribute('outcome')
                Duration = $duration
                Error = $failure
            }
        }
    )

    $passed = @($testRows | Where-Object Outcome -eq 'Passed').Count
    $failed = @($testRows | Where-Object Outcome -eq 'Failed').Count
    $skipped = @($testRows | Where-Object { $_.Outcome -notin @('Passed', 'Failed') }).Count
    $statusClass = if ($failed -eq 0) { 'passed' } else { 'failed' }
    $rowHtml = foreach ($test in $testRows) {
        $outcomeClass = if ($test.Outcome -eq 'Passed') { 'passed' } elseif ($test.Outcome -eq 'Failed') { 'failed' } else { 'skipped' }
        $errorHtml = if ($test.Error) { "<details><summary>Failure details</summary><pre>$(ConvertTo-HtmlText $test.Error)</pre></details>" } else { '' }
        "<tr><td>$(ConvertTo-HtmlText $test.Name)</td><td class='$outcomeClass'>$(ConvertTo-HtmlText $test.Outcome)</td><td>$(ConvertTo-HtmlText $test.Duration)</td><td>$errorHtml</td></tr>"
    }

    $html = @"
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>TfL Journey Planner Test Report</title>
  <style>
    body { color: #202124; font: 15px/1.5 -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; margin: 32px auto; max-width: 1100px; padding: 0 20px; }
    h1 { margin-bottom: 4px; }
    .muted { color: #5f6368; }
    .summary { display: flex; gap: 12px; margin: 24px 0; }
    .metric { border: 1px solid #dadce0; border-radius: 6px; min-width: 110px; padding: 12px 16px; }
    .metric strong { display: block; font-size: 24px; }
    .passed { color: #137333; }
    .failed { color: #b3261e; }
    .skipped { color: #8a5a00; }
    table { border-collapse: collapse; width: 100%; }
    th, td { border-bottom: 1px solid #dadce0; padding: 10px; text-align: left; vertical-align: top; }
    th { background: #f8f9fa; }
    pre { max-height: 280px; overflow: auto; white-space: pre-wrap; }
    @media (max-width: 640px) { .summary { flex-wrap: wrap; } table { font-size: 13px; } th, td { padding: 7px; } }
  </style>
</head>
<body>
  <h1>TfL Journey Planner Test Report</h1>
  <p class="muted">Generated $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz') · Headless Chromium · Filter: $(ConvertTo-HtmlText $(if ($TestFilter) { $TestFilter } else { 'All tests' }))</p>
  <div class="summary">
    <div class="metric"><strong>$($testRows.Count)</strong>Total</div>
    <div class="metric passed"><strong>$passed</strong>Passed</div>
    <div class="metric failed"><strong>$failed</strong>Failed</div>
    <div class="metric skipped"><strong>$skipped</strong>Skipped/other</div>
  </div>
  <p class="$statusClass">Run status: $(if ($failed -eq 0) { 'Passed' } else { 'Failed' })</p>
  <table>
    <thead><tr><th>Test</th><th>Outcome</th><th>Duration</th><th>Details</th></tr></thead>
    <tbody>$($rowHtml -join [Environment]::NewLine)</tbody>
  </table>
  <p class="muted">Raw TRX: $(ConvertTo-HtmlText $trxPath)</p>
</body>
</html>
"@
    Set-Content -Path $htmlPath -Value $html -Encoding utf8
    Write-Host "[report] HTML: $htmlPath"
    Write-Host "[report] TRX:  $trxPath"
}
finally {
    Pop-Location
}

exit $testExitCode
