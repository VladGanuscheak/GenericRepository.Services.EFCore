using GenericRepository.Enums;
using GenericRepository.Options;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace GenericRepository.Services.EFCore
{
    public abstract class CommonGenericDbRepository
    {
        protected DbContext Context;

        public bool SetAsNoTracking { get; set; } = true;

        protected CommonGenericDbRepository(DbContext context)
        {
            Context = context;
        }

        private IQueryable<TType> SortEntities<TType>(IQueryable<TType> entities,
            string sortingFieldName,
            SortingOrder sortingOrder)
        {
            if (!string.IsNullOrWhiteSpace(sortingFieldName))
            {
                var entityType = typeof(TType);
                var propertyInfo = entityType.GetProperty(sortingFieldName) ?? throw new ArgumentException("There is no such a sorting key!");
                
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

        protected IQueryable<TDestination> List<TType, TDestination>(
            IQueryable<TType> entities,
            [Required] ComplexDataModelOptions<TType, TDestination> options)
        {
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

        protected IQueryable<TType> List<TType>(IQueryable<TType> entities, 
            [Required] DataModelOptions<TType> options)
        {
            if (options.EntitySearchClause != null)
            {
                entities = entities.Where(options.EntitySearchClause);
            }

            var result = SortEntities(entities, options.SortingFieldName, options.SortingOrder);

            return result;
        }
    }
}
