FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore Presentation/NotificationService.csproj
RUN dotnet publish Presentation/NotificationService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5051
EXPOSE 5051

ENTRYPOINT ["dotnet", "NotificationService.dll"]
