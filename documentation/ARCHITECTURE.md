**Project: Conway's Game of Life**

**1. Overview**

This document details the architecture and implementation of the Conway's Game of Life full-stack application. The system comprises a .NET 7 backend API responsible for state management and computation, and a React frontend for user interaction and visualization. The entire system is containerized with Docker for portability and consistent deployment.

**2. Backend Architecture**

The backend follows a clean, layered architecture to promote separation of concerns, testability, and maintainability.

- **`GameOfLife.Core` (The Domain Layer)**

  - **Purpose:** This is the heart of the application. It contains all the core business logic, models, and algorithms for the Game of Life.
  - **Key Components:**
    - `IGameOfLifeRule`: An interface defining how a cell's next state is determined. The `BsRule` provides a flexible implementation based on standard B/S notation.
    - `BoardState`: An immutable-by-convention record representing the state of the grid at a single point in time. It uses a `bool[]` for memory efficiency and includes a `Signature()` method for fast, hash-based state comparison.
    - `GameOfLifeEngine`: A stateless service that takes a `BoardState` and a `Rule`, and computes the next `BoardState`.
    - `SequenceRunner`: A static utility class for higher-level operations, such as advancing a board `N` steps or finding its final conclusion (detecting stable states or cycles).
  - **Principles:** This project has zero external dependencies (no web, no database). It is pure C# logic, making it extremely fast and easy to unit test. This adheres to the **Single Responsibility Principle (SRP)**.

- **`GameOfLife.API` (The Application & Infrastructure Layer)**

  - **Purpose:** This ASP.NET Core project exposes the core logic via a RESTful API and serves the frontend application. It handles HTTP requests, persistence, and static file hosting.
  - **Key Components:**
    - `BoardsController`: The main entry point for all HTTP requests. It handles board creation (`POST`), state updates (`PUT`), retrieval (`GET`), and state progression (`POST /next`, `/advance`, `/final`).
    - `IBoardService`: An abstraction that orchestrates calls to the core engine and the persistence layer. Its existence allows for easier testing and dependency injection. This adheres to the **Dependency Inversion Principle (DIP)**.
    - `IBoardStore` / `FileBoardStore`: The persistence implementation. To meet the requirement of retaining state across restarts, the default implementation is `FileBoardStore`, which serializes board states to a JSON file on disk. This provides simple, durable storage without requiring a database.
    - `DTOs`: A set of record types (`CreateBoardRequest`, `BoardResponse`, etc.) used to define the public contract of the API, decoupling it from internal domain models.
    - **Static File Hosting**: The API is configured to serve the production build of the React frontend from its `wwwroot` directory, along with a fallback route to support SPA client-side routing.

- **`GameOfLife.Core.Tests` (The Testing Layer)**
  - **Purpose:** Contains xUnit tests for the `GameOfLife.Core` project.
  - **Strategy:** The tests focus on the core logic, verifying known patterns (still lifes, oscillators) and edge cases to ensure the engine's correctness.

**3. Frontend Architecture**

The frontend is a single-page application (SPA) built with React and TypeScript, acting as a client to the backend API.

- **`domain/` directory:** This folder contains TypeScript models (`BoardState`, `Rule`) that mirror the backend's core concepts. This allows the frontend to manage its view state efficiently without needing to re-fetch data for simple UI interactions.
- **`components/` directory:**
  - `CanvasBoard.tsx`: A performant component that renders the game board using the HTML `<canvas>` element. This is far more efficient for large grids than rendering thousands of individual `<div>` elements. It also handles all user input for drawing on the grid.
  - `ControlPanel.tsx`: A simple component containing all UI controls (buttons, sliders) for managing the simulation.
- **`services/` directory:**
  - `apiGolService.ts`: This is the **central component** of the frontend's logic. It acts as an API client, encapsulating all HTTP communication with the backend. It is responsible for:
    - Creating the initial board on the server.
    - Synchronizing local user edits (toggling cells) with the server using the `PUT /api/boards/{id}` endpoint.
    - Requesting state transitions (`next`, `advance`) from the server.
    - Updating the local state with the server's authoritative response.
- **State Management:** The application uses standard React hooks (`useState`, `useEffect`) for managing UI state. This is sufficient for the current scope, as the authoritative application state now resides on the backend.

**4. Deployment & Hosting**

- **Unified Container:** The entire application is packaged into a single Docker image. A multi-stage `Dockerfile` first builds the React application, then builds the .NET API, and finally copies the React build output into the API's `wwwroot` directory.
- **Docker Compose:** The `docker-compose.yml` file defines a single service that runs the unified container. It also defines a Docker volume to persist the `boards.json` file, ensuring data survives container restarts.
- **Single Entry Point:** The user accesses both the UI and the API through the same port (e.g., `http://localhost:5001`), which simplifies networking and eliminates the need for CORS configuration.
