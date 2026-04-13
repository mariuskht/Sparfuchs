import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CryptoService } from './crypto.service';
import { AuthService } from './auth.service';
import { Category, CategoryResponse } from '../models/category.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CategoryService {

  private readonly apiUrl = `${environment.apiUrl}/categories`;

  constructor(
    private http: HttpClient,
    private crypto: CryptoService,
    private auth: AuthService,
  ) {}

  async getAll(): Promise<Category[]> {
    const responses = await firstValueFrom(this.http.get<CategoryResponse[]>(this.apiUrl));
    return Promise.all(responses.map(r => this.decrypt(r)));
  }

  async create(name: string, color?: string, description?: string): Promise<Category> {
    const key = this.auth.encryptionKey;
    const [encryptedName, encryptedColor, encryptedDescription] = await Promise.all([
      this.crypto.encrypt(name, key),
      color ? this.crypto.encrypt(color, key) : Promise.resolve(null),
      description ? this.crypto.encrypt(description, key) : Promise.resolve(null),
    ]);

    const response = await firstValueFrom(
      this.http.post<CategoryResponse>(this.apiUrl, { encryptedName, encryptedColor, encryptedDescription }),
    );
    return this.decrypt(response);
  }

  async update(id: string, name: string, color?: string, description?: string): Promise<void> {
    const key = this.auth.encryptionKey;
    const [encryptedName, encryptedColor, encryptedDescription] = await Promise.all([
      this.crypto.encrypt(name, key),
      color ? this.crypto.encrypt(color, key) : Promise.resolve(null),
      description ? this.crypto.encrypt(description, key) : Promise.resolve(null),
    ]);

    await firstValueFrom(
      this.http.put(`${this.apiUrl}/${id}`, { encryptedName, encryptedColor, encryptedDescription }),
    );
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
  }

  private async decrypt(r: CategoryResponse): Promise<Category> {
    if (r.isDefault) {
      // Default categories are not personal data — stored as plaintext
      return { id: r.id, isDefault: true, name: r.name!, color: r.color, description: null };
    }

    const key = this.auth.encryptionKey;
    const [name, color, description] = await Promise.all([
      this.crypto.decrypt(r.encryptedName!, key),
      r.encryptedColor ? this.crypto.decrypt(r.encryptedColor, key) : Promise.resolve(null),
      r.encryptedDescription ? this.crypto.decrypt(r.encryptedDescription, key) : Promise.resolve(null),
    ]);
    return { id: r.id, isDefault: false, name, color, description };
  }
}
