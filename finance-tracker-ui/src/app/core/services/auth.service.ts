import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CryptoService } from './crypto.service';
import { CurrentUser, LoginResponse, RecoverVerifyResponse, RegisterRequest } from '../models/auth.model';
import { environment } from '../../../environments/environment';

const SESSION_STORAGE_KEY = 'ft_mk';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private _masterKeyRaw: Uint8Array<ArrayBuffer> | null = null;
  private _encryptionKey: CryptoKey | null = null;
  private _sessionData: LoginResponse | null = null;
  private _pendingRecoveryPhrase: string | null = null;
  hasCookieSession = false;
  currentUser = signal<CurrentUser | null>(null);

  get isLoggedIn(): boolean {
    return this._encryptionKey !== null;
  }

  get encryptionKey(): CryptoKey {
    if (!this._encryptionKey) throw new Error('Not authenticated');
    return this._encryptionKey;
  }

  constructor(
    private http: HttpClient,
    private crypto: CryptoService,
    private router: Router,
  ) {}

  // Called by APP_INITIALIZER before route guards run.
  // If the JWT cookie is valid and the master key is still in sessionStorage (same tab, page refresh),
  // the session is fully restored without requiring the password again.
  async restoreSession(): Promise<void> {
    try {
      const data = await firstValueFrom(
        this.http.get<LoginResponse>(`${environment.apiUrl}/auth/me`),
      );
      this._sessionData = data;
      this.hasCookieSession = true;

      const storedKey = sessionStorage.getItem(SESSION_STORAGE_KEY);
      if (storedKey) {
        const masterKeyRaw = this.crypto.fromBase64(storedKey);
        await this._finalizeSession(masterKeyRaw, data);
      }
    } catch {
      this.hasCookieSession = false;
    }
  }

  async register(email: string, username: string, password: string): Promise<void> {
    const masterKeyRaw = this.crypto.generateMasterKey();

    const encSaltB64 = this.crypto.generateSaltB64();
    const passwordWrapKey = await this.crypto.derivePasswordWrapKey(password, encSaltB64);
    const passwordWrappedKey = await this.crypto.wrapKey(masterKeyRaw, passwordWrapKey);

    const recoveryPhrase = this.crypto.generateRecoveryPhrase();
    const recSaltB64 = this.crypto.generateSaltB64();
    const recoveryWrapKey = await this.crypto.deriveRecoveryWrapKey(recoveryPhrase, recSaltB64);
    const recoveryWrappedKey = await this.crypto.wrapKey(masterKeyRaw, recoveryWrapKey);
    const recoveryVerifier = await this.crypto.deriveRecoveryVerifier(recoveryPhrase, recSaltB64);

    const masterKey = await this.crypto.importAesKey(masterKeyRaw);
    const [encEmail, encUsername] = await Promise.all([
      this.crypto.encrypt(email, masterKey),
      this.crypto.encrypt(username, masterKey),
    ]);

    const body: RegisterRequest = {
      email,
      password,
      encryptedEmail: encEmail,
      encryptedUsername: encUsername,
      encryptionSalt: encSaltB64,
      passwordWrappedKey,
      recoverySalt: recSaltB64,
      recoveryWrappedKey,
      recoveryVerifier,
    };
    const data = await firstValueFrom(
      this.http.post<LoginResponse>(`${environment.apiUrl}/auth/register`, body),
    );

    this._pendingRecoveryPhrase = recoveryPhrase;
    await this._finalizeSession(masterKeyRaw, data, { username, email });
    this.router.navigate(['/dashboard']);
  }

  async login(email: string, password: string): Promise<void> {
    const data = await firstValueFrom(
      this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password }),
    );

    const passwordWrapKey = await this.crypto.derivePasswordWrapKey(password, data.encryptionSalt);
    const masterKeyRaw = await this.crypto.unwrapKey(data.passwordWrappedKey, passwordWrapKey);

    await this._finalizeSession(masterKeyRaw, data);
    this.router.navigate(['/dashboard']);
  }

  async reauth(password: string): Promise<void> {
    if (!this._sessionData) throw new Error('No session data — call restoreSession() first');

    const passwordWrapKey = await this.crypto.derivePasswordWrapKey(
      password,
      this._sessionData.encryptionSalt,
    );
    const masterKeyRaw = await this.crypto.unwrapKey(
      this._sessionData.passwordWrappedKey,
      passwordWrapKey,
    );

    await this._finalizeSession(masterKeyRaw, this._sessionData);
    this.router.navigate(['/dashboard']);
  }

  async recover(email: string, phrase: string): Promise<void> {
    const { recoverySalt } = await firstValueFrom(
      this.http.post<{ recoverySalt: string }>(
        `${environment.apiUrl}/auth/recover/challenge`,
        { email },
      ),
    );

    const recoveryVerifier = await this.crypto.deriveRecoveryVerifier(phrase, recoverySalt);
    const data = await firstValueFrom(
      this.http.post<RecoverVerifyResponse>(
        `${environment.apiUrl}/auth/recover/verify`,
        { email, recoveryVerifier },
      ),
    );

    const recoveryWrapKey = await this.crypto.deriveRecoveryWrapKey(phrase, data.recoverySalt);
    const masterKeyRaw = await this.crypto.unwrapKey(data.recoveryWrappedKey, recoveryWrapKey);

    await this._finalizeSession(masterKeyRaw, data);
    this.router.navigate(['/dashboard']);
  }

  consumeRecoveryPhrase(): string | null {
    const phrase = this._pendingRecoveryPhrase;
    this._pendingRecoveryPhrase = null;
    return phrase;
  }

  async regenerateRecoveryKey(): Promise<string> {
    if (!this._masterKeyRaw) throw new Error('Not authenticated');

    const phrase = this.crypto.generateRecoveryPhrase();
    const recSaltB64 = this.crypto.generateSaltB64();
    const recoveryWrapKey = await this.crypto.deriveRecoveryWrapKey(phrase, recSaltB64);
    const recoveryWrappedKey = await this.crypto.wrapKey(this._masterKeyRaw, recoveryWrapKey);
    const recoveryVerifier = await this.crypto.deriveRecoveryVerifier(phrase, recSaltB64);

    await firstValueFrom(
      this.http.post(`${environment.apiUrl}/auth/recovery`, {
        recoverySalt: recSaltB64,
        recoveryWrappedKey,
        recoveryVerifier,
      }),
    );

    return phrase;
  }

  logout(): void {
    this._masterKeyRaw = null;
    this._encryptionKey = null;
    this._sessionData = null;
    this._pendingRecoveryPhrase = null;
    this.hasCookieSession = false;
    this.currentUser.set(null);
    sessionStorage.removeItem(SESSION_STORAGE_KEY);

    firstValueFrom(
      this.http.post(`${environment.apiUrl}/auth/logout`, {}),
    ).catch(() => {});

    this.router.navigate(['/login']);
  }

  private async _finalizeSession(
    masterKeyRaw: Uint8Array<ArrayBuffer>,
    data: LoginResponse,
    knownUser?: { username: string; email: string },
  ): Promise<void> {
    this._masterKeyRaw = masterKeyRaw;
    this._encryptionKey = await this.crypto.importAesKey(masterKeyRaw);
    this._sessionData = data;
    this.hasCookieSession = true;

    // sessionStorage survives page refresh but clears on tab/browser close
    sessionStorage.setItem(SESSION_STORAGE_KEY, this.crypto.toBase64(masterKeyRaw));

    const username = knownUser?.username
      ?? await this.crypto.decrypt(data.encryptedUsername, this._encryptionKey);
    const email = knownUser?.email
      ?? await this.crypto.decrypt(data.encryptedEmail, this._encryptionKey);

    this.currentUser.set({ username, email });
  }
}
