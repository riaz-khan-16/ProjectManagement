## Project Overview

The requirement was to design develop a collaboration board to help small teams manage projects, assign tasks and communicate instantly. 
I have developed a collaboration board by which a small team can manage projects and assign tasks. Due to shortage of time I was not able to develop the communication and notification functionality. 

##  Architecture Overview

**Backend API** ASP.NET Core 8 Web API:  Handles authentication, projects, and tasks API.
**Database** MongoDB: Stores users, projects, tasks, and chat history. 
**Frontend**  Angular 18:  Responsive web client consuming APIs and listening on SignalR hub. 
**Caching** Redis: for fast retrieval. 


## Features Implemented

### Backend
- **User Management**
  - JWT-based authentication
  - User registration with duplicate email validation
  - Password hashing
  - Profile endpoint: `GET /api/users/{id}` with Redis caching

- **Project Management**
    - Projects: Users can create projects with a name, description, createdBy, and members list.
    - Tasks: Tasks belong to projects and include: Title, Description, Status, Assignee, DueDate
    - Caching: Cache frequently-read lists (e.g., tasks for a project) in Redis using keys and invalidate/update on changes.

### Frontend
- **Authentication**
  - Login & registration linked with backend API
  - JWT stored in `localStorage` for session management
- **Dashboard**
  - List all projects and their tasks
  - Task creation, editing, and deletion UI



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



