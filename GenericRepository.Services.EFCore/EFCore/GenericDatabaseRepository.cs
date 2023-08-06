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
    public class GenericDatabaseRepository<TDbContext, TEntity> : CommonGenericDbRepository, 
        IGenericRepository<TEntity> 
        where TDbContext : DbContext 
        where TEntity : class
    {
        private TDbContext _context;

        public GenericDatabaseRepository(TDbContext context)
        {
            _context = context;
        }

        public TEntity Get([Required] DataModelOptions<TEntity> options) 
            => List(options).FirstOrDefault();

        public TDestination Get<TDestination>([Required] ComplexDataModelOptions<TEntity, TDestination> options)
            => List(options).FirstOrDefault();

        public IEnumerable<TEntity> List([Required] DataModelOptions<TEntity> options)
            => List(_context.Set<TEntity>().AsNoTracking(), options);

        public IEnumerable<TDestination> List<TDestination>([Required] ComplexDataModelOptions<TEntity, TDestination> options)
            => List(_context.Set<TEntity>().AsNoTracking(), options);

        public void Create([Required] TEntity entity)
        {
            _context.Set<TEntity>()
                .Add(entity);

            _context.SaveChanges();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059", Justification = "<Waiting>")]
        public void Update([Required] Expression<Func<TEntity, bool>> searchClause, [Required] TEntity entity)
        {
            var item = List(_context.Set<TEntity>(), new DataModelOptions<TEntity> { EntitySearchClause = searchClause })
                .FirstOrDefault();

            item = entity;

            _context.Update(item);

            _context.SaveChanges();
        }

        public void Delete([Required] Expression<Func<TEntity, bool>> searchClause)
        {
            var entities = List(_context.Set<TEntity>(), new DataModelOptions<TEntity> { EntitySearchClause = searchClause })
                .ToList();

            _context.Set<TEntity>()
                .RemoveRange(entities);

            _context.SaveChanges();
        }
    }
}
