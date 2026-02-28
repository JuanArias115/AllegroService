import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FirebaseAuthService } from '../../core/auth/firebase-auth.service';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  loading = false;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: FirebaseAuthService,
    private readonly toast: ToastService,
    private readonly router: Router
  ) {}

  async submit(): Promise<void> {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    try {
      await this.auth.login(this.form.controls.email.value, this.form.controls.password.value);

      if (!this.auth.hasGlampingAccess) {
        this.router.navigate(['/no-access']);
        return;
      }

      this.router.navigate(['/']);
    } catch {
      this.toast.error('Invalid credentials or Firebase configuration issue.');
    } finally {
      this.loading = false;
    }
  }
}
