export default class ObsoleteOption<T> {
  private constructor(private readonly value: T | null) {}

  static some<T>(value: T): ObsoleteOption<T> {
    return new ObsoleteOption(value);
  }

  static none<T>(): ObsoleteOption<T> {
    return new ObsoleteOption<T>(null);
  }

  isSome(): boolean {
    return this.value !== null;
  }

  isNone(): boolean {
    return this.value === null;
  }

  unwrap(): T {
    if (this.value === null) throw new Error("Cannot unwrap None");
    return this.value;
  }

  unwrapOr(defaultValue: T): T {
    return this.value ?? defaultValue;
  }

  map<U>(fn: (value: T) => U): ObsoleteOption<U> {
    return this.value === null
      ? ObsoleteOption.none()
      : ObsoleteOption.some(fn(this.value));
  }
}
