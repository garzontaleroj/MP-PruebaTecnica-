export type TaskStatus = 'Pending' | 'InProgress' | 'Done';

export interface User {
  id: string;
  name: string;
  email: string;
  createdAt: string;
}

export interface TaskMetadata {
  priority: string | null;
  estimatedCompletionDate: string | null;
  tags: string[] | null;
  metadata: Record<string, unknown> | null;
}

export interface TaskItem {
  id: string;
  title: string;
  description: string | null;
  userId: string;
  userName: string;
  status: TaskStatus;
  additionalData: TaskMetadata | null;
  createdAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string | null;
  userId: string;
  additionalData: TaskMetadata;
}

export interface CreateUserRequest {
  name: string;
  email: string;
}
