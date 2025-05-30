// EFormServices.Infrastructure/Data/MockDbSet.cs
// Got code 30/05/2025
using System.Collections;
using System.Linq.Expressions;

namespace EFormServices.Infrastructure.Data;

public class MockDbSet<T> : IQueryable<T>, IEnumerable<T> where T : class
{
    private readonly List<T> _data;
    private readonly IQueryable<T> _query;

    public MockDbSet()
    {
        _data = new List<T>();
        _query = _data.AsQueryable();
    }

    public MockDbSet(IEnumerable<T> initialData)
    {
        _data = new List<T>(initialData);
        _query = _data.AsQueryable();
    }

    public void Add(T entity)
    {
        _data.Add(entity);
    }

    public void Remove(T entity)
    {
        _data.Remove(entity);
    }

    public void Update(T entity)
    {
        var existing = _data.FirstOrDefault(e => e.Equals(entity));
        if (existing != null)
        {
            var index = _data.IndexOf(existing);
            _data[index] = entity;
        }
    }

    public T? Find(params object?[]? keyValues)
    {
        return _data.FirstOrDefault();
    }

    public Type ElementType => _query.ElementType;
    public Expression Expression => _query.Expression;
    public IQueryProvider Provider => _query.Provider;

    public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
}