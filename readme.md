

## Project Overview
.  
This system demonstrates a modular architecture integrating **.NET 8 Web API**, **MongoDB**, and **Angular 18**.

---

##  Architecture Overview

**Backend API** ASP.NET Core 8 Web API:  Handles authentication, projects, and tasks API.
**Database** MongoDB: Stores users, projects, tasks, and chat history. 
**Frontend**  Angular 18:  Responsive web client consuming APIs and listening on SignalR hub. 
---

## Features Implemented

### Backend
- **User Management**
  - JWT-based authentication
  - User registration with duplicate email validation
  - Password hashing
  - Profile endpoint: `GET /api/users/{id}` with optional Redis caching

- **Project Management**
  - CRUD operations for projects
  - Fields: `Name`, `Description`, `CreatedBy`, `Members`

- **Task Management**
  - Tasks belong to projects
  - Fields: `Title`, `Description`, `Status`, `Assignee`, `DueDate`, `CreatedBy`, `CreatedAt`

---

### Frontend
- **Authentication**
  - Login & registration linked with backend API
  - JWT stored in `localStorage` for session management
- **Dashboard**
  - List all projects and their tasks
  - Task creation, editing, and deletion UI
  - Clean gray-themed interface for usability



## Setup Instructions

1.  Clone the Repository

git clone https://github.com/riaz-khan-16/ProjectManagement.git
cd ProjectManagement

2. Backend Setup
cd Backend
dotnet restore
dotnet build
dotnet run


MongoDB Configuration:
In appsettings.json, update the connection string:

"ConnectionStrings": {
  "DefaultConnection": "mongodb://localhost:27017/ProjectManagementDB"
}

The backend will run at:
 https://localhost:7043

3️. Frontend Setup
cd Frontend
npm install
ng serve

The frontend will run at:
http://localhost:4200
