# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Employee.API/Employee.API.csproj", "Employee.API/"]
COPY ["Employee.Services/Employee.Services.csproj", "Employee.Services/"]
COPY ["Employee.Domain/Employee.Domain.csproj", "Employee.Domain/"]
COPY ["Employee.Contracts/Employee.Contracts.csproj", "Employee.Contracts/"]
COPY ["Employee.Mapping/Employee.Mapping.csproj", "Employee.Mapping/"]

# Restore dependencies
RUN dotnet restore "Employee.API/Employee.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Employee.API"
RUN dotnet build "Employee.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Employee.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Employee.API.dll"]