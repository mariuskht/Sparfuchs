import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { AccountService } from '../../core/services/account.service';
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

  constructor(private fb: FormBuilder, private accountService: AccountService) {
    this.form = this.fb.group({
      name:    ['', Validators.required],
      balance: [0, Validators.required],
      type:    [AccountType.Checking, Validators.required],
    });
  }

  async ngOnInit() {
    this.accounts.set(await this.accountService.getAll());
    this.loading.set(false);
  }

  openCreate() {
    this.form.reset({ balance: 0, type: AccountType.Checking });
    this.editingId.set(null);
    this.showForm.set(true);
  }

  openEdit(account: Account) {
    this.form.setValue({ name: account.name, balance: account.balance, type: account.type });
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
      this.accounts.set(await this.accountService.getAll());
      this.showForm.set(false);
    } catch { this.error = 'Failed to save account.'; }
    finally   { this.saving = false; }
  }

  async onDelete(id: string) {
    if (!confirm('Delete this account and all its transactions?')) return;
    await this.accountService.delete(id);
    this.accounts.set(await this.accountService.getAll());
  }

  typeInfo(type: AccountType) {
    return this.accountTypes.find(t => t.value === type) ?? this.accountTypes[0];
  }
}
