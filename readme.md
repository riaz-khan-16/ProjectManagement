
## Project Overview
The requirement was to design develop a collaboration board to help small teams manage projects, assign tasks and communicate instantly.
I have developed a collaboration board by which a small team can manage projects and assign tasks. Due to shortage of time I was not able to develop the communication and notification functionality. 

Team members can register and login to participate int TeamSync. After login they will see a dashboard containing All project list and their corresponding tasks. They can add new project by using the "Add new project" option. They can add new tasks for the project by using the form provided in the tasks column.

## Project Demo


See the Demo Video: https://youtu.be/z47stdl0Huw


Figure 1: Login Page
<img width="939" height="263" alt="image" src="https://github.com/user-attachments/assets/ff04aa4b-6285-4d88-946c-ad8a666adf6d" /> 



                                                    
Figure 2: Registrtion Page
<img width="858" height="356" alt="image" src="https://github.com/user-attachments/assets/7b684649-bbd9-472c-a54f-797f3214d454" />
                                               

                                       
Figure 3: TeamSync Project and task management Dashboard

<img width="891" height="400" alt="image" src="https://github.com/user-attachments/assets/aeca94f2-e358-4f1a-8c8e-dd37b637bc18" />


                                        






##  Architecture Overview

**Backend API** ASP.NET Core 8 Web API Handles authentication, projects, and tasks API.  
**Database** MongoDB Primary data store for users, projects.  
**Frontend**  Angular  Frontend client that consumes APIs.  
**Caching**   Redis caching layer for user profiles, tasks and projects.   



## Features Implemented

### Backend
- **User Management**
  - JWT-based authentication
  - User registration with duplicate email validation
  - Password hashing
  - Profile endpoint: `GET /api/users/{id}` with Redis caching

- **Project Management**
    - Projects: Users can create projects with a name, description, createdBy, and members list.
    - Tasks: Tasks belong to projects and include: Title, Status, Assignee, DueDate
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
`cd Backend`  
`dotnet restore`    
`dotnet build`  
`dotnet run`  


MongoDB Configuration:  
In appsettings.json, update the connection string:   

`"ConnectionStrings": {
  "DefaultConnection": "mongodb://localhost:27017/ProjectManagementDB"
}`

The backend will run at:  
 `https://localhost:7043`

3️. Frontend Setup  
`cd Frontend`
`npm install`
`ng serve`

The frontend will run at:  
`http://localhost:4200`

4. Run Redis server locally  
   `redis-server`



## How the system is working

The application integrates Angular, ASP.NET Core Web API, MongoDB, Redis, and JWT authentication to provide an efficient project and task management platform for small teams. The angular frontend offers a responsive interface where users can register, log in , and manage projects and tasks. It communicates with the backend via RESTful APIs and stores the JWT token in local storage for authentication. The ASP.NET Core backend handles logics, user authentication, and data operations and interacting with MongoDB as the primary database for storing user, project, and task data. To enhance performance, Redis is used as a caching layer that stores frequently accessed data such as project and task lists, reducing load on MongoDB and improving response times. Cached data is invalidated automatically upon updates to ensure accuracy. 




