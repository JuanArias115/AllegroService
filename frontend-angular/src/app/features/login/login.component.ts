import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
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
    this.loadingEmail = true;

    try {
      await this.auth.signInWithEmailPassword(
        this.form.controls.email.value,
        this.form.controls.password.value
      );

      await this.resolvePostAuthNavigation();
    } catch {
      this.errorMessage = 'No fue posible iniciar sesion con email/password. Verifica tus credenciales.';
    } finally {
      this.loadingEmail = false;
    }
  }

  async signInWithGoogle(): Promise<void> {
    if (this.loading) {
      return;
    }

    this.errorMessage = '';
    this.loadingGoogle = true;

    try {
      await this.auth.signInWithGoogle();
      await this.resolvePostAuthNavigation();
    } catch {
      this.errorMessage = 'No fue posible iniciar sesion con Google. Reintenta o valida la configuracion de Firebase.';
      this.loadingGoogle = false;
    }
  }

  private async resolvePostAuthNavigation(): Promise<void> {
    if (!this.auth.hasGlampingAccess) {
      this.errorMessage = 'Tu usuario no tiene glamping_id valido. Contacta a un administrador.';
      await this.router.navigate(['/no-access']);
      return;
    }

    await this.router.navigate(['/']);
  }
}
