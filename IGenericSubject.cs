public interface IGenericSubject
{
    int Count { get; }

    void OnNext(object value);

    void DisposeAll();
}
