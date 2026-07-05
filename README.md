# TaskPulse Azure Microservices

A professional, enterprise-grade task management system built with .NET 8, Clean Architecture, and Microsoft Azure Cloud services.

## 🚀 Features & Technologies Demonstrated

- **Clean Architecture**: The code is strictly separated into Domain, Application, Infrastructure, and API layers to ensure it is highly maintainable.
- **Azure SQL Database**: Uses Entity Framework Core to store task data permanently in an Azure SQL Database.
- **Redis Caching**: Uses `IDistributedCache` to temporarily store API responses, drastically speeding up the "Get All Tasks" web requests.
- **OData Integration**: Allows users to filter and sort data directly from the API web address using the official `Microsoft.AspNetCore.OData` package.
- **Azure Functions (Queue Storage)**: Uses a `QueueTrigger` to automatically run background code the exact second a new note is dropped into an Azure Storage bucket.
- **Notification Microservice**: A completely independent microservice designed to receive data from the Azure Function and process email notifications.
- **CI/CD Pipeline**: Automated build workflows configured using GitHub Actions (`build.yml`) to ensure code quality on every upload.

## 🛠️ Getting Started Locally

### Prerequisites
- .NET 8 SDK
- LocalDB (built into Windows) for simulating the Azure SQL database.
- Azure Storage Emulator (Azurite) for simulating the cloud buckets.
- Redis Server running on `localhost:6379`.

### How to Run
1. Run `dotnet ef database update --project TaskPulse.Infrastructure --startup-project TaskPulseApi` to build the database tables.
2. Run `dotnet run` inside the `TaskPulseApi` folder to start the main API.
3. Run `func start` inside the `TaskPulse.Functions` folder to start the background worker.
4. Run `dotnet run` inside the `TaskPulse.Notification` folder to start the microservice.

## 🔄 Architecture Flow
When a user creates a new task:
1. The Task is securely saved to the Azure SQL Database.
2. The Redis cache is automatically cleared to guarantee the frontend receives fresh data next time.
3. A text note is dropped into an Azure Storage Queue bucket.
4. The Azure Function instantly detects the note, converts it to JSON text, and fires it over the network to the Notification Microservice.
