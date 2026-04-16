import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class CryptoService {

  generateMasterKey(): Uint8Array<ArrayBuffer> {
    return crypto.getRandomValues(new Uint8Array(32));
  }

  generateSaltB64(): string {
    return this.toBase64(crypto.getRandomValues(new Uint8Array(16)));
  }

  async importAesKey(raw: Uint8Array<ArrayBuffer>): Promise<CryptoKey> {
    return crypto.subtle.importKey('raw', raw, { name: 'AES-GCM' }, false, ['encrypt', 'decrypt']);
  }

  // Key wrapping: base64( IV[12] || AES-GCM-ciphertext+tag )

  async wrapKey(masterKeyRaw: Uint8Array<ArrayBuffer>, wrappingKey: CryptoKey): Promise<string> {
    const iv = crypto.getRandomValues(new Uint8Array(12));
    const ciphertext = await crypto.subtle.encrypt({ name: 'AES-GCM', iv }, wrappingKey, masterKeyRaw);
    const combined = new Uint8Array(12 + ciphertext.byteLength);
    combined.set(iv);
    combined.set(new Uint8Array(ciphertext), 12);
    return this.toBase64(combined);
  }

  async unwrapKey(wrappedB64: string, wrappingKey: CryptoKey): Promise<Uint8Array<ArrayBuffer>> {
    const combined = this.fromBase64(wrappedB64);
    const plaintext = await crypto.subtle.decrypt(
      { name: 'AES-GCM', iv: combined.slice(0, 12) },
      wrappingKey,
      combined.slice(12),
    );
    return new Uint8Array(plaintext);
  }

  async derivePasswordWrapKey(password: string, saltB64: string): Promise<CryptoKey> {
    const base = await this._importPbkdf2Material(password);
    return crypto.subtle.deriveKey(
      { name: 'PBKDF2', hash: 'SHA-512', salt: this.fromBase64(saltB64), iterations: 600_000 },
      base,
      { name: 'AES-GCM', length: 256 },
      false,
      ['encrypt', 'decrypt'],
    );
  }

  async deriveRecoveryWrapKey(phrase: string, saltB64: string): Promise<CryptoKey> {
    const base = await this._importPbkdf2Material(phrase);
    return crypto.subtle.deriveKey(
      { name: 'PBKDF2', hash: 'SHA-512', salt: this.fromBase64(saltB64), iterations: 600_000 },
      base,
      { name: 'AES-GCM', length: 256 },
      false,
      ['encrypt', 'decrypt'],
    );
  }

  // Deterministic verifier the server can compare without knowing the phrase or master key
  async deriveRecoveryVerifier(phrase: string, saltB64: string): Promise<string> {
    const base = await this._importPbkdf2Material(phrase);
    const bits = await crypto.subtle.deriveBits(
      { name: 'PBKDF2', hash: 'SHA-256', salt: this.fromBase64(saltB64), iterations: 200_000 },
      base,
      256,
    );
    return this.toBase64(bits);
  }

  // Data encryption: base64( IV[12] || AES-GCM-ciphertext+tag )

  async encrypt(plaintext: string, key: CryptoKey): Promise<string> {
    const iv = crypto.getRandomValues(new Uint8Array(12));
    const ciphertext = await crypto.subtle.encrypt(
      { name: 'AES-GCM', iv },
      key,
      new TextEncoder().encode(plaintext),
    );
    const combined = new Uint8Array(12 + ciphertext.byteLength);
    combined.set(iv);
    combined.set(new Uint8Array(ciphertext), 12);
    return this.toBase64(combined);
  }

  async decrypt(ciphertextB64: string, key: CryptoKey): Promise<string> {
    const combined = this.fromBase64(ciphertextB64);
    const plaintext = await crypto.subtle.decrypt(
      { name: 'AES-GCM', iv: combined.slice(0, 12) },
      key,
      combined.slice(12),
    );
    return new TextDecoder().decode(plaintext);
  }

  generateRecoveryPhrase(): string {
    const bytes = crypto.getRandomValues(new Uint8Array(12));
    const hex = Array.from(bytes, b => b.toString(16).padStart(2, '0').toUpperCase());
    const groups: string[] = [];
    for (let i = 0; i < hex.length; i += 2) {
      groups.push(hex[i] + hex[i + 1]);
    }
    return groups.join('-');
  }

  toBase64(data: ArrayBuffer | Uint8Array): string {
    const bytes = data instanceof Uint8Array ? data : new Uint8Array(data);
    var binary = '';
    const chunkSize = 8192;
    for (let i = 0; i < bytes.length; i += chunkSize) {
      const chunk = bytes.subarray(i, i + chunkSize);
      binary += String.fromCharCode(...chunk);
    }
    return btoa(binary);
  }

  fromBase64(b64: string): Uint8Array<ArrayBuffer> {
    return new Uint8Array(Uint8Array.from(atob(b64), c => c.charCodeAt(0)));
  }

  private async _importPbkdf2Material(material: string): Promise<CryptoKey> {
    return crypto.subtle.importKey(
      'raw',
      new TextEncoder().encode(material),
      'PBKDF2',
      false,
      ['deriveKey', 'deriveBits'],
    );
  }
}
