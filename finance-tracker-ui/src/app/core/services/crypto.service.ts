import { Injectable } from '@angular/core';

/**
 * All cryptographic operations run exclusively in the browser via the Web Crypto API.
 * No keys, plaintexts, or secrets ever leave the browser or reach the server.
 *
 * Algorithm:  AES-256-GCM  (authenticated encryption)
 * Key derivation: PBKDF2-SHA512, 600 000 iterations
 * Wire format: Base64( IV(12 bytes) | ciphertext+tag(n+16 bytes) )
 */
@Injectable({ providedIn: 'root' })
export class CryptoService {

  private readonly PBKDF2_ITERATIONS = 600_000;
  private readonly IV_LENGTH = 12;

  // ─── Key derivation ───────────────────────────────────────────────────────

  /** Generates 32 cryptographically random bytes, returned as Base64. */
  generateSalt(): string {
    const salt = crypto.getRandomValues(new Uint8Array(32));
    return this.toBase64(salt);
  }

  /**
   * Derives a non-extractable AES-256-GCM key from a password and Base64-encoded salt.
   * The key stays inside the browser's WebCrypto key store — it cannot be exported.
   */
  async deriveKey(password: string, saltBase64: string): Promise<CryptoKey> {
    const passwordKey = await crypto.subtle.importKey(
      'raw',
      new TextEncoder().encode(password),
      'PBKDF2',
      false,
      ['deriveKey'],
    );

    return crypto.subtle.deriveKey(
      {
        name: 'PBKDF2',
        salt: this.fromBase64(saltBase64).buffer as ArrayBuffer,
        iterations: this.PBKDF2_ITERATIONS,
        hash: 'SHA-512',
      },
      passwordKey,
      { name: 'AES-GCM', length: 256 },
      true,           // extractable so we can persist it in sessionStorage for refresh survival
      ['encrypt', 'decrypt'],
    );
  }

  /** Exports an AES-GCM key to a Base64 string for sessionStorage. */
  async exportKey(key: CryptoKey): Promise<string> {
    const raw = await crypto.subtle.exportKey('raw', key);
    return this.toBase64(new Uint8Array(raw));
  }

  /** Re-imports a Base64 AES-GCM key from sessionStorage. */
  async importSessionKey(keyBase64: string): Promise<CryptoKey> {
    const raw = this.fromBase64(keyBase64).buffer as ArrayBuffer;
    return crypto.subtle.importKey(
      'raw',
      raw,
      { name: 'AES-GCM', length: 256 },
      true,
      ['encrypt', 'decrypt'],
    );
  }

  // ─── Encrypt / Decrypt ───────────────────────────────────────────────────

  /**
   * Encrypts a string with AES-256-GCM.
   * Returns Base64( IV(12) | ciphertext+authTag(n+16) )
   */
  async encrypt(plaintext: string, key: CryptoKey): Promise<string> {
    const iv = crypto.getRandomValues(new Uint8Array(this.IV_LENGTH));
    const encoded = new TextEncoder().encode(plaintext);

    // Web Crypto automatically appends the 16-byte auth tag to the ciphertext
    const ciphertext = await crypto.subtle.encrypt(
      { name: 'AES-GCM', iv },
      key,
      encoded,
    );

    const result = new Uint8Array(this.IV_LENGTH + ciphertext.byteLength);
    result.set(iv, 0);
    result.set(new Uint8Array(ciphertext), this.IV_LENGTH);

    return this.toBase64(result);
  }

  /**
   * Decrypts a Base64-encoded AES-256-GCM ciphertext.
   * Throws if the data was tampered with or the wrong key is used.
   */
  async decrypt(ciphertextBase64: string, key: CryptoKey): Promise<string> {
    const data = this.fromBase64(ciphertextBase64);
    const iv = data.slice(0, this.IV_LENGTH);
    const ciphertext = data.slice(this.IV_LENGTH);

    const plaintext = await crypto.subtle.decrypt(
      { name: 'AES-GCM', iv },
      key,
      ciphertext,
    );

    return new TextDecoder().decode(plaintext);
  }

  // ─── Helpers ─────────────────────────────────────────────────────────────

  toBase64(bytes: Uint8Array): string {
    return btoa(String.fromCharCode(...bytes));
  }

  fromBase64(b64: string): Uint8Array {
    return Uint8Array.from(atob(b64), c => c.charCodeAt(0));
  }
}
