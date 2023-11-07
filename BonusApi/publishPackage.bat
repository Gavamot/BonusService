@echo off

set /p nugetPath="Enter nuget file path: "

nuget.exe push %nugetPath% HeBa5Dzyk_tdeDJQr5tn -Source ezs
pause
