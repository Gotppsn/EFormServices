// EFormServices.Infrastructure/Extensions/MockQueryExtensions.cs
// Got code 30/05/2025
using System.Linq.Expressions;

namespace EFormServices.Infrastructure.Extensions;

public static class MockQueryExtensions
{
    public static Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.FirstOrDefault());
    }

    public static Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.FirstOrDefault(predicate));
    }

    public static Task<int> CountAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.Count());
    }

    public static Task<int> CountAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.Count(predicate));
    }

    public static Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.ToList());
    }

    public static Task<T[]> ToArrayAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.ToArray());
    }

    public static Task<bool> AnyAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.Any());
    }

    public static Task<bool> AnyAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.Any(predicate));
    }

    public static Task<T> SingleAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.Single());
    }

    public static Task<T> SingleAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.Single(predicate));
    }

    public static Task<T?> SingleOrDefaultAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.SingleOrDefault());
    }

    public static Task<T?> SingleOrDefaultAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(queryable.SingleOrDefault(predicate));
    }
}