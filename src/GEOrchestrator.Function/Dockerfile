FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS base

# copy csproj and restore as distinct layers
WORKDIR /src
#COPY ["src/GEOrchestrator.Function/GEOrchestrator.Function.csproj", "GEOrchestrator.Function/"]
#COPY ["src/GEOrchestrator.Core/GEOrchestrator.Core.csproj", "GEOrchestrator.Core/"]
#RUN dotnet restore "GEOrchestrator.Function/GEOrchestrator.Function.csproj"
COPY src/ .
WORKDIR /src/GEOrchestrator.Function/

RUN dotnet publish "GEOrchestrator.Function.csproj" --runtime linux-x64 -c Release -o /out -p:PublishSingleFile=true -p:DebugType=None --self-contained true

# build runtime image
FROM alpine:3.7
WORKDIR /function_runner
COPY --from=base /out ./