import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LocationService } from '../../services/location.service';

interface PurchaseBillItem {
  id: string;
  item: string;
  batch: string;
  standardCost: number;
  standardPrice: number;
  quantity: number;
  discount: number;
  totalCost: number;
  totalSelling: number;
}

@Component({
  selector: 'app-purchase-bill-add',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './purchase-bill-add.component.html',
  styleUrl: './purchase-bill-add.component.css'
})
export class PurchaseBillAddComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly locationService = inject(LocationService);
  private readonly router = inject(Router);

  readonly items = ['Mango', 'Apple', 'Banana', 'Orange', 'Grapes', 'Kiwi', 'Strawberry'];
  locations: Array<{ location_Code: string; location_Name: string }> = [];
  billItems: PurchaseBillItem[] = [];
  filteredItems: string[] = [];
  errorMessage = '';
  successMessage = '';
  itemIdCounter = 0;

  readonly form = this.formBuilder.group({
    item: ['', [Validators.required]],
    batch: ['', [Validators.required]],
    standardCost: [null as number | null, [Validators.required, Validators.min(0)]],
    standardPrice: [null as number | null, [Validators.required, Validators.min(0)]],
    quantity: [null as number | null, [Validators.required, Validators.min(1)]],
    discount: [0, [Validators.min(0), Validators.max(100)]]
  });

  ngOnInit(): void {
    this.loadLocations();
  }

  loadLocations(): void {
    this.locationService.getLocations().subscribe({
      next: (response) => {
        this.locations = response.locations;
      },
      error: () => {
        this.errorMessage = 'Failed to load batch locations.';
      }
    });
  }

  onItemInput(event: Event): void {
    const input = (event.target as HTMLInputElement).value.toLowerCase();
    if (!input) {
      this.filteredItems = [];
      return;
    }
    this.filteredItems = this.items.filter((item) => item.toLowerCase().includes(input));
  }

  selectItem(item: string): void {
    this.form.patchValue({ item });
    this.filteredItems = [];
  }

  get totalCostDisplay(): number {
    const standardCost = this.form.get('standardCost')?.value ?? 0;
    const quantity = this.form.get('quantity')?.value ?? 0;
    const discount = this.form.get('discount')?.value ?? 0;
    return standardCost * quantity * (1 - discount / 100);
  }

  get totalSellingDisplay(): number {
    const standardPrice = this.form.get('standardPrice')?.value ?? 0;
    const quantity = this.form.get('quantity')?.value ?? 0;
    return standardPrice * quantity;
  }

  addItem(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage = 'Please fill all required fields correctly.';
      return;
    }

    const value = this.form.getRawValue();
    const billItem: PurchaseBillItem = {
      id: String(++this.itemIdCounter),
      item: value.item ?? '',
      batch: value.batch ?? '',
      standardCost: value.standardCost ?? 0,
      standardPrice: value.standardPrice ?? 0,
      quantity: value.quantity ?? 0,
      discount: value.discount ?? 0,
      totalCost: this.totalCostDisplay,
      totalSelling: this.totalSellingDisplay
    };

    this.billItems.push(billItem);
    this.form.reset({ discount: 0 });
    this.errorMessage = '';
    this.successMessage = '';
    this.filteredItems = [];
  }

  removeItem(id: string): void {
    this.billItems = this.billItems.filter((item) => item.id !== id);
  }

  get totalItemsCount(): number {
    return this.billItems.length;
  }

  get totalQuantitySum(): number {
    return this.billItems.reduce((sum, item) => sum + item.quantity, 0);
  }

  get grandTotalCost(): number {
    return this.billItems.reduce((sum, item) => sum + item.totalCost, 0);
  }

  get grandTotalSelling(): number {
    return this.billItems.reduce((sum, item) => sum + item.totalSelling, 0);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
