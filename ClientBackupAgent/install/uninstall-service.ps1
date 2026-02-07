param(
    [string]$PublishDir = "..\\publish-client"
)

$serviceName = "MYMSqlBackup"

Write-Host "Stopping Windows Service $serviceName..." -ForegroundColor Cyan
sc.exe stop $serviceName 2>$null | Out-Null

Start-Sleep -Seconds 3

Write-Host "Deleting Windows Service $serviceName..." -ForegroundColor Cyan
sc.exe delete $serviceName 2>$null | Out-Null

Write-Host "Removing published files at $PublishDir..." -ForegroundColor Cyan
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
}

Write-Host "Uninstall complete." -ForegroundColor Green
