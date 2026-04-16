import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatCardModule, MatDividerModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  recoveryPhrase: string | null = null;
  regenerating = false;
  copied = false;

  constructor(public auth: AuthService) {}

  ngOnInit(): void {
    // Show the phrase once if it was just generated (registration or regeneration)
    this.recoveryPhrase = this.auth.consumeRecoveryPhrase();
  }

  async regenerate(): Promise<void> {
    if (this.regenerating) return;
    this.regenerating = true;
    this.copied = false;

    try {
      this.recoveryPhrase = await this.auth.regenerateRecoveryKey();
    } finally {
      this.regenerating = false;
    }
  }

  copyPhrase(): void {
    if (!this.recoveryPhrase) return;
    navigator.clipboard.writeText(this.recoveryPhrase).then(() => {
      this.copied = true;
      setTimeout(() => (this.copied = false), 3000);
    });
  }
}
