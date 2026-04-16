import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CryptoService } from './crypto.service';
import { AuthService } from './auth.service';
import { Transaction, TransactionResponse } from '../models/transaction.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TransactionService {

  private readonly apiUrl = `${environment.apiUrl}/transactions`;

  constructor(
    private http: HttpClient,
    private crypto: CryptoService,
    private auth: AuthService,
  ) {}

  async getAll(accountId?: string): Promise<Transaction[]> {
    const url = accountId ? `${this.apiUrl}?accountId=${accountId}` : this.apiUrl;
    const responses = await firstValueFrom(this.http.get<TransactionResponse[]>(url));
    return Promise.all(responses.map(r => this.decrypt(r)));
  }

  async create(
    accountId: string,
    categoryId: string,
    amount: number,
    transactionDate: Date,
    note?: string,
  ): Promise<Transaction> {
    const key = this.auth.encryptionKey;
    const [encryptedAmount, encryptedNote] = await Promise.all([
      this.crypto.encrypt(amount.toString(), key),
      note ? this.crypto.encrypt(note, key) : Promise.resolve(null),
    ]);

    const response = await firstValueFrom(
      this.http.post<TransactionResponse>(this.apiUrl, {
        accountId,
        categoryId,
        encryptedAmount,
        encryptedNote,
        transactionDate: transactionDate.toISOString(),
      }),
    );

    return this.decrypt(response);
  }

  async update(
    id: string,
    accountId: string,
    categoryId: string,
    amount: number,
    transactionDate: Date,
    note: string | undefined,
  ): Promise<void> {
    const key = this.auth.encryptionKey;
    const [encryptedAmount, encryptedNote] = await Promise.all([
      this.crypto.encrypt(amount.toString(), key),
      note ? this.crypto.encrypt(note, key) : Promise.resolve(null),
    ]);

    await firstValueFrom(
      this.http.put(`${this.apiUrl}/${id}`, {
        accountId,
        categoryId,
        encryptedAmount,
        encryptedNote,
        transactionDate: transactionDate.toISOString(),
      }),
    );
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
  }

  private async decrypt(r: TransactionResponse): Promise<Transaction> {
    const key = this.auth.encryptionKey;
    const [amountStr, note] = await Promise.all([
      this.crypto.decrypt(r.encryptedAmount, key),
      r.encryptedNote ? this.crypto.decrypt(r.encryptedNote, key) : Promise.resolve(null),
    ]);
    return {
      id: r.id,
      accountId: r.accountId,
      categoryId: r.categoryId,
      amount: parseFloat(amountStr),
      note,
      transactionDate: new Date(r.transactionDate),
    };
  }
}
