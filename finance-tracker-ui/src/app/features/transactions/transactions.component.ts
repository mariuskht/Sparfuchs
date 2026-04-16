import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { TransactionService } from '../../core/services/transaction.service';
import { AccountService } from '../../core/services/account.service';
import { CategoryService } from '../../core/services/category.service';
import { Transaction } from '../../core/models/transaction.model';
import { Account } from '../../core/models/account.model';
import { Category } from '../../core/models/category.model';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule,
  ],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.scss',
})
export class TransactionsComponent implements OnInit {
  transactions = signal<Transaction[]>([]);
  accounts = signal<Account[]>([]);
  categories = signal<Category[]>([]);
  loading = signal(true);
  showForm = signal(false);
  editingId = signal<string | null>(null);
  form: FormGroup;
  saving = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private transactionService: TransactionService,
    private accountService: AccountService,
    private categoryService: CategoryService,
  ) {
    this.form = this.fb.group({
      accountId:       ['', Validators.required],
      categoryId:      ['', Validators.required],
      amount:          [null, Validators.required],
      note:            [''],
      transactionDate: [new Date().toISOString().substring(0, 10), Validators.required],
    });
  }

  async ngOnInit() {
    const [t, a, c] = await Promise.all([
      this.transactionService.getAll(),
      this.accountService.getAll(),
      this.categoryService.getAll(),
    ]);
    this.transactions.set(t);
    this.accounts.set(this.accountService.withTransactionTotals(a, t));
    this.categories.set(c);
    this.loading.set(false);
  }

  openCreate() {
    this.form.reset({ transactionDate: new Date().toISOString().substring(0, 10) });
    this.editingId.set(null);
    this.showForm.set(true);
  }

  openEdit(tx: Transaction) {
    this.form.setValue({
      accountId:       tx.accountId,
      categoryId:      tx.categoryId,
      amount:          tx.amount,
      note:            tx.note ?? '',
      transactionDate: tx.transactionDate.toISOString().substring(0, 10),
    });
    this.editingId.set(tx.id);
    this.showForm.set(true);
  }

  closeForm() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.error = null;
  }

  accountName(id: string) { return this.accounts().find(a => a.id === id)?.name ?? '—'; }
  categoryName(id: string) { return this.categories().find(c => c.id === id)?.name ?? '—'; }

  private async reload() {
    const [t, a] = await Promise.all([
      this.transactionService.getAll(),
      this.accountService.getAll(),
    ]);
    this.transactions.set(t);
    this.accounts.set(this.accountService.withTransactionTotals(a, t));
  }

  async onSubmit() {
    if (this.form.invalid) return;
    this.saving = true; this.error = null;
    try {
      const { accountId, categoryId, amount, note, transactionDate } = this.form.value;
      const date = new Date(transactionDate);
      const id = this.editingId();
      if (id) {
        await this.transactionService.update(
          id, accountId, categoryId, amount, date, note || undefined,
        );
      } else {
        await this.transactionService.create(accountId, categoryId, amount, date, note || undefined);
      }
      await this.reload();
      this.closeForm();
    } catch { this.error = 'Failed to save transaction.'; }
    finally   { this.saving = false; }
  }

  async onDelete(tx: Transaction) {
    if (!confirm('Delete this transaction?')) return;
    await this.transactionService.delete(tx.id);
    await this.reload();
  }
}
