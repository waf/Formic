using Microsoft.Data.Entity.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Formic.Utility
{
    public class Expressions
    {
        public static IQueryable<object> FilterByEntityParameters(IQueryable<object> allData, IEntityType entity, IDictionary<string, string[]> parameters)
        {
            // end goal is to create `.Where(entity => ...some condition based on parameters...)`
            ParameterExpression entityParameter = Expression.Parameter(entity.ClrType, "entity");
            // create nested list. ?prop1=valA&prop1=valB&prop2=valC becomes [[prop1 == valA, prop1 == valB], [prop2=valC]]
            var searchCriteria = parameters.Select(property =>
            {
                Expression propertyGetter = Expression.Property(entityParameter, Reflection.GetClrPropertyForEntity(entity, property.Key));
                return property.Value.ToArray().Select(value =>
                {
                    return Expression.Equal(propertyGetter, Expression.Constant(Utility.Convert(value, propertyGetter.Type)));
                });
            });
            // assemble nested list into (prop1 == valA || prop1 == valB) && prop2=valC
            var condition = searchCriteria.Select(or => or.Aggregate(Expression.OrElse)).Aggregate(Expression.AndAlso);

            var lambda = Expression.Lambda(condition, new[] { entityParameter });
            var whereClause = Expression.Call(typeof(Queryable), "Where", new[] { allData.ElementType }, allData.Expression, lambda);
            return (IQueryable<object>)allData.Provider.CreateQuery(whereClause);
        }
    }
}
