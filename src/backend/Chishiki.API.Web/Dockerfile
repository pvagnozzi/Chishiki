FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/Chishiki.API.Web/Chishiki.API.Web.csproj", "src/backend/Chishiki.API.Web/"]
COPY ["src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.ServiceDefaults/Chishiki.Infrastructure.Aspire.ServiceDefaults.csproj", "src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.ServiceDefaults/"]
RUN dotnet restore "./src/backend/Chishiki.API.Web/Chishiki.API.Web.csproj"
COPY . .
WORKDIR "/src/src/backend/Chishiki.API.Web"
RUN dotnet build "./Chishiki.API.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Chishiki.API.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chishiki.API.Web.dll"]