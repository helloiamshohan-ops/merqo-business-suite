using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Applies snake_case table, column, key, foreign-key and index names so that hand-written reporting SQL
/// stays readable and predictable alongside the EF model.
/// </summary>
public static class SnakeCaseNaming
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName)) entity.SetTableName(ToSnakeCase(tableName));

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName)) property.SetColumnName(ToSnakeCase(columnName));
            }

            foreach (var key in entity.GetKeys())
            {
                var name = key.GetName();
                if (!string.IsNullOrEmpty(name)) key.SetName(ToSnakeCase(name));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var name = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(name)) foreignKey.SetConstraintName(ToSnakeCase(name));
            }

            foreach (var index in entity.GetIndexes())
            {
                var name = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(name)) index.SetDatabaseName(ToSnakeCase(name));
            }
        }
    }

    public static string ToSnakeCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var builder = new StringBuilder(value.Length + 8);
        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsUpper(c))
            {
                var previousIsLowerOrDigit = i > 0 && (char.IsLower(value[i - 1]) || char.IsDigit(value[i - 1]));
                var nextIsLower = i + 1 < value.Length && char.IsLower(value[i + 1]);
                if (i > 0 && (previousIsLowerOrDigit || nextIsLower) && builder[^1] != '_') builder.Append('_');
                builder.Append(char.ToLowerInvariant(c));
            }
            else if (c == '.')
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(c);
            }
        }
        return builder.ToString();
    }
}
