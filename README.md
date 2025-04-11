# EEase Web API

EEase Web API is a RESTful API service that provides a platform for creating, sharing, and discovering travel and tour routes. It allows users to create customized travel routes based on their preferences, share these routes with friends, and discover routes created by others.


## 🌟 Features

- **User Management**
  - Registration and login
  - Email verification system
  - Profile management (personal information, profile photo, preferences)
  - Customization based on user preferences and interests

- **Route Management**
  - Creating customized travel routes
  - Location and point-based route planning
  - Liking and sharing routes
  - Commenting on and evaluating routes

- **Friendship and Social Features**
  - Searching for users and adding friends
  - Sharing routes with friends
  - Multiple user support for group travels

- **Location and City Data**
  - Worldwide city database
  - Local attractions and tourist spots
  - Restaurants and dining places

- **Currency Support**
  - Working with different currencies
  - Setting currency according to user preference

- **AI Support**
  - Smart route recommendations with Gemini AI integration
  - Personalized suggestions based on user preferences

## 🛠️ Technical Features

### Architecture

The project is developed based on Clean Architecture principles and consists of the following layers:

- **Core**
  - **EEaseWebAPI.Domain**: Entities, value objects, and domain rules
  - **EEaseWebAPI.Application**: Business logic, command and query handlers (CQRS), interface definitions

- **Infrastructure**
  - **EEaseWebAPI.Infrastructure**: Integration with external services, middleware structures
  - **EEaseWebAPI.Persistence**: Data access layer, database operations

- **Presentation**
  - **EEaseWebAPI.API**: API endpoints, controllers

### Technologies Used

- **.NET 8**: Latest generation .NET platform
- **Entity Framework Core**: ORM tool
- **PostgreSQL**: Relational database
- **MediatR**: CQRS and Mediator pattern implementation
- **FluentValidation**: Data validation library
- **JWT Authentication**: Authentication and authorization
- **Swagger/OpenAPI**: API documentation
- **Gemini AI API**: Artificial intelligence integration
- **Google Places API**: Location and place information

## 🚀 Setup and Running

### Requirements

- .NET 8 SDK
- PostgreSQL
- IDE (Visual Studio, VS Code, Rider, etc.)

### Steps

1. Clone the repository:
   ```
   git clone https://github.com/deniz1976/eease-web-api.git
   cd eease-web-api
   ```

2. Install dependencies:
   ```
   dotnet restore
   ```

3. Set up the database connection:
   Update the PostgreSQL connection string in the `appsettings.json` file or set up your own environment variables.

4. Apply database migrations:
   ```
   dotnet ef database update
   ```

5. Run the API:
   ```
   dotnet run --project Presentation/EEaseWebAPI.API
   ```

## 🐳 Docker Deployment

### Prerequisites

- Docker installed on your system
- PostgreSQL instance accessible from Docker container

### Building and Running with Docker

1. Build the Docker image:
   ```bash
   docker build -t eease-web-api:latest .
   ```

2. Run the container:
   ```bash
   docker run -d -p 8080:80 --name eease-api \
     -e "ConnectionStrings__PostgreSQL=Host=your-db-host;Database=eease;Username=your-username;Password=your-password" \
     -e "ASPNETCORE_ENVIRONMENT=Production" \
     eease-web-api:latest
   ```

### Important Docker Configuration Notes

- **Database Connection**: You must provide a valid PostgreSQL connection string as shown above. The container cannot access your local database using `localhost` - use proper network addressing.

- **Required Environment Variables**:
  - `ConnectionStrings__PostgreSQL`: PostgreSQL connection string
  - `ASPNETCORE_ENVIRONMENT`: Set to `Production` for production deployments
  - `Token__SecurityKey`: Your JWT security key (if not in configuration files)
  - `Token__Issuer`: JWT issuer (if not in configuration files)
  - `Token__Audience`: JWT audience (if not in configuration files)

- **Database Connectivity**: Ensure the database instance is initialized with required migrations before starting the container. The application expects the database schema to be already set up.

- **External API Access**: If your application uses external services (Gemini AI, Google Places API), ensure to provide these API keys as environment variables.

### Docker Compose Example

For a more complete setup with PostgreSQL:

```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__PostgreSQL=Host=db;Database=eease;Username=postgres;Password=yourStrongPassword
      - Token__SecurityKey=your-very-secure-jwt-key-that-is-at-least-32-bytes
      - Token__Issuer=eease-api
      - Token__Audience=eease-clients
    depends_on:
      - db
    restart: unless-stopped
    
  db:
    image: postgres:15
    ports:
      - "5432:5432"
    environment:
      POSTGRES_PASSWORD: yourStrongPassword
      POSTGRES_USER: postgres
      POSTGRES_DB: eease
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  postgres_data:
```

Save this as `docker-compose.yml` and run:
```bash
docker-compose up -d
```

### ⚠️ Important: In-Memory Cache and Required Database Tables

This application heavily relies on in-memory caching for optimal performance, particularly for world cities and currency data. Before starting the application, please ensure:

1. The following tables are properly set up and populated in your database:
   - `AllWorldCities`: Contains city data used throughout the application
   - `Currencies`: Contains currency information for financial operations

2. If these tables are empty or missing, the application will not be able to populate the cache on startup, which may result in:
   - Errors when attempting to access location-based features
   - Failures in currency conversion operations
   - Degraded performance or partial functionality

3. To verify and populate these tables:
   - Check the database schema after running migrations
   - Use provided seed data scripts if available
   - Import required data through the database management tools

These caches are configured with the following keys in `appsettings.json`:
```json
"CacheConfiguration": {
  "AllCitiesCacheKey": "AllWorldCities_Cache",
  "AllCurrenciesCacheKey": "AllCurrencies_Cache"
}
```

## 🔐 API Authentication

The API uses JWT (JSON Web Token) based authentication. To access endpoints that require authentication:

1. Obtain a token using the `/api/Auth/Login` or `/api/Auth/Register` endpoint
2. Use the obtained token in requests inside the `Authorization` header in the format `Bearer {token}`
3. If you want to use authorization in swagger, you just paste token without Bearer word.

## 📚 API Documentation

For detailed information about all the endpoints of the API and their usage, use the Swagger/OpenAPI documentation. It is available at the `/swagger` endpoint when the application is running.

### Basic Endpoints

- **Authentication**
  - `POST /api/Auth/Register`: New user registration
  - `POST /api/Auth/Login`: User login and token retrieval

- **User Operations**
  - `GET /api/Users/GetUserInfo`: Get user information
  - `PUT /api/Users/UpdateUser`: Update user information
  - `PUT /api/Users/UpdateUserPreferences`: Update user preferences

- **Route Operations**
  - `POST /api/Route/CreateCustomRoute`: Create a custom route
  - `GET /api/Route/GetAllRoutes`: List all routes
  - `GET /api/Route/GetRouteById/{routeId}`: View a specific route

- **City and Location Operations**
  - `GET /api/City/GetAllCities`: List all cities

## 🔄 Contributing

1. Fork the project
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License. See the `LICENSE` file for details.

## 📞 Contact

[denizmine38@hotmail.com](mailto:eease@example.com)

Project Link: [https://github.com/deniz1976/eease-web-api](https://github.com/deniz1976/eease-web-api)

## 📊 Version History

- **v1.0.0** (October 2023) - First official release
  - User registration and authentication
  - Route creation and sharing
  - Basic city data
  
- **v1.1.0** (December 2023) - Social features
  - Friendship system added
  - Group route planning
  
- **v1.2.0** (February 2024) - AI integration
  - Personalized route suggestions with Gemini AI
  - Recommendations based on user preferences
  
- **v1.3.0** (April 2024) - Performance improvements
  - Cache mechanisms added
  - Speed optimizations
  - Rate limiting added

## 🔒 Security and Sensitive Information

When deploying this application, please be aware of the following security considerations:

### Secrets Management

The repository contains placeholder values in configuration files. Replace these with actual values using one of these recommended approaches:

1. **Environment Variables** (Preferred for production)
   ```bash
   # Linux/macOS
   export ConnectionStrings__PostgreSQL="your_actual_connection_string"
   
   # Windows PowerShell
   $env:ConnectionStrings__PostgreSQL="your_actual_connection_string"
   ```

2. **User Secrets** (Recommended for development)
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:PostgreSQL" "your_actual_connection_string"
   dotnet user-secrets set "Token:SecurityKey" "your_actual_security_key"
   ```

3. **Azure KeyVault or AWS Secrets Manager** (For cloud deployments)

### Critical Secrets to Replace

Make sure to replace the following placeholder values with actual secrets:
- Database connection strings
- JWT security keys
- Email service credentials
- API keys (Gemini AI, Google Places, etc.)

### Never Commit Secrets

Always ensure that actual secrets are never committed to the repository. Use `.gitignore` to exclude files that might contain sensitive information.

---
