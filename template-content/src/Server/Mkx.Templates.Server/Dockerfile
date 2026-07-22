# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/Server/Mkx.Templates.Server/Mkx.Templates.Server.csproj", "src/Server/Mkx.Templates.Server/"]
COPY ["src/Sdk/Mkx.Templates.Sdk.Server.Api/Mkx.Templates.Sdk.Server.Api.csproj", "src/Sdk/Mkx.Templates.Sdk.Server.Api/"]
COPY ["src/Sdk/Mkx.Templates.Sdk.Server.Application/Mkx.Templates.Sdk.Server.Application.csproj", "src/Sdk/Mkx.Templates.Sdk.Server.Application/"]
COPY ["src/Sdk/Mkx.Templates.Sdk.Server.Infrastructure/Mkx.Templates.Sdk.Server.Infrastructure.csproj", "src/Sdk/Mkx.Templates.Sdk.Server.Infrastructure/"]
COPY ["src/Sdk/Mkx.Templates.Sdk.Server.Domain/Mkx.Templates.Sdk.Server.Domain.csproj", "src/Sdk/Mkx.Templates.Sdk.Server.Domain/"]
COPY ["src/Sdk/Mkx.Templates.Sdk.Server.Shared/Mkx.Templates.Sdk.Server.Shared.csproj", "src/Sdk/Mkx.Templates.Sdk.Server.Shared/"]
COPY ["src/Shared/Mkx.Templates.Sdk.Shared/Mkx.Templates.Sdk.Shared.csproj", "src/Shared/Mkx.Templates.Sdk.Shared/"]
COPY ["src/Client/Mkx.Templates.Client/Mkx.Templates.Client.csproj", "src/Client/Mkx.Templates.Client/"]
COPY ["src/Shared/Mkx.Templates.Shared/Mkx.Templates.Shared.csproj", "src/Shared/Mkx.Templates.Shared/"]
COPY ["src/Server/Mkx.Templates.Application/Mkx.Templates.Application.csproj", "src/Server/Mkx.Templates.Application/"]
COPY ["src/Core/Mkx.Templates.Infrastructure/Mkx.Templates.Infrastructure.csproj", "src/Core/Mkx.Templates.Infrastructure/"]
COPY ["src/Core/Mkx.Templates.Domain/Mkx.Templates.Domain.csproj", "src/Core/Mkx.Templates.Domain/"]
RUN dotnet restore "./src/Server/Mkx.Templates.Server/Mkx.Templates.Server.csproj"
COPY . .
WORKDIR "/src/src/Server/Mkx.Templates.Server"
RUN dotnet build "./Mkx.Templates.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Mkx.Templates.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mkx.Templates.Server.dll"]