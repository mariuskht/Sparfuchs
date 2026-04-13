export interface RegisterRequest {
  email: string;
  password: string;
  encryptedUsername: string;
  encryptedEmail: string;
  encryptionSalt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  encryptionSalt: string;
  encryptedUsername: string;
  encryptedEmail: string;
}

export interface CurrentUser {
  username: string;
  email: string;
}
