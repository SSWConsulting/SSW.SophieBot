. ".\common.ps1"

$currentPath = (Get-Location).Path
Write-Host $solutionPaths

foreach ($solutionPath in $solutionPaths) {    
    $solutionAbsPath = (Join-Path $currentPath $solutionPath)
    Set-Location $solutionAbsPath
    dotnet test --no-build --no-restore --collect:"XPlat Code Coverage"
    if (-Not $?) {
        Write-Host ("Test failed for the solution: " + $solutionPath)
        Set-Location $currentPath
        exit $LASTEXITCODE
    }
}

Set-Location $currentPath