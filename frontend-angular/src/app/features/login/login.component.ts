import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FirebaseAuthService } from '../../core/auth/firebase-auth.service';
import { IconComponent } from '../../shared/components/icon/icon.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, IconComponent],
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
    private readonly router: Router,
    private readonly translate: TranslateService
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
      this.errorMessage = this.errorText(error, 'errors.loginEmail');
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
      this.errorMessage = this.errorText(error, 'errors.loginGoogle');
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
      return this.translate.instant(fallback);
    }

    if (error.status === 403) {
      return this.auth.noAccessState?.message ?? this.translate.instant('noaccess.generic');
    }

    return this.translate.instant(fallback);
  }
}
