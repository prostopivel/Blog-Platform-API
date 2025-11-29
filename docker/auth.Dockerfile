FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 6001
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://0.0.0.0:5001

# Create directories for data protection and storage
RUN mkdir -p /app/keys && chmod 755 /app/keys

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BlogPlatform.Auth/BlogPlatform.Auth.API/BlogPlatform.Auth.API.csproj", "src/BlogPlatform.Auth/BlogPlatform.Auth.API/"]
COPY ["src/BlogPlatform.Auth/BlogPlatform.Auth.Core/BlogPlatform.Auth.Core.csproj", "src/BlogPlatform.Auth/BlogPlatform.Auth.Core/"]
COPY ["src/BlogPlatform.Auth/BlogPlatform.Auth.Infrastructure/BlogPlatform.Auth.Infrastructure.csproj", "src/BlogPlatform.Auth/BlogPlatform.Auth.Infrastructure/"]
COPY ["src/BlogPlatform.Auth/BlogPlatform.Auth.Grpc/BlogPlatform.Auth.Grpc.csproj", "src/BlogPlatform.Auth/BlogPlatform.Auth.Grpc/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Common/BlogPlatform.Shared.Common.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Common/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Messaging/BlogPlatform.Shared.Messaging.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Messaging/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Grpc/BlogPlatform.Shared.Grpc.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Grpc/"]
RUN dotnet restore "src/BlogPlatform.Auth/BlogPlatform.Auth.API/BlogPlatform.Auth.API.csproj"

COPY . .
WORKDIR "/src/src/BlogPlatform.Auth/BlogPlatform.Auth.API"
RUN dotnet build "BlogPlatform.Auth.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogPlatform.Auth.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Ensure keys directory exists
RUN mkdir -p /app/keys && chmod 755 /app/keys

ENTRYPOINT ["dotnet", "BlogPlatform.Auth.API.dll"]