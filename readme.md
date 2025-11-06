
## Project Overview
This project is a real time  collaboration system that allows users to manage projects and tasks. It uses Angular for the frontend and ASP.NET Core Web API for the backend, with MongoDB as the main database. JWT authentication secures user access, while Redis improves performance through caching. RabbitMQ handles background event processing, and SignalR enables real-time notifications.

## Project Demo


See the Demo Video: https://youtu.be/Z0SXReOwJ2U


Figure 1: Login Page
<img width="939" height="263" alt="image" src="https://github.com/user-attachments/assets/ff04aa4b-6285-4d88-946c-ad8a666adf6d" /> 



                                                    
Figure 2: Registrtion Page
<img width="858" height="356" alt="image" src="https://github.com/user-attachments/assets/7b684649-bbd9-472c-a54f-797f3214d454" />
                                               

                                       
Figure 3: TeamSync Project and task management Dashboard

<img width="891" height="400" alt="image" src="https://github.com/user-attachments/assets/aeca94f2-e358-4f1a-8c8e-dd37b637bc18" />


Figure 4: Notification functionality for connected members 
<img width="882" height="172" alt="image" src="https://github.com/user-attachments/assets/36d67a3a-4951-4134-9014-a115504010f8" />



                                        






##  Architecture Overview

**Backend API:** ASP.NET Core 8 Web API Handles authentication, projects, and tasks API.  
**Database:** MongoDB Primary data store for users, projects.  
**Frontend:**  Angular  Frontend client that consumes APIs and listens on SignalR.
**Caching:**   Redis caching layer for user profiles, tasks and projects.  
**Notification:** Worker RabbitMQ consumer that broadcasts via SignalR.
**SignalR Hub:** Real-time bridge between backend and Angular frontend.




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
    - Events: On task create/update/delete, an event is published to RabbitMQ (TaskCreated/TaskUpdated/TaskDeleted) with relevant payload.
- **Real-time Notifications**
    - Notification flow: A background consumer listens to RabbitMQ events and broadcasts
updates to connected clients via SignalR.
    - Behavior: When a task is assigned or updated, all connected members of the project
receive an instant update.
- **Frontend**
  - Login & registration linked with backend API
  - JWT stored in `localStorage` for session management
  - List all projects and their tasks
  - Task creation, editing, and deletion UI
  - Live updates: Connected to SignalR hub for task and chat updates. It displaystoast notifications
for changes.



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

This application uses Angular, ASP.NET Core Web API, MongoDB, Redis, RabbitMQ, and SignalR to provide a fast and realtime collaboration for small teams. 

**The Angular** Frontend allows users to register , log in and manage projects and tasks. I communicates with the backend through REST APIs and uses JWT tokens for secure authentication. 

**The ASP.NET Core Web API** backend handles all logic, user authentication, and data operations. It connects to MongoDB, which stores all main data such as users, projects, tasks, and messages.

To make the application faster, **Redis** is used as a caching system. It stores frequently accessed data (like project and task lists) so that the backend doesn’t have to query MongoDB every time. This helps to reduce load on the database and improve response time. When any data changes (for example, when a task is added or updated), the cache is invalidated to make sure users always see the latest information. I have used unique cache keys to easily update or remove only the necessary cache items instead of clearing everything.

**RabbitMQ** is used to handle background communication between different parts of the system. When an event happens, such as a new message or a project update, the backend sends that event to a RabbitMQ queue. This message is then received by a consumer service that processes it in the background.

Once RabbitMQ receives an event, it works together with **SignalR** to send real-time updates to users. SignalR allows instant communication between the server and all connected clients. For example, when a team member sends a chat message or creates a task, everyone else in the same project can see the update right away without refreshing the page.

**Conclusion**: 
By combining Redis, RabbitMQ, and SignalR, the system becomes faster, more efficient, and real-time.
Redis improves performance with caching.
RabbitMQ manages message delivery reliably.
SignalR enables instant communication between users.

