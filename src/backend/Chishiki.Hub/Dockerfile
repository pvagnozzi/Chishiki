FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/Chishiki.Hub/Chishiki.Hub.csproj", "src/backend/Chishiki.Hub/"]
COPY ["src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.ServiceDefaults/Chishiki.Infrastructure.Aspire.ServiceDefaults.csproj", "src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.ServiceDefaults/"]
COPY ["src/shared/Chishiki.Hub.Contracts/Chishiki.Hub.Contracts.csproj", "src/shared/Chishiki.Hub.Contracts/"]
RUN dotnet restore "./src/backend/Chishiki.Hub/Chishiki.Hub.csproj"
COPY . .
WORKDIR "/src/src/backend/Chishiki.Hub"
RUN dotnet build "./Chishiki.Hub.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Chishiki.Hub.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chishiki.Hub.dll"]
