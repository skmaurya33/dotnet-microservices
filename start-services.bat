@echo off
echo ========================================
echo Starting .NET Microservices
echo ========================================
echo.

echo Creating new terminal windows...
echo.

echo Starting Auth Service (Port 5286)...
start "Auth Service" cmd /k "cd MsRestApiAuth && dotnet run"

echo Starting Comment Service (Port 5188)...
start "Comment Service" cmd /k "cd MsRestApiComment && dotnet run"

echo.
echo ========================================
echo Services are starting in separate windows
echo ========================================
echo.
echo Auth Service: http://localhost:5286
echo Comment Service: http://localhost:5188
echo.
echo ========================================
echo Log Locations:
echo ========================================
echo Console: Check terminal windows
echo Files: 
echo   - MsRestApiAuth\Logs\auth-service-*.txt
echo   - MsRestApiComment\Logs\comment-service-*.txt
echo.
echo ========================================
echo Test Endpoint:
echo ========================================
echo POST http://localhost:5188/api/comment/test-event
echo.
echo Press any key to view log files...
pause > nul

echo.
echo ========================================
echo Current Log Files:
echo ========================================
dir /B MsRestApiAuth\Logs\*.txt 2>nul
dir /B MsRestApiComment\Logs\*.txt 2>nul
echo.
echo To view logs in real-time:
echo   tail -f MsRestApiAuth\Logs\auth-service-*.txt
echo   tail -f MsRestApiComment\Logs\comment-service-*.txt
echo.
pause 