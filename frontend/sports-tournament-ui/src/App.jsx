import { useEffect, useState } from "react";
import api from "./api/api";
import "./App.css";

function App() {
  const [users, setUsers] = useState([]);
  const [teams, setTeams] = useState([]);

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [role, setRole] = useState("Student");

  const [teamName, setTeamName] = useState("");
  const [sportType, setSportType] = useState("Football");
  const [captainId, setCaptainId] = useState("");
  const [memberUserIds, setMemberUserIds] = useState([]);

  async function fetchUsers() {
    const response = await api.get("/Users");
    setUsers(response.data);
  }

  async function fetchTeams() {
    const response = await api.get("/Teams");
    setTeams(response.data);
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

  function handleMemberSelection(e) {
    const selectedValues = Array.from(
      e.target.selectedOptions,
      (option) => option.value
    );

    setMemberUserIds(selectedValues);
  }

  useEffect(() => {
    fetchUsers();
    fetchTeams();
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
