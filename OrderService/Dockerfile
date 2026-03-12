FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore Presentation/OrderService.csproj
RUN dotnet publish Presentation/OrderService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5050
EXPOSE 5050

ENTRYPOINT ["dotnet", "OrderService.dll"]
