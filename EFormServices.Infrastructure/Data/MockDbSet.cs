// EFormServices.Infrastructure/Data/MockDbSet.cs
// Got code 30/05/2025
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections;
using System.Linq.Expressions;

namespace EFormServices.Infrastructure.Data;

public class MockDbSet<T> : DbSet<T>, IQueryable<T>, IEnumerable<T> where T : class
{
    private readonly List<T> _data;
    private readonly IQueryable<T> _query;

    public override Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityType => throw new NotImplementedException();

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

    public override EntityEntry<T> Add(T entity)
    {
        _data.Add(entity);
        return null!;
    }

    public override EntityEntry<T> Remove(T entity)
    {
        _data.Remove(entity);
        return null!;
    }

    public override EntityEntry<T> Update(T entity)
    {
        var existing = _data.FirstOrDefault(e => e.Equals(entity));
        if (existing != null)
        {
            var index = _data.IndexOf(existing);
            _data[index] = entity;
        }
        return null!;
    }

    public override T? Find(params object?[]? keyValues)
    {
        return _data.FirstOrDefault();
    }

    public override EntityEntry<T> Entry(T entity)
    {
        return null!;
    }

    public override void AddRange(params T[] entities)
    {
        _data.AddRange(entities);
    }

    public override void AddRange(IEnumerable<T> entities)
    {
        _data.AddRange(entities);
    }

    public override void RemoveRange(params T[] entities)
    {
        foreach (var entity in entities)
            _data.Remove(entity);
    }

    public override void RemoveRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
            _data.Remove(entity);
    }

    public override EntityEntry<T> Attach(T entity)
    {
        if (!_data.Contains(entity))
            _data.Add(entity);
        return null!;
    }

    // IQueryable<T> implementation
    Type IQueryable.ElementType => typeof(T);
    Expression IQueryable.Expression => _query.Expression;
    IQueryProvider IQueryable.Provider => _query.Provider;

    // IEnumerable<T> implementation
    public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
}