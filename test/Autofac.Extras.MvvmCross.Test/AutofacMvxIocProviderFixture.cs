using System;
using Autofac.Core;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Extras.MvvmCross.Test
{
    public class AutofacMvxIocProviderFixture : IDisposable
    {
        IContainer _container;

        AutofacMvxIocProvider _provider;

        public AutofacMvxIocProviderFixture()
        {
            _container = new ContainerBuilder().Build();
            _provider = new AutofacMvxIocProvider(_container);
        }

        [Fact]
        public void CanResolveReturnsTrueWhenMatchingTypeIsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(_container);

            Assert.True(_provider.CanResolve<object>());
        }

        [Fact]
        public void CanResolveReturnsFalseWhenNoMatchingTypeIsRegistered()
        {
            Assert.False(_provider.CanResolve<object>());
        }

        [Fact]
        public void CanResolveThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.Throws<ArgumentNullException>(() => _provider.CanResolve(null));
        }

        [Fact]
        public void ResolveCreateAndIoCConstructReturnsRegisteredType()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(_container);

            Assert.IsType<object>(_provider.Resolve<object>());
            Assert.IsType<object>(_provider.Create<object>());
            Assert.IsType<object>(_provider.IoCConstruct<object>());
        }

        [Fact]
        public void ResolveCreateAndIoCConstructThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.Throws<ComponentNotRegisteredException>(() => _provider.Resolve<object>());
            Assert.Throws<ComponentNotRegisteredException>(() => _provider.Create<object>());
            Assert.Throws<ComponentNotRegisteredException>(() => _provider.IoCConstruct<object>());
        }

        [Fact]
        public void ResolveCreateAndIoCConstructThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Resolve(null));
            Assert.Throws<ArgumentNullException>(() => _provider.Create(null));
            Assert.Throws<ArgumentNullException>(() => _provider.IoCConstruct(null));
        }

        [Fact]
        public void GetSingletonReturnsSingletonIfTypeRegisteredAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).SingleInstance();
            builder.Update(_container);

            Assert.IsType<object>(_provider.GetSingleton<object>());
            Assert.Same(_provider.GetSingleton<object>(), _provider.GetSingleton<object>());
        }

        [Fact]
        public void GetSingletonThrowsDependencyResolutionExceptionIfTypeRegisteredButNotAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(_container);

            Assert.Throws<DependencyResolutionException>(() => _provider.GetSingleton<object>());
        }

        [Fact]
        public void GetSingletonThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.Throws<ComponentNotRegisteredException>(() => _provider.GetSingleton<object>());
        }

        [Fact]
        public void GetSingletonThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.Throws<ArgumentNullException>(() => _provider.GetSingleton(null));
        }

        [Fact]
        public void TryResolveResolvesOutParameterWhenMatchingTypeRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(_container);

            object foo;
            var success = _provider.TryResolve(out foo);

            Assert.IsType<object>(foo);
            Assert.True(success);
        }

        [Fact]
        public void RegisterTypeRegistersConcreteTypeAgainstInterface()
        {
            _provider.RegisterType<IInterface, Concrete>();
            var instance = _provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, _provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeWithDelegateRegistersConcreteTypeAgainstInterface()
        {
            _provider.RegisterType<IInterface>(() => new Concrete());
            var instance = _provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, _provider.Resolve<IInterface>());

            _provider.RegisterType(typeof(IInterface), () => new Concrete());
            Assert.NotSame(_provider.Resolve<IInterface>(), _provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeWithDelegateAndTypeParameterRegistersConcreteTypeAgainstInterface()
        {
            _provider.RegisterType(typeof(IInterface), () => new Concrete());
            var instance = _provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, _provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoFromOrToTypeArgument()
        {
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterType(null, typeof(object)));
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterType(typeof(object), (Type)null));
        }

        [Fact]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterType((Func<object>)null));
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterType(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterType(typeof(object), (Func<object>)null));
        }

        [Fact]
        public void RegisterSingletonRegistersConcreteTypeAsSingletonAgainstInterface()
        {
            var concreteViaFunc = new Concrete();
            _provider.RegisterSingleton<IInterface>(() => concreteViaFunc);
            Assert.Equal(concreteViaFunc, _provider.Resolve<IInterface>());
            Assert.Same(_provider.Resolve<IInterface>(), _provider.Resolve<IInterface>());

            var concreteInstance = new Concrete();
            _provider.RegisterSingleton<IInterface>(concreteInstance);
            Assert.Equal(concreteInstance, _provider.Resolve<IInterface>());
            Assert.Same(_provider.Resolve<IInterface>(), _provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterSingletoneThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterSingleton((IInterface)null));
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterSingleton((Func<IInterface>)null));
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterSingleton(null, new object()));
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterSingleton(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => _provider.RegisterSingleton(typeof(object), null));
        }

        [Fact]
        public void CallbackWhenRegisteredFiresSuccessfully()
        {
            var called = false;
            _provider.CallbackWhenRegistered<IInterface>(() => called = true);

            _provider.RegisterType<IInterface, Concrete>();
            Assert.True(called);
        }

        [Fact]
        public void CallbackWhenRegisteredThrowsArgumentNullExceptionWhenCalledWithNoTypeOrActionArgument()
        {
            Assert.Throws<ArgumentNullException>(() => _provider.CallbackWhenRegistered(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => _provider.CallbackWhenRegistered(typeof(object), null));
        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        private interface IInterface
        {
        }

        private class Concrete : IInterface
        {
        }
    }
}
