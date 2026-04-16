export interface RegisterRequest {
  /** Plaintext email — sent over HTTPS, HMAC computed server-side with secret key. */
  email: string;
  password: string;
  encryptedUsername: string;
  encryptedEmail: string;
  encryptionSalt: string;
  passwordWrappedKey: string;
  recoverySalt: string;
  recoveryWrappedKey: string;
  recoveryVerifier: string;
}

export interface LoginRequest {
  /** Plaintext email — sent over HTTPS, HMAC computed server-side with secret key. */
  email: string;
  password: string;
}

export interface LoginResponse {
  encryptionSalt: string;
  passwordWrappedKey: string;
  encryptedUsername: string;
  encryptedEmail: string;
}

export interface RecoverVerifyResponse extends LoginResponse {
  recoverySalt: string;
  recoveryWrappedKey: string;
}

export interface CurrentUser {
  username: string;
  email: string;
}
