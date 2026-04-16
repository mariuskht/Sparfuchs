import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { AccountService } from '../../core/services/account.service';
import { TransactionService } from '../../core/services/transaction.service';
import { Account, AccountType } from '../../core/models/account.model';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule,
  ],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss',
})
export class AccountsComponent implements OnInit {
  accounts = signal<Account[]>([]);
  loading = signal(true);
  showForm = signal(false);
  editingId = signal<string | null>(null);
  form: FormGroup;
  saving = false;
  error: string | null = null;

  readonly accountTypes = [
    { value: AccountType.Checking,   label: 'Checking',     icon: '🏦' },
    { value: AccountType.Saving,     label: 'Savings',      icon: '🏧' },
    { value: AccountType.Portfolio,  label: 'Portfolio',    icon: '📈' },
    { value: AccountType.CreditCard, label: 'Credit Card',  icon: '💳' },
  ];

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private transactionService: TransactionService,
  ) {
    this.form = this.fb.group({
      name:    ['', Validators.required],
      balance: [0, Validators.required],
      type:    [AccountType.Checking, Validators.required],
    });
  }

  async ngOnInit() {
    await this.loadAccounts();
    this.loading.set(false);
  }

  private async loadAccounts() {
    const [accounts, transactions] = await Promise.all([
      this.accountService.getAll(),
      this.transactionService.getAll(),
    ]);
    this.accounts.set(this.accountService.withTransactionTotals(accounts, transactions));
  }

  openCreate() {
    this.form.reset({ balance: 0, type: AccountType.Checking });
    this.editingId.set(null);
    this.showForm.set(true);
  }

  openEdit(account: Account) {
    this.form.setValue({ name: account.name, balance: account.initialBalance, type: account.type });
    this.editingId.set(account.id);
    this.showForm.set(true);
  }

  async onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = null;
    try {
      const { name, balance, type } = this.form.value;
      const id = this.editingId();
      if (id) await this.accountService.update(id, name, balance, type);
      else     await this.accountService.create(name, balance, type);
      await this.loadAccounts();
      this.showForm.set(false);
    } catch { this.error = 'Failed to save account.'; }
    finally   { this.saving = false; }
  }

  async onDelete(id: string) {
    if (!confirm('Delete this account and all its transactions?')) return;
    await this.accountService.delete(id);
    await this.loadAccounts();
  }

  typeInfo(type: AccountType) {
    return this.accountTypes.find(t => t.value === type) ?? this.accountTypes[0];
  }
}
