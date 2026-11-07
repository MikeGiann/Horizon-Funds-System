FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Horizon.Funds.Api.csproj", "."]
RUN dotnet restore "Horizon.Funds.Api.csproj"
COPY . .
RUN dotnet build "Horizon.Funds.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Horizon.Funds.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Horizon.Funds.Api.dll"]