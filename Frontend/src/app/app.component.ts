import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

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

  // --- Auth ---
  isLoggedIn = false;
  showRegister = false;
  token: string | null = null;
  userEmail = '';
  authForm = { email: '', password: '', role: '', department: '' };

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



  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.token = localStorage.getItem('token');
    if (this.token) {
      this.isLoggedIn = true;
      this.userEmail = localStorage.getItem('userEmail') || '';
      this.loadProjects();

    }
    this.loadAllUsers();
  }

   // 🔹 Method to fetch users and log them

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

  // --- Auth Methods ---
  toggleAuthMode() { this.showRegister = !this.showRegister; }

  register() {
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

 


  // --- Projects Methods ---
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
              },
              error: err => console.error('Failed to load tasks', err)
            });
        },
        error: err => { console.error('Failed to load projects', err); if (err.status === 401) this.logout(); }
      });
  }

  addUserId(): void {
  const selectedUserId = this.projectForm.assignedUserId;

  if (selectedUserId && !this.projectForm.userIds.includes(selectedUserId)) {
    this.projectForm.userIds.push(selectedUserId); // add ID
    this.projectForm.assignedUserId = '';          // reset dropdown
  }
}

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
        .subscribe(() => { this.loadProjects(); this.cancelProjectEdit(); });
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

  editProject(project: any) { this.projectForm = { ...project, membersStr: project.members.join(',') }; this.editingProject = true; }
  cancelProjectEdit() { this.projectForm = { id: null, name: '', description: '', membersStr: '' }; this.editingProject = false; }
  deleteProject(id: string) { if (!confirm('Delete this project?')) return; this.http.delete(`${this.apiUrl}/Projects/${id}`, this.getAuthHeaders()).subscribe(() => this.loadProjects()); }

  // --- Tasks Methods ---
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
