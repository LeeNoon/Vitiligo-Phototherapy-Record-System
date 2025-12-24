# Use ASP.NET Core Runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VitiligoTracker.csproj", "./"]
RUN dotnet restore "VitiligoTracker.csproj"
COPY . .
RUN dotnet build "VitiligoTracker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VitiligoTracker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Create a directory for the database
RUN mkdir -p /app/data
ENTRYPOINT ["dotnet", "VitiligoTracker.dll"]
