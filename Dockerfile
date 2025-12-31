# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies (cached layer)
COPY ["src/api/Sebigy.Dialogisera.Api.csproj", "src/api/"]
RUN dotnet restore "src/api/Sebigy.Dialogisera.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/api"
RUN dotnet build "Sebigy.Dialogisera.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sebigy.Dialogisera.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Cloud Run sets PORT environment variable (defaults to 8080)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy published app
COPY --from=publish /app/publish .

# Run as non-root user for security
USER $APP_UID

ENTRYPOINT ["dotnet", "Sebigy.Dialogisera.Api.dll"]