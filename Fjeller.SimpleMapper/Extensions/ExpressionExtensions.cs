using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Fjeller.SimpleMapper.Extensions;

internal static class ExpressionExtensions
{
	public static PropertyInfo? FindProperty( this LambdaExpression lambdaExpression )
	{
		Expression currentExpression = lambdaExpression;
		MemberInfo memberInfo = currentExpression.FindMember(  );

		PropertyInfo? result = memberInfo as PropertyInfo;
		if ( result == null )
		{
			throw new ArgumentException( "The expression must resolve a property" );
		}

		return result;
	}

	public static MemberInfo FindMember( this Expression expressionToCheck )
	{
		bool done = false;

		while ( !done )
		{
			switch ( expressionToCheck.NodeType )
			{
				case ExpressionType.Convert:
					expressionToCheck = ( (UnaryExpression)expressionToCheck ).Operand;
					break;
				case ExpressionType.Lambda:
					expressionToCheck = ( (LambdaExpression)expressionToCheck ).Body;
					break;
				case ExpressionType.MemberAccess:
					MemberExpression memberExpression = (MemberExpression)expressionToCheck;

					if ( memberExpression.Expression != null &&
					     memberExpression.Expression.NodeType != ExpressionType.Parameter &&
					     memberExpression.Expression.NodeType != ExpressionType.Convert )
					{
						throw new ArgumentException( $"Expression '{memberExpression}' must resolve to top-level member and not any child object's properties. Use a custom resolver on the child type or the AfterMap option instead.", nameof( expressionToCheck ) );
					}

					MemberInfo member = memberExpression.Member;

					return member;
				default:
					done = true;
					break;
			}
		}

		throw new Exception( "Custom configuration for members is only supported for top-level individual members on a type." );
	}
}
