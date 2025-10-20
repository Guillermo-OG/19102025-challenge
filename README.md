# Conway's Game of Life - Challenge

This repository contains a full-stack implementation of Conway's Game of Life, built as a coding challenge.

The project is containerized using Docker and consists of a .NET 7 backend API that also serves a React (TypeScript) frontend as a single, unified application.

## Features

**Backend API (.NET 7)**

- **Board Management:** Create new boards of any size, update their state, and retrieve them by ID.
- **State Calculation:** Endpoints to calculate the next state, advance `N` generations, or find the final conclusive state of a board.
- **Durable Persistence:** Board states are retained across restarts by saving them to a JSON file inside a Docker volume.
- **Custom Rules:** Supports custom Game of Life rules via B/S notation (e.g., "B3/S23" for Conway's classic).
- **API Documentation:** Fully documented with Swagger/OpenAPI.

**Frontend (React & TypeScript)**

- **Interactive Canvas:** A highly performant canvas-based grid for visualizing and interacting with the simulation.
- **Board Editor:** Easily turn cells on or off by clicking or painting on the grid.
- **Simulation Controls:** Play, pause, step forward, clear, and randomize the board.
- **Real-time Adjustments:** Change the simulation speed and rules on the fly.
- **API-Driven:** All game logic and state progression is handled by the backend API.

## Tech Stack

- **Backend:** .NET 7, ASP.NET Core Web API
- **Frontend:** React, TypeScript
- **Containerization:** Docker & Docker Compose

## How to Run

1.  **Prerequisites:** Ensure you have Docker Desktop installed and running on your machine.
2.  **Clone:** Clone this repository to your local machine.
3.  **Build & Run:** Open a terminal in the root directory of the project and run the following command:
    ```bash
    docker-compose up --build
    ```
    This will build the Docker image for the unified application and start the container.
4.  **Access the Application:**
    - **Frontend UI & API:** Open your browser and navigate to `http://localhost:5001`.
    - **Backend API Docs (Swagger):** Open your browser and navigate to `http://localhost:5001/swagger`.

## How to Stop

To stop the application, press `Ctrl + C` in the terminal where `docker-compose` is running. To remove the container, network, and data volume, run:

```bash
docker-compose down
```
