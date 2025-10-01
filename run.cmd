@echo off
REM Build and run Context Menu Editor with elevation
echo Building...
dotnet build
if %ERRORLEVEL% EQU 0 (
    echo Launching with administrator privileges...
    powershell -Command "Start-Process '.\bin\Debug\net8.0-windows\win-x64\ContextMenuEditor.exe' -Verb RunAs"
) else (
    echo Build failed.
    exit /b 1
)
