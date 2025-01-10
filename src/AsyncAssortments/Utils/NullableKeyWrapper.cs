namespace AsyncAssortments;

internal readonly struct NullableKeyWrapper<T> : IEquatable<NullableKeyWrapper<T>>, IEquatable<T> {
    public readonly T? Value;

    public NullableKeyWrapper(T? value) {
        this.Value = value;
    }

    public NullableKeyWrapper() {
        this.Value = default!;
    }

    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(this.Value!);
    
    public override bool Equals(object? obj) {
        if (obj is NullableKeyWrapper<T> wrapper) {
            return this.Equals(wrapper);
        }
        else if (obj is T value) {
            return this.Equals(value);
        }
        else {
            return false;
        }
    }

    public bool Equals(NullableKeyWrapper<T> other) => EqualityComparer<T>.Default.Equals(this.Value!, other.Value!);
    
    public bool Equals(T? other) => EqualityComparer<T>.Default.Equals(this.Value!, other!);
    
    public static implicit operator NullableKeyWrapper<T>(T? value) => new NullableKeyWrapper<T>(value);

    public static implicit operator T?(NullableKeyWrapper<T> value) => value.Value;
}