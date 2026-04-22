FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5268
ENV ASPNETCORE_URLS=http://+:5268

USER app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src

# Копируем решение и проекты
COPY ["DripRateServer.slnx", "./"]
COPY ["Api/Api.csproj", "Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["SharedSettings/SharedSettings.csproj", "SharedSettings/"]

# Restore для всего решения
RUN dotnet restore

# Копируем исходники
COPY . .

# Собираем
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c $configuration -o /app/build --no-incremental

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Api.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
