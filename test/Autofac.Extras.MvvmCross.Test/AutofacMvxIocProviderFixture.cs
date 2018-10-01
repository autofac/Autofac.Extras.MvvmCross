using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Registration;
using MvvmCross;
using MvvmCross.Exceptions;
using MvvmCross.IoC;
using Xunit;

namespace Autofac.Extras.MvvmCross.Test
{
    public class AutofacMvxIocProviderFixture : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private interface IHasDependentProperty
        {
            IInterface1 Dependency { get; set; }

            IInterface2 MarkedDependency { get; set; }
        }

        private interface IInterface
        {
        }

        private interface IInterface1
        {
        }

        private interface IInterface2
        {
        }

        [Fact]
        public void CallbackWhenRegisteredFiresSuccessfully()
        {
            var called = false;
            var provider = this.CreateProvider();
            provider.CallbackWhenRegistered<IInterface>(() => called = true);
            provider.RegisterType<IInterface, Concrete>();
            Assert.True(called);
        }

        [Fact]
        public void CallbackWhenRegisteredThrowsArgumentNullExceptionWhenCalledWithNoTypeOrActionArgument()
        {
            var provider = this.CreateProvider();
            Assert.Throws<ArgumentNullException>(() => provider.CallbackWhenRegistered(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => provider.CallbackWhenRegistered(typeof(object), null));
        }

        [Fact]
        public void CanCreateChildContainers()
        {
            // Arrange
            var rootContainer = this.CreateProvider();
            rootContainer.RegisterType<IInterface, Concrete>();

            // Act
            var childContainer = rootContainer.CreateChildContainer();
            childContainer.RegisterType<IInterface1, Concrete1>();

            // Assert
            Assert.True(childContainer.CanResolve<IInterface>());
            Assert.True(childContainer.CanResolve<IInterface1>());
            Assert.True(rootContainer.CanResolve<IInterface>());
            Assert.False(rootContainer.CanResolve<IInterface1>());
        }

        [Fact]
        public void CanResolveReturnsFalseWhenNoMatchingTypeIsRegistered()
        {
            var provider = this.CreateProvider();
            Assert.False(provider.CanResolve<object>());
        }

        [Fact]
        public void CanResolveReturnsTrueWhenMatchingTypeIsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var provider = this.CreateProvider(builder.Build());
            Assert.True(provider.CanResolve<object>());
        }

        [Fact]
        public void CanResolveThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            var provider = this.CreateProvider();
            Assert.Throws<ArgumentNullException>(() => provider.CanResolve(null));
        }

        public void Dispose()
        {
            foreach (var disposable in this._disposables)
            {
                disposable.Dispose();
            }

            this._disposables.Clear();
        }

        [Fact]
        public void GetSingletonReturnsSingletonIfTypeRegisteredAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).SingleInstance();
            var provider = this.CreateProvider(builder.Build());
            Assert.IsType<object>(provider.GetSingleton<object>());
            Assert.Same(provider.GetSingleton<object>(), provider.GetSingleton<object>());
        }

        [Fact]
        public void GetSingletonThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            var provider = this.CreateProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSingleton(null));
        }

        [Fact]
        public void GetSingletonThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            var provider = this.CreateProvider();
            Assert.Throws<ComponentNotRegisteredException>(() => provider.GetSingleton<object>());
        }

        [Fact]
        public void GetSingletonThrowsDependencyResolutionExceptionIfTypeRegisteredButNotAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var provider = this.CreateProvider(builder.Build());

            Assert.Throws<DependencyResolutionException>(() => provider.GetSingleton<object>());
        }

        [Fact]
        public void IfSetInOptions_OnNonResolvableProperty_Throws()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                using (var p = this.CreateProvider(options: new MvxPropertyInjectorOptions()
                {
                    ThrowIfPropertyInjectionFails = true,
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                }))
                {
                }
            });
        }

        [Fact]
        public void IgnoresNonResolvableProperty()
        {
            // Arrange
            var provider = this.CreateProvider(options:
                new MvxPropertyInjectorOptions()
                {
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                });

            // Act
            var obj = Mvx.IoCProvider.IoCConstruct<HasDependantProperty>();

            // Assert
            Assert.NotNull(obj);
            Assert.Null(obj.Dependency);
            Assert.Null(obj.MarkedDependency);
        }

        [Fact]
        public void InjectsOnlyMarkedProperties_WithCustomAttribute_IfEnabled()
        {
            // Arrange
            var provider = this.CreateProvider(options: new AutofacPropertyInjectorOptions()
            {
                InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties,
                PropertyInjectionSelector = new CustomAttributeSelector(typeof(MyInjectionAttribute)),
            });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();

            // Act
            var obj = Mvx.IoCProvider.IoCConstruct<HasDependantProperty>();

            // Assert
            Assert.NotNull(obj);
            Assert.NotNull(obj.Dependency);
            Assert.Null(obj.MarkedDependency);
        }

        [Fact]
        public void InjectsOnlyMarkedProperties_WithCustomAttribute_IfEnabled_Lazy()
        {
            // Arrange
            var provider = this.CreateProvider(options: new AutofacPropertyInjectorOptions()
            {
                InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties,
                PropertyInjectionSelector = new CustomAttributeSelector(typeof(MyInjectionAttribute)),
            });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();
            Mvx.IoCProvider.RegisterSingleton<IHasDependentProperty>(Mvx.IoCProvider.IoCConstruct<HasDependantProperty>);

            // Act
            var obj = Mvx.IoCProvider.Resolve<IHasDependentProperty>();

            // Assert
            Assert.NotNull(obj);
            Assert.NotNull(obj.Dependency);
            Assert.Null(obj.MarkedDependency);
        }

        [Fact]
        public void InjectsOnlyMarkedPropertiesIfEnabled()
        {
            // Arrange
            var provider = this.CreateProvider(options:
                new MvxPropertyInjectorOptions()
                {
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();

            // Act
            var obj = Mvx.IoCProvider.IoCConstruct<HasDependantProperty>();

            // Assert
            Assert.NotNull(obj);
            Assert.Null(obj.Dependency);
            Assert.NotNull(obj.MarkedDependency);
        }

        [Fact]
        public void InjectsOnlyMarkedPropertiesIfEnabled_Lazy()
        {
            // Arrange
            var provider = this.CreateProvider(options:
                new MvxPropertyInjectorOptions()
                {
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();
            Mvx.IoCProvider.RegisterSingleton<IHasDependentProperty>(Mvx.IoCProvider.IoCConstruct<HasDependantProperty>);

            // Act
            var obj = Mvx.IoCProvider.Resolve<IHasDependentProperty>();

            // Assert
            Assert.NotNull(obj);
            Assert.Null(obj.Dependency);
            Assert.NotNull(obj.MarkedDependency);
        }

        [Fact]
        public void InjectsPropertiesIfEnabled()
        {
            // Arrange
            var provider = this.CreateProvider(options:
                new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();

            // Act
            var obj = Mvx.IoCProvider.IoCConstruct<HasDependantProperty>();

            // Assert
            Assert.NotNull(obj);
            Assert.NotNull(obj.Dependency);
            Assert.NotNull(obj.MarkedDependency);
        }

        [Fact]
        public void PropertyInjectionCanBeCustomized()
        {
            var builder = new ContainerBuilder();
            var options = new AutofacPropertyInjectorOptions
            {
                InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties,
                PropertyInjectionSelector = new DelegatePropertySelector((pi, obj) => pi.Name != "PropertyToSkip"),
            };
            var provider = new ChildAutofacMvxIocProvider(builder.Build(), options);
            this._disposables.Add(provider);
            provider.RegisterType(() => new Concrete());
            provider.RegisterType(typeof(Exception), () => new DivideByZeroException());
            var resolved = provider.Resolve<Concrete>();

            Assert.IsType<DivideByZeroException>(resolved.PropertyToInject);
            Assert.Null(resolved.PropertyToSkip);
        }

        [Fact]
        public void PropertyInjectionCanBeEnabled()
        {
            var builder = new ContainerBuilder();
            var provider = new ChildAutofacMvxIocProvider(builder.Build(), new AutofacPropertyInjectorOptions { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties });
            this._disposables.Add(provider);
            provider.RegisterType(() => new Concrete());
            provider.RegisterType(typeof(Exception), () => new DivideByZeroException());
            var resolved = provider.Resolve<Concrete>();

            // Default behavior is to inject all unset properties.
            Assert.IsType<DivideByZeroException>(resolved.PropertyToInject);
            Assert.IsType<DivideByZeroException>(resolved.PropertyToSkip);
        }

        [Fact]
        public void PropertyInjectionOffByDefault()
        {
            var provider = this.CreateProvider();
            provider.RegisterType(() => new Concrete());
            var resolved = provider.Resolve<Concrete>();
            Assert.Null(resolved.PropertyToInject);
        }

        [Fact]
        public void RegisterSingletoneThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            var provider = this.CreateProvider();
            Assert.Throws<ArgumentNullException>(() => provider.RegisterSingleton((IInterface)null));
            Assert.Throws<ArgumentNullException>(() => provider.RegisterSingleton((Func<IInterface>)null));
            Assert.Throws<ArgumentNullException>(() => provider.RegisterSingleton(null, new object()));
            Assert.Throws<ArgumentNullException>(() => provider.RegisterSingleton(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => provider.RegisterSingleton(typeof(object), null));
        }

        [Fact]
        public void RegisterSingletonRegistersConcreteTypeAsSingletonAgainstInterface()
        {
            var provider = this.CreateProvider();
            var concreteViaFunc = new Concrete();
            provider.RegisterSingleton<IInterface>(() => concreteViaFunc);
            Assert.Equal(concreteViaFunc, provider.Resolve<IInterface>());
            Assert.Same(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());

            var concreteInstance = new Concrete();
            provider.RegisterSingleton<IInterface>(concreteInstance);
            Assert.Equal(concreteInstance, provider.Resolve<IInterface>());
            Assert.Same(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeRegistersConcreteTypeAgainstInterface()
        {
            var provider = this.CreateProvider();
            provider.RegisterType<IInterface, Concrete>();
            var instance = provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoFromOrToTypeArgument()
        {
            var provider = this.CreateProvider();
            Assert.Throws<ArgumentNullException>(() => provider.RegisterType(null, typeof(object)));
            Assert.Throws<ArgumentNullException>(() => provider.RegisterType(typeof(object), (Type)null));
        }

        [Fact]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            var provider = this.CreateProvider();
            Assert.Throws<ArgumentNullException>(() => provider.RegisterType((Func<object>)null));
            Assert.Throws<ArgumentNullException>(() => provider.RegisterType(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => provider.RegisterType(typeof(object), (Func<object>)null));
        }

        [Fact]
        public void RegisterTypeWithDelegateAndTypeParameterRegistersConcreteTypeAgainstInterface()
        {
            var provider = this.CreateProvider();
            provider.RegisterType(typeof(IInterface), () => new Concrete());
            var instance = provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeWithDelegateRegistersConcreteTypeAgainstInterface()
        {
            var provider = this.CreateProvider();
            provider.RegisterType<IInterface>(() => new Concrete());
            var instance = provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, provider.Resolve<IInterface>());
            provider.RegisterType(typeof(IInterface), () => new Concrete());
            Assert.NotSame(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());
        }

        [Fact]
        public void ResolveCreateAndIoCConstructReturnsRegisteredType()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var provider = this.CreateProvider(builder.Build());

            Assert.IsType<object>(provider.Resolve<object>());
            Assert.IsType<object>(provider.Create<object>());
            Assert.IsType<object>(provider.IoCConstruct<object>());
        }

        [Fact]
        public void ResolveCreateAndIoCConstructThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            var provider = this.CreateProvider();
            Assert.Throws<ArgumentNullException>(() => provider.Resolve(null));
            Assert.Throws<ArgumentNullException>(() => provider.Create(null));
            Assert.Throws<ArgumentNullException>(() => provider.IoCConstruct(null));
        }

        [Fact]
        public void ResolveCreateAndIoCConstructThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            var provider = this.CreateProvider();
            Assert.Throws<MvxIoCResolveException>(() => provider.Resolve<object>());
            Assert.Throws<MvxIoCResolveException>(() => provider.Create<object>());
            Assert.NotNull(provider.IoCConstruct<object>());
        }

        [Fact]
        public void TryResolveResolvesOutParameterWhenMatchingTypeRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var provider = this.CreateProvider(builder.Build());

            object foo;
            var success = provider.TryResolve(out foo);

            Assert.IsType<object>(foo);
            Assert.True(success);
        }

        private AutofacMvxIocProvider CreateProvider(IContainer container = null, MvxPropertyInjectorOptions options = null)
        {
            if (container == null)
            {
                container = new ContainerBuilder().Build();
            }

            var provider = new AutofacMvxIocProvider(container, options ?? new MvxPropertyInjectorOptions());
            this._disposables.Add(provider);
            return provider;
        }

        private class Concrete : IInterface
        {
            public Exception PropertyToInject { get; set; }

            public Exception PropertyToSkip { get; set; }
        }

        private class Concrete1 : IInterface1
        {
        }

        private class Concrete2 : IInterface2
        {
        }

        private class CustomAttributeSelector : IPropertySelector
        {
            private readonly Type _customAttributeType;

            public CustomAttributeSelector(Type customAttributeType)
            {
                if (customAttributeType == null) throw new ArgumentNullException(nameof(customAttributeType));
                this._customAttributeType = customAttributeType;
            }

            public bool InjectProperty(PropertyInfo propertyInfo, object instance)
            {
                return propertyInfo.GetCustomAttributes(this._customAttributeType).Any();
            }
        }

        private class HasDependantProperty : IHasDependentProperty
        {
            [MyInjection]
            public IInterface1 Dependency { get; set; }

            [MvxInject]
            public IInterface2 MarkedDependency { get; set; }
        }

        private class MyInjectionAttribute : Attribute
        {
        }
    }
}
