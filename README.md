# University Sports Tournament System

A full-stack tournament management system developed with ASP.NET Core Web API, PostgreSQL, and React.

This project allows university users to:

- Register and log in
- Create and manage sports teams
- Send team invitations and join requests
- Create tournaments
- Invite teams to tournaments
- Request tournament participation
- Generate fixtures automatically
- Enter match results
- View live standings

---

# Technologies Used

## Backend

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- PostgreSQL
- Swagger/OpenAPI
- BCrypt.Net

## Frontend

- React
- Axios
- CSS

## Database

- PostgreSQL

---

# Features

## Authentication System

- User registration
- User login
- Password hashing with BCrypt
- Local session management using localStorage

---

## Team Management

### Team Creation

Users can create teams and automatically become the captain.

### Team Invitations

Team captains can:

- Invite users to their team
- View join requests
- Accept or reject requests

### Join Requests

Users can:

- Browse available teams
- Request to join teams
- Accept or reject invitations

---

## Tournament Management

### Tournament Creation

Tournament organizers and facility managers can:

- Create tournaments
- Define:
  - Sport type
  - Tournament format
  - Start/end dates

### Tournament Registration Requests

Team captains can:

- Send tournament participation requests

Tournament organizers can:

- Accept or reject requests

### Tournament Invitations

Organizers can:

- Invite teams directly to tournaments

Team captains can:

- Accept or reject tournament invitations

---

## Fixture System

- Automatic fixture generation
- Round Robin fixture support
- Match scheduling
- Match status tracking

---

## Match Result System

Tournament organizers can:

- Enter match scores
- Complete fixtures

---

## Standings System

Standings are automatically updated after every completed match.

Tracked statistics:

- Played
- Wins
- Draws
- Losses
- Points

---

# User Roles

## Student

Can:

- Create teams
- Join teams
- Participate in tournaments

---

## FacultyMember

Can:

- Create teams
- Join teams
- Participate in tournaments

---

## TournamentOrganizer

Can:

- Create tournaments
- Manage tournament requests
- Generate fixtures
- Enter match results

---

## FacilityManager

Can:

- Create tournaments
- Manage tournament requests
- Generate fixtures
- Enter match results

---

# Project Structure

```text
backend/
└── SportsTournament.Api/
    ├── Controllers/
    ├── Models/
    ├── DTOs/
    ├── Data/
    └── Migrations/

frontend/
└── sports-tournament-ui/
    ├── src/
    ├── components/
    ├── api/
    └── styles/
```

---

# Database Models

Main entities used in the system:

```text
User
Team
TeamMember
TeamInvitation
TeamJoinRequest

Tournament
TournamentTeam
TournamentInvitation
TournamentJoinRequest

Fixture
MatchResult
Standing
Facility
```

---

# API Endpoints

## Authentication

```text
POST /api/Auth/register
POST /api/Auth/login
```

---

## Teams

```text
GET    /api/Teams
GET    /api/Teams/{id}
POST   /api/Teams

POST   /api/Teams/invite
GET    /api/Teams/invitations/user/{userId}
POST   /api/Teams/invitations/respond

POST   /api/Teams/join-request
GET    /api/Teams/join-requests/team/{teamId}
POST   /api/Teams/join-requests/respond
```

---

## Tournaments

```text
GET    /api/Tournaments
GET    /api/Tournaments/{id}
POST   /api/Tournaments

POST   /api/Tournaments/invite-team
GET    /api/Tournaments/invitations/team/{teamId}
POST   /api/Tournaments/invitations/respond

POST   /api/Tournaments/join-request
GET    /api/Tournaments/join-requests/tournament/{tournamentId}
POST   /api/Tournaments/join-requests/respond

POST   /api/Tournaments/{id}/generate-fixtures
GET    /api/Tournaments/{id}/standings
```

---

## Fixtures

```text
GET  /api/Fixtures
POST /api/Fixtures/enter-result
```

---

## Development Tools

```text
DELETE /api/Dev/reset-database
```

---

# Setup Instructions

## Backend Setup

```bash
cd backend/SportsTournament.Api

dotnet restore
dotnet ef database update
dotnet run
```

Backend runs on:

```text
http://localhost:5072
```

Swagger:

```text
http://localhost:5072/swagger
```

---

## Frontend Setup

```bash
cd frontend/sports-tournament-ui

npm install
npm run dev
```

Frontend runs on:

```text
http://localhost:5173
```

---

# Example Workflow

## Team Flow

```text
1. User registers
2. User creates a team
3. Captain invites users OR users send join requests
4. Captain accepts requests
5. Team becomes ready for tournaments
```

---

## Tournament Flow

```text
1. Organizer creates tournament
2. Team captains request participation OR organizer invites teams
3. Organizer/captains accept requests
4. Tournament reaches enough teams
5. Fixtures are generated
6. Match results are entered
7. Standings update automatically
```

---

# Future Improvements

Possible future improvements:

- JWT authentication
- Real-time notifications
- Email invitations
- Knockout bracket visualization
- File/image upload support
- Mobile responsive improvements
- Role-based authorization middleware
- Live match tracking
- Statistics dashboard

---

# License

This project was developed for educational purposes.

