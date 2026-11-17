import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateTaskRequest, CreateUserRequest, TaskItem, TaskStatus, User } from './task.models';

@Injectable({ providedIn: 'root' })
export class TaskApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/users`);
  }

  createUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/users`, request);
  }

  getTasks(status?: string): Observable<TaskItem[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<TaskItem[]>(`${this.apiUrl}/tasks`, { params });
  }

  createTask(request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${this.apiUrl}/tasks`, request);
  }

  updateTaskStatus(id: string, status: TaskStatus): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.apiUrl}/tasks/${id}/status`, { status });
  }
}
