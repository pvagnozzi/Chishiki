# Vedere https://aka.ms/customizecontainer per informazioni su come personalizzare il contenitore di debug e su come Visual Studio usa questo Dockerfile per compilare le immagini per un debug più rapido.

# Questa fase viene usata durante l'esecuzione da Visual Studio in modalità rapida (impostazione predefinita per la configurazione di debug)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 11111
EXPOSE 30000


# Questa fase viene usata per compilare il progetto di servizio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/Chishiki.Host/Chishiki.Host.csproj", "src/backend/Chishiki.Host/"]
COPY ["src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.ServiceDefaults/Chishiki.Infrastructure.Aspire.ServiceDefaults.csproj", "src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.ServiceDefaults/"]
RUN dotnet restore "src/backend/Chishiki.Host/Chishiki.Host.csproj"
COPY . .
WORKDIR "/src/src/backend/Chishiki.Host"
RUN dotnet build "./Chishiki.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Questa fase viene usata per pubblicare il progetto di servizio da copiare nella fase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Chishiki.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Questa fase viene usata nell'ambiente di produzione o durante l'esecuzione da Visual Studio in modalità normale (impostazione predefinita quando non si usa la configurazione di debug)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chishiki.Host.dll"]