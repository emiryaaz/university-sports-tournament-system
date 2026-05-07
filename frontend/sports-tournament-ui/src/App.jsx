import { useEffect, useState } from "react";
import api from "./api/api";
import Sidebar from "./components/Sidebar";
import Message from "./components/Message";
import "./App.css";

function App() {
  const [activePage, setActivePage] = useState("dashboard");

  const [currentUser, setCurrentUser] = useState(
    JSON.parse(localStorage.getItem("currentUser"))
  );

  const [authMode, setAuthMode] = useState("login");
  const [loginEmail, setLoginEmail] = useState("");
  const [loginPassword, setLoginPassword] = useState("");
  const [registerFullName, setRegisterFullName] = useState("");
  const [registerEmail, setRegisterEmail] = useState("");
  const [registerPassword, setRegisterPassword] = useState("");
  const [registerRole, setRegisterRole] = useState("Student");

  const [users, setUsers] = useState([]);
  const [teams, setTeams] = useState([]);
  const [tournaments, setTournaments] = useState([]);

  const [selectedTournament, setSelectedTournament] = useState(null);
  const [standings, setStandings] = useState([]);
  const [matchResults, setMatchResults] = useState({});

  const [message, setMessage] = useState("");
  const [messageType, setMessageType] = useState("");

  const [teamName, setTeamName] = useState("");
  const [sportType, setSportType] = useState("Football");

  const [teamInvitations, setTeamInvitations] = useState([]);
  const [joinRequests, setJoinRequests] = useState([]);
  const [inviteTeamId, setInviteTeamId] = useState("");
  const [invitedUserId, setInvitedUserId] = useState("");
  const [selectedCaptainTeamId, setSelectedCaptainTeamId] = useState("");

  const [tournamentName, setTournamentName] = useState("");
  const [tournamentSportType, setTournamentSportType] = useState("Football");
  const [tournamentFormat, setTournamentFormat] = useState("RoundRobin");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  const [tournamentInvitations, setTournamentInvitations] = useState([]);
  const [tournamentJoinRequests, setTournamentJoinRequests] = useState([]);
  const [selectedTournamentForRequests, setSelectedTournamentForRequests] =
    useState("");
  const [inviteTournamentId, setInviteTournamentId] = useState("");
  const [inviteTournamentTeamId, setInviteTournamentTeamId] = useState("");

  function showSuccess(text) {
    setMessage(text);
    setMessageType("success");
  }

  function showError(error) {
    const backendMessage =
      error.response?.data || "An unexpected error occurred.";

    setMessage(
      typeof backendMessage === "string"
        ? backendMessage
        : JSON.stringify(backendMessage)
    );

    setMessageType("error");
  }

  function getMyCaptainTeams() {
    if (!currentUser) return [];
    return teams.filter((team) => team.captainId === currentUser.id);
  }

  function canManageTournaments() {
    return (
      currentUser?.role === "TournamentOrganizer" ||
      currentUser?.role === "FacilityManager"
    );
  }

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

  async function fetchStandings(tournamentId) {
    const response = await api.get(`/Tournaments/${tournamentId}/standings`);
    setStandings(response.data);
  }

  async function fetchTournamentDetails(tournamentId) {
    const response = await api.get(`/Tournaments/${tournamentId}`);
    setSelectedTournament(response.data);
    await fetchStandings(tournamentId);
  }

  async function login(e) {
    e.preventDefault();

    try {
      const response = await api.post("/Auth/login", {
        email: loginEmail,
        password: loginPassword,
      });

      setCurrentUser(response.data);
      localStorage.setItem("currentUser", JSON.stringify(response.data));

      setLoginEmail("");
      setLoginPassword("");

      showSuccess("Login successful.");
    } catch (error) {
      showError(error);
    }
  }

  async function register(e) {
    e.preventDefault();

    try {
      const response = await api.post("/Auth/register", {
        fullName: registerFullName,
        email: registerEmail,
        password: registerPassword,
        role: registerRole,
      });

      setCurrentUser(response.data);
      localStorage.setItem("currentUser", JSON.stringify(response.data));

      setRegisterFullName("");
      setRegisterEmail("");
      setRegisterPassword("");
      setRegisterRole("Student");

      await fetchUsers();
      showSuccess("Registration successful.");
    } catch (error) {
      showError(error);
    }
  }

  function logout() {
    setCurrentUser(null);
    localStorage.removeItem("currentUser");
    setSelectedTournament(null);
    setStandings([]);
    showSuccess("Logged out successfully.");
  }

  async function createTeam(e) {
    e.preventDefault();

    try {
      await api.post("/Teams", {
        name: teamName,
        sportType,
        captainId: currentUser.id,
      });

      setTeamName("");
      setSportType("Football");

      await fetchTeams();
      await fetchUsers();
      showSuccess("Team created successfully.");
    } catch (error) {
      showError(error);
    }
  }

  async function fetchTeamInvitations() {
    if (!currentUser) return;

    const response = await api.get(`/Teams/invitations/user/${currentUser.id}`);
    setTeamInvitations(response.data);
  }

  async function fetchJoinRequests(teamId) {
    if (!teamId) {
      setJoinRequests([]);
      return;
    }

    const response = await api.get(`/Teams/join-requests/team/${teamId}`);
    setJoinRequests(response.data);
  }

  async function inviteUserToTeam(e) {
    e.preventDefault();

    try {
      await api.post("/Teams/invite", {
        teamId: Number(inviteTeamId),
        invitedUserId: Number(invitedUserId),
        invitedByUserId: currentUser.id,
      });

      setInviteTeamId("");
      setInvitedUserId("");

      showSuccess("Invitation sent successfully.");
    } catch (error) {
      showError(error);
    }
  }

  async function respondToInvitation(invitationId, accept) {
    try {
      await api.post("/Teams/invitations/respond", {
        invitationId,
        accept,
      });

      await fetchTeamInvitations();
      await fetchTeams();
      await fetchUsers();

      showSuccess(accept ? "Invitation accepted." : "Invitation rejected.");
    } catch (error) {
      showError(error);
    }
  }

  async function requestToJoinTeam(teamId) {
    try {
      await api.post("/Teams/join-request", {
        teamId,
        requestedUserId: currentUser.id,
      });

      showSuccess("Join request sent successfully.");
    } catch (error) {
      showError(error);
    }
  }

  async function respondToJoinRequest(joinRequestId, accept) {
    try {
      await api.post("/Teams/join-requests/respond", {
        joinRequestId,
        accept,
        captainId: currentUser.id,
      });

      await fetchJoinRequests(selectedCaptainTeamId);
      await fetchTeams();
      await fetchUsers();

      showSuccess(accept ? "Join request accepted." : "Join request rejected.");
    } catch (error) {
      showError(error);
    }
  }

  async function createTournament(e) {
    e.preventDefault();

    try {
      await api.post("/Tournaments", {
        name: tournamentName,
        sportType: tournamentSportType,
        format: tournamentFormat,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
      });

      setTournamentName("");
      setTournamentSportType("Football");
      setTournamentFormat("RoundRobin");
      setStartDate("");
      setEndDate("");

      await fetchTournaments();
      showSuccess("Tournament created successfully.");
    } catch (error) {
      showError(error);
    }
  }

  async function requestToJoinTournament(tournamentId, teamId) {
    try {
      await api.post("/Tournaments/join-request", {
        tournamentId,
        teamId,
        requestedByUserId: currentUser.id,
      });

      showSuccess("Tournament join request sent successfully.");
    } catch (error) {
      showError(error);
    }
  }

  async function inviteTeamToTournament(e) {
    e.preventDefault();

    try {
      await api.post("/Tournaments/invite-team", {
        tournamentId: Number(inviteTournamentId),
        teamId: Number(inviteTournamentTeamId),
        invitedByUserId: currentUser.id,
      });

      setInviteTournamentId("");
      setInviteTournamentTeamId("");

      showSuccess("Tournament invitation sent successfully.");
    } catch (error) {
      showError(error);
    }
  }

  async function fetchTournamentJoinRequests(tournamentId) {
    if (!tournamentId) {
      setTournamentJoinRequests([]);
      return;
    }

    const response = await api.get(
      `/Tournaments/join-requests/tournament/${tournamentId}`
    );

    setTournamentJoinRequests(response.data);
  }

  async function respondToTournamentJoinRequest(joinRequestId, accept) {
    try {
      await api.post("/Tournaments/join-requests/respond", {
        joinRequestId,
        accept,
        organizerId: currentUser.id,
      });

      await fetchTournamentJoinRequests(selectedTournamentForRequests);
      await fetchTournaments();

      if (selectedTournament) {
        await fetchTournamentDetails(selectedTournament.id);
      }

      showSuccess(
        accept
          ? "Tournament join request accepted."
          : "Tournament join request rejected."
      );
    } catch (error) {
      showError(error);
    }
  }

  async function fetchTournamentInvitationsForMyTeams() {
    const myCaptainTeams = getMyCaptainTeams();
    const allInvitations = [];

    for (const team of myCaptainTeams) {
      const response = await api.get(`/Tournaments/invitations/team/${team.id}`);

      allInvitations.push(
        ...response.data.map((invitation) => ({
          ...invitation,
          teamName: team.name,
          teamId: team.id,
        }))
      );
    }

    setTournamentInvitations(allInvitations);
  }

  async function respondToTournamentInvitation(invitationId, accept) {
    try {
      await api.post("/Tournaments/invitations/respond", {
        invitationId,
        accept,
        captainId: currentUser.id,
      });

      await fetchTournamentInvitationsForMyTeams();
      await fetchTournaments();

      if (selectedTournament) {
        await fetchTournamentDetails(selectedTournament.id);
      }

      showSuccess(
        accept
          ? "Tournament invitation accepted."
          : "Tournament invitation rejected."
      );
    } catch (error) {
      showError(error);
    }
  }

  async function generateFixtures(tournamentId) {
    try {
      await api.post(`/Tournaments/${tournamentId}/generate-fixtures`);
      await fetchTournaments();
      await fetchTournamentDetails(tournamentId);
      showSuccess("Fixtures generated successfully.");
    } catch (error) {
      showError(error);
    }
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

    if (
      !result ||
      result.homeScore === undefined ||
      result.awayScore === undefined
    ) {
      setMessage("Please enter both scores.");
      setMessageType("error");
      return;
    }

    try {
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

      showSuccess("Match result submitted successfully.");
    } catch (error) {
      showError(error);
    }
  }

  async function resetDatabase() {
    const confirmed = window.confirm(
      "Are you sure? This will delete all data and reset IDs."
    );

    if (!confirmed) return;

    try {
      await api.delete("/Dev/reset-database");

      setUsers([]);
      setTeams([]);
      setTournaments([]);
      setSelectedTournament(null);
      setStandings([]);
      setMatchResults({});
      setTeamInvitations([]);
      setJoinRequests([]);
      setTournamentInvitations([]);
      setTournamentJoinRequests([]);

      await fetchUsers();
      await fetchTeams();
      await fetchTournaments();

      showSuccess("Database reset successfully.");
    } catch (error) {
      showError(error);
    }
  }

  useEffect(() => {
    fetchUsers();
    fetchTeams();
    fetchTournaments();

    if (currentUser) {
      fetchTeamInvitations();
    }
  }, []);

  if (!currentUser) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <h1>University Sports Tournament System</h1>
          <p>Login or register to continue.</p>

          <Message message={message} messageType={messageType} />

          <div className="auth-tabs">
            <button
              className={authMode === "login" ? "active" : ""}
              onClick={() => setAuthMode("login")}
            >
              Login
            </button>

            <button
              className={authMode === "register" ? "active" : ""}
              onClick={() => setAuthMode("register")}
            >
              Register
            </button>
          </div>

          {authMode === "login" ? (
            <form onSubmit={login} className="form">
              <input
                type="email"
                placeholder="Email"
                value={loginEmail}
                onChange={(e) => setLoginEmail(e.target.value)}
                required
              />

              <input
                type="password"
                placeholder="Password"
                value={loginPassword}
                onChange={(e) => setLoginPassword(e.target.value)}
                required
              />

              <button type="submit">Login</button>
            </form>
          ) : (
            <form onSubmit={register} className="form">
              <input
                type="text"
                placeholder="Full name"
                value={registerFullName}
                onChange={(e) => setRegisterFullName(e.target.value)}
                required
              />

              <input
                type="email"
                placeholder="Email"
                value={registerEmail}
                onChange={(e) => setRegisterEmail(e.target.value)}
                required
              />

              <input
                type="password"
                placeholder="Password"
                value={registerPassword}
                onChange={(e) => setRegisterPassword(e.target.value)}
                required
              />

              <select
                value={registerRole}
                onChange={(e) => setRegisterRole(e.target.value)}
              >
                <option value="Student">Student</option>
                <option value="FacultyMember">Faculty Member</option>
                <option value="TournamentOrganizer">Tournament Organizer</option>
                <option value="FacilityManager">Facility Manager</option>
              </select>

              <button type="submit">Register</button>
            </form>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="app-layout">
      <Sidebar activePage={activePage} setActivePage={setActivePage} />

      <main className="main-content">
        <header className="topbar">
          <div>
            <h1>University Sports Tournament System</h1>
            <p>Manage users, teams, tournaments, fixtures and standings.</p>
          </div>

          <div className="user-box">
            <span>
              {currentUser.fullName} ({currentUser.role})
            </span>
            <button onClick={logout}>Logout</button>
          </div>
        </header>

        <Message message={message} messageType={messageType} />

        {activePage === "dashboard" && (
          <section className="dashboard-grid">
            <div className="stat-card">
              <span>Total Users</span>
              <strong>{users.length}</strong>
            </div>

            <div className="stat-card">
              <span>Total Teams</span>
              <strong>{teams.length}</strong>
            </div>

            <div className="stat-card">
              <span>Total Tournaments</span>
              <strong>{tournaments.length}</strong>
            </div>

            <div className="stat-card">
              <span>Selected Tournament</span>
              <strong>
                {selectedTournament ? selectedTournament.name : "None"}
              </strong>
            </div>
          </section>
        )}

        {activePage === "users" && (
          <section className="card">
            <h2>Users</h2>

            {users.length === 0 ? (
              <p>No users found.</p>
            ) : (
              <ul className="list">
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
        )}

        {activePage === "teams" && (
          <>
            <section className="card">
              <h2>Create Team</h2>
              <p className="muted">
                You will automatically become the captain of the team.
              </p>

              <form onSubmit={createTeam} className="form">
                <input
                  type="text"
                  placeholder="Team name"
                  value={teamName}
                  onChange={(e) => setTeamName(e.target.value)}
                  required
                />

                <select
                  value={sportType}
                  onChange={(e) => setSportType(e.target.value)}
                >
                  <option value="Football">Football</option>
                  <option value="Basketball">Basketball</option>
                  <option value="Volleyball">Volleyball</option>
                  <option value="Tennis">Tennis</option>
                </select>

                <button type="submit">Create Team</button>
              </form>
            </section>

            <section className="card">
              <h2>Available Teams</h2>

              {teams.length === 0 ? (
                <p>No teams found.</p>
              ) : (
                <ul className="list">
                  {teams.map((team) => {
                    const isMember = team.members.includes(
                      currentUser.fullName
                    );
                    const isCaptain = team.captainId === currentUser.id;

                    return (
                      <li key={team.id}>
                        <strong>{team.name}</strong> — {team.sportType}
                        <br />
                        Captain: {team.captainName}
                        <br />
                        Members: {team.members.join(", ")}

                        {!isMember && !isCaptain && (
                          <div className="button-row">
                            <button onClick={() => requestToJoinTeam(team.id)}>
                              Request to Join
                            </button>
                          </div>
                        )}
                      </li>
                    );
                  })}
                </ul>
              )}
            </section>

            <section className="card">
              <h2>My Team Invitations</h2>

              <button onClick={fetchTeamInvitations}>
                Refresh Invitations
              </button>

              {teamInvitations.length === 0 ? (
                <p>No pending invitations.</p>
              ) : (
                <ul className="list">
                  {teamInvitations.map((invitation) => (
                    <li key={invitation.id}>
                      <strong>{invitation.teamName}</strong> —{" "}
                      {invitation.sportType}
                      <br />
                      Invited by: {invitation.invitedBy}
                      <br />
                      Status: {invitation.status}

                      <div className="button-row">
                        <button
                          onClick={() =>
                            respondToInvitation(invitation.id, true)
                          }
                        >
                          Accept
                        </button>

                        <button
                          className="secondary-button"
                          onClick={() =>
                            respondToInvitation(invitation.id, false)
                          }
                        >
                          Reject
                        </button>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            {getMyCaptainTeams().length > 0 && (
              <section className="card">
                <h2>Captain Panel</h2>

                <h3>Invite User to My Team</h3>

                <form onSubmit={inviteUserToTeam} className="form">
                  <select
                    value={inviteTeamId}
                    onChange={(e) => setInviteTeamId(e.target.value)}
                    required
                  >
                    <option value="">Select your team</option>
                    {getMyCaptainTeams().map((team) => (
                      <option key={team.id} value={team.id}>
                        {team.name} ({team.sportType})
                      </option>
                    ))}
                  </select>

                  <select
                    value={invitedUserId}
                    onChange={(e) => setInvitedUserId(e.target.value)}
                    required
                  >
                    <option value="">Select user to invite</option>
                    {users
                      .filter((user) => user.id !== currentUser.id)
                      .map((user) => (
                        <option key={user.id} value={user.id}>
                          {user.fullName} ({user.role})
                        </option>
                      ))}
                  </select>

                  <button type="submit">Send Invitation</button>
                </form>

                <h3>Review Join Requests</h3>

                <div className="form">
                  <select
                    value={selectedCaptainTeamId}
                    onChange={async (e) => {
                      setSelectedCaptainTeamId(e.target.value);
                      await fetchJoinRequests(e.target.value);
                    }}
                  >
                    <option value="">Select your team</option>
                    {getMyCaptainTeams().map((team) => (
                      <option key={team.id} value={team.id}>
                        {team.name} ({team.sportType})
                      </option>
                    ))}
                  </select>
                </div>

                {joinRequests.length === 0 ? (
                  <p>No pending join requests.</p>
                ) : (
                  <ul className="list">
                    {joinRequests.map((request) => (
                      <li key={request.id}>
                        <strong>{request.requestedUser}</strong> wants to join{" "}
                        {request.teamName}
                        <br />
                        Status: {request.status}

                        <div className="button-row">
                          <button
                            onClick={() =>
                              respondToJoinRequest(request.id, true)
                            }
                          >
                            Accept
                          </button>

                          <button
                            className="secondary-button"
                            onClick={() =>
                              respondToJoinRequest(request.id, false)
                            }
                          >
                            Reject
                          </button>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}
              </section>
            )}
          </>
        )}

        {activePage === "tournaments" && (
          <>
            <section className="card">
              <h2>Create Tournament</h2>

              {!canManageTournaments() ? (
                <p className="muted">
                  Only tournament organizers or facility managers can create
                  tournaments.
                </p>
              ) : (
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
                    <option value="SingleElimination">
                      Single Elimination
                    </option>
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

                  <button type="submit">Create Tournament</button>
                </form>
              )}
            </section>

            <section className="card">
              <h2>Tournaments</h2>

              {tournaments.length === 0 ? (
                <p>No tournaments found.</p>
              ) : (
                <ul className="list">
                  {tournaments.map((tournament) => (
                    <li key={tournament.id}>
                      <strong>{tournament.name}</strong> —{" "}
                      {tournament.sportType}
                      <br />
                      Format: {tournament.format}
                      <br />
                      Status: {tournament.status}
                      <br />
                      Teams: {tournament.teams.join(", ") || "No teams yet"}

                      <div className="button-row">
                        <button
                          onClick={() =>
                            fetchTournamentDetails(tournament.id)
                          }
                        >
                          View Details
                        </button>

                        {canManageTournaments() &&
                          tournament.status === "Draft" && (
                            <button
                              onClick={() =>
                                generateFixtures(tournament.id)
                              }
                            >
                              Generate Fixtures
                            </button>
                          )}
                      </div>

                      {getMyCaptainTeams().length > 0 &&
                        tournament.status === "Draft" && (
                          <div className="form mini-form">
                            <label>Register one of my teams</label>

                            <select
                              onChange={(e) => {
                                if (e.target.value) {
                                  requestToJoinTournament(
                                    tournament.id,
                                    Number(e.target.value)
                                  );
                                  e.target.value = "";
                                }
                              }}
                              defaultValue=""
                            >
                              <option value="">
                                Select team to request registration
                              </option>
                              {getMyCaptainTeams()
                                .filter(
                                  (team) =>
                                    team.sportType === tournament.sportType
                                )
                                .map((team) => (
                                  <option key={team.id} value={team.id}>
                                    {team.name}
                                  </option>
                                ))}
                            </select>
                          </div>
                        )}
                    </li>
                  ))}
                </ul>
              )}
            </section>

            {canManageTournaments() && (
              <section className="card">
                <h2>Organizer Panel</h2>

                <h3>Invite Team to Tournament</h3>

                <form onSubmit={inviteTeamToTournament} className="form">
                  <select
                    value={inviteTournamentId}
                    onChange={(e) => setInviteTournamentId(e.target.value)}
                    required
                  >
                    <option value="">Select tournament</option>
                    {tournaments
                      .filter((tournament) => tournament.status === "Draft")
                      .map((tournament) => (
                        <option key={tournament.id} value={tournament.id}>
                          {tournament.name} ({tournament.sportType})
                        </option>
                      ))}
                  </select>

                  <select
                    value={inviteTournamentTeamId}
                    onChange={(e) =>
                      setInviteTournamentTeamId(e.target.value)
                    }
                    required
                  >
                    <option value="">Select team</option>
                    {teams.map((team) => (
                      <option key={team.id} value={team.id}>
                        {team.name} ({team.sportType})
                      </option>
                    ))}
                  </select>

                  <button type="submit">Send Tournament Invitation</button>
                </form>

                <h3>Review Tournament Join Requests</h3>

                <div className="form">
                  <select
                    value={selectedTournamentForRequests}
                    onChange={async (e) => {
                      setSelectedTournamentForRequests(e.target.value);
                      await fetchTournamentJoinRequests(e.target.value);
                    }}
                  >
                    <option value="">Select tournament</option>
                    {tournaments.map((tournament) => (
                      <option key={tournament.id} value={tournament.id}>
                        {tournament.name}
                      </option>
                    ))}
                  </select>
                </div>

                {tournamentJoinRequests.length === 0 ? (
                  <p>No pending tournament join requests.</p>
                ) : (
                  <ul className="list">
                    {tournamentJoinRequests.map((request) => (
                      <li key={request.id}>
                        <strong>{request.teamName}</strong> wants to join{" "}
                        {request.tournamentName}
                        <br />
                        Requested by: {request.requestedBy}
                        <br />
                        Status: {request.status}

                        <div className="button-row">
                          <button
                            onClick={() =>
                              respondToTournamentJoinRequest(
                                request.id,
                                true
                              )
                            }
                          >
                            Accept
                          </button>

                          <button
                            className="secondary-button"
                            onClick={() =>
                              respondToTournamentJoinRequest(
                                request.id,
                                false
                              )
                            }
                          >
                            Reject
                          </button>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}
              </section>
            )}

            {getMyCaptainTeams().length > 0 && (
              <section className="card">
                <h2>My Tournament Invitations</h2>

                <button onClick={fetchTournamentInvitationsForMyTeams}>
                  Refresh Tournament Invitations
                </button>

                {tournamentInvitations.length === 0 ? (
                  <p>No pending tournament invitations.</p>
                ) : (
                  <ul className="list">
                    {tournamentInvitations.map((invitation) => (
                      <li key={invitation.id}>
                        <strong>{invitation.tournamentName}</strong> —{" "}
                        {invitation.sportType}
                        <br />
                        Team: {invitation.teamName}
                        <br />
                        Format: {invitation.format}
                        <br />
                        Invited by: {invitation.invitedBy}
                        <br />
                        Status: {invitation.status}

                        <div className="button-row">
                          <button
                            onClick={() =>
                              respondToTournamentInvitation(
                                invitation.id,
                                true
                              )
                            }
                          >
                            Accept
                          </button>

                          <button
                            className="secondary-button"
                            onClick={() =>
                              respondToTournamentInvitation(
                                invitation.id,
                                false
                              )
                            }
                          >
                            Reject
                          </button>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}
              </section>
            )}

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
                  <ul className="list">
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

                        {fixture.status === "Scheduled" &&
                          canManageTournaments() && (
                            <div className="score-form">
                              <input
                                type="number"
                                min="0"
                                placeholder="Home score"
                                value={
                                  matchResults[fixture.id]?.homeScore ?? ""
                                }
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
                                value={
                                  matchResults[fixture.id]?.awayScore ?? ""
                                }
                                onChange={(e) =>
                                  updateMatchResultInput(
                                    fixture.id,
                                    "awayScore",
                                    e.target.value
                                  )
                                }
                              />

                              <button
                                onClick={() => submitMatchResult(fixture.id)}
                              >
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
          </>
        )}

        {activePage === "devtools" && (
          <section className="card danger-card">
            <h2>Development Tools</h2>
            <p>This button clears all test data and resets IDs.</p>

            <button className="danger-button" onClick={resetDatabase}>
              Reset Database
            </button>
          </section>
        )}
      </main>
    </div>
  );
}

export default App;
