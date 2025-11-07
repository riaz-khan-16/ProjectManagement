import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as signalR from '@microsoft/signalr';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class App {
  apiUrl = 'https://localhost:7162/api';
   editingProject = false;

  users: any[] = [];  //  store users for dropdown
  userProjects: any[] = []; // Projects assigned to the current user

  // --- Auth ---
  isLoggedIn = false;
  showRegister = false;
  token: string | null = null;
  userEmail = '';
  
  currentUserId: string | null = null;
  currentUserName: string | null = null;

  authForm = { email: '', password: '', role: '', department: '', name:'' };

  // --- Projects / Tasks ---
  projects: any[] = [];
  projectForm: any = { 
  id: null,
  name: '',
  description: '',
  userIds: [],          // store selected user IDs
  assignedUserId: '',    // currently selected user from dropdown
  editingProject: false

   };

  newTaskTitle: any = {};
  newTaskAssignee: any = {};
  newTaskDueDate: any = {};
  editingTask: any = null;
  editingTaskProjectId = '';


  messages: string[] = []; // store incoming notifications
  hubConnection!: signalR.HubConnection;
  toastMessages: string[] = []; // for toast message
 sms: {
  projectId: string;
  senderId: string;
  senderName: string;
  message: string;
}[] = []; // I will use it  for storing team chat


  
  openChatProjectId: string | null = null;  // get project id 
  newProjectMessage: { [projectId: string]: string } = {}; // this will stor new message for a project id




  constructor(private http: HttpClient) {}

  ngOnInit() {
     this.startSignalRConnection(); // it will connect with signalR
  
    this.token = localStorage.getItem('token');
    if (this.token) {
      this.isLoggedIn = true;
      this.userEmail = localStorage.getItem('userEmail') || '';
      this.currentUserId = localStorage.getItem('userId');
      this.currentUserName = localStorage.getItem('userName');
      this.userEmail = localStorage.getItem('userEmail') || '';
      this.loadProjects();
      

    }
    this.loadAllUsers();
   
  }

   // This method will fetch users and loig them

   async startSignalRConnection() {
  this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7162/hubs/notifications')
    .withAutomaticReconnect()   // retry if disconnected
    .configureLogging(signalR.LogLevel.Information)
    .build();
    



  // It willl lsiten for backend notifications
  this.hubConnection.on('ReceiveNotification', (message: string) => {
    console.log(' New notification:', message);
    this.messages.push(message);

    // Show as toast message
  this.showToast(message);
  });

  try {
    await this.hubConnection.start();
     
   
    //it will recieve team chat 
    this.hubConnection.on('ReceiveProjectMessage', (senderName, message, projectId ) => { 
        console.log("received message from Hub")
        console.log(`${senderName}: ${message}`);
        this.sms.push({ projectId, senderId: '', senderName, message });
      });



    console.log(' Connected to SignalR hub');
  } catch (err) {
    console.error(' SignalR connection error:', err);
    setTimeout(() => this.startSignalRConnection(), 5000); // retry
  }
}



// It will make SignalR Group
joinSignalRGroups() {
  if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
    console.warn(' !!!Hub not connected yet, skipping group join.');
    return;
  }

  const projectIds = this.userProjects.map(p => p.id);
  console.log(' Joining groups for projects:', projectIds);

  this.hubConnection.invoke('JoinProjectGroups', projectIds)
    .then(() => console.log(' Joined project groups on SignalR'))
    .catch(err => console.error(' Error joining groups:', err));
}



  //All Methods  made for team chat 

  openTeamChat(projectId: string) {
  this.openChatProjectId = projectId;

  // fetch history from API
  this.http.get<any[]>(`${this.apiUrl}/TeamChat/${projectId}`)
    .subscribe({
      next: (messages) => {
        // merge old messages with current SignalR ones
        this.sms = this.sms.filter(m => m.projectId !== projectId); // remove duplicates
        messages.forEach(msg => {
          this.sms.push({
            projectId: projectId,
            senderId:'',
            senderName: msg.senderName,
            message: msg.message
          });
        });
        console.log('Loaded chat history:', messages);
      },
      error: (err) => {
        console.error('Failed to load chat history:', err);
      }
    });
}

  sendProjectMessage(projectId: string) {
  console.log('Sending teamchat msg to the project id: ', projectId);
  
  const message = this.newProjectMessage[projectId]?.trim();
  if (!message) return;
   
  console.log("the message is: ",message);
  console.log("See the current sms",this.sms);

 

  // Optionally clear input
  this.newProjectMessage[projectId] = '';

  // Send to backend via SignalR
  this.hubConnection.invoke('SendMessageToProject', projectId, message, this.currentUserName);
  console.log("message is send to hub");

}


//get project name by giving it's id
getProjectNameById(projectId: string): string {
  const project = this.projects.find(p => p.id === projectId);
  return project ? project.name : 'Unknown Project';
}


//for teamchat of a project
getMessagesForProject(projectId: string) {
  

  if (!this.sms) return [];
  return this.sms.filter(m => m.projectId === projectId);
}

//methods for team chat End 











 //to see assigned projects of the currrent user
  showCurrentUserProjects() {
  if (!this.currentUserId) return;

  this.userProjects = this.projects.filter(project =>
    project.userIds.includes(this.currentUserId)
  );
  console.log('Assigned projects for the current user:', this.userProjects);
  this.joinSignalRGroups();


}

// It is the toast method made for showing notification
showToast(message: string) {
  this.toastMessages.push(message);

  // auto remove after 5 seconds
  setTimeout(() => {
    this.removeToast(message);
  }, 5000);
}

// close the toast 
removeToast(message: string) {
  const index = this.toastMessages.indexOf(message);
  if (index !== -1) {
    this.toastMessages.splice(index, 1);
  }
}


// method os loading all users 
loadAllUsers(): void {
    this.http.get<any[]>(`${this.apiUrl}/Users`).subscribe({
      next: (data) => {
        this.users = data;
        console.log('All users loaded:', this.users);
      },
      error: (err) => {
        console.error(' Error loading users:', err);
      }
    });
  }

  // it will toggle
  toggleAuthMode() { this.showRegister = !this.showRegister; }



// method for registering
register() {
    const payload = {
      email: this.authForm.email,
      password: this.authForm.password,
      role: this.authForm.role,
      department: this.authForm.department,
      name:this.authForm.name
    };
    this.http.post(`${this.apiUrl}/Auth/register`, payload, { responseType: 'text' })
      .subscribe({
        next: () => {
          alert('Registration successful! Please login.');
          this.showRegister = false;
          this.authForm = { email: '', password: '', role: '', department: '', name:'' };
        },
        error: err => alert('Registration failed: ' + err.error)
      });
  }
  

// method for login
  login() {
    this.http.post<{ token: string, userId: string, email: string, name: string }>(`${this.apiUrl}/Auth/login`, this.authForm)
      .subscribe({
        next: res => {
          this.token = res.token;
          if (this.token) {
            localStorage.setItem('token', this.token);
            localStorage.setItem('userEmail', this.authForm.email);
            localStorage.setItem('userId', res.userId);
            localStorage.setItem('userName', res.name);


            this.isLoggedIn = true;
            this.userEmail = this.authForm.email;

             //store in component variables
            this.currentUserId = res.userId;
            this.currentUserName = res.name;
          

            this.loadProjects();
          }
        },
        error: err => alert('Login failed: ' + err.error)
      });
  }

  logout() {
    this.isLoggedIn = false;
    this.token = null;
    this.userEmail = '';
    localStorage.removeItem('token');
    localStorage.removeItem('userEmail');
  }

  private getAuthHeaders() {
    return { headers: new HttpHeaders({ Authorization: `Bearer ${this.token}` }) };
  }

 


  // this is Projects Methods: load all projects ---
  loadProjects() {
    this.http.get<any[]>(`${this.apiUrl}/Projects`, this.getAuthHeaders())
      .subscribe({
        next: projects => {
          this.projects = projects;
          this.http.get<any[]>(`${this.apiUrl}/Tasks`, this.getAuthHeaders())
            .subscribe({
              next: tasks => {
                this.projects.forEach(project => {
                  project.tasks = tasks.filter(task => task.projectId === project.id);
                });

                // Filter  the projects for the current user who logged in
            this.showCurrentUserProjects();

              },
              error: err => console.error('Failed to load tasks', err)
            });
        },
        error: err => { console.error('Failed to load projects', err); if (err.status === 401) this.logout(); }
      });
  }

 

// add new member to a project
  addUserId(): void {
  const selectedUserId = this.projectForm.assignedUserId;

  if (selectedUserId && !this.projectForm.userIds.includes(selectedUserId)) {
    this.projectForm.userIds.push(selectedUserId); // add ID
    this.projectForm.assignedUserId = '';          // reset dropdown
  }

  // Show success toast
    this.showToast('User added!' );
   
}


// this method will show user email if i GIVE him id
getUserEmailById(id: string): string {
  const user = this.users.find(u => u.Id === id);
  return user ? user.Email : 'Unknown';
}


// this method will show name if i give him my id
getUserNameById(id: string): string {
  const user = this.users.find(u => u.Id === id);
  return user ? user.Name : 'Unknown';
}


// it will save the proejct in 
saveProject() {
    const payload = {
      name: this.projectForm.name,
      description: this.projectForm.description,
      
      userIds: this.projectForm.userIds,
      createdBy: this.userEmail,
      tasks: []



    };
    if (this.projectForm.id) {
      this.http.put(`${this.apiUrl}/Projects/${this.projectForm.id}`, payload, this.getAuthHeaders())
        .subscribe(() => { 
          alert('Project updated successfully');
          this.loadProjects(); this.cancelProjectEdit(); 
        });
    } else {
      this.http.post(`${this.apiUrl}/Projects`, payload, this.getAuthHeaders())
        .subscribe(() => { this.loadProjects(); this.projectForm = {
            id: null,
            name: '',
            description: '',
            userIds: [],
            assignedUserId: '' 
            
             }; });
    }
  }

// will update project
  editProject(project: any) {
     this.projectForm = { 
    ...project, membersStr: project.members.join(',') 
  };
   this.editingProject = true; }


  cancelProjectEdit() { this.projectForm = { id: null, name: '', description: '', membersStr: '' }; this.editingProject = false; }
  deleteProject(id: string) { if (!confirm('Delete this project?')) return; this.http.delete(`${this.apiUrl}/Projects/${id}`, this.getAuthHeaders()).subscribe(() => this.loadProjects()); }

  // Tasks Methods: add new task 
  createTask(projectId: string) {
    if (!this.newTaskTitle[projectId]) return;
    const payload = {
      title: this.newTaskTitle[projectId],
      description: '',
      status: 'Pending',
      assignee: this.newTaskAssignee[projectId] || '',
      dueDate: this.newTaskDueDate[projectId] || new Date().toISOString(),
      createdBy: this.userEmail,
      projectId
    };
    this.http.post(`${this.apiUrl}/Tasks`, payload, this.getAuthHeaders())
      .subscribe(() => { this.loadProjects(); this.newTaskTitle[projectId] = ''; });
  }

  

  editTask(task: any, projectId: string) { this.editingTask = { ...task }; this.editingTaskProjectId = projectId; }
  
  
  
  // update a task
  updateTask() { this.http.put(`${this.apiUrl}/Tasks/${this.editingTask.id}`, this.editingTask, this.getAuthHeaders()).subscribe(() => { this.loadProjects(); this.cancelTaskEdit(); }); }
  
  
  cancelTaskEdit() { this.editingTask = null; this.editingTaskProjectId = ''; }
  
  // delete the taskkk
  deleteTask(taskId: string) { if (!confirm('Delete this task?')) return; this.http.delete(`${this.apiUrl}/Tasks/${taskId}`, this.getAuthHeaders()).subscribe(() => this.loadProjects()); }
}
