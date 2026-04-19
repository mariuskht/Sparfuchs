import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatRippleModule } from '@angular/material/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { MatIconModule } from '@angular/material/icon';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatRippleModule, MatTooltipModule, MatIconModule
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  readonly navItems: NavItem[] = [
    { label: 'Dashboard',    icon: 'dashboard', route: '/dashboard'    },
    { label: 'Konten',     icon: 'account_balance_wallet', route: '/accounts'     },
    { label: 'Transaktionen', icon: 'shopping_bag_speed', route: '/transactions' },
    { label: 'Kategorien',   icon: 'label', route: '/categories'   },
    { label: 'Profil',      icon: 'person', route: '/profile'      },
  ];

  constructor(public auth: AuthService) {}
}
