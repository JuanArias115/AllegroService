import { Injectable } from '@angular/core';
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
import { DecodedClaims, getGlampingIdFromClaims, parseJwtClaims } from './claims.util';

@Injectable({ providedIn: 'root' })
export class FirebaseAuthService {
  private auth: Auth | null = null;
  private readonly userSubject = new BehaviorSubject<User | null>(null);
  private readonly tokenSubject = new BehaviorSubject<string | null>(null);
  private readonly claimsSubject = new BehaviorSubject<DecodedClaims | null>(null);
  private readonly initializedSubject = new BehaviorSubject<boolean>(false);

  readonly user$ = this.userSubject.asObservable();
  readonly token$ = this.tokenSubject.asObservable();
  readonly claims$ = this.claimsSubject.asObservable();
  readonly initialized$ = this.initializedSubject.asObservable();

  constructor(private readonly runtimeConfig: RuntimeConfigService) {}

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
      this.userSubject.next(user);

      if (!user) {
        this.tokenSubject.next(null);
        this.claimsSubject.next(null);
        this.initializedSubject.next(true);
        return;
      }

      const token = await user.getIdToken();
      this.tokenSubject.next(token);
      this.claimsSubject.next(parseJwtClaims(token));
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

  get isAuthenticated(): boolean {
    return !!this.userSubject.value;
  }

  get currentEmail(): string {
    return this.userSubject.value?.email ?? '';
  }

  get currentRole(): string | null {
    return this.claimsSubject.value?.role ?? null;
  }

  get glampingId(): string | null {
    return getGlampingIdFromClaims(this.claimsSubject.value);
  }

  get hasGlampingAccess(): boolean {
    return !!this.glampingId;
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
