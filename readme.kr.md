[English](./readme.md)

# 로또 번호 분석 및 추천 프로젝트

## 개요
한국 로또 6/45 역대 당첨 데이터를 수집·저장하고, 통계 기반으로 번호를 추천하는 ASP.NET MVC 웹 애플리케이션입니다.

## 주요 기능

### 데이터 관리
- 공식 복권 API에서 최신 회차 결과를 자동으로 가져와 DB에 저장합니다.
- 전체 당첨 이력 또는 최근 30회차 이력을 조회할 수 있습니다.

### 분석 페이지
- **빈도 분석 (`/Analytics`):** 전략별 10세트 추천 번호를 표시합니다 (역대 전체 상위, 최근 2년/1년/반년/10회, Hot+Cold 혼합, Cold 역발상, 홀짝 균형, 고저 균형, 빈도 가중 랜덤).
- **최근 분석 (`/AnalyticsRecent`):** 최근 30회차 기준 번호별 출현 빈도 순위.

### 주간 추천 번호 (`/WeeklySuggestedNumbers`)

#### 구간별 자리 분포 비교 테이블
각 당첨 자리(1~6번째, 보너스)에서 번호 구간(1–9, 10–19, 20–29, 30–39, 40–45)이 차지하는 비율을 역대 전체 / 최근 52·26·10회 기준으로 표시합니다.

#### 분포 기반 최대 빈도 번호 세트
- 6개 자리 각각에서 지정 기간(10 / 26 / 52회 또는 역대 전체) 내 가장 많이 나온 구간을 찾습니다.
- 그 구간에서 역대 가장 자주 출현한 번호를 선택합니다.
- 괄호 안 숫자 = 역대 전체 출현 횟수.

#### 트렌드 + 오버듀 세트 — *2026-03-13 업데이트*
- 6개 자리 각각에서 지정 기간 내 트렌딩 구간을 찾은 뒤, 그 구간에서 **가장 오랫동안 나오지 않은 번호**를 선택합니다.
- **트렌드 창(10 / 26 / 52회 / 역대 전체)당 A/B 2세트 생성:** B세트는 A세트 번호를 모두 제외하여 서로 다른 번호 조합이 나옵니다.
- **역대 당첨 조합 중복 방지:** 생성된 6개 번호 조합이 역대 당첨 이력과 동일할 경우, 세트 내에서 가장 덜 오버듀한 번호(가장 최근에 나온 번호)를 다음 오버듀 번호로 자동 교체합니다. 고유한 조합이 될 때까지 반복합니다.
- **오버듀 기반 보너스 선택:** 보너스 번호는 "보너스로서 가장 오래 나오지 않은 번호"를 기준으로 선택합니다. (기존에 빈도 기반으로 선택하여 항상 같은 번호가 나오던 버그 수정)
- 괄호 안 숫자 = 마지막 출현 이후 경과 회차 수 (보너스 열은 보너스로 마지막 출현 이후 경과 회차 수).

## 추천 로직 요약

| 번호 선택 | 로직 | 의미 |
|-----------|------|------|
| 빈도 세트 | 트렌딩 구간 내 역대 가장 많이 나온 번호 | 검증된 강한 번호 |
| 오버듀 세트 | 트렌딩 구간 내 가장 오랫동안 안 나온 번호 | 출현 주기상 이제 나올 때가 된 번호 |

## 기술 스택
- **백엔드:** C#, ASP.NET MVC 5
- **ORM:** Entity Framework
- **프론트엔드:** Bootstrap, jQuery, DataTables.js
- **외부 API:** 동행복권 공식 당첨 결과 API (`dhlottery.co.kr`)

## DB 설정 (Docker)

프로젝트 루트에 `docker-compose.yml`이 포함되어 있으며, SQL Server 2022 Express를 Docker로 실행합니다.

### 1. SQL Server 컨테이너 시작
```bash
docker-compose up -d
```

컨테이너 이름 `getrich_sqlserver`, 포트 **1433**으로 실행됩니다.

| 항목 | 값 |
|------|----|
| 호스트 | `localhost,1433` |
| SA 비밀번호 | `GetRich1234!A` |
| 에디션 | Express |

데이터는 Docker 볼륨(`sqlserver_data`)에 저장되어 컨테이너를 재시작해도 유지됩니다.

### 2. 데이터베이스 및 테이블 생성

SSMS, Azure Data Studio 또는 `sqlcmd`로 접속 후 아래 SQL을 실행합니다.

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

### 3. 연결 문자열 (이미 설정되어 있음)

`Lotto/Web.config`는 Docker 컨테이너에 맞게 미리 설정되어 있습니다.
```xml
<add name="GetRichEntities"
     connectionString="Data Source=localhost,1433;Initial Catalog=GetRich;User Id=sa;Password=GetRich1234!A;"
     providerName="System.Data.SqlClient" />
```
기본 Docker 설정을 사용하는 경우 별도 수정이 필요 없습니다.

### 컨테이너 종료 / 삭제
```bash
docker-compose down        # 종료 (볼륨 유지)
docker-compose down -v     # 종료 + 데이터 전체 삭제
```

## 실행 방법
1. DB 컨테이너를 시작합니다: `docker-compose up -d` (위 참조).
2. Visual Studio에서 `Lotto.sln` 파일을 엽니다.
3. NuGet 패키지를 복원합니다.
4. 솔루션을 빌드합니다 (빌드 > 솔루션 빌드).
5. 프로젝트를 실행합니다 (Ctrl+F5).
6. 당첨 데이터 수집은 `POST /LottoMng/GetLatestLottoNumbers`를 호출합니다 (반복 호출 시 누락된 다음 회차를 순서대로 가져옵니다).

## 프로젝트 구조
```
Lotto/
├── Controllers/
│   └── LottoMngController.cs      # 페이지 및 API 액션
├── Repository/
│   └── LottoMngRepository.cs      # 데이터 접근 및 번호 추천 로직 전체
├── Core/
│   └── LottoCore.cs               # 번호 카운팅, 딕셔너리 유틸리티
├── Models/
│   ├── Lotto_History.cs           # 당첨 이력 엔티티
│   └── WeeklySuggestedViewModel.cs
└── Views/LottoMng/
    ├── WeeklySuggestedNumbers.cshtml
    ├── Analytics.cshtml
    ├── History.cshtml
    └── RecentHistory.cshtml
```

## 변경 이력

### 2026-03-13
- **트렌드+오버듀 세트 A/B 2세트 생성:** 트렌드 창(10/26/52회/역대)별로 서로 다른 2개 세트 생성. B세트는 A세트 번호 전체 제외.
- **역대 당첨 조합 중복 방지 추가:** 생성된 세트가 역대 당첨 이력과 동일하면 자동 교체.
- **보너스 번호 오버듀 기반으로 수정:** 오버듀 세트의 보너스가 항상 같은 번호로 나오던 버그 수정. 이제 보너스 출현 기준 가장 오래 안 나온 번호로 선택.
- **오버듀 세트 괄호 숫자 수정:** 역대 출현 횟수 대신 마지막 출현 이후 경과 회차 수를 표시.
