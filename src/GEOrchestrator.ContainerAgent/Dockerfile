FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS base

# copy csproj and restore as distinct layers
WORKDIR /src
#RUN dotnet restore "GEOrchestrator.ContainerAgent/GEOrchestrator.ContainerAgent.csproj"
COPY src/ .
WORKDIR /src/GEOrchestrator.ContainerAgent/

RUN dotnet publish "GEOrchestrator.ContainerAgent.csproj" --runtime linux-x64 -c Release -o /out -p:PublishSingleFile=true -p:DebugType=None --self-contained true

# build runtime image
FROM alpine:3.7
WORKDIR /function_runner
COPY --from=base /out ./