# API Documentation

## Base URL
`https://localhost:5001` or `http://localhost:5000`

## Authentication Endpoints

### Register User
**POST** `/api/auth/register`

Request Body:
```json
{
  "username": "john.doe",
  "password": "SecurePass123!",
  "email": "john.doe@example.com",
  "role": 1
}
```

Role Values:
- `1` = Admin
- `2` = Marketing
- `3` = Sales
- `4` = Accounting

Response:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": 2,
    "username": "john.doe",
    "email": "john.doe@example.com",
    "role": "Admin",
    "isActive": true,
    "createdAt": "2024-12-01T10:00:00Z"
  }
}
```

### Login
**POST** `/api/auth/login`

Request Body:
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

Response:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@example.com",
      "role": "Admin",
      "isActive": true,
      "createdAt": "2024-12-01T10:00:00Z"
    }
  }
}
```

### Get All Users
**GET** `/api/auth/users`

Headers:
```
Authorization: Bearer {token}
```

Response:
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    {
      "id": 1,
      "username": "admin",
      "email": "admin@example.com",
      "role": "Admin",
      "isActive": true,
      "createdAt": "2024-12-01T10:00:00Z"
    }
  ]
}
```

### Get User by ID
**GET** `/api/auth/users/{id}`

### Update User Role
**PUT** `/api/auth/users/{id}/role`

Request Body:
```json
2
```

### Update User Status
**PUT** `/api/auth/users/{id}/status`

Request Body:
```json
true
```

---

## Employee Endpoints

### Get All Employees
**GET** `/api/employees?includeInactive=false`

Headers:
```
Authorization: Bearer {token}
```

Query Parameters:
- `includeInactive` (optional, default: false) - Include inactive employees

Response:
```json
{
  "success": true,
  "message": "Employees retrieved successfully",
  "data": [
    {
      "id": 1,
      "firstName": "Jane",
      "lastName": "Smith",
      "email": "jane.smith@company.com",
      "phone": "+1234567890",
      "department": "Engineering",
      "position": "Software Developer",
      "salary": 75000.00,
      "hireDate": "2024-01-15T00:00:00Z",
      "isActive": true,
      "createdAt": "2024-12-01T10:00:00Z",
      "updatedAt": null
    }
  ]
}
```

### Get Employee by ID
**GET** `/api/employees/{id}`

Headers:
```
Authorization: Bearer {token}
```

Response:
```json
{
  "success": true,
  "message": "Employee retrieved successfully",
  "data": {
    "id": 1,
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@company.com",
    "phone": "+1234567890",
    "department": "Engineering",
    "position": "Software Developer",
    "salary": 75000.00,
    "hireDate": "2024-01-15T00:00:00Z",
    "isActive": true,
    "createdAt": "2024-12-01T10:00:00Z",
    "updatedAt": null
  }
}
```

### Create Employee
**POST** `/api/employees`

Headers:
```
Authorization: Bearer {token}
```

Request Body:
```json
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

Response:
```json
{
  "success": true,
  "message": "Employee created successfully",
  "data": {
    "id": 1,
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@company.com",
    "phone": "+1234567890",
    "department": "Engineering",
    "position": "Software Developer",
    "salary": 75000.00,
    "hireDate": "2024-01-15T00:00:00Z",
    "isActive": true,
    "createdAt": "2024-12-01T10:00:00Z",
    "updatedAt": null
  }
}
```

### Update Employee
**PUT** `/api/employees/{id}`

Headers:
```
Authorization: Bearer {token}
```

Request Body (all fields optional):
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@company.com",
  "phone": "+1234567890",
  "department": "Engineering",
  "position": "Senior Software Developer",
  "salary": 95000.00,
  "hireDate": "2024-01-15T00:00:00Z",
  "isActive": true
}
```

Response:
```json
{
  "success": true,
  "message": "Employee updated successfully",
  "data": {
    "id": 1,
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@company.com",
    "phone": "+1234567890",
    "department": "Engineering",
    "position": "Senior Software Developer",
    "salary": 95000.00,
    "hireDate": "2024-01-15T00:00:00Z",
    "isActive": true,
    "createdAt": "2024-12-01T10:00:00Z",
    "updatedAt": "2024-12-01T11:30:00Z"
  }
}
```

### Deactivate Employee (Soft Delete)
**DELETE** `/api/employees/{id}`

Headers:
```
Authorization: Bearer {token}
```

Response:
```json
{
  "success": true,
  "message": "Employee deactivated successfully",
  "data": null
}
```

### Permanently Delete Employee
**DELETE** `/api/employees/{id}/permanent`

Headers:
```
Authorization: Bearer {token}
```

Response:
```json
{
  "success": true,
  "message": "Employee permanently deleted",
  "data": null
}
```

---

## Error Responses

All endpoints return a consistent error format:

```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

Common HTTP Status Codes:
- `200 OK` - Successful GET, PUT, DELETE
- `201 Created` - Successful POST
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## Running the API

```powershell
dotnet run
```

Access Swagger UI at: `https://localhost:5001/swagger` (Development mode only)

## Default Admin Credentials

- **Username**: `admin`
- **Password**: `Admin@123`
- **Role**: Admin

**⚠️ Important**: Change the default admin password and JWT secret key before deploying to production!
