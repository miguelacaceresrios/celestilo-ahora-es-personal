# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo del proyecto y restaurar dependencias
COPY backend.csproj .
RUN dotnet restore

# Copiar el resto del código y compilar
COPY . .
RUN dotnet build -c Release -o /app/build

# Etapa 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiar los archivos publicados
COPY --from=publish /app/publish .

# Exponer el puerto
EXPOSE 5019

# Configurar variables de entorno para ASP.NET
ENV ASPNETCORE_URLS=http://+:5019
ENV ASPNETCORE_ENVIRONMENT=Production

# Punto de entrada
ENTRYPOINT ["dotnet", "backend.dll"]