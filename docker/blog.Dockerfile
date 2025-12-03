FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002
EXPOSE 6002
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://0.0.0.0:5002

# Create directories for data protection and storage
RUN mkdir -p /app/keys && chmod 755 /app/keys

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BlogPlatform.Blog/BlogPlatform.Blog.API/BlogPlatform.Blog.API.csproj", "src/BlogPlatform.Blog/BlogPlatform.Blog.API/"]
COPY ["src/BlogPlatform.Blog/BlogPlatform.Blog.Core/BlogPlatform.Blog.Core.csproj", "src/BlogPlatform.Blog/BlogPlatform.Blog.Core/"]
COPY ["src/BlogPlatform.Blog/BlogPlatform.Blog.Infrastructure/BlogPlatform.Blog.Infrastructure.csproj", "src/BlogPlatform.Blog/BlogPlatform.Blog.Infrastructure/"]
COPY ["src/BlogPlatform.Blog/BlogPlatform.Blog.Grpc/BlogPlatform.Blog.Grpc.csproj", "src/BlogPlatform.Blog/BlogPlatform.Blog.Grpc/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Common/BlogPlatform.Shared.Common.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Common/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Data/BlogPlatform.Shared.Data.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Data/"]
COPY ["src/BlogPlatform.Shared/BlogPlatform.Shared.Grpc/BlogPlatform.Shared.Grpc.csproj", "src/BlogPlatform.Shared/BlogPlatform.Shared.Grpc/"]
RUN dotnet restore "src/BlogPlatform.Blog/BlogPlatform.Blog.API/BlogPlatform.Blog.API.csproj"

COPY . .
WORKDIR "/src/src/BlogPlatform.Blog/BlogPlatform.Blog.API"
RUN dotnet build "BlogPlatform.Blog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogPlatform.Blog.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Ensure keys directory exists
RUN mkdir -p /app/keys && chmod 755 /app/keys

ENTRYPOINT ["dotnet", "BlogPlatform.Blog.API.dll"]