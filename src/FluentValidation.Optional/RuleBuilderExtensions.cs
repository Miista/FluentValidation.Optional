using System;
using System.Linq.Expressions;
using FluentValidation.Internal;
using FluentValidation.Optional.Validators;
using Optional;
using Optional.Unsafe;

namespace FluentValidation.Optional
{
    public static class RuleBuilderExtensions
    {
        /// <summary>
        /// Triggers an action to specify validation rules to the contained value when the <see cref="Option{T}"/> contains a value.
        /// </summary>
        /// <param name="rule">The current rule</param>
        /// <param name="action">An action to be invoked if the rule is valid</param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, TProperty> WhenPresent<T, TProperty>(
            this IRuleBuilderInitial<T, Option<TProperty>> rule,
            Func<IRuleBuilderInitial<T, TProperty>, IRuleBuilderOptions<T, TProperty>> action)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            if (action == null) throw new ArgumentNullException(nameof(action));
            
            // Cast to extract the actual rule instance from the internal api.
            var actualRuleBuilder = rule as RuleBuilder<T, Option<TProperty>>
                                    ?? throw new ArgumentException(
                                        $"Rule is not an instance of '{typeof(RuleBuilder<T, Option<TProperty>>)}'.");
            var actualRule = actualRuleBuilder.Rule;
            var actualRuleExpression = actualRule.Expression as Expression<Func<T, Option<TProperty>>>
                                       ?? throw new ArgumentException(
                                           $"Rule does not point to a property of type '{typeof(Option<TProperty>)}'.");

            // Provide a transformation function. This is fine as the internal model requires a Func<object, object>
            actualRule.Transformer = value => ((Option<TProperty>) value).ValueOrDefault();
            
            // Create a new RuleBuilder that has the new type as the destination.
            var nestedRuleBuilder = new RuleBuilder<T, TProperty>(actualRule, actualRuleBuilder.ParentValidator);

            return action(nestedRuleBuilder).WhenPresent(actualRuleExpression);
        }

        /// <summary>
        /// Specifies a condition limiting when the validator should run.
        /// The validator will only be executed if the <see cref="Option{T}"/> contains a value.
        /// </summary>
        /// <param name="rule">The current rule</param>
        /// <param name="expression">The expression representing the <see cref="Option{T}"/> to evaluate against</param>
        /// <param name="applyConditionTo">Whether the condition should be applied to the current rule or all rules in the chain</param>
        /// <typeparam name="T">The type of object being validated</typeparam>
        /// <typeparam name="TElement">The type of the property being validated</typeparam>
        /// <typeparam name="TProperty">The type of property contained in the <see cref="Option{T}"/></typeparam>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, TElement> WhenPresent<T, TElement, TProperty>(
            this IRuleBuilderOptions<T, TElement> rule,
            Expression<Func<T, Option<TProperty>>> expression,
            ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            return rule.When(arg => expression.Compile()(arg).HasValue, applyConditionTo);
        }
        
        /// <summary>
        /// Specifies a condition limiting when the validator should run.
        /// The validator will only be executed if the <see cref="Option{T}"/> does not contain a value.
        /// </summary>
        /// <param name="rule">The current rule</param>
        /// <param name="expression">The expression representing the <see cref="Option{T}"/> to evaluate against</param>
        /// <param name="applyConditionTo">Whether the condition should be applied to the current rule or all rules in the chain</param>
        /// <typeparam name="T">The type of object being validated</typeparam>
        /// <typeparam name="TElement">The type of the property being validated</typeparam>
        /// <typeparam name="TProperty">The type of property contained in the <see cref="Option{T}"/></typeparam>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, TElement> UnlessPresent<T, TElement, TProperty>(
            this IRuleBuilderOptions<T, TElement> rule,
            Expression<Func<T, Option<TProperty>>> expression,
            ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            return rule.Unless(arg => expression.Compile()(arg).HasValue, applyConditionTo);
        }
        
        /// <summary>
        /// Defines a 'not none' validator on the current rule builder.
        /// Validation will fail if the property is <see cref="Option.None{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of object being validated</typeparam>
        /// <typeparam name="TProperty">Type of property being validated</typeparam>
        /// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, Option<TProperty>> NotNone<T, TProperty>(
            this IRuleBuilder<T, Option<TProperty>> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new OptionNotNoneValidator<TProperty>());
        }
        
        /// <summary>
        /// Defines a 'null' validator on the current rule builder.
        /// Validation will fail if the property is <see cref="Option.Some{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of object being validated</typeparam>
        /// <typeparam name="TProperty">Type of property being validated</typeparam>
        /// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, Option<TProperty>> None<T, TProperty>(
            this IRuleBuilder<T, Option<TProperty>> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new OptionNoneValidator<TProperty>());
        }
    }
}