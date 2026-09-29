@echo off
chcp 65001 >nul 2>&1
title IRM v1.0.2 - Cai dat

:: Xin quyen Admin
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Dang xin quyen Administrator...
    powershell -Command "Start-Process cmd -ArgumentList '/c cd /d \"%~dp0\" && \"%~f0\"' -Verb RunAs"
    exit /b
)

echo.
echo   IRM v1.0.2 - Dang khoi dong trinh cai dat...
echo.

powershell -STA -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-irm-ui.ps1"

if %errorlevel% neq 0 (
    echo.
    echo   Neu giao dien khong hien, ban co the chay:
    echo   powershell -STA -ExecutionPolicy Bypass -File install-irm-ui.ps1
    echo.
    pause
)
