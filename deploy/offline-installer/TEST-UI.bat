@echo off
chcp 65001 >nul 2>&1
title IRM v1.0.2 — Test UI

:: Xin quyen Admin
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Dang xin quyen Administrator...
    powershell -Command "Start-Process cmd -ArgumentList '/c cd /d \"%~dp0\" && \"%~f0\"' -Verb RunAs"
    exit /b
)

echo Dang khoi dong giao dien...
powershell -STA -NoProfile -ExecutionPolicy Bypass -File "%~dp0test-ui.ps1"
if %errorlevel% neq 0 (
    echo.
    echo LOI: Script gap loi. Nhan phim bat ky de xem chi tiet.
    pause
)
