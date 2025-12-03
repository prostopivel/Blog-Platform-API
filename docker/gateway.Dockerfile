FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

# Create directories for data protection and storage
RUN mkdir -p /app/keys && chmod 755 /app/keys

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BlogPlatform.ApiGateway/BlogPlatform.ApiGateway.API/BlogPlatform.ApiGateway.API.csproj", "src/BlogPlatform.ApiGateway/BlogPlatform.ApiGateway.API/"]
COPY ["src/BlogPlatform.ApiGateway/BlogPlatform.ApiGateway.Core/BlogPlatform.ApiGateway.Core.csproj", "src/BlogPlatform.ApiGateway/BlogPlatform.ApiGateway.Core/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Common/BlogPlatform.Shared.Common.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Common/"]
RUN dotnet restore "src/BlogPlatform.ApiGateway/BlogPlatform.ApiGateway.API/BlogPlatform.ApiGateway.API.csproj"

COPY . .
WORKDIR "/src/src/BlogPlatform.ApiGateway/BlogPlatform.ApiGateway.API"
RUN dotnet build "BlogPlatform.ApiGateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogPlatform.ApiGateway.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Ensure keys directory exists
RUN mkdir -p /app/keys && chmod 755 /app/keys

ENTRYPOINT ["dotnet", "BlogPlatform.ApiGateway.API.dll"]