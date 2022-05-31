FROM mcr.microsoft.com/dotnet/sdk:3.1.419 AS build
WORKDIR /code
COPY ../../ .
RUN ls
RUN dotnet publish ./src/GEOrchestrator.WorkflowManager/ -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:3.1.25
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "GEOrchestrator.WorkflowManager.dll"]