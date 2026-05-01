import { useEffect, useState } from "react";
import api from "./api/api";
import "./App.css";

function App() {
  const [users, setUsers] = useState([]);
  const [teams, setTeams] = useState([]);
  const [tournaments, setTournaments] = useState([]);
  const [selectedTournament, setSelectedTournament] = useState(null);
  const [matchResults, setMatchResults] = useState({});

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [role, setRole] = useState("Student");

  const [teamName, setTeamName] = useState("");
  const [sportType, setSportType] = useState("Football");
  const [captainId, setCaptainId] = useState("");
  const [memberUserIds, setMemberUserIds] = useState([]);

  const [tournamentName, setTournamentName] = useState("");
  const [tournamentSportType, setTournamentSportType] = useState("Football");
  const [tournamentFormat, setTournamentFormat] = useState("RoundRobin");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [tournamentTeamIds, setTournamentTeamIds] = useState([]);

  const [standings, setStandings] = useState([]);

  async function fetchUsers() {
    const response = await api.get("/Users");
    setUsers(response.data);
  }

  async function fetchTeams() {
    const response = await api.get("/Teams");
    setTeams(response.data);
  }

  async function fetchTournaments() {
    const response = await api.get("/Tournaments");
    setTournaments(response.data);
  }

  async function fetchTournamentDetails(tournamentId) {
    const response = await api.get(`/Tournaments/${tournamentId}`);
    setSelectedTournament(response.data);
    await fetchStandings(tournamentId);
  }

  async function createUser(e) {
    e.preventDefault();

    await api.post("/Users", {
      fullName,
      email,
      role,
    });

    setFullName("");
    setEmail("");
    setRole("Student");

    fetchUsers();
  }

  async function createTeam(e) {
    e.preventDefault();

    await api.post("/Teams", {
      name: teamName,
      sportType,
      captainId: Number(captainId),
      memberUserIds: memberUserIds.map(Number),
    });

    setTeamName("");
    setSportType("Football");
    setCaptainId("");
    setMemberUserIds([]);

    fetchTeams();
    fetchUsers();
  }

  async function createTournament(e) {
    e.preventDefault();

    await api.post("/Tournaments", {
      name: tournamentName,
      sportType: tournamentSportType,
      format: tournamentFormat,
      startDate: new Date(startDate).toISOString(),
      endDate: new Date(endDate).toISOString(),
      teamIds: tournamentTeamIds.map(Number),
    });

    setTournamentName("");
    setTournamentSportType("Football");
    setTournamentFormat("RoundRobin");
    setStartDate("");
    setEndDate("");
    setTournamentTeamIds([]);

    fetchTournaments();
  }

  async function generateFixtures(tournamentId) {
    await api.post(`/Tournaments/${tournamentId}/generate-fixtures`);
    await fetchTournaments();
    await fetchTournamentDetails(tournamentId);
  }

  function updateMatchResultInput(fixtureId, field, value) {
    setMatchResults((previous) => ({
      ...previous,
      [fixtureId]: {
        ...previous[fixtureId],
        [field]: value,
      },
    }));
  }

  async function submitMatchResult(fixtureId) {
    const result = matchResults[fixtureId];

    if (!result || result.homeScore === undefined || result.awayScore === undefined) {
      alert("Please enter both scores.");
      return;
    }

    await api.post("/Fixtures/enter-result", {
      fixtureId,
      homeScore: Number(result.homeScore),
      awayScore: Number(result.awayScore),
    });

    setMatchResults((previous) => {
      const copy = { ...previous };
      delete copy[fixtureId];
      return copy;
    });

    await fetchTournaments();

    if (selectedTournament) {
      await fetchTournamentDetails(selectedTournament.id);
    }
  }

  async function fetchStandings(tournamentId) {
    const response = await api.get(`/Tournaments/${tournamentId}/standings`);
    setStandings(response.data);
  }

  function handleMemberSelection(e) {
    const selectedValues = Array.from(
      e.target.selectedOptions,
      (option) => option.value
    );

    setMemberUserIds(selectedValues);
  }

  function handleTournamentTeamSelection(e) {
    const selectedValues = Array.from(
      e.target.selectedOptions,
      (option) => option.value
    );

    setTournamentTeamIds(selectedValues);
  }

  useEffect(() => {
    fetchUsers();
    fetchTeams();
    fetchTournaments();
  }, []);

  return (
    <div className="container">
      <h1>University Sports Tournament System</h1>

      <section className="card">
        <h2>Create User</h2>

        <form onSubmit={createUser} className="form">
          <input
            type="text"
            placeholder="Full name"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            required
          />

          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />

          <select value={role} onChange={(e) => setRole(e.target.value)}>
            <option value="Student">Student</option>
            <option value="FacultyMember">Faculty Member</option>
            <option value="TournamentOrganizer">Tournament Organizer</option>
            <option value="FacilityManager">Facility Manager</option>
          </select>

          <button type="submit">Create User</button>
        </form>
      </section>

      <section className="card">
        <h2>Create Team</h2>

        <form onSubmit={createTeam} className="form">
          <input
            type="text"
            placeholder="Team name"
            value={teamName}
            onChange={(e) => setTeamName(e.target.value)}
            required
          />

          <select value={sportType} onChange={(e) => setSportType(e.target.value)}>
            <option value="Football">Football</option>
            <option value="Basketball">Basketball</option>
            <option value="Volleyball">Volleyball</option>
            <option value="Tennis">Tennis</option>
          </select>

          <select
            value={captainId}
            onChange={(e) => setCaptainId(e.target.value)}
            required
          >
            <option value="">Select captain</option>
            {users.map((user) => (
              <option key={user.id} value={user.id}>
                {user.fullName} ({user.role})
              </option>
            ))}
          </select>

          <label>Members</label>
          <select
            multiple
            value={memberUserIds}
            onChange={handleMemberSelection}
          >
            {users.map((user) => (
              <option key={user.id} value={user.id}>
                {user.fullName} ({user.role})
              </option>
            ))}
          </select>

          <small>Ctrl ile birden fazla üye seçebilirsin.</small>

          <button type="submit">Create Team</button>
        </form>
      </section>

      <section className="card">
        <h2>Create Tournament</h2>

        <form onSubmit={createTournament} className="form">
          <input
            type="text"
            placeholder="Tournament name"
            value={tournamentName}
            onChange={(e) => setTournamentName(e.target.value)}
            required
          />

          <select
            value={tournamentSportType}
            onChange={(e) => setTournamentSportType(e.target.value)}
          >
            <option value="Football">Football</option>
            <option value="Basketball">Basketball</option>
            <option value="Volleyball">Volleyball</option>
            <option value="Tennis">Tennis</option>
          </select>

          <select
            value={tournamentFormat}
            onChange={(e) => setTournamentFormat(e.target.value)}
          >
            <option value="RoundRobin">Round Robin</option>
            <option value="SingleElimination">Single Elimination</option>
          </select>

          <label>Start Date</label>
          <input
            type="datetime-local"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            required
          />

          <label>End Date</label>
          <input
            type="datetime-local"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
            required
          />

          <label>Tournament Teams</label>
          <select
            multiple
            value={tournamentTeamIds}
            onChange={handleTournamentTeamSelection}
            required
          >
            {teams
              .filter((team) => team.sportType === tournamentSportType)
              .map((team) => (
                <option key={team.id} value={team.id}>
                  {team.name} ({team.sportType})
                </option>
              ))}
          </select>

          <small>En az 2 takım seçmelisin.</small>

          <button type="submit">Create Tournament</button>
        </form>
      </section>

      <section className="card">
        <h2>Tournaments</h2>

        {tournaments.length === 0 ? (
          <p>No tournaments found.</p>
        ) : (
          <ul>
            {tournaments.map((tournament) => (
              <li key={tournament.id}>
                <strong>{tournament.name}</strong> — {tournament.sportType}
                <br />
                Format: {tournament.format}
                <br />
                Status: {tournament.status}
                <br />
                Teams: {tournament.teams.join(", ")}
                <br />

                <button onClick={() => fetchTournamentDetails(tournament.id)}>
                  View Details
                </button>

                {tournament.status === "Draft" && (
                  <button onClick={() => generateFixtures(tournament.id)}>
                    Generate Fixtures
                  </button>
                )}
              </li>
            ))}
          </ul>
        )}
      </section>

      {selectedTournament && (
        <section className="card">
          <h2>{selectedTournament.name} Details</h2>

          <p>
            <strong>Status:</strong> {selectedTournament.status}
          </p>

          <h3>Fixtures</h3>

          {selectedTournament.fixtures.length === 0 ? (
            <p>No fixtures generated yet.</p>
          ) : (
            <ul>
              {selectedTournament.fixtures.map((fixture) => (
                <li key={fixture.id} className="fixture-item">
                  <strong>
                    #{fixture.id} — {fixture.homeTeamName} vs{" "}
                    {fixture.awayTeamName}
                  </strong>
                  <br />
                  Date: {new Date(fixture.matchDate).toLocaleString()}
                  <br />
                  Status: {fixture.status}

                  {fixture.status === "Scheduled" && (
                    <div className="score-form">
                      <input
                        type="number"
                        min="0"
                        placeholder="Home score"
                        value={matchResults[fixture.id]?.homeScore ?? ""}
                        onChange={(e) =>
                          updateMatchResultInput(
                            fixture.id,
                            "homeScore",
                            e.target.value
                          )
                        }
                      />

                      <input
                        type="number"
                        min="0"
                        placeholder="Away score"
                        value={matchResults[fixture.id]?.awayScore ?? ""}
                        onChange={(e) =>
                          updateMatchResultInput(
                            fixture.id,
                            "awayScore",
                            e.target.value
                          )
                        }
                      />

                      <button onClick={() => submitMatchResult(fixture.id)}>
                        Submit Result
                      </button>
                    </div>
                  )}
                </li>
              ))}
            </ul>
	)}
	<h3>Standings</h3>

	{standings.length === 0 ? (
	  <p>No standings available yet.</p>
	) : (
	  <table>
	    <thead>
	      <tr>
	        <th>Team</th>
	        <th>Played</th>
	        <th>Wins</th>
	        <th>Draws</th>
	        <th>Losses</th>
	        <th>Points</th>
	      </tr>
	    </thead>
	    <tbody>
	      {standings.map((standing) => (
	        <tr key={standing.teamId}>
	          <td>{standing.teamName}</td>
	          <td>{standing.played}</td>
	          <td>{standing.wins}</td>
	          <td>{standing.draws}</td>
	          <td>{standing.losses}</td>
	          <td>{standing.points}</td>
	        </tr>
	      ))}
	    </tbody>
	  </table>
	)}
        </section>
      )}

      <section className="card">
        <h2>Users</h2>

        {users.length === 0 ? (
          <p>No users found.</p>
        ) : (
          <ul>
            {users.map((user) => (
              <li key={user.id}>
                <strong>{user.fullName}</strong> — {user.role}
                {user.teams?.length > 0 && (
                  <span> | Teams: {user.teams.join(", ")}</span>
                )}
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="card">
        <h2>Teams</h2>

        {teams.length === 0 ? (
          <p>No teams found.</p>
        ) : (
          <ul>
            {teams.map((team) => (
              <li key={team.id}>
                <strong>{team.name}</strong> — {team.sportType}
                <br />
                Captain: {team.captainName}
                <br />
                Members: {team.members.join(", ")}
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}

export default App;
