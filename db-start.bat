@echo off
echo [1/3] SQL Server 컨테이너 시작 중...
docker-compose up -d

echo [2/3] SQL Server 초기화 대기 중 (20초)...
timeout /t 20 /nobreak

echo [3/3] GetRich 데이터베이스 생성 중...
docker exec getrich_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "GetRich1234!A" -No -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'GetRich') CREATE DATABASE GetRich"

echo.
echo 완료! Visual Studio에서 프로젝트를 실행하세요.
pause
