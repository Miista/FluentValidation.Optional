using System;
using System.Linq.Expressions;
using FluentValidation.Internal;
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
        public Option<string> Text { get; }

        public Entity(string name, Option<int> age, int number, Option<int> optionalInt, Option<string> text)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Age = age;
            Number = number;
            OptionalInt = optionalInt;
            Text = text.NotNull();
        }
    }

    public class SimpleEntity
    {
        public string Name { get; }
        public Option<string> LastName { get; }

        public SimpleEntity(string name, Option<string> lastName) => (Name, LastName) = (name, lastName);
    }

    class Program
    {
        static void Main(string[] args)
        {
            // {
            //     var simpleEntity = new SimpleEntity(
            //         name: "Zero",
            //         lastName: Option.None<string>() //"Hour".Some()
            //     );
            //     var simpleEntityValidator = new SimpleEntityValidator();
            //     var validationResult = simpleEntityValidator.Validate(simpleEntity);
            //     Console.WriteLine(validationResult.IsValid);
            //     foreach (var error in validationResult.Errors)
            //     {
            //         Console.WriteLine(error);
            //     }
            // }
            var entity = new Entity(
                name: "Zero",
                age: Option.None<int>(), 
                number: 0,
                optionalInt: Option.None<int>(),
                text: Option.None<string>());
            var entityValidator = new EntityValidator();
            var validationResult = entityValidator.Validate(entity);
            Console.WriteLine(validationResult.IsValid);
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine(error);
            }
        }
    }

    public static class Unwrapper
    {
        public static IRuleBuilderOptions<T, TProperty> WhenSome<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
        {
            // Cast to extract the actual rule instance from the internal api.
            var actualRuleBuilder = rule as RuleBuilder<T, TProperty>
                                    ?? throw new ArgumentException(
                                        $"Rule is not an instance of '{typeof(RuleBuilder<T, TProperty>)}'.");
            var actualRule = actualRuleBuilder.Rule;
            var actualRuleExpression = actualRule.Expression as Expression<Func<T, Option<TProperty>>>
                                       ?? throw new ArgumentException(
                                           $"Rule does not point to a property of type '{typeof(Option<TProperty>)}'.");
            
            // Provide a transformation function. This is fine as the internal model requires a Func<object, object>
            actualRule.Transformer = value => ((Option<TProperty>) value).ValueOrDefault();
            
            // Create a new RuleBuilder that has the new type as the destination.
            var nestedRuleBuilder = new RuleBuilder<T, TProperty>(actualRule, actualRuleBuilder.ParentValidator);
            
            return nestedRuleBuilder.When(arg => actualRuleExpression.Compile()(arg).HasValue);
        }
        
        public static IRuleBuilderOptions<T, TProperty> WhenNone<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
        {
            // Cast to extract the actual rule instance from the internal api.
            var actualRuleBuilder = rule as RuleBuilder<T, TProperty>
                                    ?? throw new ArgumentException(
                                        $"Rule is not an instance of '{typeof(RuleBuilder<T, TProperty>)}'.");
            var actualRule = actualRuleBuilder.Rule;
            var actualRuleExpression = actualRule.Expression as Expression<Func<T, Option<TProperty>>>
                                       ?? throw new ArgumentException(
                                           $"Rule does not point to a property of type '{typeof(Option<TProperty>)}'.");
            
            // Provide a transformation function. This is fine as the internal model requires a Func<object, object>
            actualRule.Transformer = value => ((Option<TProperty>) value).ValueOrDefault();
            
            // Create a new RuleBuilder that has the new type as the destination.
            var nestedRuleBuilder = new RuleBuilder<T, TProperty>(actualRule, actualRuleBuilder.ParentValidator);
            
            return nestedRuleBuilder.When(arg => actualRuleExpression.Compile()(arg).HasValue == false);
        }

        
        public static IRuleBuilderOptions<T, TProperty> Unwrap<T, TProperty>(this IRuleBuilderInitial<T, Option<TProperty>> rule)
        {
            // Cast to extract the actual rule instance from the internal api.
            var actualRuleBuilder = rule as RuleBuilder<T, Option<TProperty>>
                                    ?? throw new ArgumentException(
                                        $"Rule is not an instance of '{typeof(RuleBuilder<T, Option<TProperty>>)}'.");
            var actualRule = actualRuleBuilder.Rule;

            // Provide a transformation function. This is fine as the internal model requires a Func<object, object>
            actualRule.Transformer = value => ((Option<TProperty>) value).ValueOrDefault();
            
            // Create a new RuleBuilder that has the new type as the destination.
            var nestedRuleBuilder = new RuleBuilder<T, TProperty>(actualRule, actualRuleBuilder.ParentValidator);

            return nestedRuleBuilder;

        }
    }
    
    public class SimpleEntityValidator : AbstractValidator<SimpleEntity>
    {
        public SimpleEntityValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(3);
            RuleFor(x => x.LastName)
                .Unwrap()
                .NotEmpty()
                .MinimumLength(5)
                .WhenNone()
                // .Empty()
                // .WhenNone()
                // .When(entity => false)
            ;
            // RuleFor(x => x.LastName)
            //     .Unwrap()
            //     .NotEmpty()
            //     .WhenSome()
            // ;
        }
    }

    public class EntityValidator : AbstractValidator<Entity>
    {
        public EntityValidator()
        {
            // RuleFor(x => x.Name)
            //     .NotNull()
            //     .NotEmpty();
            // RuleFor(x => x.Age)
            //     .None()
            //     .When(x => x.Name == "Zero");
            // RuleFor(x => x.Number)
            //     .Equal(x => x.Age.ValueOrDefault())
            //     .WhenPresent(x => x.Age);
            // RuleFor(x => x.Age)
            //     .WhenPresent(value => value.GreaterThanOrEqualTo(0))
            //     .WithMessage("lol");
            RuleFor(x => x.Text)
                .Unwrap()
                .NotEmpty()
                .WhenSome()
                // .WhenPresent(x => x.NotEmpty())
                ;
            RuleFor(x => x.Age)
                .Unwrap()
                .GreaterThanOrEqualTo(0)
                .WhenSome()
                // .WhenPresent(age => age.GreaterThanOrEqualTo(0))
                ;
            RuleFor(x => x.Number)
                .GreaterThanOrEqualTo(0);

            // RuleFor(x => x.Text)
            //     .NotNone()
            //     .UnlessPresent(x => x.Age)
            //     .WithMessage("lol");
            // RuleFor(x => x.Age)
            //     .NotNone()
            //     .When(x => x.Name == "Age");
            // RuleFor(x => x.Age)
            //     .WhenPresent(value => value.Equal(0))
            //     .When(x => x.Name == "Zero");
            // RuleFor(x => x.Age)
            //     .WhenPresent(value => value.Equal(1))
            //     .When(x => x.Name == "One");
            // RuleFor(x => x.Number)
            //     .Equal(0)
            //     .When(x => x.Name == "Zero")
            //     .WhenPresent(x => x.Age);
            // RuleFor(x => x.Age)
            //     .NotNone()
            //     .WhenPresent(value => value.InclusiveBetween(0, 10));
            // RuleFor(x => x.Age)
            //     .WhenPresent()
            //     .InclusiveBetween(0, 10)
            //     .When(x => x.Age.HasValue);
            // RuleFor(x => x.Age.ValueOrDefault())
            //     .InclusiveBetween(0, 10)
            //     .WithName(nameof(Entity.Age))
            //     .When(x => x.Age.HasValue);
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
            // RuleFor(x => x.Age.ValueOrDefault())
            //     .InclusiveBetween(0, 10)
            //     .WithName(nameof(Entity.Age))
            //     .When(x => x.Age.HasValue);
            // RuleFor(x => x.Age)
            //     .Must(option => option.HasValue)
            //     .WithMessage("'{PropertyName}' must contain a value.");
        }
    }
}