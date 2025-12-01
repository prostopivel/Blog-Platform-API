using BlogPlatform.Shared.Common.Models;
using Dapper;
using System.Data;

namespace BlogPlatform.Shared.Data
{
    public class EntityWithCountTypeHandler<T>
        : SqlMapper.TypeHandler<EntityWithCount<T>>
    where T : class, new()
    {
        public override EntityWithCount<T> Parse(object value)
        {
            if (value is IDictionary<string, object> dict)
            {
                var entity = new T();
                var entityWithCount = new EntityWithCount<T> { Entity = entity };

                var entityProperties = typeof(T)
                    .GetProperties();
                var totalCountProperty = typeof(EntityWithCount<T>)
                    .GetProperty(nameof(EntityWithCount<T>.TotalCount));

                foreach (var kvp in dict)
                {
                    var entityProp = entityProperties.FirstOrDefault(p =>
                        string.Equals(p.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));

                    if (entityProp != null && kvp.Value != DBNull.Value)
                    {
                        entityProp.SetValue(entity, kvp.Value);
                    }
                    else if (string.Equals(kvp.Key, nameof(EntityWithCount<T>.TotalCount),
                        StringComparison.OrdinalIgnoreCase) && kvp.Value != DBNull.Value)
                    {
                        totalCountProperty?.SetValue(entityWithCount, Convert.ToInt64(kvp.Value));
                    }
                }

                return entityWithCount;
            }

            return new EntityWithCount<T>();
        }

        public override void SetValue(IDbDataParameter parameter,
            EntityWithCount<T>? value)
        {
            parameter.Value = value;
        }
    }
}
