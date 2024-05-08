#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV MYSQL_CONN="Server=MYSQL5044.site4now.net;Database=db_aa82e7_utbasi;Uid=aa82e7_utbasi;Pwd=Kb!Ega7NWAt7t6@"
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN apt-get update && apt-get install -y libfontconfig1

COPY ["AsistenciaProcess.csproj", "."]
RUN dotnet restore "./AsistenciaProcess.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./AsistenciaProcess.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AsistenciaProcess.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AsistenciaProcess.dll"]