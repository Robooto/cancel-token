FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["manager-api/manager-api.csproj", "manager-api/"]
RUN dotnet restore "manager-api/manager-api.csproj"
COPY . .
WORKDIR "/src/manager-api"
RUN dotnet build "manager-api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "manager-api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "manager-api.dll"]
