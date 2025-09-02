# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files for AuthService + shared libs
COPY ["services/AuthService/AuthService.csproj", "services/AuthService/"]
COPY ["shared/Auth0/Auth0.csproj", "shared/Auth0/"]
COPY ["shared/Database/Database.csproj", "shared/Database/"]
COPY ["shared/Utils/Utils.csproj", "shared/Utils/"]
COPY ["shared/Models/Models.csproj", "shared/Models/"]
COPY ["shared/Services/Services.csproj", "shared/Services/"]
COPY ["shared/Monitoring/Monitoring.csproj", "shared/Monitoring/"]
COPY ["shared/cache/cache.csproj", "shared/cache/"]
COPY ["shared/Message-Queue/Message-Queue.csproj", "shared/Message-Queue/"]
COPY ["shared/Microservice/Microservice.csproj", "shared/Microservice/"]

# Restore
RUN dotnet restore "services/AuthService/AuthService.csproj"

# Copy all source
COPY . .

# Build + Publish
WORKDIR "/src/services/AuthService"
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Set environment and expose port
ENV ASPNETCORE_URLS=http://+:7230
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 7230
EXPOSE 5000
ENTRYPOINT ["dotnet", "AuthService.dll"]
