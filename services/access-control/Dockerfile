FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003
ENV ASPNETCORE_URLS=http://+:5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/AccessControl.Domain/AccessControl.Domain.csproj src/AccessControl.Domain/
COPY src/AccessControl.Application/AccessControl.Application.csproj src/AccessControl.Application/
COPY src/AccessControl.Infrastructure/AccessControl.Infrastructure.csproj src/AccessControl.Infrastructure/
COPY src/AccessControl.API/AccessControl.API.csproj src/AccessControl.API/
RUN dotnet restore src/AccessControl.API/AccessControl.API.csproj
COPY . .
RUN dotnet publish src/AccessControl.API/AccessControl.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AccessControl.API.dll"]
