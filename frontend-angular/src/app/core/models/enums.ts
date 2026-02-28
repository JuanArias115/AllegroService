export const UNIT_STATUS_OPTIONS = [
  { value: 1, label: 'Available' },
  { value: 2, label: 'Occupied' },
  { value: 3, label: 'Dirty' },
  { value: 4, label: 'Maintenance' }
];

export const RESERVATION_STATUS_OPTIONS = [
  { value: 1, label: 'Pending' },
  { value: 2, label: 'Confirmed' },
  { value: 3, label: 'CheckedIn' },
  { value: 4, label: 'CheckedOut' },
  { value: 5, label: 'Cancelled' }
];

export const LOCATION_TYPE_OPTIONS = [
  { value: 1, label: 'Warehouse' },
  { value: 2, label: 'Kitchen' },
  { value: 3, label: 'Bar' },
  { value: 4, label: 'UnitMinibar' }
];

export const CHARGE_SOURCE_OPTIONS = [
  { value: 1, label: 'Room' },
  { value: 2, label: 'Minibar' },
  { value: 3, label: 'Restaurant' },
  { value: 4, label: 'Extra' }
];

export const PAYMENT_METHOD_OPTIONS = [
  { value: 1, label: 'Cash' },
  { value: 2, label: 'Card' },
  { value: 3, label: 'Transfer' },
  { value: 4, label: 'Online' }
];

export const USER_TENANT_ROLE_OPTIONS = [
  { value: 1, label: 'Admin' },
  { value: 2, label: 'Reception' },
  { value: 3, label: 'Restaurant' },
  { value: 4, label: 'Inventory' }
];

export const USER_TENANT_STATUS_OPTIONS = [
  { value: 1, label: 'Pending' },
  { value: 2, label: 'Active' },
  { value: 3, label: 'Disabled' }
];

export function labelOf(options: { value: number; label: string }[], value: number): string {
  return options.find((item) => item.value === value)?.label ?? String(value);
}
