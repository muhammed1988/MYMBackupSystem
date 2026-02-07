param(
    [string]$PublishDir = "..\\bin\\Release\\net10.0\\publish"
)

Write-Host "Publishing ClientBackupAgent..." -ForegroundColor Cyan
dotnet publish ..\\ClientBackupAgent\\ClientBackupAgent.csproj -c Release -o $PublishDir

$serviceName = "MYMSqlBackup"
$displayName = "MYM SQL Backup Service"
$description = "Automated SQL Server backup and upload agent"

$exePath = (Resolve-Path (Join-Path $PublishDir "ClientBackupAgent.exe")).Path

Write-Host "Installing Windows Service $serviceName..." -ForegroundColor Cyan

sc.exe stop $serviceName 2>$null | Out-Null
sc.exe delete $serviceName 2>$null | Out-Null

sc.exe create $serviceName binPath= "`"$exePath`"" start= auto DisplayName= "$displayName"
sc.exe description $serviceName "$description"

sc.exe start $serviceName

Write-Host "Service $serviceName installed and started." -ForegroundColor Green
