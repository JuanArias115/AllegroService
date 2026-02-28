import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FirebaseAuthService } from '../../core/auth/firebase-auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  loadingEmail = false;
  loadingGoogle = false;
  errorMessage = '';

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: FirebaseAuthService,
    private readonly router: Router
  ) {}

  get loading(): boolean {
    return this.loadingEmail || this.loadingGoogle;
  }

  async signInWithEmailPassword(): Promise<void> {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    this.auth.clearNoAccessState();
    this.loadingEmail = true;

    try {
      await this.auth.signInWithEmailPassword(
        this.form.controls.email.value,
        this.form.controls.password.value
      );

      await this.resolvePostAuthNavigation();
    } catch (error: unknown) {
      this.errorMessage = this.errorText(error, 'No fue posible iniciar sesion con email/password. Verifica tus credenciales.');
    } finally {
      this.loadingEmail = false;
    }
  }

  async signInWithGoogle(): Promise<void> {
    if (this.loading) {
      return;
    }

    this.errorMessage = '';
    this.auth.clearNoAccessState();
    this.loadingGoogle = true;

    try {
      await this.auth.signInWithGoogle();
      await this.resolvePostAuthNavigation();
    } catch (error: unknown) {
      this.errorMessage = this.errorText(error, 'No fue posible iniciar sesion con Google. Reintenta o valida la configuracion de Firebase.');
    } finally {
      this.loadingGoogle = false;
    }
  }

  private async resolvePostAuthNavigation(): Promise<void> {
    try {
      await this.auth.loadUserTenant(true);
    } catch {
      await this.router.navigate(['/no-access']);
      return;
    }

    if (!this.auth.isActiveSession()) {
      await this.router.navigate(['/no-access']);
      return;
    }

    await this.router.navigate(['/']);
  }

  private errorText(error: unknown, fallback: string): string {
    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    if (error.status === 403) {
      return this.auth.noAccessState?.message ?? 'Tu cuenta no esta habilitada todavia. Contacta a un administrador.';
    }

    return fallback;
  }
}
