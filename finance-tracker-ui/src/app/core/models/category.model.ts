/** Raw response from the API. */
export interface CategoryResponse {
  id: string;
  isDefault: boolean;
  // Plaintext — only set for default (system) categories
  name: string | null;
  color: string | null;
  // Encrypted — only set for user-created categories
  encryptedName: string | null;
  encryptedColor: string | null;
  encryptedDescription: string | null;
}

/** Decrypted category for use in the UI. */
export interface Category {
  id: string;
  isDefault: boolean;
  name: string;
  color: string | null;
  description: string | null;
}
