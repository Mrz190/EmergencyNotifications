# Используем образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Копируем файлы проекта
COPY *.csproj ./
RUN dotnet restore

# Копируем все остальные файлы и собираем проект
COPY . ./
RUN dotnet publish -c Release -o out

# Используем образ ASP.NET Core для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Копируем опубликованные файлы из предыдущей стадии сборки
COPY --from=build /app/out .

# Запускаем приложение
ENTRYPOINT ["dotnet", "API.dll"]
