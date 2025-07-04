@echo off
echo ========================================
echo .NET Microservices Log Viewer
echo ========================================
echo.

:menu
echo Choose an option:
echo 1. View Auth Service logs
echo 2. View Comment Service logs
echo 3. View both services logs
echo 4. View latest log entries (tail)
echo 5. List all log files
echo 6. Exit
echo.
set /p choice="Enter your choice (1-6): "

if "%choice%"=="1" goto auth_logs
if "%choice%"=="2" goto comment_logs
if "%choice%"=="3" goto both_logs
if "%choice%"=="4" goto tail_logs
if "%choice%"=="5" goto list_logs
if "%choice%"=="6" goto exit

echo Invalid choice. Please try again.
goto menu

:auth_logs
echo.
echo ========================================
echo Auth Service Logs
echo ========================================
type MsRestApiAuth\Logs\*.txt 2>nul
if errorlevel 1 echo No log files found in MsRestApiAuth\Logs\
goto menu

:comment_logs
echo.
echo ========================================
echo Comment Service Logs
echo ========================================
type MsRestApiComment\Logs\*.txt 2>nul
if errorlevel 1 echo No log files found in MsRestApiComment\Logs\
goto menu

:both_logs
echo.
echo ========================================
echo All Service Logs
echo ========================================
echo --- Auth Service ---
type MsRestApiAuth\Logs\*.txt 2>nul
if errorlevel 1 echo No Auth Service logs found
echo.
echo --- Comment Service ---
type MsRestApiComment\Logs\*.txt 2>nul
if errorlevel 1 echo No Comment Service logs found
goto menu

:tail_logs
echo.
echo ========================================
echo Latest Log Entries (Press Ctrl+C to stop)
echo ========================================
powershell -command "Get-Content MsRestApiAuth\Logs\*.txt,MsRestApiComment\Logs\*.txt -Wait -Tail 10"
goto menu

:list_logs
echo.
echo ========================================
echo Available Log Files
echo ========================================
echo Auth Service logs:
dir /B MsRestApiAuth\Logs\*.txt 2>nul
if errorlevel 1 echo No Auth Service logs found
echo.
echo Comment Service logs:
dir /B MsRestApiComment\Logs\*.txt 2>nul
if errorlevel 1 echo No Comment Service logs found
goto menu

:exit
echo.
echo Goodbye!
pause 