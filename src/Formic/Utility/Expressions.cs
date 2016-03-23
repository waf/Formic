using Microsoft.Data.Entity.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Formic.Utility
{
    public static class Expressions
    {
        /// <summary>
        /// filter the provided queryable with the provided parameters.
        /// e.g. given parameters {"property1": ["val1", "val2"], "property2": ["val3"]}
        ///      return a filtered queryable where ((property1 == "val1" || property1 == "val2") && property2 == "val3")
        /// </summary>
        /// <param name="queryable">the source data to filter</param>
        /// <param name="entity">the entity type of the iqueryable</param>
        /// <param name="parameters">parameter criteria</param>
        /// <returns></returns>
        public static IQueryable<object> FilterByEntityParameters(IQueryable<object> queryable, IEntityType entity, IDictionary<string, string[]> parameters)
        {
            // end-goal is to create the expression `.Where(entity => ...some condition based on parameters...)`

            // this is our lambda parameter
            ParameterExpression entityParameter = Expression.Parameter(entity.ClrType, "entity");

            // this is our lambda body
            var searchCriteria = parameters
                // create nested list of boolean expressions.
                // { "property1": ["val1", "val2"], "property2": ["val3"]} becomes [[property1 == val1, property1 == val2], [property2 == val3]]
                .Select(property =>
                {
                    Expression propertyGetter = Expression.Property(entityParameter, Reflection.GetClrPropertyForEntity(entity, property.Key));
                    return property.Value.Select(value =>
                    {
                        return Expression.Equal(propertyGetter, Expression.Constant(Utility.Convert(value, propertyGetter.Type)));
                    });
                })
                // assemble nested booleans into a single boolean expression: (property1 == val1 || property1 == val2) && property2 == val3
                .Select(or => or.Aggregate(Expression.OrElse)).Aggregate(Expression.AndAlso);

            // assemble lambda, use as the condition in a where clause invoked on our iqueryable
            var lambda = Expression.Lambda(searchCriteria, entityParameter);
            var whereClause = Expression.Call(typeof(Queryable), "Where", new[] { queryable.ElementType }, queryable.Expression, lambda);
            return (IQueryable<object>)queryable.Provider.CreateQuery(whereClause);
        }
    }
}
