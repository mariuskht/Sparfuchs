import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reauth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './reauth.component.html',
  styleUrl: './reauth.component.scss',
})
export class ReauthComponent {
  form: FormGroup;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
  ) {
    // If there's no cookie session at all, redirect straight to login
    if (!auth.hasCookieSession) {
      router.navigate(['/login']);
    }

    this.form = this.fb.group({
      password: ['', Validators.required],
    });
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = null;

    try {
      await this.auth.reauth(this.form.value.password);
    } catch {
      this.error = 'Incorrect password. Please try again.';
    } finally {
      this.loading = false;
    }
  }

  goToLogin(): void {
    this.auth.logout();
  }
}
