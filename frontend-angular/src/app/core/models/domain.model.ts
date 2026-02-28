export interface Unit {
  id: string;
  name: string;
  type: string;
  capacity: number;
  status: number;
}

export interface Guest {
  id: string;
  fullName: string;
  documentId?: string | null;
  phone: string;
  email: string;
}

export interface Category {
  id: string;
  name: string;
}

export interface Product {
  id: string;
  sku: string;
  name: string;
  categoryId: string;
  categoryName?: string;
  unit: string;
  salePrice: number;
  costPrice?: number | null;
  isActive: boolean;
  trackStock: boolean;
}

export interface Location {
  id: string;
  name: string;
  type: number;
  unitId?: string | null;
}

export interface Reservation {
  id: string;
  code: string;
  guestId: string;
  guestName?: string;
  unitId?: string | null;
  unitName?: string | null;
  checkInDate: string;
  checkOutDate: string;
  status: number;
  totalEstimated: number;
}

export interface Stay {
  id: string;
  reservationId?: string | null;
  unitId: string;
  checkInAt: string;
  checkOutAt?: string | null;
  status: number;
  openFolioId?: string | null;
}

export interface ChargeItem {
  id: string;
  productId?: string | null;
  qty: number;
  unitPrice: number;
  total: number;
}

export interface Charge {
  id: string;
  source: number;
  description: string;
  qty: number;
  unitPrice: number;
  total: number;
  items: ChargeItem[];
  createdAt: string;
}

export interface Payment {
  id: string;
  amount: number;
  method: number;
  status: number;
  paidAt?: string | null;
  reference?: string | null;
  createdAt: string;
}

export interface FolioDetail {
  id: string;
  stayId: string;
  status: number;
  openedAt: string;
  closedAt?: string | null;
  chargesTotal: number;
  paymentsTotal: number;
  balance: number;
  charges: Charge[];
  payments: Payment[];
}

export interface CheckInResponse {
  stayId: string;
  folioId: string;
}

export interface CheckOutResponse {
  stayId: string;
  folioId: string;
  balance: number;
  checkOutAt: string;
}
