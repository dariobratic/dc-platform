FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5004
ENV ASPNETCORE_URLS=http://+:5004

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Audit.Domain/Audit.Domain.csproj src/Audit.Domain/
COPY src/Audit.Application/Audit.Application.csproj src/Audit.Application/
COPY src/Audit.Infrastructure/Audit.Infrastructure.csproj src/Audit.Infrastructure/
COPY src/Audit.API/Audit.API.csproj src/Audit.API/
RUN dotnet restore src/Audit.API/Audit.API.csproj
COPY . .
RUN dotnet publish src/Audit.API/Audit.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Audit.API.dll"]
