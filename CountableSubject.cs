using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Collections.Generic;
using System.Threading;

public class CountableSubject<T> : ISubject<T>, IGenericSubject
{
    private readonly ISubject<T> baseSubject;

    private List<IDisposable> disposables = new List<IDisposable>();

    private int counter;

    public int Count
    {
        get
        {
            return counter;
        }
    }

    public CountableSubject() : this(new Subject<T>()) { }

    public CountableSubject(ISubject<T> baseSubject)
    {
        this.baseSubject = baseSubject;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        Interlocked.Increment(ref counter);

        IDisposable subscriptionDisposable = baseSubject.Subscribe(observer);

        CompositeDisposable compositeDisposable = new CompositeDisposable()
        {
            subscriptionDisposable,
            Disposable.Create(() => Interlocked.Decrement(ref counter)),
            Disposable.Create(() => disposables.Remove(subscriptionDisposable))
        };

        disposables.Add(compositeDisposable);

        return compositeDisposable;
    }

    public void OnNext(T value)
    {
        baseSubject.OnNext(value);
    }

    public void OnNext(object value)
    {
        baseSubject.OnNext((T) value);
    }

    public void OnCompleted()
    {
        baseSubject.OnCompleted();
    }

    public void OnError(Exception error)
    {
        baseSubject.OnError(error);
    }

    public void DisposeAll()
    {
        foreach (IDisposable disposable in disposables.ToList())
        {
            disposable.Dispose();
        }
    }
}
