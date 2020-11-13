using FluentAssertions;
using Optional;
using Xunit;

namespace FluentValidation.Optional.Tests
{
    public class RuleBuilderExtensionTests
    {
        public class Entity
        {
            public Option<int> Age { get; set; }
            public string Name { get; set; }
        }

        public class WhenPresentTests
        {
            private class EntityValidator : AbstractValidator<Entity>
            {
                public EntityValidator()
                {
                    RuleFor(x => x.Age)
                        .WhenPresent(value => value.GreaterThanOrEqualTo(0));
                }
            }

            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(0x7fffffff)]
            public void Can_use_WhenPresent_IsValid(int value)
            {
                // Arrange
                var entity = new Entity {Age = value.Some()};
                var sut = new EntityValidator();

                // Act
                var result = sut.Validate(entity);

                // Assert
                result.Should().NotBeNull();
                result.IsValid.Should().BeTrue(because: "'Age' is greater than or equal to 0");
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(unchecked((int) 0x80000000))]
            public void Can_use_WhenPresent_IsNotValid(int value)
            {
                // Arrange
                var entity = new Entity {Age = value.Some()};
                var sut = new EntityValidator();

                // Act
                var result = sut.Validate(entity);

                // Assert
                result.Should().NotBeNull();
                result.IsValid.Should().BeFalse(because: "'Age' is not greater than or equal to 0");
            }
        }
        
        public class NoneTests
        {
            private class EntityValidator : AbstractValidator<Entity>
            {
                public EntityValidator()
                {
                    RuleFor(x => x.Age)
                        .None();
                }
            }

            [Fact]
            public void Can_use_None_IsValid()
            {
                // Arrange
                var entity = new Entity {Age = Option.None<int>()};
                var sut = new EntityValidator();

                // Act
                var result = sut.Validate(entity);

                // Assert
                result.Should().NotBeNull();
                result.IsValid.Should().BeTrue(because: "'Age' is None");
            }
            
            [Fact]
            public void Can_use_None_IsNotValid()
            {
                // Arrange
                var entity = new Entity {Age = 0.Some()};
                var sut = new EntityValidator();

                // Act
                var result = sut.Validate(entity);

                // Assert
                result.Should().NotBeNull();
                result.IsValid.Should().BeFalse(because: "'Age' is not None");
            }
        }

        public class WhenPresentConditionTests
        {
            private class EntityValidator : AbstractValidator<Entity>
            {
                public EntityValidator()
                {
                    RuleFor(x => x.Name)
                        .Equal("Age")
                        .WhenPresent(x => x.Age);
                }
            }

            [Fact]
            public void Can_use_WhenPresent_IsValid()
            {
                // Arrange
                var entity = new Entity {Name = "Age", Age = default(int).Some()};
                var sut = new EntityValidator();

                // Act
                var result = sut.Validate(entity);

                // Assert
                result.Should().NotBeNull();
                result.IsValid.Should().BeTrue(because: "'Name' is 'Age' and 'Age' is Some");
            }

            [Fact]
            public void Can_use_WhenPresent_IsNotValid()
            {
                // Arrange
                var entity = new Entity {Name = "NotAge", Age = default(int).Some()};
                var sut = new EntityValidator();

                // Act
                var result = sut.Validate(entity);

                // Assert
                result.Should().NotBeNull();
                result.IsValid.Should().BeFalse(because: "'Name' is 'NotAge' and 'Age' is Some");
            }
        }
        
        public class UnlessPresentConditionTests
        {
            private class EntityValidator : AbstractValidator<Entity>
            {
                public EntityValidator()
                {
                    RuleFor(x => x.Name)
                        .Equal("Age")
                        .UnlessPresent(x => x.Age);
                }
            }

            [Fact]
            public void Can_use_UnlessPresent_IsValid()
            {
                // Arrange
                var entity = new Entity {Name = "NotAge", Age = default(int).Some()};
                var sut = new EntityValidator();

                // Act
                var result = sut.Validate(entity);

                // Assert
                result.Should().NotBeNull();
                result.IsValid.Should().BeTrue(because: "'Name' is 'NotAge' and 'Age' is Some");
            }

            [Fact]
            public void Can_use_UnlessPresent_IsNotValid()
            {
                // Arrange
                var entity = new Entity {Name = "NotAge", Age = Option.None<int>()};
                var sut = new EntityValidator();

                // Act
                var result = sut.Validate(entity);

                // Assert
                result.Should().NotBeNull();
                result.IsValid.Should().BeFalse(because: "'Name' is 'NotAge' and 'Age' is None");
            }
        }
    }
}