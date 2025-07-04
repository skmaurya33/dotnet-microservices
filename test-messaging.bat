@echo off
echo Testing NServiceBus messaging system...
echo.

echo 1. Testing the test endpoint (no auth required)
echo POST http://localhost:5001/api/comment/test-event
curl -X POST http://localhost:5001/api/comment/test-event -H "Content-Type: application/json"

echo.
echo.
echo 2. Check Auth service logs for received events:
echo.
if exist "MsRestApiAuth\Logs\auth-service-*.txt" (
    echo === Auth Service Logs ===
    type "MsRestApiAuth\Logs\auth-service-*.txt"
) else (
    echo No auth service logs found yet
)

echo.
echo.
echo 3. Check Comment service logs for published events:
echo.
if exist "MsRestApiComment\Logs\comment-service-*.txt" (
    echo === Comment Service Logs ===
    type "MsRestApiComment\Logs\comment-service-*.txt"
) else (
    echo No comment service logs found yet
)

echo.
echo Test complete! Check the logs above for success messages.
pause 