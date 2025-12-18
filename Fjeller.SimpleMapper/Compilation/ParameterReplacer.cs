using System.Linq.Expressions;

namespace Fjeller.SimpleMapper.Compilation;

/// ======================================================================================================================
/// <summary>
/// Expression visitor that replaces parameter references in an expression tree.
/// Used for composing custom property mapping expressions into the compiled mapper.
/// </summary>
/// ======================================================================================================================
internal class ParameterReplacer : ExpressionVisitor
{
	private readonly ParameterExpression _oldParameter;
	private readonly ParameterExpression _newParameter;

	/// ======================================================================================================================
	/// <summary>
	/// Creates a new ParameterReplacer that will replace oldParameter with newParameter
	/// </summary>
	/// <param name="oldParameter">The parameter to replace</param>
	/// <param name="newParameter">The parameter to use as replacement</param>
	/// ======================================================================================================================
	public ParameterReplacer( ParameterExpression oldParameter, ParameterExpression newParameter )
	{
		_oldParameter = oldParameter;
		_newParameter = newParameter;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Visits a parameter expression and replaces it if it matches the old parameter
	/// </summary>
	/// <param name="node">The parameter expression to visit</param>
	/// <returns>The new parameter if it matches, otherwise the original parameter</returns>
	/// ======================================================================================================================
	protected override Expression VisitParameter( ParameterExpression node )
	{
		return node == _oldParameter ? _newParameter : base.VisitParameter( node );
	}
}
