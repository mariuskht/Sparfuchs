import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CryptoService } from './crypto.service';
import { CurrentUser, LoginRequest, LoginResponse, RegisterRequest } from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private readonly apiUrl = `${environment.apiUrl}/auth`;

  private _encryptionKey: CryptoKey | null = null;

  private readonly TOKEN_KEY = 'ft_token';
  private readonly KEY_KEY   = 'ft_key';
  private readonly USER_KEY  = 'ft_user';

  currentUser = signal<CurrentUser | null>(null);

  constructor(
    private http: HttpClient,
    private crypto: CryptoService,
    private router: Router,
  ) {}

  // ─── Session restore (called via APP_INITIALIZER before any route) ──────

  async restoreSession(): Promise<void> {
    const token   = sessionStorage.getItem(this.TOKEN_KEY);
    const keyB64  = sessionStorage.getItem(this.KEY_KEY);
    const userRaw = sessionStorage.getItem(this.USER_KEY);

    if (!token || !keyB64 || !userRaw) return;

    try {
      this._encryptionKey = await this.crypto.importSessionKey(keyB64);
      this.currentUser.set(JSON.parse(userRaw) as CurrentUser);
    } catch {
      // Stored data corrupt — force clean login
      this.clearSession();
    }
  }

  // ─── Public API ─────────────────────────────────────────────────────────

  get encryptionKey(): CryptoKey {
    if (!this._encryptionKey) throw new Error('Not authenticated: encryption key not available.');
    return this._encryptionKey;
  }

  get isLoggedIn(): boolean {
    return this._encryptionKey !== null && !!sessionStorage.getItem(this.TOKEN_KEY);
  }

  getToken(): string | null {
    return sessionStorage.getItem(this.TOKEN_KEY);
  }

  async register(email: string, username: string, password: string): Promise<void> {
    const encryptionSalt = this.crypto.generateSalt();
    const key = await this.crypto.deriveKey(password, encryptionSalt);

    const [encryptedEmail, encryptedUsername] = await Promise.all([
      this.crypto.encrypt(email, key),
      this.crypto.encrypt(username, key),
    ]);

    await firstValueFrom(
      this.http.post(`${this.apiUrl}/register`, {
        email,
        password,
        encryptedEmail,
        encryptedUsername,
        encryptionSalt,
      } satisfies RegisterRequest),
    );

    await this.login(email, password);
  }

  async login(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>(`${this.apiUrl}/login`, {
        email,
        password,
      } satisfies LoginRequest),
    );

    const key = await this.crypto.deriveKey(password, response.encryptionSalt);

    const [username, decryptedEmail] = await Promise.all([
      this.crypto.decrypt(response.encryptedUsername, key),
      this.crypto.decrypt(response.encryptedEmail, key),
    ]);

    const user: CurrentUser = { username, email: decryptedEmail };

    // Persist session so page refresh doesn't force re-login
    const keyB64 = await this.crypto.exportKey(key);
    sessionStorage.setItem(this.TOKEN_KEY, response.token);
    sessionStorage.setItem(this.KEY_KEY,   keyB64);
    sessionStorage.setItem(this.USER_KEY,  JSON.stringify(user));

    this._encryptionKey = key;
    this.currentUser.set(user);

    await this.router.navigate(['/dashboard']);
  }

  logout(): void {
    this._encryptionKey = null;
    this.currentUser.set(null);
    this.clearSession();
    this.router.navigate(['/login']);
  }

  // ─── Private ────────────────────────────────────────────────────────────

  private clearSession(): void {
    sessionStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.KEY_KEY);
    sessionStorage.removeItem(this.USER_KEY);
  }
}
