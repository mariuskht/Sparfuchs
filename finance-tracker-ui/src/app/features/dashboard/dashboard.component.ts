import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/services/auth.service';
import { AccountService } from '../../core/services/account.service';
import { TransactionService } from '../../core/services/transaction.service';
import { CategoryService } from '../../core/services/category.service';
import { Account, AccountType } from '../../core/models/account.model';
import { Transaction } from '../../core/models/transaction.model';
import { Category } from '../../core/models/category.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  accounts = signal<Account[]>([]);
  transactions = signal<Transaction[]>([]);
  categories = signal<Category[]>([]);
  loading = signal(true);

  constructor(
    public auth: AuthService,
    private accountService: AccountService,
    private transactionService: TransactionService,
    private categoryService: CategoryService,
  ) {}

  async ngOnInit() {
    const [accounts, transactions, categories] = await Promise.all([
      this.accountService.getAll(),
      this.transactionService.getAll(),
      this.categoryService.getAll(),
    ]);
    this.accounts.set(this.accountService.withTransactionTotals(accounts, transactions));
    this.transactions.set(transactions);
    this.categories.set(categories);
    this.loading.set(false);
  }

  get totalBalance() { return this.accounts().reduce((s, a) => s + a.balance, 0); }
  get totalAccounts() { return this.accounts().length; }
  accountName(id: string) { return this.accounts().find(a => a.id === id)?.name ?? '—'; }
  categoryName(id: string) { return this.categories().find(c => c.id === id)?.name ?? '—'; }

  readonly monthlySummary = computed(() => {
    const now = new Date();
    const month = now.getMonth();
    const year  = now.getFullYear();
    let expenses = 0, income = 0;
    for (const t of this.transactions()) {
      const d = new Date(t.transactionDate);
      if (d.getMonth() !== month || d.getFullYear() !== year) continue;
      if (t.amount < 0) expenses += t.amount;
      else              income   += t.amount;
    }
    return { expenses: Math.abs(expenses), income };
  });

  get monthlyExpenses() { return this.monthlySummary().expenses; }
  get monthlyIncome()   { return this.monthlySummary().income;   }

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
