using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Helpers used by <see cref="Persistence.AppDbContext.OnModelCreating(ModelBuilder)"/> to apply
/// cross-cutting concerns (snake_case naming, global soft-delete query filter) in one place.
/// </summary>
internal static class ModelBuilderExtensions
{
    /// <summary>
    /// Renames every table, column, key, foreign key, and index to snake_case so the generated
    /// schema uses consistent lower_snake identifiers.
    /// </summary>
    public static void ApplySnakeCaseNaming(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName is "__EFMigrationsHistory")
                continue;
            if (!string.IsNullOrEmpty(tableName))
            {
                entity.SetTableName(SnakeCase.From(tableName));
            }

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(SnakeCase.From(columnName));
                }
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName))
                {
                    key.SetName(SnakeCase.From(keyName));
                }
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var fkName = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(fkName))
                {
                    foreignKey.SetConstraintName(SnakeCase.From(fkName));
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexName))
                {
                    index.SetDatabaseName(SnakeCase.From(indexName));
                }
            }
        }
    }

    /// <summary>
    /// Installs the global query filter <c>WHERE is_deleted = false</c> on every entity that
    /// derives from <see cref="SoftDeletableEntity"/> (spec §3.2).
    /// <para>
    /// Concrete entity types are each their own root (the abstract <c>SoftDeletableEntity</c> base is
    /// never an entity type itself, so EF doesn't build a TPH hierarchy across them). That means we
    /// install the filter directly on every leaf type.
    /// </para>
    /// </summary>
    public static void ApplySoftDeleteQueryFilter(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (entity.IsOwned()) continue;
            if (!typeof(SoftDeletableEntity).IsAssignableFrom(entity.ClrType)) continue;

            var parameter = Expression.Parameter(entity.ClrType, "e");
            var isDeleted = Expression.Property(parameter, nameof(SoftDeletableEntity.IsDeleted));
            var notDeleted = Expression.Equal(isDeleted, Expression.Constant(false));
            var lambda = Expression.Lambda(notDeleted, parameter);
            builder.Entity(entity.ClrType).HasQueryFilter(lambda);
        }
    }
}
