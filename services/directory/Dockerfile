FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Directory.Domain/Directory.Domain.csproj src/Directory.Domain/
COPY src/Directory.Application/Directory.Application.csproj src/Directory.Application/
COPY src/Directory.Infrastructure/Directory.Infrastructure.csproj src/Directory.Infrastructure/
COPY src/Directory.API/Directory.API.csproj src/Directory.API/
RUN dotnet restore src/Directory.API/Directory.API.csproj
COPY . .
RUN dotnet publish src/Directory.API/Directory.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Directory.API.dll"]
