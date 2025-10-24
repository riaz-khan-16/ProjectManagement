import { bootstrapApplication } from '@angular/platform-browser';
import { Component } from '@angular/core';                         // decorator to define a component
import { HttpClient, HttpHeaders, provideHttpClient } from '@angular/common/http'; // For making HTTP requests to backend APIs.
import { CommonModule } from '@angular/common'; // Provides common Angular directives like *ngIf, *ngFor
import { FormsModule } from '@angular/forms';  // Enables template-driven forms using ngModel.

// component declaration
@Component({
  selector: 'app-root',  // 
  standalone: true,   //ndicates no NgModule is needed.
  imports: [CommonModule, FormsModule], 

  //The full HTML for UI
  template: `    

  <div class="container my-5">

    <!-- Title -->
    <h1 class="mb-4 text-center">Project and Tasks Management Dashboard</h1>


    <!-- LOGIN / REGISTER -->

    <!--This <div> is only shown if the user is not logged in.-->

    <div *ngIf="!isLoggedIn" class="card mb-4">

      <!--If showRegister is true → displays Register.Else → displays Login. -->
      <div class="card-header">{{ showRegister ? 'Register' : 'Login' }}</div> 
    
      <div class="card-body">
        <!--If showRegister is true it will execute the register() function else it will execute login function -->
        <form (ngSubmit)="showRegister ? register() : login()" class="row g-3">

          <div class="col-md-4">
            <! --  --->
            <input type="email" [(ngModel)]="authForm.email" name="email" class="form-control" placeholder="Email" required />
          </div>
          <div class="col-md-4">
            <input type="password" [(ngModel)]="authForm.password" name="password" class="form-control" placeholder="Password" required />
          </div>

           <!-- Role input (only for registration) -->
            <div class="col-md-4" *ngIf="showRegister">
              <input type="text" [(ngModel)]="authForm.role" name="role" class="form-control" placeholder="Role" required />
            </div>

            <!-- Department input (only for registration) -->
            <div class="col-md-4" *ngIf="showRegister">
              <input type="text" [(ngModel)]="authForm.department" name="department" class="form-control" placeholder="Department" required />
            </div>
          <div class="col-md-4">
            <button type="submit" class="btn btn-primary w-100">{{ showRegister ? 'Register' : 'Login' }}</button>
          </div>
        </form>
        <div class="mt-3 text-center">
          <a href="#" (click)="toggleAuthMode()">
            {{ showRegister ? 'Already have an account? Login' : 'New here? Register' }}
          </a>
        </div>
      </div>
    </div>


    





    <!-- LOGGED IN DASHBOARD . This will show if user is logged in-->
    <div *ngIf="isLoggedIn">

      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-3">
        <h3>Welcome, {{ userEmail }}</h3>
        <button class="btn btn-outline-danger" (click)="logout()">Logout</button>
      </div>



      <!-- Project Form -->
      <div class="card mb-4">
        <div class="card-header">{{ editingProject ? 'Edit Project' : 'Add New Project' }}</div>
        <div class="card-body">
          <form (ngSubmit)="saveProject()" class="row g-3">
            <div class="col-md-3">
              <input type="text" [(ngModel)]="projectForm.name" name="name" class="form-control" placeholder="Project Name" required />
            </div>
            <div class="col-md-3">
              <input type="text" [(ngModel)]="projectForm.description" name="description" class="form-control" placeholder="Description" />
            </div>
            <div class="col-md-3">
              <input type="text" [(ngModel)]="projectForm.membersStr" name="members" class="form-control" placeholder="Members (comma-separated)" />
            </div>
            <div class="col-md-3">
              <button type="submit" class="btn btn-primary">{{ editingProject ? 'Update' : 'Add' }}</button>
              <button type="button" *ngIf="editingProject" (click)="cancelProjectEdit()" class="btn btn-secondary ms-2">Cancel</button>
            </div>
          </form>
        </div>
      </div>

      <!-- Projects Table -->
      <div class="card">
        <div class="card-header">All Projects</div>
        <div class="card-body p-0">
          <table class="table table-hover mb-0">
            <thead class="table-light">
              <tr>
                <th>Name</th>
                <th>Description</th>
                <th>Created By</th>
                <th>Members</th>
                <th>Tasks</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let project of projects">
                <td>{{ project.name }}</td>
                <td>{{ project.description }}</td>
                <td>{{ project.createdBy }}</td>
                <td>{{ project.members.join(', ') }}</td>

                <!-- Tasks Column -->
                <td class="tasks-column">
                        <div *ngFor="let task of project.tasks" class="d-flex align-items-center mb-1">
                          
                          <!-- Check if this task is being edited -->
                          <div *ngIf="editingTask && editingTask.id === task.id" class="d-flex w-100 gap-1">
                            <input type="text" class="form-control form-control-sm" [(ngModel)]="editingTask.title" />
                            <input type="text" class="form-control form-control-sm" [(ngModel)]="editingTask.assignee" />
                            <select class="form-select form-select-sm" [(ngModel)]="editingTask.status">
                              <option>Pending</option>
                              <option>In Progress</option>
                              <option>Completed</option>
                            </select>
                            <button class="btn btn-sm btn-success" (click)="updateTask()">✔️</button>
                            <button class="btn btn-sm btn-secondary" (click)="cancelTaskEdit()">✖️</button>
                          </div>

                          <!-- Normal display if not editing -->
                          <div *ngIf="!editingTask || editingTask.id !== task.id" class="d-flex align-items-center w-100">
                            <span class="badge bg-info text-dark me-2">{{ task.status }}</span>
                            <span class="me-auto">{{ task.title }} ({{ task.assignee }})</span>
                            <button class="btn btn-sm btn-warning me-1" (click)="editTask(task, project.id)">✏️</button>
                            <button class="btn btn-sm btn-danger" (click)="deleteTask(task.id)">🗑️</button>
                          </div>

                        </div>

                  <!-- Add Task Inputs -->
                  <div class="input-group input-group-sm mt-2">
                    <input type="text" class="form-control" placeholder="Title" [(ngModel)]="newTaskTitle[project.id]" name="taskTitle-{{project.id}}" />
                    <input type="text" class="form-control" placeholder="Assignee" [(ngModel)]="newTaskAssignee[project.id]" name="taskAssignee-{{project.id}}" />
                    <input type="date" class="form-control" [(ngModel)]="newTaskDueDate[project.id]" name="taskDue-{{project.id}}" />
                    <button class="btn btn-primary" (click)="createTask(project.id)">Add</button>
                  </div>
                </td>

                <!-- Project Actions -->
                <td>
                  <button class="btn btn-sm btn-success" (click)="editProject(project)">Edit</button>
                  <button class="btn btn-sm btn-danger ms-1" (click)="deleteProject(project.id)">Delete</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
  `,
  //CSS scoped to this component.
  styles: [`
    .tasks-column {
      max-height: 250px;
      overflow-y: auto;
    }
  `]
})
class App {
  apiUrl = 'https://localhost:7162/api';

  // --- Auth ---
  isLoggedIn = false;   // tracks login status
  showRegister = false;  // toggles between register and login 
  token: string | null = null; //stores JWT tokens after login
  userEmail = '';             // current logged in user
  // authForm = { email: '', password: '', role:'' };  //Holds email and password input values.
  authForm = {
  email: '',
  password: '',
  role: '',        // new
  department: ''   // new
};

  // --- Projects / Tasks ---
  projects: any[] = [];
  projectForm: any = { id: null, name: '', description: '', membersStr: '' };
  editingProject = false;

  newTaskTitle: any = {};
  newTaskAssignee: any = {};
  newTaskDueDate: any = {};
  editingTask: any = null;
  editingTaskProjectId = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.token = localStorage.getItem('token');
    if (this.token) {
      this.isLoggedIn = true;
      this.userEmail = localStorage.getItem('userEmail') || '';
      this.loadProjects();
    }
  }

  // --- AUTH ---
  toggleAuthMode() { this.showRegister = !this.showRegister; }

  // register() {
  //   this.http.post(`${this.apiUrl}/Auth/register`, this.authForm, { responseType: 'text' })
  //     .subscribe({
  //       next: () => { alert('Registration successful! Please login.'); this.showRegister = false; },
  //       error: err => alert('Registration failed: ' + err.error)
  //     });
  // }

  register() {
  // Only include all fields when registering
  const payload = {
    email: this.authForm.email,
    password: this.authForm.password,
    role: this.authForm.role,
    department: this.authForm.department
  };

  this.http.post(`${this.apiUrl}/Auth/register`, payload, { responseType: 'text' })
    .subscribe({
      next: () => {
        alert('Registration successful! Please login.');
        this.showRegister = false;

        // Optional: clear form after registration
        this.authForm = { email: '', password: '', role: '', department: '' };
      },
      error: err => alert('Registration failed: ' + err.error)
    });
}

  
  login() {
    this.http.post<{ token: string }>(`${this.apiUrl}/Auth/login`, this.authForm)
      .subscribe({
        next: res => {
          this.token = res.token;
          if (this.token) {
            localStorage.setItem('token', this.token);
            localStorage.setItem('userEmail', this.authForm.email);
            this.isLoggedIn = true;
            this.userEmail = this.authForm.email;
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

  // --- Projects + Tasks ---
  loadProjects() {
    // Load all projects first
    this.http.get<any[]>(`${this.apiUrl}/Projects`, this.getAuthHeaders())
      .subscribe({
        next: projects => {
          this.projects = projects;

          // Load all tasks separately
          this.http.get<any[]>(`${this.apiUrl}/Tasks`, this.getAuthHeaders())
            .subscribe({
              next: tasks => {
                // Attach tasks to the correct project
                this.projects.forEach(project => {
                  project.tasks = tasks.filter(task => task.projectId === project.id);
                });
              },
              error: err => console.error('Failed to load tasks', err)
            });
        },
        error: err => {
          console.error('Failed to load projects', err);
          if (err.status === 401) this.logout();
        }
      });
  }

  saveProject() {
    const payload = {
      name: this.projectForm.name,
      description: this.projectForm.description,
      members: this.projectForm.membersStr ? this.projectForm.membersStr.split(',') : [],
      createdBy: this.userEmail,
      tasks: []
    };
    if (this.projectForm.id) {
      this.http.put(`${this.apiUrl}/Projects/${this.projectForm.id}`, payload, this.getAuthHeaders())
        .subscribe(() => { this.loadProjects(); this.cancelProjectEdit(); });
    } else {
      this.http.post(`${this.apiUrl}/Projects`, payload, this.getAuthHeaders())
        .subscribe(() => { this.loadProjects(); this.projectForm = { id: null, name: '', description: '', membersStr: '' }; });
    }
  }


  

  editProject(project: any) { this.projectForm = { ...project, membersStr: project.members.join(',') }; this.editingProject = true; }
  cancelProjectEdit() { this.projectForm = { id: null, name: '', description: '', membersStr: '' }; this.editingProject = false; }
  deleteProject(id: string) { if (!confirm('Delete this project?')) return; this.http.delete(`${this.apiUrl}/Projects/${id}`, this.getAuthHeaders()).subscribe(() => this.loadProjects()); }

  // --- Tasks ---
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
  updateTask() { this.http.put(`${this.apiUrl}/Tasks/${this.editingTask.id}`, this.editingTask, this.getAuthHeaders()).subscribe(() => { this.loadProjects(); this.cancelTaskEdit(); }); }
  cancelTaskEdit() { this.editingTask = null; this.editingTaskProjectId = ''; }
  deleteTask(taskId: string) { if (!confirm('Delete this task?')) return; this.http.delete(`${this.apiUrl}/Tasks/${taskId}`, this.getAuthHeaders()).subscribe(() => this.loadProjects()); }

}

bootstrapApplication(App, { providers: [provideHttpClient()] });
