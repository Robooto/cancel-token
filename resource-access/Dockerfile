FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["resource-access/resource-access.csproj", "resource-access/"]
RUN dotnet restore "resource-access/resource-access.csproj"
COPY . .
WORKDIR "/src/resource-access"
RUN dotnet build "resource-access.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "resource-access.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "resource-access.dll"]
