namespace AsyncAssortments;

internal class NullableKeyWrapperComparer<T> : IEqualityComparer<NullableKeyWrapper<T>> {
    private readonly IEqualityComparer<T> comparer;

    public NullableKeyWrapperComparer(IEqualityComparer<T> comparer) {
        this.comparer = comparer;
    }

    public bool Equals(NullableKeyWrapper<T> x, NullableKeyWrapper<T> y) => this.comparer.Equals(x.Value!, y.Value!);

    public int GetHashCode(NullableKeyWrapper<T> obj) => obj.Value == null ? 0 : this.comparer.GetHashCode(obj.Value);
}