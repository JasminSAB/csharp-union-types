# Proves the compile-time claims from the post by building the project four ways
# and reading the real compiler output. Run: pwsh ./verify.ps1  (or powershell -File verify.ps1)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$failed = 0

function Build([string[]] $properties) {
    $output = & dotnet build -c Release --no-incremental @properties 2>&1 | Out-String
    return @{ Output = $output; Succeeded = ($LASTEXITCODE -eq 0) }
}

function Check([string] $claim, [bool] $ok, [string] $detail) {
    $label = if ($ok) { 'PASS' } else { 'FAIL' }
    Write-Host "[$label] $claim"
    Write-Host "       $detail"
    if (-not $ok) { $script:failed++ }
}

function FirstDiagnostic([string] $output, [string] $id) {
    $line = ($output -split "`n" | Where-Object { $_ -match $id } | Select-Object -First 1)
    if ($line) { return ($line -replace '^.*?(warning|error) ', '$1 ' -replace '\s*\[.*$', '').Trim() }
    return "no $id in the build output"
}

Write-Host "SDK $(dotnet --version)"
Write-Host ''

# 1. Three cases, exhaustive switch, no default arm: clean build.
$b = Build @()
$warnings = ([regex]::Matches($b.Output, 'warning CS\d+') | ForEach-Object Value | Sort-Object -Unique) -join ', '
Check 'Three cases and an exhaustive switch build without warnings' ($b.Succeeded -and -not $warnings) ($(if ($warnings) { "warnings: $warnings" } else { 'build succeeded, 0 warnings' }))

# 2. A fourth case: the compiler names the switch and the missing case.
$b = Build @('-p:GiftCard=true')
$ok = $b.Succeeded -and $b.Output -match "warning CS8509.*GiftCard' is not covered"
Check "Adding GiftCard produces CS8509 naming the missing case" $ok (FirstDiagnostic $b.Output 'CS8509')

# 3. The same fourth case with a discard arm: the warning is gone, the bug is silent.
$b = Build @('-p:GiftCard=true', '-p:DiscardArm=true')
$ok = $b.Succeeded -and $b.Output -notmatch 'CS8509'
Check 'A discard arm hides the missing GiftCard case (no CS8509)' $ok ($(if ($ok) { 'build succeeded, no CS8509' } else { FirstDiagnostic $b.Output 'CS8509' }))

# 4. Assigning a type that is not a case of the union does not compile.
$b = Build @('-p:WrongType=true')
$ok = (-not $b.Succeeded) -and $b.Output -match 'error CS0029'
Check 'Assigning a string to PaymentMethod is a compile error' $ok (FirstDiagnostic $b.Output 'error CS')

# 5. Run the run-time checks from Program.cs on a clean build.
Build @() | Out-Null
Write-Host ''
& dotnet run -c Release --no-build
if ($LASTEXITCODE -ne 0) { $failed++ }

Write-Host ''
if ($failed -eq 0) { Write-Host 'verify.ps1: all compile-time and run-time checks passed.'; exit 0 }
Write-Host "verify.ps1: $failed check(s) failed."; exit 1
