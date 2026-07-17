# TaskPulse Web Application

A beautiful, responsive task management system built with **.NET 8 MVC (Model-View-Controller)**. 

## 🚀 Features
- **Full Web UI**: Dark-mode frontend built with HTML, CSS, and Razor Pages.
- **In-Memory Caching**: Uses Local Memory Caching to drastically speed up data retrieval.
- **Dual Database Support**: Uses SQL Server (LocalDB) for local development, and automatically falls back to an In-Memory Database for free cloud deployment.
- **Clean Architecture**: Separates code into Domain, Application, Infrastructure, and API layers.

## 🛠️ How to Run Locally

### Prerequisites
- .NET 8 SDK installed on your computer.

### Quick Start
1. Open your terminal and navigate to the `TaskPulseApi` folder.
2. Run the command: `dotnet run`
3. Look at the terminal output to find the local URL (usually `http://localhost:5172`).
4. Open your web browser and go to `http://localhost:5172/Tasks` to see the beautiful web interface!

## 🌍 How to Deploy Online (Render.com)
This application is fully configured to be deployed to **Render.com** for free!
1. Create a free account on [Render.com](https://render.com).
2. Click **New +** and select **Web Service**.
3. Connect your GitHub account and select this repository.
4. Render will automatically detect the `Dockerfile` and build the application.
5. In the settings, make sure the Environment is set to `Production` (this tells the app to use the free In-Memory database so it doesn't crash looking for SQL Server).
6. Click **Deploy** and your website will be live on the internet!
