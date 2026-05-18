# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/PFP.Domain/PFP.Domain.csproj src/PFP.Domain/
COPY src/PFP.Application/PFP.Application.csproj src/PFP.Application/
COPY src/PFP.Infrastructure/PFP.Infrastructure.csproj src/PFP.Infrastructure/
COPY src/PFP.API/PFP.API.csproj src/PFP.API/

RUN dotnet restore src/PFP.API/PFP.API.csproj

COPY src/ src/

RUN dotnet publish src/PFP.API/PFP.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["/app/docker-entrypoint.sh"]
