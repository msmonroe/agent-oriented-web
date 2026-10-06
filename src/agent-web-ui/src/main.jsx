import React, { useEffect, useState } from "react";
import { createRoot } from "react-dom/client";
import "./styles.css";

const API = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

function Component({ component }) {
  const { type, props } = component;

  switch (type) {
    case "eyebrow":
      return <p className="eyebrow">{props.text}</p>;
    case "heading":
      return <h1>{props.text}</h1>;
    case "text":
      return <p>{props.text}</p>;
    case "input":
      return (
        <label>
          {props.label}
          <input name={props.name} placeholder={props.placeholder} />
        </label>
      );
    case "select":
      return (
        <label>
          {props.label}
          <select name={props.name} defaultValue="">
            <option value="" disabled>Select one</option>
            {props.options?.map(option => (
              <option key={option} value={option}>{option}</option>
            ))}
          </select>
        </label>
      );
    case "button":
      return <button className="primary">{props.label}</button>;
    default:
      return null;
  }
}

function App() {
  const [navigation, setNavigation] = useState([]);
  const [experience, setExperience] = useState(null);

  async function loadExperience(capability = "content.home") {
    const response = await fetch(`${API}/api/experience/${capability}`);
    setExperience(await response.json());
  }

  useEffect(() => {
    fetch(`${API}/api/navigation`)
      .then(response => response.json())
      .then(model => setNavigation(model.items));

    loadExperience();
  }, []);

  return (
    <div className="site">
      <header>
        <strong>AgentWeb</strong>
        <nav>
          {navigation.map(item => (
            <button key={item.intent} onClick={() => loadExperience(item.intent)}>
              {item.label}
            </button>
          ))}
        </nav>
      </header>

      <main>
        {experience?.components?.map((component, index) => (
          <Component key={index} component={component} />
        ))}
      </main>
    </div>
  );
}

createRoot(document.getElementById("root")).render(<App />);
