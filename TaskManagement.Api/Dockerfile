# This is a multi stage build, so lets start with the build phase
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["TaskManagement.Domain/TaskManagement.Domain.csproj", "TaskManagement.Domain/"]
COPY ["TaskManagement.Application/TaskManagement.Application.csproj", "TaskManagement.Application/"]
COPY ["TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "TaskManagement.Infrastructure/"]
COPY ["TaskManagement.Api/TaskManagement.Api.csproj", "TaskManagement.Api/"]

# Restore dependencies for the API project (it will restore all referenced projects)
RUN dotnet restore "TaskManagement.Api/TaskManagement.Api.csproj"

COPY . .

# Build the project in release mode and output into the docker container on the path /app/publish
WORKDIR /src/TaskManagement.Api
RUN dotnet build "TaskManagement.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TaskManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
