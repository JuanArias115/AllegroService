import { Injectable } from '@angular/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { filter } from 'rxjs/operators';
import { initializeApp } from 'firebase/app';
import {
  Auth,
  User,
  browserLocalPersistence,
  getAuth,
  onIdTokenChanged,
  setPersistence,
  signInWithEmailAndPassword,
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

  async login(email: string, password: string): Promise<void> {
    this.ensureInitialized();
    await signInWithEmailAndPassword(this.auth!, email, password);
    await this.refreshToken();
  }

  async logout(): Promise<void> {
    if (!this.auth) {
      return;
    }

    await signOut(this.auth);
    this.userSubject.next(null);
    this.tokenSubject.next(null);
    this.claimsSubject.next(null);
  }

  async getValidToken(): Promise<string | null> {
    if (!this.auth?.currentUser) {
      return null;
    }

    return this.refreshToken();
  }

  get isAuthenticated(): boolean {
    return !!this.userSubject.value;
  }

  get currentEmail(): string {
    return this.userSubject.value?.email ?? '';
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

  private async refreshToken(): Promise<string | null> {
    if (!this.auth?.currentUser) {
      return null;
    }

    const token = await this.auth.currentUser.getIdToken(true);
    this.tokenSubject.next(token);
    this.claimsSubject.next(parseJwtClaims(token));
    return token;
  }
}
