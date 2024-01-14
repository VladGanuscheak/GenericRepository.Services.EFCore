using GenericRepository.Interfaces;
using GenericRepository.Options;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace GenericRepository.Services.EFCore
{
    public class GenericDatabaseRepository<TEntity> : CommonGenericDbRepository, 
        IGenericRepository<TEntity>
        where TEntity : class
    {
        public GenericDatabaseRepository(DbContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        public TEntity Get([Required] DataModelOptions<TEntity> options) 
            => List(SetAsNoTracking
                ? Context.Set<TEntity>().AsNoTracking()
                : Context.Set<TEntity>(), options).FirstOrDefault();

        /// <inheritdoc/>
        public TDestination Get<TDestination>([Required] ComplexDataModelOptions<TEntity, TDestination> options)
            => List(SetAsNoTracking
                ? Context.Set<TEntity>().AsNoTracking()
                : Context.Set<TEntity>(), options).FirstOrDefault();

        /// <inheritdoc/>
        public IEnumerable<TEntity> List([Required] DataModelOptions<TEntity> options)
            => List(SetAsNoTracking
                ? Context.Set<TEntity>().AsNoTracking()
                : Context.Set<TEntity>(),
                options);

        /// <inheritdoc/>
        public IEnumerable<TDestination> List<TDestination>([Required] ComplexDataModelOptions<TEntity, TDestination> options)
            => List(SetAsNoTracking
                ? Context.Set<TEntity>().AsNoTracking()
                : Context.Set<TEntity>(),
                options);

        /// <inheritdoc/>
        public void Create([Required] TEntity entity)
        {
            Context.Set<TEntity>()
                .Add(entity);

            Context.SaveChanges();
        }

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059", Justification = "<Waiting>")]
        public void Update([Required] Expression<Func<TEntity, bool>> searchClause, [Required] TEntity entity)
        {
            var item = List(Context.Set<TEntity>().AsNoTracking(), new DataModelOptions<TEntity> { EntitySearchClause = searchClause })
                .FirstOrDefault();

            item = entity;

            Context.Update(item);

            Context.SaveChanges();
        }

        /// <inheritdoc/>
        public void Delete([Required] Expression<Func<TEntity, bool>> searchClause)
        {
            var entities = List(Context.Set<TEntity>().AsNoTracking(), new DataModelOptions<TEntity> { EntitySearchClause = searchClause });

            entities.ExecuteDelete();

            Context.SaveChanges();
        }
    }
}
