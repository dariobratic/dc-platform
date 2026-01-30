FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5007
ENV ASPNETCORE_URLS=http://+:5007
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AdminApi.API/AdminApi.API.csproj AdminApi.API/
RUN dotnet restore AdminApi.API/AdminApi.API.csproj
COPY . .
RUN dotnet publish AdminApi.API/AdminApi.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AdminApi.API.dll"]
