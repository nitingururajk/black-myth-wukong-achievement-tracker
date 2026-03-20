# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY bmw_web/bmw_web.csproj bmw_web/
COPY vendor/blackwukong-dlls/ vendor/blackwukong-dlls/

RUN dotnet restore bmw_web/bmw_web.csproj

COPY bmw_web/ bmw_web/

RUN dotnet publish bmw_web/bmw_web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "bmw_web.dll"]
