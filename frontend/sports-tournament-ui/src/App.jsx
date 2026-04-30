import { useEffect, useState } from "react";
import api from "./api/api";
import "./App.css";

function App() {
  const [users, setUsers] = useState([]);
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [role, setRole] = useState("Student");

  async function fetchUsers() {
    const response = await api.get("/Users");
    setUsers(response.data);
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

  useEffect(() => {
    fetchUsers();
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
        <h2>Users</h2>

        {users.length === 0 ? (
          <p>No users found.</p>
        ) : (
          <ul>
            {users.map((user) => (
              <li key={user.id}>
                <strong>{user.fullName}</strong> — {user.role}
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}

export default App;
