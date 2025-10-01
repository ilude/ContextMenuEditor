# Quick run - just launch the already-built executable with elevation
# Use this if you've already built the app and just want to run it

$exePath = ".\bin\Debug\net8.0-windows\win-x64\ContextMenuEditor.exe"

if (Test-Path $exePath) {
    Write-Host "Launching Context Menu Editor with administrator privileges..." -ForegroundColor Cyan
    Start-Process -FilePath $exePath -Verb RunAs
} else {
    Write-Host "Error: Executable not found." -ForegroundColor Red
    Write-Host "Run './run-elevated.ps1' to build and run." -ForegroundColor Yellow
}
