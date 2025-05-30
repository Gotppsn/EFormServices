// EFormServices.Infrastructure/Data/MockDbSet.cs
// Got code 30/05/2025
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace EFormServices.Infrastructure.Data;

public class MockDbSet<T> : DbSet<T>, IQueryable<T>, IEnumerable<T> where T : class
{
    private readonly ObservableCollection<T> _data;
    private readonly IQueryable<T> _query;

    public MockDbSet()
    {
        _data = new ObservableCollection<T>();
        _query = _data.AsQueryable();
    }

    public MockDbSet(IEnumerable<T> initialData)
    {
        _data = new ObservableCollection<T>(initialData);
        _query = _data.AsQueryable();
    }

    public override T Add(T entity)
    {
        _data.Add(entity);
        return entity;
    }

    public override T Remove(T entity)
    {
        _data.Remove(entity);
        return entity;
    }

    public override T Update(T entity)
    {
        var existing = _data.FirstOrDefault(e => e.Equals(entity));
        if (existing != null)
        {
            var index = _data.IndexOf(existing);
            _data[index] = entity;
        }
        return entity;
    }

    public override T? Find(params object[] keyValues)
    {
        return _data.FirstOrDefault();
    }

    public override ValueTask<T?> FindAsync(params object[] keyValues)
    {
        return ValueTask.FromResult(Find(keyValues));
    }

    public override ValueTask<T?> FindAsync(object[] keyValues, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Find(keyValues));
    }

    public Type ElementType => _query.ElementType;
    public Expression Expression => _query.Expression;
    public IQueryProvider Provider => _query.Provider;

    public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

    public override IAsyncEnumerable<T> AsAsyncEnumerable()
    {
        return new AsyncEnumerable<T>(_data);
    }

    private class AsyncEnumerable<TEntity> : IAsyncEnumerable<TEntity>
    {
        private readonly IEnumerable<TEntity> _enumerable;

        public AsyncEnumerable(IEnumerable<TEntity> enumerable)
        {
            _enumerable = enumerable;
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumerator<TEntity>(_enumerable.GetEnumerator());
        }
    }

    private class AsyncEnumerator<TEntity> : IAsyncEnumerator<TEntity>
    {
        private readonly IEnumerator<TEntity> _enumerator;

        public AsyncEnumerator(IEnumerator<TEntity> enumerator)
        {
            _enumerator = enumerator;
        }

        public TEntity Current => _enumerator.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_enumerator.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}