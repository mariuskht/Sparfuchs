import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CryptoService } from './crypto.service';
import { AuthService } from './auth.service';
import { Account, AccountResponse, AccountType } from '../models/account.model';
import { Transaction } from '../models/transaction.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AccountService {

  private readonly apiUrl = `${environment.apiUrl}/accounts`;

  constructor(
    private http: HttpClient,
    private crypto: CryptoService,
    private auth: AuthService,
  ) {}

  async getAll(): Promise<Account[]> {
    const responses = await firstValueFrom(this.http.get<AccountResponse[]>(this.apiUrl));
    return Promise.all(responses.map(r => this.decrypt(r)));
  }

  async getById(id: string): Promise<Account> {
    const response = await firstValueFrom(this.http.get<AccountResponse>(`${this.apiUrl}/${id}`));
    return this.decrypt(response);
  }

  async create(name: string, initialBalance: number, type: AccountType): Promise<Account> {
    const key = this.auth.encryptionKey;
    const [encryptedName, encryptedBalance] = await Promise.all([
      this.crypto.encrypt(name, key),
      this.crypto.encrypt(initialBalance.toString(), key),
    ]);
    const response = await firstValueFrom(
      this.http.post<AccountResponse>(this.apiUrl, { encryptedName, encryptedBalance, type }),
    );
    return this.decrypt(response);
  }

  async update(id: string, name: string, initialBalance: number, type: AccountType): Promise<void> {
    const key = this.auth.encryptionKey;
    const [encryptedName, encryptedBalance] = await Promise.all([
      this.crypto.encrypt(name, key),
      this.crypto.encrypt(initialBalance.toString(), key),
    ]);
    await firstValueFrom(
      this.http.put(`${this.apiUrl}/${id}`, { encryptedName, encryptedBalance, type }),
    );
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
  }

  // Computes live balances by adding transaction totals to each account's initialBalance.
  // The server stores initialBalance only — running balance is derived client-side.
  withTransactionTotals(accounts: Account[], transactions: Transaction[]): Account[] {
    const totals = new Map<string, number>(accounts.map(a => [a.id, a.initialBalance]));
    for (const t of transactions) {
      const current = totals.get(t.accountId);
      if (current !== undefined) totals.set(t.accountId, current + t.amount);
    }
    return accounts.map(a => ({ ...a, balance: totals.get(a.id) ?? a.initialBalance }));
  }

  private async decrypt(r: AccountResponse): Promise<Account> {
    const key = this.auth.encryptionKey;
    const [name, balanceStr] = await Promise.all([
      this.crypto.decrypt(r.encryptedName, key),
      this.crypto.decrypt(r.encryptedBalance, key),
    ]);
    const initialBalance = parseFloat(balanceStr);
    return {
      id: r.id,
      name,
      initialBalance,
      balance: initialBalance,
      type: r.type,
      createdAt: new Date(r.createdAt),
      updatedAt: new Date(r.updatedAt),
    };
  }
}
