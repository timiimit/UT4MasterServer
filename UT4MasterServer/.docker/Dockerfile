#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UT4MasterServer.csproj", "UT4MasterServer/"]
RUN dotnet restore "UT4MasterServer/UT4MasterServer.csproj"
WORKDIR "/src/UT4MasterServer"
COPY . .
# We don't use build
#RUN dotnet build "UT4MasterServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UT4MasterServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UT4MasterServer.dll"]
