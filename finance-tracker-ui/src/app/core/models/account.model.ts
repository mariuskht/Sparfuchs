export enum AccountType {
  Checking = 0,
  Saving = 1,
  Portfolio = 2,
  CreditCard = 3,
}

/** Raw response from the API — all sensitive fields are encrypted. */
export interface AccountResponse {
  id: string;
  encryptedName: string;
  encryptedBalance: string;
  type: AccountType;
  createdAt: string;
  updatedAt: string;
}

/** Decrypted account for use in the UI. */
export interface Account {
  id: string;
  name: string;
  /** Opening/starting balance stored on the server (never auto-updated by transactions). */
  initialBalance: number;
  /** Current balance = initialBalance + Σ transactions. Equals initialBalance until withTransactionTotals() is called. */
  balance: number;
  type: AccountType;
  createdAt: Date;
  updatedAt: Date;
}
