import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TaskApiService } from './core/task-api.service';
import { CreateTaskRequest, TaskItem, TaskStatus, User } from './core/task.models';

@Component({
  selector: 'app-root',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly api = inject(TaskApiService);

  readonly statuses: TaskStatus[] = ['Pending', 'InProgress', 'Done'];
  readonly tasks = signal<TaskItem[]>([]);
  readonly users = signal<User[]>([]);
  readonly statusFilter = signal('');
  readonly loadingTasks = signal(false);
  readonly savingTask = signal(false);
  readonly savingUser = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly taskForm = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', Validators.maxLength(2000)],
    userId: ['', Validators.required],
    priority: ['Medium'],
    estimatedCompletionDate: [''],
    tags: ['']
  });

  readonly userForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]]
  });

  ngOnInit(): void {
    this.loadUsers();
    this.loadTasks();
  }

  loadUsers(): void {
    this.api.getUsers().subscribe({
      next: (users) => this.users.set(users),
      error: (error: unknown) => this.showError(error)
    });
  }

  loadTasks(): void {
    this.loadingTasks.set(true);
    this.api.getTasks(this.statusFilter() || undefined).subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
        this.loadingTasks.set(false);
      },
      error: (error: unknown) => {
        this.loadingTasks.set(false);
        this.showError(error);
      }
    });
  }

  filterTasks(status: string): void {
    this.statusFilter.set(status);
    this.loadTasks();
  }

  createUser(): void {
    if (this.userForm.invalid || this.savingUser()) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.clearMessages();
    this.savingUser.set(true);
    this.api.createUser(this.userForm.getRawValue()).subscribe({
      next: (user) => {
        this.users.update((users) => [...users, user].sort((a, b) => a.name.localeCompare(b.name)));
        this.taskForm.controls.userId.setValue(user.id);
        this.userForm.reset();
        this.savingUser.set(false);
        this.successMessage.set('Colaborador creado correctamente.');
      },
      error: (error: unknown) => {
        this.savingUser.set(false);
        this.showError(error);
      }
    });
  }

  createTask(): void {
    if (this.taskForm.invalid || this.savingTask()) {
      this.taskForm.markAllAsTouched();
      return;
    }

    const formValue = this.taskForm.getRawValue();
    const tags = formValue.tags.split(',').map((tag) => tag.trim()).filter(Boolean);
    const request: CreateTaskRequest = {
      title: formValue.title,
      description: formValue.description || null,
      userId: formValue.userId,
      additionalData: {
        priority: formValue.priority || null,
        estimatedCompletionDate: formValue.estimatedCompletionDate
          ? new Date(`${formValue.estimatedCompletionDate}T00:00:00Z`).toISOString()
          : null,
        tags,
        metadata: null
      }
    };

    this.clearMessages();
    this.savingTask.set(true);
    this.api.createTask(request).subscribe({
      next: () => {
        this.taskForm.reset({ title: '', description: '', userId: formValue.userId,
          priority: 'Medium', estimatedCompletionDate: '', tags: '' });
        this.savingTask.set(false);
        this.successMessage.set('Tarea creada correctamente.');
        this.loadTasks();
      },
      error: (error: unknown) => {
        this.savingTask.set(false);
        this.showError(error);
      }
    });
  }

  changeStatus(task: TaskItem, status: string): void {
    if (!this.statuses.includes(status as TaskStatus) || status === task.status) {
      return;
    }

    this.clearMessages();
    this.api.updateTaskStatus(task.id, status as TaskStatus).subscribe({
      next: () => {
        this.successMessage.set('Estado actualizado correctamente.');
        this.loadTasks();
      },
      error: (error: unknown) => this.showError(error)
    });
  }

  statusLabel(status: TaskStatus): string {
    return {
      Pending: 'Pendiente',
      InProgress: 'En progreso',
      Done: 'Completada'
    }[status];
  }

  metadataTags(task: TaskItem): string[] {
    return task.additionalData?.tags ?? [];
  }

  private clearMessages(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  private showError(error: unknown): void {
    let message = 'No se pudo completar la operación. Verifica que la API esté disponible.';
    if (error instanceof HttpErrorResponse) {
      const payload: unknown = error.error;
      if (typeof payload === 'object' && payload !== null && 'message' in payload
          && typeof payload.message === 'string') {
        message = payload.message;
      } else if (typeof payload === 'object' && payload !== null && 'title' in payload
          && typeof payload.title === 'string') {
        message = payload.title;
      } else if (error.message) {
        message = error.message;
      }
    }

    this.errorMessage.set(message);
    this.successMessage.set('');
  }
}
