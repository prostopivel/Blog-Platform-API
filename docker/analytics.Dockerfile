FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://0.0.0.0:5003

# Create directories for data protection and storage
RUN mkdir -p /app/keys && chmod 755 /app/keys

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BlogPlatform.Analytics/BlogPlatform.Analytics.API/BlogPlatform.Analytics.API.csproj", "src/BlogPlatform.Analytics/BlogPlatform.Analytics.API/"]
COPY ["src/BlogPlatform.Analytics/BlogPlatform.Analytics.Core/BlogPlatform.Analytics.Core.csproj", "src/BlogPlatform.Analytics/BlogPlatform.Analytics.Core/"]
COPY ["src/BlogPlatform.Analytics/BlogPlatform.Analytics.Infrastructure/BlogPlatform.Analytics.Infrastructure.csproj", "src/BlogPlatform.Analytics/BlogPlatform.Analytics.Infrastructure/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Common/BlogPlatform.Shared.Common.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Common/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Grpc/BlogPlatform.Shared.Grpc.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Grpc/"]
RUN dotnet restore "src/BlogPlatform.Analytics/BlogPlatform.Analytics.API/BlogPlatform.Analytics.API.csproj"

COPY . .
WORKDIR "/src/src/BlogPlatform.Analytics/BlogPlatform.Analytics.API"
RUN dotnet build "BlogPlatform.Analytics.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogPlatform.Analytics.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Ensure keys directory exists
RUN mkdir -p /app/keys && chmod 755 /app/keys

ENTRYPOINT ["dotnet", "BlogPlatform.Analytics.API.dll"]