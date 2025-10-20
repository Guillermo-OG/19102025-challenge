
FROM node:18-alpine AS ui
WORKDIR /ui
COPY src/frontend/package*.json ./
RUN npm ci --no-audit --prefer-offline
COPY src/frontend ./
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src


COPY src/backend/GameOfLife.API/GameOfLife.API.csproj src/backend/GameOfLife.API/
COPY src/backend/GameOfLife.Core/GameOfLife.Core.csproj src/backend/GameOfLife.Core/
RUN dotnet restore src/backend/GameOfLife.API/GameOfLife.API.csproj


COPY src/backend/ ./src/backend/

WORKDIR /src/src/backend/GameOfLife.API
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

COPY --from=ui /ui/build ./wwwroot


ENV ASPNETCORE_URLS=http://+:80
ENV Data__Path=/app/data/boards.json

EXPOSE 80
ENTRYPOINT ["dotnet", "GameOfLife.API.dll"]