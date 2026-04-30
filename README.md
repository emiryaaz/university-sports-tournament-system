## Backend Test Flow

### 1. Run the Backend

```bash
cd backend/SportsTournament.Api
dotnet run

Swagger UI:

http://localhost:5072/swagger
2. Reset Database

Development testing için tüm verileri ve ID sequence değerlerini sıfırlar.

DELETE /api/Dev/reset-database
3. Create Users
POST /api/Users
{
  "fullName": "User 1",
  "email": "user1@example.com",
  "role": "Student"
}

En az 4 user oluştur.

4. Create Teams
POST /api/Teams
{
  "name": "Team 1",
  "sportType": "Football",
  "captainId": 1,
  "memberUserIds": [1]
}

En az 2 takım oluştur. RoundRobin testi için 4 takım önerilir.

5. Create Tournament
POST /api/Tournaments
{
  "name": "Football Tournament",
  "sportType": "Football",
  "format": "RoundRobin",
  "startDate": "2026-05-01T09:00:00Z",
  "endDate": "2026-05-30T18:00:00Z",
  "teamIds": [1, 2, 3, 4]
}
6. Generate Fixtures
POST /api/Tournaments/1/generate-fixtures

Bu işlem turnuvadaki takımlara göre maçları otomatik oluşturur.

4 takım için 6 fixture oluşur:

Team 1 vs Team 2
Team 1 vs Team 3
Team 1 vs Team 4
Team 2 vs Team 3
Team 2 vs Team 4
Team 3 vs Team 4
7. View Tournament Details
GET /api/Tournaments/1

Bu endpoint turnuva bilgilerini, takımları ve fixture listesini gösterir.

Fixture sonucunu girerken kullanılacak değer:

"fixtures": [
  {
    "id": 1,
    "homeTeamName": "Team 1",
    "awayTeamName": "Team 2",
    "status": "Scheduled"
  }
]

Buradaki id, fixtureId değeridir.

8. Enter Match Result
POST /api/Fixtures/enter-result
{
  "fixtureId": 1,
  "homeScore": 3,
  "awayScore": 1
}

Sonuç girildikten sonra:

Fixture status = Completed
Standings updated
Tournament status checked
9. View Fixture Result
GET /api/Fixtures/1/result
10. View Standings
GET /api/Tournaments/1/standings

Örnek çıktı:

[
  {
    "teamId": 1,
    "teamName": "Team 1",
    "played": 1,
    "wins": 1,
    "draws": 0,
    "losses": 0,
    "points": 3
  }
]
11. Tournament Finish Rule

Bir turnuvadaki tüm fixture’lar Completed olduğunda tournament status otomatik olarak:

Finished

olur.

Kontrol etmek için:

GET /api/Tournaments/1
