using System;
using System.Linq;
using System.Threading;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Collections.Generic;

public class CountableSubject<T> : ISubject<T>, IGenericSubject
{
    private readonly ISubject<T> _baseSubject;

    private List<IDisposable> _disposables = new List<IDisposable>();

    private int _counter;

    public int Count
    {
        get
        {
            return _counter;
        }
    }

    public CountableSubject() : this(new Subject<T>()) { }

    public CountableSubject(ISubject<T> baseSubject)
    {
        _baseSubject = baseSubject;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        Interlocked.Increment(ref counter);

        IDisposable subscriptionDisposable = _baseSubject.Subscribe(observer);

        CompositeDisposable compositeDisposable = new CompositeDisposable()
        {
            subscriptionDisposable,
            Disposable.Create(() => Interlocked.Decrement(ref counter)),
            Disposable.Create(() => _disposables.Remove(subscriptionDisposable))
        };

        _disposables.Add(compositeDisposable);

        return compositeDisposable;
    }

    public void OnNext(T value)
    {
        _baseSubject.OnNext(value);
    }

    public void OnNext(object value)
    {
        _baseSubject.OnNext((T) value);
    }

    public void OnCompleted()
    {
        _baseSubject.OnCompleted();
    }

    public void OnError(Exception error)
    {
        _baseSubject.OnError(error);
    }

    public void DisposeAll()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
