// src/frontend/src/App.tsx
import React, { useState } from "react";
import "./App.css";

// A simple type for our weather data
interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

function App() {
  const [forecasts, setForecasts] = useState<WeatherForecast[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleFetchClick = async () => {
    setError(null);
    setForecasts(null);
    try {
      // IMPORTANT: We use '/api/...' because our reverse proxy (in docker-compose)
      // will route this to the backend. In a simple setup without a proxy, you'd use
      // the full URL: 'http://localhost:5001/weatherforecast'
      const response = await fetch("http://localhost:5001/weatherforecast");

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      setForecasts(data);
    } catch (e: any) {
      console.error("Failed to fetch:", e);
      setError(
        `Failed to fetch data. Is the backend running? Error: ${e.message}`
      );
    }
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>Game of Life - Test Environment</h1>
        <p>Click the button to test the API connection.</p>
        <button onClick={handleFetchClick}>Fetch Weather Data</button>

        {error && (
          <div style={{ color: "red", marginTop: "20px" }}>{error}</div>
        )}

        {forecasts && (
          <div
            style={{ marginTop: "20px", textAlign: "left", fontSize: "16px" }}
          >
            <h3>API Response:</h3>
            <pre
              style={{
                backgroundColor: "#282c34",
                padding: "10px",
                borderRadius: "5px",
              }}
            >
              {JSON.stringify(forecasts, null, 2)}
            </pre>
          </div>
        )}
      </header>
    </div>
  );
}

export default App;
