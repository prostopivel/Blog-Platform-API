using Dapper;
using System.Data;

namespace BlogPlatform.Shared.Data
{
    internal class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            if (value is Guid guid)
            {
                return guid;
            }

            if (value is string str && Guid.TryParse(str, out var result))
            {
                return result;
            }

            if (value is byte[] bytes && bytes.Length == 16)
            {
                return new Guid(bytes);
            }

            throw new InvalidCastException($"Can not parse {value} to Guid");
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value;
            parameter.DbType = DbType.Guid;
        }
    }
}
