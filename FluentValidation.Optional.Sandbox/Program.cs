using System;
using System.Linq.Expressions;
using FluentValidation.Internal;
using FluentValidation.Resources;
using FluentValidation.Validators;
using Optional;
using Optional.Unsafe;

namespace FluentValidation.Optional.Sandbox
{
    public class Entity
    {
        public string Name { get; }
        public Option<int> Age { get; }
        public int Number { get; }
        public Option<int> OptionalInt { get; }

        public Entity(string name, Option<int> age, int number, Option<int> optionalInt)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Age = age;
            Number = number;
            OptionalInt = optionalInt;
        }
    }

    public class EntityValidator : AbstractValidator<Entity>
    {
        public EntityValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty();
            RuleFor(x => x.Age)
                .NotNone()
                .When(x => x.Name == "Age");
            RuleFor(x => x.Age)
                .WhenPresent(value => value.InclusiveBetween(0, 10));
            RuleFor(x => x.Age)
                .NotNone()
                .WhenPresent(value => value.InclusiveBetween(0, 10));
            // RuleFor(x => x.Age)
            //     .WhenPresent()
            //     .InclusiveBetween(0, 10)
            //     .When(x => x.Age.HasValue);
            RuleFor(x => x.Age.ValueOrDefault())
                .InclusiveBetween(0, 10)
                .WithName(nameof(Entity.Age))
                .When(x => x.Age.HasValue);
            // RuleFor(x => x.Age)
            //     .Some()
            //     .DependentRules(() =>
            //     {
            //         RuleFor(x => x.Age.ValueOrDefault())
            //             .InclusiveBetween(0, 10)
            //             .WithName(nameof(Entity.Age));
            //     });
            // RuleFor(x => x.Age)
            //     .Must(option => option.HasValue)
            //     .WithMessage("'{PropertyName}' must contain a value.")
            //     .DependentRules(() =>
            //     {
            //         RuleFor(x => x.Age.ValueOrDefault())
            //             .InclusiveBetween(0, 10)
            //             .WithName(nameof(Entity.Age));
            //     });
            RuleFor(x => x.Age.ValueOrDefault())
                .InclusiveBetween(0, 10)
                .WithName(nameof(Entity.Age))
                .When(x => x.Age.HasValue);
            // RuleFor(x => x.Age)
            //     .Must(option => option.HasValue)
            //     .WithMessage("'{PropertyName}' must contain a value.");
        }
    }

    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WhenPresent<T, TProperty>(
            this IRuleBuilder<T, Option<TProperty>> ruleBuilder,
            Func<IRuleBuilderInitial<T, TProperty>, IRuleBuilderOptions<T, TProperty>> configurator)
        {
            if (ruleBuilder == null) throw new ArgumentNullException(nameof(ruleBuilder));
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            
            // Cast to extract the actual rule instance from the internal api.
            var actualRuleBuilder = (RuleBuilder<T, Option<TProperty>>) ruleBuilder;
            var rule = actualRuleBuilder.Rule;

            // Create new property rule
            var propertyRule = PropertyRule.Create((Expression<Func<T, Option<TProperty>>>) rule.Expression);
            // Provide a transformation function. This is fine as the internal model requires a Func<object, object>
            propertyRule.Transformer = value => ((Option<TProperty>) value).ValueOrDefault();
            
            // Create a new RuleBuilder that has the new type as the destination.
            var nestedRuleBuilder = new RuleBuilder<T, TProperty>(propertyRule, actualRuleBuilder.ParentValidator);

            return configurator(nestedRuleBuilder).When(IsSome(rule));

            static Func<T, bool> IsSome(PropertyRule rule)
            {
                return arg =>
                {
                    var propertyValue = rule.PropertyFunc(arg);

                    if (propertyValue is Option<TProperty> option)
                        return option.HasValue;
                    else
                        return false;
                };
            }
        }
        
        public static IRuleBuilderOptions<T, Option<TProperty>> NotNone<T, TProperty>(
            this IRuleBuilderInitial<T, Option<TProperty>> ruleBuilder)
        {
            return ruleBuilder
                .Must(option => option.HasValue)
                .WithMessage("'{PropertyName}' must contain a value.");
        }

        // public static IRuleBuilderOptions<T, TProperty> Value<T, TProperty>(
        //     this IRuleBuilder<T, Option<TProperty>> ruleBuilder)
        //     where TProperty : IComparable<TProperty>, IComparable
        // {
        //     // Cast to extract the actual rule instance from the internal api.
        //     var actualRuleBuilder = (RuleBuilder<T, Option<TProperty>>) ruleBuilder;
        //     var rule = actualRuleBuilder.Rule;
        //     // Provide a transformation function. This is fine as the internal model requires a Func<object, object>
        //     rule.Transformer = value => ((Option<TProperty>) value).ValueOrDefault();
        //     // Create a new RuleBuilder that has the new type as the destination.
        //     var nestedRuleBuilder = new RuleBuilder<T, TProperty>(rule, actualRuleBuilder.ParentValidator);
        //
        //     return nestedRuleBuilder;
        // }
        
        public static IRuleBuilderOptions<T, Option<TProperty>> NotNone<T, TProperty>(
            this IRuleBuilder<T, Option<TProperty>> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new OptionNotNoneValidator<TProperty>());
        }
        
        public static IRuleBuilderOptions<T, Option<TProperty>> None<T, TProperty>(
            this IRuleBuilder<T, Option<TProperty>> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new OptionNoneValidator<TProperty>());
        }
    }
    
    public class OptionNotNoneValidator<T> : PropertyValidator
    {
        public OptionNotNoneValidator()
            : base("'{PropertyName}' must contain a value.")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is Option<T> option)
            {
                return option.HasValue;
            }

            return false;
        }
    }
    
    public class OptionNoneValidator<T> : PropertyValidator
    {
        public OptionNoneValidator()
            : base("'{PropertyName}' must not contain a value.")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is Option<T> option)
            {
                return !option.HasValue;
            }

            return false;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var entity = new Entity("Age", 11.Some(), 10, Option.None<int>());
            var entityValidator = new EntityValidator();
            var validationResult = entityValidator.Validate(entity);
            Console.WriteLine(validationResult.IsValid);
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine(error);
            }
        }
    }
}