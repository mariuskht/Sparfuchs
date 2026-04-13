import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../core/models/category.model';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
  ],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
})
export class CategoriesComponent implements OnInit {
  categories = signal<Category[]>([]);
  loading = signal(true);
  showForm = signal(false);
  form: FormGroup;
  saving = false;
  error: string | null = null;

  constructor(private fb: FormBuilder, private categoryService: CategoryService) {
    this.form = this.fb.group({
      name:        ['', Validators.required],
      color:       ['#6366f1'],
      description: [''],
    });
  }

  async ngOnInit() {
    this.categories.set(await this.categoryService.getAll());
    this.loading.set(false);
  }

  get userCategories()    { return this.categories().filter(c => !c.isDefault); }
  get defaultCategories() { return this.categories().filter(c =>  c.isDefault); }

  async onSubmit() {
    if (this.form.invalid) return;
    this.saving = true; this.error = null;
    try {
      const { name, color, description } = this.form.value;
      await this.categoryService.create(name, color || undefined, description || undefined);
      this.categories.set(await this.categoryService.getAll());
      this.showForm.set(false);
      this.form.reset({ color: '#6366f1' });
    } catch { this.error = 'Failed to save category.'; }
    finally   { this.saving = false; }
  }

  async onDelete(id: string) {
    if (!confirm('Delete this category?')) return;
    await this.categoryService.delete(id);
    this.categories.set(await this.categoryService.getAll());
  }
}
