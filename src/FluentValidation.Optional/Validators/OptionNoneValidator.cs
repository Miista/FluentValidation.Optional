using FluentValidation.Validators;
using Optional;

namespace FluentValidation.Optional.Validators
{
    internal class OptionNoneValidator<T> : PropertyValidator
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
}