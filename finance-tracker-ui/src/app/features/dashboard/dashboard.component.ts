import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/services/auth.service';
import { AccountService } from '../../core/services/account.service';
import { TransactionService } from '../../core/services/transaction.service';
import { Account, AccountType } from '../../core/models/account.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  accounts = signal<Account[]>([]);
  loading = signal(true);

  constructor(
    public auth: AuthService,
    private accountService: AccountService,
    private transactionService: TransactionService,
  ) {}

  async ngOnInit() {
    const [accounts, transactions] = await Promise.all([
      this.accountService.getAll(),
      this.transactionService.getAll(),
    ]);
    this.accounts.set(this.accountService.withTransactionTotals(accounts, transactions));
    this.loading.set(false);
  }

  get totalBalance() { return this.accounts().reduce((s, a) => s + a.balance, 0); }
  get totalAccounts() { return this.accounts().length; }

  accountTypeIcon(type: AccountType): string {
    const map: Record<AccountType, string> = {
      [AccountType.Checking]:   'account_balance',
      [AccountType.Saving]:     'savings',
      [AccountType.Portfolio]:  'trending_up',
      [AccountType.CreditCard]: 'credit_card',
    };
    return map[type] ?? 'account_balance_wallet';
  }

  accountTypeLabel(type: AccountType): string {
    const map: Record<AccountType, string> = {
      [AccountType.Checking]:   'Checking',
      [AccountType.Saving]:     'Savings',
      [AccountType.Portfolio]:  'Portfolio',
      [AccountType.CreditCard]: 'Credit Card',
    };
    return map[type] ?? '';
  }
}
