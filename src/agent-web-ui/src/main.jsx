import React, { useEffect, useState } from "react";
import { createRoot } from "react-dom/client";
import "./styles.css";

const API = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

function App() {
  const [navigation, setNavigation] = useState([]);
  const [content, setContent] = useState(null);

  async function loadContent(intent = "content.home") {
    const response = await fetch(`${API}/api/content/${intent}`);
    setContent(await response.json());
  }

  useEffect(() => {
    fetch(`${API}/api/navigation`)
      .then(response => response.json())
      .then(model => setNavigation(model.items));

    loadContent();
  }, []);

  return (
    <div className="site">
      <header>
        <strong>AgentWeb</strong>
        <nav>
          {navigation.map(item => (
            <button key={item.intent} onClick={() => loadContent(item.intent)}>
              {item.label}
            </button>
          ))}
        </nav>
      </header>

      <main>
        <p className="eyebrow">GENERIC HOST / AGENT-PROVIDED EXPERIENCE</p>
        <h1>{content?.heading ?? "Loading experience..."}</h1>
        <p>{content?.body}</p>
      </main>
    </div>
  );
}

createRoot(document.getElementById("root")).render(<App />);
