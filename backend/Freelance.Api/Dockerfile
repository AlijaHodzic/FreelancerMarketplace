FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["backend/Freelance.Api/Freelance.Api.csproj", "backend/Freelance.Api/"]
COPY ["backend/Freelance.Application/Freelance.Application.csproj", "backend/Freelance.Application/"]
COPY ["backend/Freelance.Infrastructure/Freelance.Infrastructure.csproj", "backend/Freelance.Infrastructure/"]
COPY ["backend/Freelance.Domain/Freelance.Domain.csproj", "backend/Freelance.Domain/"]
RUN dotnet restore "backend/Freelance.Api/Freelance.Api.csproj"

COPY . .
RUN dotnet publish "backend/Freelance.Api/Freelance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Freelance.Api.dll"]
