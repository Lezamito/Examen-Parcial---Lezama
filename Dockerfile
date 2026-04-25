# Usa el SDK de .NET 8 para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia los archivos del proyecto y restaura las dependencias
COPY *.csproj ./
RUN dotnet restore

# Copia el resto del código y compila
COPY . ./
RUN dotnet publish -c Release -o out

# Usa el runtime de ASP.NET Core 8 para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expone el puerto (Render lo seteará vía variable de entorno PORT)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Ejecuta la aplicación
ENTRYPOINT ["dotnet", "Parcial.dll"]
