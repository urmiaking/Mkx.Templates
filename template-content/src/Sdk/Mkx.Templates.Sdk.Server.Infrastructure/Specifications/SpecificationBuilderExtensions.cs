using System.Linq.Expressions;
using System.Reflection;
using Ardalis.Specification;
using Mkx.Templates.Sdk.Server.Shared.Enums;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Specifications;

public static class SpecificationBuilderExtensions
{
    /// <summary>
    /// Applies dynamic sorting to the specification based on a string property label (e.g. "Name", "Asset.TypeId.Value").
    /// </summary>
    public static ISpecificationBuilder<TEntity> ApplySorting<TEntity>(
        this ISpecificationBuilder<TEntity> builder,
        string? sortLabel,
        SortDirection sortDirection,
        string defaultSortProperty = "Timestamp")
        where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(sortLabel))
        {
            return ApplyDefaultSortInternal(builder, sortDirection, defaultSortProperty);
        }

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression propertyExpression = parameter;
        var currentPropertyType = typeof(TEntity);

        try
        {
            var propertyPaths = sortLabel.Split('.');
            foreach (var propertyName in propertyPaths)
            {
                var propertyInfo = currentPropertyType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    return ApplyDefaultSortInternal(builder, sortDirection, defaultSortProperty);
                }
                propertyExpression = Expression.MakeMemberAccess(propertyExpression, propertyInfo);
                currentPropertyType = propertyInfo.PropertyType;
            }

            var convertedPropertyExpression = Expression.Convert(propertyExpression, typeof(object));
            var orderByLambda = Expression.Lambda<Func<TEntity, object?>>(convertedPropertyExpression, parameter);

            IOrderedSpecificationBuilder<TEntity> orderedBuilder = sortDirection == SortDirection.Ascending
                ? builder.OrderBy(orderByLambda)
                : builder.OrderByDescending(orderByLambda);

            var isDefaultDirect = string.Equals(sortLabel, defaultSortProperty, StringComparison.OrdinalIgnoreCase);
            var isDefaultNested = sortLabel.EndsWith("." + defaultSortProperty, StringComparison.OrdinalIgnoreCase);

            if (!isDefaultDirect && !isDefaultNested)
            {
                var defaultProp = typeof(TEntity).GetProperty(defaultSortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (defaultProp != null)
                {
                    var defaultAccess = Expression.MakeMemberAccess(parameter, defaultProp);
                    var defaultConverted = Expression.Convert(defaultAccess, typeof(object));
                    var thenByDefault = Expression.Lambda<Func<TEntity, object?>>(defaultConverted, parameter);

                    if (sortDirection == SortDirection.Ascending)
                    {
                        orderedBuilder.ThenBy(thenByDefault);
                    }
                    else
                    {
                        orderedBuilder.ThenByDescending(thenByDefault);
                    }
                }
            }

            return builder;
        }
        catch
        {
            return ApplyDefaultSortInternal(builder, sortDirection, defaultSortProperty);
        }
    }

    private static ISpecificationBuilder<TEntity> ApplyDefaultSortInternal<TEntity>(
        ISpecificationBuilder<TEntity> builder,
        SortDirection sortDirection,
        string defaultSortProperty)
        where TEntity : class
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var propertyInfo = typeof(TEntity).GetProperty(defaultSortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
        {
            return builder;
        }

        var propertyAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
        var convertedPropertyAccess = Expression.Convert(propertyAccess, typeof(object));
        var defaultOrderByLambda = Expression.Lambda<Func<TEntity, object?>>(convertedPropertyAccess, parameter);

        if (sortDirection == SortDirection.Ascending)
        {
            builder.OrderBy(defaultOrderByLambda);
        }
        else
        {
            builder.OrderByDescending(defaultOrderByLambda);
        }

        return builder;
    }
}
