#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["UT4MasterServer/UT4MasterServer.csproj", "UT4MasterServer/"]
COPY ["UT4MasterServer.Authentication/UT4MasterServer.Authentication.csproj", "UT4MasterServer.Authentication/"]
COPY ["UT4MasterServer.Common/UT4MasterServer.Common.csproj", "UT4MasterServer.Common/"]
COPY ["UT4MasterServer.Models/UT4MasterServer.Models.csproj", "UT4MasterServer.Models/"]
COPY ["UT4MasterServer.Serializers/UT4MasterServer.Serializers.csproj", "UT4MasterServer.Serializers/"]
COPY ["UT4MasterServer.Services/UT4MasterServer.Services.csproj", "UT4MasterServer.Services/"]
RUN dotnet restore "UT4MasterServer/UT4MasterServer.csproj"
COPY . .
WORKDIR "/src/UT4MasterServer"
# We don't use build
#RUN dotnet build "UT4MasterServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UT4MasterServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UT4MasterServer.dll"]
