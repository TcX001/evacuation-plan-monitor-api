FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/EvacuationAPI.csproj ./
RUN dotnet restore

COPY backend/. .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

USER app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EvacuationAPI.dll"]