# Epic5Task Project

This project is a .NET 9 Web API designed for event and ticket management. It follows Clean Architecture principles and uses modern .NET libraries for a robust, maintainable solution.

## Project Overview

The system allows users to:
- **Events**: Create, update, and retrieve events with various ticket capacities and prices.
- **Tickets**: Create (book), confirm, and cancel tickets.
- **Users**: Automated user creation based on the phone number provided in the context (via custom header).

## Architectural & Technical Decisions

### Clean Architecture
The solution is organized into four main layers:
1.  **Domain**: Contains entities, enums, and aggregate roots. It has no dependencies on other layers.
2.  **Application**: Contains business logic, MediatR commands/queries, handlers, validators, and interfaces.
3.  **Infrastructure**: Implements the interfaces defined in the Application layer. Currently uses an in-memory data store for simplicity.
4.  **Api**: The entry point, containing controllers, middlewares, and configuration.

### Key Technical Choices
-   **MediatR & CQRS**: Used for decoupling the request/response handling and implementing the CQRS pattern.
-   **FluentValidation**: Integrated into the MediatR pipeline to ensure all incoming requests are validated before reaching the handlers.
-   **In-Memory Data Store**: A static `Data` class in the Infrastructure layer manages the state during the application's runtime.
-   **Custom Middleware**: 
    -   `ExceptionMiddleware`: Provides global error handling and consistent API responses.
    -   `PhoneNumberHeaderMiddleware`: Extracts the user's phone number from a custom `X-Phone-Number` header to provide user context.
-   **Testing Strategy**:
    -   **Unit Tests**: Cover individual providers and validators.
    -   **Integration Tests**: Verify the full command/query pipeline including handlers and their interaction with the real data providers.

## Technologies Used
-   **.NET 9**
-   **MediatR** for Command/Query dispatching.
-   **FluentValidation** for request validation.
-   **NUnit**, **Moq**, and **AwesomeAssertions** for testing.
-   **Swashbuckle (Swagger)** for API documentation.

## Getting Started

### Prerequisites
-   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Running the Project
1.  Navigate to the project root directory.
2.  Run the following command:
    ```bash
    dotnet run --project src/Epic5Task.Api/Epic5Task.Api.csproj
    ```
3.  The API will be available at `https://localhost:5001` (or the port specified in `launchSettings.json`).

### API Documentation (Swagger)
Once the application is running, you can access the interactive Swagger UI at:
```
https://localhost:5001/swagger
```
Use this UI to explore the available endpoints, view request/response models, and test the API directly.

**Note**: Most operations require an `X-Phone-Number` header to identify the user. In Swagger, you can provide this value in the header field.

## Project Structure
```text
src/
├── Epic5Task.Api/           # API Controllers, Middlewares, Program.cs
├── Epic5Task.Application/   # Business Logic, Commands, Queries, Validators
├── Epic5Task.Domain/        # Entities, Enums, Aggregate Roots
├── Epic5Task.Infrastructure/# Data Providers, In-Memory Data Context
└── Tests/                   # Unit and Integration Tests
```
