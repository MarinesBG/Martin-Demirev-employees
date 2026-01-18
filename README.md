# Employee Collaboration Tracker

A full-stack application for analyzing employee collaboration patterns from CSV data. The system identifies which pairs of employees have worked together the longest across multiple projects.

## 🚀 Features

- **CSV Upload & Processing**: Upload employee project data in CSV format
- **Overlap Calculation**: Automatically calculates work overlap between employees
- **Multi-Project Support**: Tracks collaboration across multiple projects
- **Date Format Flexibility**: Supports multiple date formats (ISO, US, EU)
- **RESTful API**: Clean .NET 8 Web API with Swagger documentation
- **React Frontend**: Modern React + TypeScript UI with Vite
- **Docker Support**: Fully containerized with Docker Compose
- **Comprehensive Testing**: Unit tests, integration tests, and health checks
- **Health Monitoring**: Built-in health check endpoints

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Quick Start with Docker](#quick-start-with-docker)
- [Development Setup](#development-setup)
- [Running Tests](#running-tests)
- [API Documentation](#api-documentation)
- [CSV Format](#csv-format)
- [Architecture](#architecture)
- [Troubleshooting](#troubleshooting)
- [Technologies Used](#technologies-used)

## 🔧 Prerequisites

### For Docker (Recommended):
- [Docker](https://www.docker.com/get-started) (20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) (2.0+)

### For Local Development:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (20.x or later)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 📁 Project Structure

```
Martin-Demirev-employees/
├── Employee/                          # .NET backend
│   ├── Employee.API/                  # Web API project
│   │   ├── Controllers/               # API controllers (endpoints)
│   │   ├── Filters/                   # Swagger / action / exception filters
│   │   ├── Dockerfile                 # Dockerfile for API
│   │   └── .dockerignore
│   ├── Employee.Services/             # Business logic layer
│   │   └── Services/                  # Service implementations
│   ├── Employee.Domain/               # Domain models (entities, value objects)
│   ├── Employee.Contracts/            # DTOs and interfaces (contracts between layers)
│   ├── Employee.Mapping/              # AutoMapper profiles / mapping configurations
│   ├── Employee.Tests/                # Unit tests
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Mapping/
│   │   └── Filters/
│   └── Employee.IntegrationTests/     # Integration tests
│       ├── Controllers/
│       ├── HealthChecks/
│       └── Api/
├── frontend/                          # React frontend
│   ├── src/                           # Source code
│   ├── Dockerfile                     # Frontend Dockerfile
│   ├── nginx.conf                     # Nginx config for production
│   └── .dockerignore
├── docker-compose.yml                 # Production docker-compose
├── docker-compose.dev.yml             # Development docker-compose
└── README.md                          # Repo README / dev instructions
```

## 🐳 Quick Start with Docker

### 1. Clone the Repository

git clone https://github.com/MarinesBG/Martin-Demirev-employees.git cd Martin-Demirev-employees

### 2. Build and Run with Docker Compose

#### Production Mode (Optimized Build):

Build and start all services

docker-compose up --build -d

View logs

docker-compose logs -f

Stop all services

docker-compose down

#### Development Mode (with Hot Reload):

Start with development configuration

docker-compose -f docker-compose.yml -f docker-compose.dev.yml up --build

Stop services

docker-compose down

### 3. Access the Application

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost | React application |
| **API** | http://localhost:5000 | Backend API |
| **Swagger** | http://localhost:5000/swagger | API Documentation |
| **Health Check** | http://localhost:5000/health | API Health Status |

### 4. Stop and Clean Up

Stop containers

docker-compose down

Remove volumes (clean slate)

docker-compose down -v

Remove images

docker-compose down --rmi all

## 💻 Development Setup

### Backend (.NET API)

cd Employee

Restore dependencies

dotnet restore

Build solution

dotnet build

Run API

cd Employee.API dotnet run

Run tests

dotnet test

Run with watch mode

dotnet watch run --project Employee.API

### Frontend (React + Vite)

cd frontend

Install dependencies

npm install

Run development server

npm run dev

Build for production

npm run build

Preview production build

npm run preview

## 🧪 Running Tests

### Run All Tests

From repository root

dotnet test

With detailed output

dotnet test --verbosity detailed

With code coverage

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

### Run Specific Test Projects

Unit tests only

dotnet test Employee.Tests

Integration tests only

dotnet test Employee.IntegrationTests

Specific test class

dotnet test --filter "FullyQualifiedName~EmployeeControllerTests"

### Test Coverage

- **Unit Tests**: 49 tests covering services, controllers, mapping, and filters
- **Integration Tests**: 22 tests for end-to-end API scenarios
- **Total**: 71+ tests with comprehensive coverage

## 📚 API Documentation

### Endpoints

#### Upload CSV File

POST /api/employee/upload Content-Type: multipart/form-data

file: employees.csv

# **Response (Success):**

{ "employeeIdA": 1, "employeeIdB": 2, "totalDays": 275, "projects": [ { "employeeId1": 1, "employeeId2": 2, "projectId": 10, "daysWorked": 275 } ] }

# **Response (No Pairs Found):**

{ "message": "No employee pairs found. Employees must work on the same project with overlapping dates.", "topPair": null, "allPairs": [], "totalPairsFound": 0 }

#### Health Check

GET /health          
# General health status GET /health/ready    
# Readiness probe GET /health/live     
# Liveness probe

**Response:**
{ "status": "Healthy", "totalDuration": "00:00:00.1234567", "entries": { "self": { "data": {}, "duration": "00:00:00.0123456", "status": "Healthy" } } }


### HTTP Status Codes
```
| Code | Description |
|------|-------------|
| 200 | Success |
| 400 | Bad Request (invalid CSV, wrong file type, invalid dates) |
| 413 | Payload Too Large (file > 10MB) |
| 499 | Client Closed Request (upload cancelled) |
| 500 | Internal Server Error |
```
## 📄 CSV Format

### Required Format

EmpID,ProjectID,DateFrom,DateTo 
1,10,2020-01-01,2020-12-31 
2,10,2020-03-01,2020-11-30 
3,15,2021-01-01,2021-06-30


### Field Descriptions
```
| Field | Type | Description | Example |
|-------|------|-------------|---------|
| **EmpID** | Integer | Employee ID | `1` |
| **ProjectID** | Integer | Project ID | `10` |
| **DateFrom** | Date | Start date | `2020-01-01` |
| **DateTo** | Date | End date (NULL = today) | `2020-12-31` |
```
### Supported Date Formats

- `yyyy-MM-dd` (ISO 8601): `2020-01-01`
- `dd/MM/yyyy` (European): `01/01/2020`
- `MM/dd/yyyy` (US): `01/01/2020`
- `NULL` or empty: Defaults to today's date

### Example CSV Files

**Simple Example:**

EmpID,ProjectID,DateFrom,DateTo 
1,10,2020-01-01,2020-12-31 
2,10,2020-03-01,2020-11-30

**Multiple Projects:**

EmpID,ProjectID,DateFrom,DateTo 
1,10,2020-01-01,2020-12-31 
2,10,2020-03-01,2020-11-30 
1,15,2021-01-01,2021-06-30 
2,15,2021-02-01,2021-05-30

**With NULL Dates:**

EmpID,ProjectID,DateFrom,DateTo 
1,10,2020-01-01,NULL 
2,10,2020-03-01,NULL


## 🏗️ Architecture

### Backend Architecture
```
──────────────────────────────────────────────
│           Employee.API (Web API)           │
│  - Controllers                             │
│  - Filters (Swagger)                       │
│  - Program.cs / Startup                    │
└───────────────┬────────────────────────────┘
                │
┌───────────────▼────────────────────────────┐
│       Employee.Mapping (AutoMapper)        │
│  - MappingProfile                          │
│  - Extensions (DI registration helpers)    │
└───────────────┬────────────────────────────┘
                │
┌───────────────▼────────────────────────────┐
│      Employee.Services (Business Logic)    │
│  - WorkCalculationService                  │
│  - CsvParserService                        │
│  - DateParserService                       │
└───────────────┬────────────────────────────┘
                │
┌───────────────▼────────────────────────────┐
│  Employee.Domain & Contracts (Models)      │
│  - Domain models                           │
│  - DTOs / ViewModels (EmpProjectRecord…)   │
│  - Interfaces (IWorkCalculationService,    │
│                ICsvParser<>, IDateParser)  │
└────────────────────────────────────────────┘
```

### Technologies & Patterns

- **Clean Architecture**: Separation of concerns with clear boundaries
- **Dependency Injection**: Built-in .NET DI container
- **Repository Pattern**: Service layer abstraction
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit and integration testing
- **FluentAssertions**: Readable test assertions
- **Moq**: Mocking framework for tests

## 🔍 Docker Commands Reference

### Basic Commands

Build services

docker-compose build

Start services (detached mode)

docker-compose up -d

View logs (all services)

docker-compose logs -f

View logs (specific service)

docker-compose logs -f employee-api

Stop services

docker-compose stop

Restart services

docker-compose restart

Remove containers

docker-compose down

Remove containers and volumes

docker-compose down -v

### Individual Service Commands

Build API only

docker build -t employee-api:latest -f Employee/Employee.API/Dockerfile ./Employee

Build Frontend only

docker build -t employee-frontend:latest ./frontend

Run API container

docker run -p 5000:8080 employee-api:latest

Run Frontend container

docker run -p 80:80 employee-frontend:latest

### Debugging Commands

Check running containers

docker ps

Execute command in running container

docker exec -it employee-api bash

View container resource usage

docker stats

Inspect container

docker inspect employee-api

View container logs

docker logs employee-api

## 🐛 Troubleshooting

### Common Issues

#### 1. Port Already in Use

**Error:** `Bind for 0.0.0.0:5000 failed: port is already allocated`

**Solution:**

Stop conflicting service

docker-compose down

Or change ports in docker-compose.yml

ports:

•	"5001:8080"  # Change 5000 to 5001

#### 2. CORS Errors

**Error:** `Access to XMLHttpRequest has been blocked by CORS policy`

**Solution:**
Check CORS origins in `Employee.API/Program.cs`:

var allowedOrigins = new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:80", "http://localhost" };

#### 3. Container Health Check Failing

**Check health status:**

docker-compose ps docker logs employee-api

**Test health endpoint manually:**

curl http://localhost:5000/health

#### 4. File Upload Fails

**Error:** `413 Payload Too Large`

**Solution:** File size limit is 10MB. For larger files, update:

[RequestSizeLimit(10_000_000)] // Increase this value

#### 5. Database/Volume Issues

Remove all volumes and start fresh

docker-compose down -v docker-compose up --build

### Debug Mode

Run containers with verbose logging:

API logs

docker-compose logs -f employee-api | grep -i error

Frontend logs

docker-compose logs -f employee-frontend


## 🛠️ Technologies Used

### Backend
- **.NET 8** - Web API framework
- **C# 12** - Programming language
- **AutoMapper 13** - Object mapping
- **Swashbuckle 6** - Swagger/OpenAPI documentation
- **xUnit 2.9** - Testing framework
- **FluentAssertions 8** - Test assertions
- **Moq 4.20** - Mocking framework

### Frontend
- **React 18** - UI library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool and dev server
- **Axios** - HTTP client
- **React Router** - Client-side routing

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **Nginx** - Web server for frontend
- **GitHub Actions** - CI/CD (optional)

## 📝 License

This project is licensed under the MIT License.

## 👤 Author

**Martin Demirev**
- GitHub: [@MarinesBG](https://github.com/MarinesBG)
- Repository: [Martin-Demirev-employees](https://github.com/MarinesBG/Martin-Demirev-employees)

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!
