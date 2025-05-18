FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /code
COPY ../../ .
RUN dotnet publish ./src/GEOrchestrator.TaskManager/ -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "GEOrchestrator.TaskManager.dll"]