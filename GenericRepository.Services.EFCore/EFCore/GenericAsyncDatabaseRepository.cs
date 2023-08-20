using GenericRepository.Interfaces;
using GenericRepository.Options;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GenericRepository.Services.EFCore
{
    public class GenericAsyncDatabaseRepository<TEntity> : CommonGenericDbRepository,
        IGenericAsyncRepository<TEntity>
        where TEntity : class
    {
        private DbContext _context;

        public bool SetAsNoTracking { get; set; } = true;

        public GenericAsyncDatabaseRepository(DbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<TEntity> GetAsync([Required] DataModelOptions<TEntity> options, 
            CancellationToken cancellationToken = default)
            => await List(SetAsNoTracking 
                ? _context.Set<TEntity>().AsNoTracking() 
                : _context.Set<TEntity>(), 
                options)
            .FirstOrDefaultAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task<TDestination> GetAsync<TDestination>([Required] ComplexDataModelOptions<TEntity, TDestination> options, 
            CancellationToken cancellationToken = default)
            => await List(SetAsNoTracking
                ? _context.Set<TEntity>().AsNoTracking()
                : _context.Set<TEntity>(),
                options)
            .FirstOrDefaultAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task<IEnumerable<TEntity>> ListAsync([Required] DataModelOptions<TEntity> options, 
            CancellationToken cancellationToken = default)
            => await List(SetAsNoTracking
                ? _context.Set<TEntity>().AsNoTracking()
                : _context.Set<TEntity>(),
                options)
            .ToListAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task<IEnumerable<TDestination>> ListAsync<TDestination>([Required] ComplexDataModelOptions<TEntity, TDestination> options, 
            CancellationToken cancellationToken = default)
            => await List(SetAsNoTracking
                ? _context.Set<TEntity>().AsNoTracking()
                : _context.Set<TEntity>(),
                options)
            .ToListAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task CreateAsync([Required] TEntity entity, 
            CancellationToken cancellationToken = default)
        {
            _context.Set<TEntity>()
                .Add(entity);

            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059", Justification = "<Waiting>")]
        public async Task UpdateAsync([Required] Expression<Func<TEntity, bool>> searchClause, 
            [Required] TEntity entity, 
            CancellationToken cancellationToken = default)
        {
            var item = await List(_context.Set<TEntity>().AsNoTracking(), new DataModelOptions<TEntity> { EntitySearchClause = searchClause })
                .FirstOrDefaultAsync(cancellationToken);

            item = entity;

            _context.Update(item);

            await _context.SaveChangesAsync(cancellationToken);
            
        }

        /// <inheritdoc/>
        public async Task DeleteAsync([Required] Expression<Func<TEntity, bool>> searchClause, CancellationToken cancellationToken = default)
        {
            var entities = await List(_context.Set<TEntity>().AsNoTracking(), new DataModelOptions<TEntity> { EntitySearchClause = searchClause })
                .ToListAsync();

            _context.Set<TEntity>()
                .RemoveRange(entities);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
