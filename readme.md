[한국어](./readme.kr.md)

# Lotto Number Analysis and Suggestion Project

## Overview
An ASP.NET MVC web application that fetches, stores, and analyzes historical Korean lottery (Lotto 6/45) data to generate statistically informed number suggestions.

## Key Features

### Data Management
- Automatically fetches the latest draw results from the official lottery API and saves them to the database.
- Browse full draw history or the most recent 30 draws.

### Analysis Pages
- **Frequency Analysis (`/Analytics`):** Displays 10 suggested sets based on various frequency strategies (all-time top, recent 2-year/1-year/6-month/10-draw, hot+cold mix, cold-only, odd-even balance, high-low balance, weighted random).
- **Recent Analysis (`/AnalyticsRecent`):** Frequency ranking for the most recent 30 draws.

### Weekly Suggested Numbers (`/WeeklySuggestedNumbers`)

#### Range Distribution Table
Shows what percentage of each draw position (1st–6th number, bonus) falls into each number range (1–9, 10–19, 20–29, 30–39, 40–45) over all-time, recent 52, 26, and 10 draws.

#### Frequency-Based Sets (분포 기반 최대 빈도 번호)
- For each of 6 draw positions, finds the number range that trended most in the given window (10 / 26 / 52 draws or all-time).
- Picks the historically most frequent number from that range.
- Parenthetical count = all-time appearance count.

#### Overdue Sets (트렌드 + 오버듀) — *Updated 2026-03-13*
- For each of 6 draw positions, finds the trending range in the given window, then picks the number from that range that has gone **the longest without appearing**.
- **2 sets per trend window (A / B):** Set B excludes all numbers already chosen in Set A, ensuring the two sets are distinct.
- **Historical duplicate guard:** If a generated 6-number combination matches any past winning draw, the least-overdue number in the set is swapped out for the next most-overdue candidate. This repeats until the set is unique in history.
- **Overdue bonus selection:** The bonus number is chosen as the number that has gone the longest without appearing *as a bonus*, independent of the main 6 numbers. (Fixes the bug where the bonus was always the same number due to frequency-only selection.)
- Parenthetical count = draws elapsed since the number last appeared (bonus column = draws since last *bonus* appearance).

## Suggestion Logic Summary

| Column | Logic | Meaning |
|--------|-------|---------|
| Frequency set | Most frequent number in trending range | Statistically strong number |
| Overdue set | Longest absent number in trending range | Number due by appearance cycle |

## Technology Stack
- **Backend:** C#, ASP.NET MVC 5
- **ORM:** Entity Framework
- **Frontend:** Bootstrap, jQuery, DataTables.js
- **External API:** Korean Lottery official draw API (`dhlottery.co.kr`)

## Database Setup (Docker)

The project uses SQL Server 2022 Express running in Docker. A `docker-compose.yml` is included at the project root.

### 1. Start the SQL Server container
```bash
docker-compose up -d
```

This starts a container named `getrich_sqlserver` on port **1433** with the following credentials:

| Item | Value |
|------|-------|
| Host | `localhost,1433` |
| SA Password | `GetRich1234!A` |
| Edition | Express |

Data is persisted in a Docker volume (`sqlserver_data`) so it survives container restarts.

### 2. Create the database and table

Connect via SSMS, Azure Data Studio, or `sqlcmd`, then run:

```sql
CREATE DATABASE GetRich;
GO

USE GetRich;
GO

CREATE TABLE [dbo].[Lotto_History] (
    [ID]                      INT            IDENTITY(1,1) NOT NULL,
    [num1]                    INT            NOT NULL,
    [num2]                    INT            NOT NULL,
    [num3]                    INT            NOT NULL,
    [num4]                    INT            NOT NULL,
    [num5]                    INT            NOT NULL,
    [num6]                    INT            NOT NULL,
    [bonus]                   INT            NOT NULL,
    [seqNo]                   INT            NOT NULL DEFAULT (0),
    [firstPriceTotal]         DECIMAL(18,0)  NULL,
    [eachReceivedFirstPrice]  DECIMAL(18,0)  NULL,
    [firstPriceSelected]      INT            NULL,
    [drawDate]                NVARCHAR(MAX)  NULL,
    CONSTRAINT [PK_dbo.Lotto_History] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO
```

### 3. Connection string (already configured)

`Lotto/Web.config` is pre-configured to connect to the Docker container:
```xml
<add name="GetRichEntities"
     connectionString="Data Source=localhost,1433;Initial Catalog=GetRich;User Id=sa;Password=GetRich1234!A;"
     providerName="System.Data.SqlClient" />
```
No changes are needed if you use the default Docker setup.

### Stop / remove the container
```bash
docker-compose down        # stop (data volume kept)
docker-compose down -v     # stop and delete all data
```

## How to Run
1. Start the database container: `docker-compose up -d` (see above).
2. Open `Lotto.sln` in Visual Studio.
3. Restore NuGet packages.
4. Build the solution (`Build > Build Solution`).
5. Run the project (`Ctrl+F5`).
6. To import draw data, call `POST /LottoMng/GetLatestLottoNumbers` (repeatable — each call fetches the next missing draw).

## Project Structure
```
Lotto/
├── Controllers/
│   └── LottoMngController.cs   # All page and API actions
├── Repository/
│   └── LottoMngRepository.cs   # All data access and suggestion logic
├── Core/
│   └── LottoCore.cs            # Low-level counting and dictionary utilities
├── Models/
│   ├── Lotto_History.cs        # Draw entity
│   └── WeeklySuggestedViewModel.cs
└── Views/LottoMng/
    ├── WeeklySuggestedNumbers.cshtml
    ├── Analytics.cshtml
    ├── History.cshtml
    └── RecentHistory.cshtml
```

## Changelog

### 2026-03-13
- **Overdue sets now generate 2 distinct sets (A/B) per trend window** — Set B excludes Set A numbers.
- **Historical duplicate guard added** — Generated sets are checked against all past winning combinations; duplicates are automatically replaced.
- **Bonus number fixed** — Overdue sets now select the bonus using overdue logic (longest absent as bonus), resolving the issue where the bonus was always the same number.
- **Count display corrected** — Overdue set parenthetical now shows "draws since last appearance" instead of all-time frequency, making the overdue rationale visible.
