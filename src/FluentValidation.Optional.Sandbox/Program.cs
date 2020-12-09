using System;
using Optional;

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

    class Program
    {
        static void Main(string[] args)
        {
            var entity = new Entity(
                name: "Zero",
                age: 0.Some(), 
                number: (-1),
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

    public class EntityValidator : AbstractValidator<Entity>
    {
        public EntityValidator()
        {
            // RuleFor(x => x.Text)
            //     .NotNone()
            //     .WhenSome(x => x.Age)
            //     .None()
            //     .WhenNone(x => x.Age);
            RuleFor(x => x.Age)
                .WhenSome(x => x.GreaterThan(0));
            // RuleFor(x => x.Text)
            //     .WhenPresent(s => s.NotEmpty());
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
            // RuleFor(x => x.Text)
            //     .WhenPresent(x => x.NotEmpty());
            // RuleFor(x => x.Age)
            //     .WhenPresent(age => age.GreaterThanOrEqualTo(0));
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