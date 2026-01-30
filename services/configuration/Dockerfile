FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5006
ENV ASPNETCORE_URLS=http://+:5006

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Configuration.API/Configuration.API.csproj Configuration.API/
RUN dotnet restore Configuration.API/Configuration.API.csproj
COPY . .
RUN dotnet publish Configuration.API/Configuration.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Configuration.API.dll"]
