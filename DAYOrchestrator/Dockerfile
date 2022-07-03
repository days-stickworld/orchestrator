FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV REDIS_HOST=172.17.0.1
ENV PUBLIC_IP=172.17.0.1

RUN apt-get update
RUN apt-get install docker.io -y

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DAYOrchestrator.csproj", "DAYOrchestrator.csproj"]
RUN dotnet restore "DAYOrchestrator.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "DAYOrchestrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DAYOrchestrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DAYOrchestrator.dll"]
