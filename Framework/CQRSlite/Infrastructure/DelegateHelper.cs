using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CQRSlite.Infrastructure
{

	internal delegate object LateBoundFunc(object target, object[] arguments);

	internal delegate void LateBoundAction(object target, object[] arguments);

	/// <summary>
	/// Helper class for working with reflection and type conversions.
	/// </summary>
	/// <remarks>This class contains a modified version of Nate Kohari's post:
	/// Fast Late-Bound Invocation with Expression Trees on 3/6/2009
	/// http://kohari.org/2009/03/06/fast-late-bound-invocation-with-expression-trees/ </remarks>
	internal static class DelegateHelper
	{

		public static Action<TBase> CastArgument<TBase, TDerived>(Expression<Action<TDerived>> source) where TDerived : TBase
		{
			if (typeof(TDerived) == typeof(TBase))
			{
				return (Action<TBase>)((Delegate)source.Compile());
			}

			ParameterExpression sourceParameter = Expression.Parameter(typeof(TBase), "source");
			var result = Expression.Lambda<Action<TBase>>(
				Expression.Invoke(
					source,
					Expression.Convert(sourceParameter, typeof(TDerived))),
				sourceParameter);
			return result.Compile();
		}

		/// <summary>
		/// Creates an Func that is capable of being late bound and yields
		/// much better performance than using direct Reflection or DynamicInvoke.
		/// </summary>
		/// <param name="method">MethodInfo representing the code to be executed by the LateBoundFunc.</param>
		/// <returns></returns>
		public static LateBoundFunc CreateFunc(MethodInfo method)
		{
			ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
			ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

			MethodCallExpression call = Expression.Call(
			  Expression.Convert(instanceParameter, method.DeclaringType),
			  method,
			  CreateParameterExpressions(method, argumentsParameter));

			Expression<LateBoundFunc> lambda = Expression.Lambda<LateBoundFunc>(
			  Expression.Convert(call, typeof(object)),
			  instanceParameter,
			  argumentsParameter);

			return lambda.Compile();
		}

		/// <summary>
		/// Creates an Action that is capable of being late bound and yields
		/// much better performance than using direct Reflection or DynamicInvoke.
		/// </summary>
		/// <param name="method">MethodInfo representing the code to be executed by the LateBoundAction.</param>
		/// <returns></returns>
		public static LateBoundAction CreateAction(MethodInfo method)
		{
			ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
			ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

			MethodCallExpression body = Expression.Call(
			  Expression.Convert(instanceParameter, method.DeclaringType),
			  method,
			  CreateParameterExpressions(method, argumentsParameter));

			Expression<LateBoundAction> lambda = Expression.Lambda<LateBoundAction>(
			  body,
			  instanceParameter,
			  argumentsParameter);

			return lambda.Compile();
		}

		private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
		{
			return method.GetParameters().Select((parameter, index) =>
			  Expression.Convert(
				Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
		}

	}
}
