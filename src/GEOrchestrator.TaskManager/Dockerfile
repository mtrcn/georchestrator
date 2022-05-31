FROM mcr.microsoft.com/dotnet/sdk:3.1.419 AS build
WORKDIR /code
COPY ../../ .
RUN ls
RUN dotnet publish ./src/GEOrchestrator.TaskManager/ -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:3.1.25
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "GEOrchestrator.TaskManager.dll"]