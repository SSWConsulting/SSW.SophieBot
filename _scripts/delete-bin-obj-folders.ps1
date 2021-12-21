$currentPath = (Get-Location).Path

Set-Location ..\

Write-host "Removing all bin and obj folders..."
Get-Childitem -Path obj,bin -Directory -Recurse | Where {$_.Fullname -notlike "*\node_modules\*"} | Remove-Item -Recurse
Write-host "Removing complete."

Set-Location $currentPath