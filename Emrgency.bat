@echo off
start cmd.exe /k "cd /d D:\CODE\C#\EmergencyNotifications\client && npm start"
timeout /t 1 /nobreak
start cmd.exe /k "set ASPNETCORE_ENVIRONMENT=Development && set ASPNETCORE_URLS=http://localhost:5041 && D:\CODE\C#\EmergencyNotifications\API\bin\Debug\net7.0\API.exe"
timeout /t 1 /nobreak
start cmd.exe /k "redis-server"

