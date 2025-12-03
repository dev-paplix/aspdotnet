# Employee Management API

A RESTful CRUD API built with ASP.NET Core for managing employees with role-based access control.

## Features

- **Employee Management**: Full CRUD operations for employee records
- **User Authentication**: JWT-based authentication system
- **Role-Based Access**: Four user levels (Admin, Marketing, Sales, Accounting)
- **SQLite Database**: Lightweight, file-based database
- **Swagger Documentation**: Interactive API documentation

## Tech Stack

- ASP.NET Core 10.0
- Entity Framework Core 9.0
- SQLite Database
- JWT Authentication
- BCrypt for password hashing

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later

### Installation

1. Restore dependencies:
```powershell
dotnet restore
```

2. Run the application:
```powershell
dotnet run
```

The API will be available at `https://localhost:5001` (or the port shown in your terminal).

### Default Credentials

- **Username**: admin
- **Password**: Admin@123
- **Role**: Admin

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and receive JWT token
- `GET /api/auth/users` - Get all users
- `GET /api/auth/users/{id}` - Get user by ID
- `PUT /api/auth/users/{id}/role` - Update user role
- `PUT /api/auth/users/{id}/status` - Activate/deactivate user

### Employees

- `GET /api/employees` - Get all employees (query param: includeInactive)
- `GET /api/employees/{id}` - Get employee by ID
- `POST /api/employees` - Create new employee
- `PUT /api/employees/{id}` - Update employee
- `DELETE /api/employees/{id}` - Soft delete (deactivate) employee
- `DELETE /api/employees/{id}/permanent` - Permanently delete employee

## User Roles

1. **Admin** - Full system access
2. **Marketing** - Marketing department access
3. **Sales** - Sales department access
4. **Accounting** - Accounting department access

## Request Examples

### Register User
```json
POST /api/auth/register
{
  "username": "john.doe",
  "password": "SecurePass123!",
  "email": "john.doe@example.com",
  "role": 2
}
```

### Login
```json
POST /api/auth/login
{
  "username": "admin",
  "password": "Admin@123"
}
```

### Create Employee
```json
POST /api/employees
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@company.com",
  "phone": "+1234567890",
  "department": "Engineering",
  "position": "Software Developer",
  "salary": 75000.00,
  "hireDate": "2024-01-15T00:00:00Z"
}
```

### Update Employee
```json
PUT /api/employees/1
{
  "position": "Senior Software Developer",
  "salary": 95000.00
}
```

## Database

The SQLite database file (`employees.db`) will be created automatically in the project root on first run. It includes:

- **Employees** table - Employee records
- **Users** table - User authentication and roles

## CORS Configuration

The API is configured to accept requests from:
- `http://localhost:3000` (React)
- `http://localhost:4200` (Angular)
- `http://localhost:5173` (Vite)

Modify the CORS policy in `Program.cs` to add your frontend URL.

## Security Notes

- Change the JWT secret key in `appsettings.json` for production
- Default admin password should be changed after first login
- All passwords are hashed using BCrypt
- JWT tokens expire after 24 hours (configurable)

## Swagger UI

Access the interactive API documentation at `/swagger` when running in development mode.
