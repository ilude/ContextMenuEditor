# Run Context Menu Editor with Administrator privileges
# This script builds and runs the application with UAC elevation

Write-Host "Building Context Menu Editor..." -ForegroundColor Cyan
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Launching with administrator privileges..." -ForegroundColor Green
    
    # Get the executable path
    $exePath = ".\bin\Debug\net8.0-windows\win-x64\ContextMenuEditor.exe"
    
    if (Test-Path $exePath) {
        Start-Process -FilePath $exePath -Verb RunAs
        Write-Host "Application launched." -ForegroundColor Green
    } else {
        Write-Host "Error: Executable not found at $exePath" -ForegroundColor Red
        Write-Host "Try running 'dotnet build' first." -ForegroundColor Yellow
    }
} else {
    Write-Host "Build failed. Please fix errors and try again." -ForegroundColor Red
}
