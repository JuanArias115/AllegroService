import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info';
  text: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly subject = new BehaviorSubject<ToastMessage[]>([]);
  readonly messages$ = this.subject.asObservable();

  success(text: string): void {
    this.push('success', text);
  }

  error(text: string): void {
    this.push('error', text);
  }

  info(text: string): void {
    this.push('info', text);
  }

  dismiss(id: string): void {
    this.subject.next(this.subject.value.filter((message) => message.id !== id));
  }

  private push(type: ToastMessage['type'], text: string): void {
    const message: ToastMessage = {
      id: `${Date.now()}-${Math.random()}`,
      type,
      text
    };

    this.subject.next([...this.subject.value, message]);
    setTimeout(() => this.dismiss(message.id), 3500);
  }
}
