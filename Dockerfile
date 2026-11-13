# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo do projeto e restaura dependencias
COPY ["BairroVivo.API/BairroVivo.API.csproj", "BairroVivo.API/"]
RUN dotnet restore "BairroVivo.API/BairroVivo.API.csproj"

# Copia o restante dos arquivos e faz o publish
COPY . .
WORKDIR "/src/BairroVivo.API"
RUN dotnet publish "BairroVivo.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BairroVivo.API.dll"]
