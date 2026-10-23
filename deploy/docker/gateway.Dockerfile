FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/Gateway/ArrowPuzzle.Gateway.Api/ArrowPuzzle.Gateway.Api.csproj"
RUN dotnet publish "services/Gateway/ArrowPuzzle.Gateway.Api/ArrowPuzzle.Gateway.Api.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5100
EXPOSE 5100
ENTRYPOINT ["dotnet", "ArrowPuzzle.Gateway.Api.dll"]
