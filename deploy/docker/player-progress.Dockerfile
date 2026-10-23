FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/PlayerProgress/ArrowPuzzle.PlayerProgress.Api/ArrowPuzzle.PlayerProgress.Api.csproj"
RUN dotnet publish "services/PlayerProgress/ArrowPuzzle.PlayerProgress.Api/ArrowPuzzle.PlayerProgress.Api.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5103
EXPOSE 5103
ENTRYPOINT ["dotnet", "ArrowPuzzle.PlayerProgress.Api.dll"]
