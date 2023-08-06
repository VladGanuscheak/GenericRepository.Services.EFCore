using GenericRepository.Enums;
using GenericRepository.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GenericRepository.Services.EFCore.Helpers
{
    public class QueryableRepositoryHelper<TEntity>
    {
        IQueryable<TEntity> _entities;

        public QueryableRepositoryHelper(IQueryable<TEntity> entities)
        {
            _entities = entities;
        }

        private IQueryable<TType> SortEntities<TType>(IQueryable<TType> entities, 
            string sortingFieldName, 
            SortingOrder sortingOrder)
        {
            if (!string.IsNullOrWhiteSpace(sortingFieldName))
            {
                var entityType = typeof(TType);
                var propertyInfo = entityType.GetProperty(sortingFieldName);

                if (propertyInfo == null)
                {
                    throw new ArgumentException();
                }

                var parameter = Expression.Parameter(entityType, "x");
                var property = Expression.Property(parameter, propertyInfo);
                var lambda = Expression.Lambda(property, parameter);
                var method = typeof(Queryable).GetMethods()
                    .Single(predicate => predicate.Name == 
                        (sortingOrder == SortingOrder.Asc ? "OrderBy" : "OrderByDescending") &&
                                       predicate.IsGenericMethodDefinition &&
                                       predicate.GetParameters().Length == 2)
                    .MakeGenericMethod(entityType, propertyInfo.PropertyType);

                entities = (IQueryable<TType>)method.Invoke(null, new object[] { entities, lambda });
            }

            return entities;
        }

        public IEnumerable<TEntity> List([Required] DataModelOptions<TEntity> options)
        {
            var result = _entities;
            if (options.EntitySearchClause != null)
            {
                result = result.Where(options.EntitySearchClause);
            }

            result = SortEntities(result, options.SortingFieldName, options.SortingOrder);

            return result;
        }

        public IEnumerable<TDestination> List<TDestination>([Required] ComplexDataModelOptions<TEntity, TDestination> options)
        {
            var entities = _entities;
            if (options.EntitySearchClause != null)
            {
                entities = entities.Where(options.EntitySearchClause);
            }

            var result = entities.Select(options.Projection);

            if (options.ProjectionSearchClause != null)
            {
                result = result.Where(options.ProjectionSearchClause);
            }

            result = SortEntities(result, options.SortingFieldName, options.SortingOrder);

            return result;
        }
    }
}
