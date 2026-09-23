FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GymAndTrainingApi/Gym.Api.csproj", "GymAndTrainingApi/"]
COPY ["Gym.Application/Gym.Application.csproj", "Gym.Application/"]
COPY ["Gym.Infrastructure/Gym.Infrastructure.csproj", "Gym.Infrastructure/"]
COPY ["Gym.Domain/Gym.Domain.csproj", "Gym.Domain/"]

RUN dotnet restore "GymAndTrainingApi/Gym.Api.csproj"

COPY . .

WORKDIR /src/GymAndTrainingApi
RUN dotnet publish "Gym.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Gym.Api.dll"]
