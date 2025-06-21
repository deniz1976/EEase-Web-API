# EEase-Web-API

EEase-Web-API is a robust backend solution built with .NET 8, following the principles of Clean Architecture. It provides a comprehensive set of features for user management, social interactions, and includes integration with external AI services. The project is containerized with Docker for easy setup and deployment.

## ✨ Key Features

-   **Authentication:** Secure user registration and login using JWT (JSON Web Tokens).
-   **User Management:** Full CRUD operations for user profiles.
-   **Social Features:** Friendship management system.
-   **AI Integration:** Connects with Gemini AI services for advanced capabilities.
-   **API Documentation:** Interactive API documentation with Swagger (OpenAPI).
-   **Rate Limiting:** Protects the API from excessive requests.
-   **Validation:** Robust request validation using FluentValidation.
-   **Clean Architecture:** A well-organized and maintainable codebase separating concerns (Domain, Application, Infrastructure, Presentation).
-   **Containerization:** Ready-to-run with Docker and Docker Compose.

## 🏗️ Architecture

This project is built upon the principles of **Clean Architecture**. This design pattern keeps business logic independent of frameworks and implementation details, resulting in a system that is:

-   **Independent of Frameworks:** The core business logic doesn't depend on .NET or any other framework.
-   **Testable:** Business rules can be tested without the UI, Database, or Web Server.
-   **Independent of UI:** The UI can change easily without changing the rest of the system.
-   **Independent of Database:** The database can be swapped out without affecting the business rules.

The solution is structured into the following layers:

-   **Core (`Domain`, `Application`):** Contains enterprise-wide business logic, entities, and application-specific business rules. It has no dependencies on other layers.
-   **Infrastructure (`Infrastructure`, `Persistence`):** Contains implementations for external concerns like databases, identity providers, and file systems. It depends on the `Application` layer.
-   **Presentation (`EEaseWebAPI.API`):** The entry point of the application, in this case, a Web API. It handles HTTP requests and depends on the `Application` layer.

## 🛠️ Technology Stack

-   **Framework:** .NET 8
-   **Architecture:** Clean Architecture
-   **API:** ASP.NET Core
-   **Database:** Entity Framework Core 8
-   **Authentication:** ASP.NET Core Identity with JWT Bearer Tokens
-   **Mediation:** MediatR for implementing the CQRS pattern.
-   **Validation:** FluentValidation
-   **API Documentation:** Swashbuckle (Swagger)
-   **Containerization:** Docker

## 🚀 Getting Started

You can run the project using either Docker (recommended) or by setting it up locally on your machine.

### Prerequisites

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Docker setup)
-   A SQL Server instance (or other database supported by EF Core if you modify the provider in `Persistence`)

---

### 🐳 Running with Docker (Recommended)

This is the simplest way to get the application running.

1.  **Clone the repository:**
    ```sh
    git clone <your-repository-url>
    cd <repository-directory>
    ```

2.  **Run with Docker Compose:**
    Open a terminal in the root directory and run:
    ```sh
    docker-compose up --build
    ```
    This command will build the Docker image and start the API service.

3.  **Access the API:**
    The API will be available at `http://localhost:8080`.

---

### 💻 Running Locally

1.  **Clone the repository:**
    ```sh
    git clone <your-repository-url>
    cd <repository-directory>
    ```

2.  **Configure Database Connection:**
    Open `Presentation/EEaseWebAPI.API/appsettings.json` and modify the `ConnectionStrings` section to point to your database.

3.  **Configure JWT Token Settings:**
    In the same `appsettings.json` file, update the `Token` section with your desired issuer, audience, and a strong security key.

4.  **Restore Dependencies:**
    Navigate to the solution's root directory and run:
    ```sh
    dotnet restore
    ```

5.  **Apply Database Migrations:**
    You need to have the `dotnet-ef` tool installed. If not, run `dotnet tool install --global dotnet-ef`.
    Navigate to the `Presentation/EEaseWebAPI.API` directory and run:
    ```sh
    dotnet ef database update --project ../../Infrastructure/EEaseWebAPI.Persistence/
    ```
    This command applies the existing migrations to your database, creating the necessary tables.

6.  **Run the application:**
    From the `Presentation/EEaseWebAPI.API` directory, run:
    ```sh
    dotnet run
    ```

## 📖 API Documentation (Swagger)

Once the application is running, you can access the interactive Swagger UI to explore and test the API endpoints.

-   **URL:** `http://localhost:8080/swagger` (if using Docker) or the URL specified in your `launchSettings.json` (e.g., `https://localhost:7123/swagger`) if running locally.

The Swagger UI includes support for JWT authentication. You can log in via the `Auth/login` endpoint to get a token, then click the "Authorize" button and enter `Bearer <your_token>` to access protected endpoints. 