using FluentValidation.Validators;
using Optional;

namespace FluentValidation.Optional.Validators
{
    internal class OptionNotNoneValidator<T> : PropertyValidator
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
}