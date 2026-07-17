# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and restore dependencies
COPY *.sln .
COPY TaskPulse.Domain/*.csproj ./TaskPulse.Domain/
COPY TaskPulse.Application/*.csproj ./TaskPulse.Application/
COPY TaskPulse.Infrastructure/*.csproj ./TaskPulse.Infrastructure/
COPY TaskPulseApi/*.csproj ./TaskPulseApi/
RUN dotnet restore

# Copy the rest of the code and build
COPY . .
WORKDIR /app/TaskPulseApi
RUN dotnet publish -c Release -o out

# Use the official runtime image for the final build
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/TaskPulseApi/out ./

# Expose port 80 and start the app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
# Set environment to Production so it uses the free In-Memory database!
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TaskPulseApi.dll"]
