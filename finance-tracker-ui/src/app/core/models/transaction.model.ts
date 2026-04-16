/** Raw response from the API — all sensitive fields are encrypted. */
export interface TransactionResponse {
  id: string;
  accountId: string;
  categoryId: string;
  encryptedAmount: string;
  encryptedNote: string | null;
  transactionDate: string;
}

/** Decrypted transaction for use in the UI. */
export interface Transaction {
  id: string;
  accountId: string;
  categoryId: string;
  amount: number;
  note: string | null;
  transactionDate: Date;
}
