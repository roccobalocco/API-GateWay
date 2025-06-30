FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HighWayToGateWay/HighWayToGateWay.csproj", "HighWayToGateWay/"]
RUN dotnet restore "HighWayToGateWay/HighWayToGateWay.csproj"
COPY . .
WORKDIR "/src/HighWayToGateWay"
RUN dotnet build "./HighWayToGateWay.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HighWayToGateWay.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HighWayToGateWay.dll"]
