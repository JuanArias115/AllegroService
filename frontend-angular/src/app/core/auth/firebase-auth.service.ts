import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { filter } from 'rxjs/operators';
import { initializeApp } from 'firebase/app';
import {
  Auth,
  AuthError,
  GoogleAuthProvider,
  User,
  browserLocalPersistence,
  getAuth,
  getRedirectResult,
  onIdTokenChanged,
  setPersistence,
  signInWithEmailAndPassword,
  signInWithPopup,
  signInWithRedirect,
  signOut
} from 'firebase/auth';
import { RuntimeConfigService } from '../config/runtime-config.service';
import { DecodedClaims, parseJwtClaims } from './claims.util';
import { labelOf, USER_TENANT_ROLE_OPTIONS, USER_TENANT_STATUS_OPTIONS } from '../models/enums';

export interface UserTenantSession {
  firebaseUid: string;
  email: string | null;
  glampingId: string;
  role: number;
  status: number;
}

export type NoAccessReason = 'pending' | 'disabled' | 'not_onboarded' | 'forbidden' | 'endpoint_missing';

export interface NoAccessState {
  reason: NoAccessReason;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class FirebaseAuthService {
  private static readonly ACTIVE_STATUS = 2;
  private static readonly ROLE_ADMIN = 1;
  private static readonly ROLE_INVENTORY = 4;
  private static readonly tenantEndpoints = ['/v1/user-tenants/me', '/v1/me', '/v1/user-tenants/current'];

  private auth: Auth | null = null;
  private readonly userSubject = new BehaviorSubject<User | null>(null);
  private readonly tokenSubject = new BehaviorSubject<string | null>(null);
  private readonly claimsSubject = new BehaviorSubject<DecodedClaims | null>(null);
  private readonly sessionSubject = new BehaviorSubject<UserTenantSession | null>(null);
  private readonly noAccessStateSubject = new BehaviorSubject<NoAccessState | null>(null);
  private readonly initializedSubject = new BehaviorSubject<boolean>(false);
  private resolvedTenantEndpointPath: string | null = null;
  private sessionLoadedForUid: string | null = null;

  readonly user$ = this.userSubject.asObservable();
  readonly token$ = this.tokenSubject.asObservable();
  readonly claims$ = this.claimsSubject.asObservable();
  readonly session$ = this.sessionSubject.asObservable();
  readonly noAccessState$ = this.noAccessStateSubject.asObservable();
  readonly initialized$ = this.initializedSubject.asObservable();

  constructor(
    private readonly runtimeConfig: RuntimeConfigService,
    private readonly http: HttpClient
  ) {}

  async initialize(): Promise<void> {
    if (this.auth) {
      this.initializedSubject.next(true);
      return;
    }

    const firebaseConfig = this.runtimeConfig.value.firebase;
    const app = initializeApp({
      apiKey: firebaseConfig.apiKey,
      authDomain: firebaseConfig.authDomain,
      projectId: firebaseConfig.projectId,
      appId: firebaseConfig.appId
    });

    this.auth = getAuth(app);
    await setPersistence(this.auth, browserLocalPersistence);

    try {
      await getRedirectResult(this.auth);
    } catch {
      // Ignore redirect result errors to allow regular auth flow.
    }

    onIdTokenChanged(this.auth, async (user) => {
      const previousUid = this.userSubject.value?.uid ?? null;
      this.userSubject.next(user);

      if (!user) {
        this.tokenSubject.next(null);
        this.claimsSubject.next(null);
        this.clearSessionState();
        this.initializedSubject.next(true);
        return;
      }

      const token = await user.getIdToken();
      this.tokenSubject.next(token);
      this.claimsSubject.next(parseJwtClaims(token));

      if (previousUid !== user.uid) {
        this.sessionSubject.next(null);
        this.sessionLoadedForUid = null;
        this.noAccessStateSubject.next(null);
      }

      this.initializedSubject.next(true);
    });
  }

  async waitUntilInitialized(): Promise<void> {
    if (this.initializedSubject.value) {
      return;
    }

    await firstValueFrom(this.initialized$.pipe(filter((ready) => ready)));
  }

  async signInWithEmailPassword(email: string, password: string): Promise<void> {
    this.ensureInitialized();
    await signInWithEmailAndPassword(this.auth!, email, password);
    await this.getIdToken(true);
  }

  async signInWithGoogle(): Promise<void> {
    this.ensureInitialized();

    const provider = new GoogleAuthProvider();
    provider.setCustomParameters({ prompt: 'select_account' });

    try {
      await signInWithPopup(this.auth!, provider);
      await this.getIdToken(true);
      return;
    } catch (error) {
      if (this.shouldFallbackToRedirect(error)) {
        await signInWithRedirect(this.auth!, provider);
        return;
      }

      throw error;
    }
  }

  async signOut(): Promise<void> {
    if (!this.auth) {
      return;
    }

    await signOut(this.auth);
    this.userSubject.next(null);
    this.tokenSubject.next(null);
    this.claimsSubject.next(null);
    this.clearSessionState();
  }

  async getIdToken(forceRefresh = false): Promise<string | null> {
    if (!this.auth?.currentUser) {
      return null;
    }

    const token = await this.auth.currentUser.getIdToken(forceRefresh);
    this.tokenSubject.next(token);
    this.claimsSubject.next(parseJwtClaims(token));
    return token;
  }

  async refreshIdToken(): Promise<string | null> {
    return this.getIdToken(true);
  }

  async loadUserTenant(forceRefresh = false): Promise<UserTenantSession | null> {
    this.ensureInitialized();

    if (!this.auth?.currentUser) {
      this.clearSessionState();
      return null;
    }

    const token = await this.getIdToken(forceRefresh);
    if (!token) {
      this.clearSessionState();
      return null;
    }

    const claims = parseJwtClaims(token);
    const firebaseUid = claims?.sub ?? this.auth.currentUser.uid;
    const fallbackEmail = claims?.email ?? this.auth.currentUser.email ?? null;

    if (!forceRefresh && this.sessionLoadedForUid === firebaseUid && this.sessionSubject.value) {
      return this.sessionSubject.value;
    }

    try {
      const payload = await this.fetchCurrentUserTenant();
      const session = this.mapSessionPayload(payload, firebaseUid, fallbackEmail);

      this.sessionSubject.next(session);
      this.noAccessStateSubject.next(this.sessionStateFromStatus(session.status));
      this.sessionLoadedForUid = firebaseUid;
      return session;
    } catch (error: unknown) {
      this.sessionSubject.next(null);
      this.sessionLoadedForUid = firebaseUid;
      this.setNoAccessFromError(error);
      throw error;
    }
  }

  get isAuthenticated(): boolean {
    return !!this.userSubject.value;
  }

  get currentEmail(): string {
    return this.userSubject.value?.email ?? '';
  }

  get currentFirebaseUid(): string | null {
    return this.sessionSubject.value?.firebaseUid
      ?? this.claimsSubject.value?.sub
      ?? this.userSubject.value?.uid
      ?? null;
  }

  get currentRole(): number | null {
    return this.sessionSubject.value?.role ?? null;
  }

  get currentRoleLabel(): string {
    const role = this.currentRole;
    return role ? labelOf(USER_TENANT_ROLE_OPTIONS, role) : 'N/A';
  }

  get glampingId(): string | null {
    return this.sessionSubject.value?.glampingId ?? null;
  }

  get currentStatus(): number | null {
    return this.sessionSubject.value?.status ?? null;
  }

  get currentStatusLabel(): string {
    const status = this.currentStatus;
    return status ? labelOf(USER_TENANT_STATUS_OPTIONS, status) : 'N/A';
  }

  get noAccessState(): NoAccessState | null {
    return this.noAccessStateSubject.value;
  }

  isActiveSession(): boolean {
    return this.sessionSubject.value?.status === FirebaseAuthService.ACTIVE_STATUS;
  }

  canViewInventory(): boolean {
    const role = this.currentRole;
    return role === FirebaseAuthService.ROLE_ADMIN || role === FirebaseAuthService.ROLE_INVENTORY;
  }

  canManageUsers(): boolean {
    return this.currentRole === FirebaseAuthService.ROLE_ADMIN;
  }

  clearNoAccessState(): void {
    this.noAccessStateSubject.next(null);
  }

  setNoAccessFromError(error: unknown): void {
    if (!(error instanceof HttpErrorResponse)) {
      this.noAccessStateSubject.next({ reason: 'forbidden', message: 'Sin acceso.' });
      return;
    }

    if (error.status === 403) {
      const code = this.extractBackendCode(error);

      if (code === 'USER_NOT_ONBOARDED') {
        this.noAccessStateSubject.next({
          reason: 'not_onboarded',
          message: 'Tu cuenta no esta habilitada todavia. Contacta a un administrador.'
        });
        return;
      }

      if (code === 'USER_DISABLED') {
        this.noAccessStateSubject.next({ reason: 'disabled', message: 'Cuenta deshabilitada.' });
        return;
      }

      this.noAccessStateSubject.next({ reason: 'forbidden', message: 'Sin acceso.' });
      return;
    }

    if (error.status === 404) {
      this.noAccessStateSubject.next({
        reason: 'endpoint_missing',
        message: 'No se pudo validar tu habilitacion en el backend. Contacta a soporte.'
      });
      return;
    }

    this.noAccessStateSubject.next({ reason: 'forbidden', message: 'Sin acceso.' });
  }

  private async fetchCurrentUserTenant(): Promise<unknown> {
    const candidates = this.buildEndpointCandidates();
    const tried = new Set<string>();

    for (const path of candidates) {
      if (tried.has(path)) {
        continue;
      }

      tried.add(path);

      try {
        const response = await firstValueFrom(this.http.get<unknown>(this.resolveApiPath(path)));
        this.resolvedTenantEndpointPath = path;
        return response;
      } catch (error: unknown) {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          if (this.resolvedTenantEndpointPath === path) {
            this.resolvedTenantEndpointPath = null;
          }
          continue;
        }

        this.resolvedTenantEndpointPath = path;
        throw error;
      }
    }

    this.resolvedTenantEndpointPath = null;
    throw new HttpErrorResponse({
      status: 404,
      statusText: 'Not Found',
      error: { detail: 'USER_TENANT_ENDPOINT_NOT_FOUND' }
    });
  }

  private mapSessionPayload(payload: unknown, fallbackUid: string, fallbackEmail: string | null): UserTenantSession {
    const raw = this.unwrapEnvelope(payload);
    if (!raw || typeof raw !== 'object') {
      throw new Error('Invalid user-tenant payload.');
    }

    const data = raw as Record<string, unknown>;
    const firebaseUid = this.stringValue(data['firebaseUid']) ?? fallbackUid;
    const email = this.stringValue(data['email']) ?? fallbackEmail;
    const glampingId = this.stringValue(data['glampingId']);
    const role = this.numberValue(data['role']);
    const status = this.numberValue(data['status']);

    if (!glampingId || role === null || status === null) {
      throw new Error('Incomplete user-tenant payload.');
    }

    return {
      firebaseUid,
      email,
      glampingId,
      role,
      status
    };
  }

  private sessionStateFromStatus(status: number): NoAccessState | null {
    if (status === FirebaseAuthService.ACTIVE_STATUS) {
      return null;
    }

    if (status === 1) {
      return { reason: 'pending', message: 'Pendiente de activacion.' };
    }

    if (status === 3) {
      return { reason: 'disabled', message: 'Cuenta deshabilitada.' };
    }

    return { reason: 'forbidden', message: 'Sin acceso.' };
  }

  private unwrapEnvelope(payload: unknown): unknown {
    if (!payload || typeof payload !== 'object') {
      return payload;
    }

    const envelope = payload as { data?: unknown };
    if (Object.prototype.hasOwnProperty.call(envelope, 'data')) {
      return envelope.data;
    }

    return payload;
  }

  private extractBackendCode(error: HttpErrorResponse): string | null {
    const payload = error.error;

    if (typeof payload === 'string' && payload.trim().length > 0) {
      return payload.trim().toUpperCase();
    }

    if (payload && typeof payload === 'object') {
      const obj = payload as Record<string, unknown>;

      if (typeof obj['code'] === 'string') {
        return obj['code'].toUpperCase();
      }

      if (typeof obj['detail'] === 'string') {
        return obj['detail'].trim().toUpperCase();
      }
    }

    return null;
  }

  private buildEndpointCandidates(): string[] {
    if (!this.resolvedTenantEndpointPath) {
      return [...FirebaseAuthService.tenantEndpoints];
    }

    return [
      this.resolvedTenantEndpointPath,
      ...FirebaseAuthService.tenantEndpoints.filter((path) => path !== this.resolvedTenantEndpointPath)
    ];
  }

  private resolveApiPath(path: string): string {
    const base = this.runtimeConfig.value.apiBaseUrl.replace(/\/$/, '');
    const cleanPath = path.startsWith('/') ? path : `/${path}`;
    return `${base}${cleanPath}`;
  }

  private numberValue(value: unknown): number | null {
    if (typeof value === 'number' && Number.isFinite(value)) {
      return value;
    }

    if (typeof value === 'string' && value.trim().length > 0) {
      const parsed = Number(value);
      return Number.isFinite(parsed) ? parsed : null;
    }

    return null;
  }

  private stringValue(value: unknown): string | null {
    if (typeof value !== 'string') {
      return null;
    }

    const clean = value.trim();
    return clean.length > 0 ? clean : null;
  }

  private clearSessionState(): void {
    this.sessionSubject.next(null);
    this.noAccessStateSubject.next(null);
    this.sessionLoadedForUid = null;
  }

  private ensureInitialized(): void {
    if (!this.auth) {
      throw new Error('Firebase auth is not initialized.');
    }
  }

  private shouldFallbackToRedirect(error: unknown): boolean {
    if (!error || typeof error !== 'object') {
      return false;
    }

    const authError = error as Partial<AuthError>;
    return authError.code === 'auth/popup-blocked'
      || authError.code === 'auth/cancelled-popup-request'
      || authError.code === 'auth/operation-not-supported-in-this-environment';
  }
}
