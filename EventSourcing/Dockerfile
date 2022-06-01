FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["EventSourcing/EventSourcing.csproj", "EventSourcing/"]
RUN dotnet restore "EventSourcing/EventSourcing.csproj"
COPY . .
WORKDIR "/src/EventSourcing"
RUN dotnet build "EventSourcing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventSourcing.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventSourcing.dll"]
