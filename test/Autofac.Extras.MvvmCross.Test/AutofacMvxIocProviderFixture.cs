using System;
using Autofac.Core;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Extras.MvvmCross.Test
{
    public class AutofacMvxIocProviderFixture : IDisposable
    {
        private IContainer _container;

        private AutofacMvxIocProvider _provider;

        public AutofacMvxIocProviderFixture()
        {
            this._container = new ContainerBuilder().Build();
            this._provider = new AutofacMvxIocProvider(this._container);
        }

        private interface IInterface
        {
        }

        [Fact]
        public void CallbackWhenRegisteredFiresSuccessfully()
        {
            bool called = false;
            this._provider.CallbackWhenRegistered<IInterface>(() => called = true);
            this.
                        _provider.RegisterType<IInterface, Concrete>();
            Assert.True(called);
        }

        [Fact]
        public void CallbackWhenRegisteredThrowsArgumentNullExceptionWhenCalledWithNoTypeOrActionArgument()
        {
            Assert.Throws<ArgumentNullException>(() => this._provider.CallbackWhenRegistered(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => this._provider.CallbackWhenRegistered(typeof(object), null));
        }

        [Fact]
        public void CanResolveReturnsFalseWhenNoMatchingTypeIsRegistered()
        {
            Assert.False(this._provider.CanResolve<object>());
        }

        [Fact]
        public void CanResolveReturnsTrueWhenMatchingTypeIsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(this._container);

            Assert.True(this._provider.CanResolve<object>());
        }

        [Fact]
        public void CanResolveThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.Throws<ArgumentNullException>(() => this._provider.CanResolve(null));
        }

        public void Dispose()
        {
            this._provider.Dispose();
        }

        [Fact]
        public void GetSingletonReturnsSingletonIfTypeRegisteredAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).SingleInstance();
            builder.Update(this._container);

            Assert.IsType<object>(this._provider.GetSingleton<object>());
            Assert.Same(this._provider.GetSingleton<object>(), this._provider.GetSingleton<object>());
        }

        [Fact]
        public void GetSingletonThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.Throws<ArgumentNullException>(() => this._provider.GetSingleton(null));
        }

        [Fact]
        public void GetSingletonThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.Throws<ComponentNotRegisteredException>(() => this._provider.GetSingleton<object>());
        }

        [Fact]
        public void GetSingletonThrowsDependencyResolutionExceptionIfTypeRegisteredButNotAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(this._container);

            Assert.Throws<DependencyResolutionException>(() => this._provider.GetSingleton<object>());
        }

        [Fact]
        public void RegisterSingletoneThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterSingleton((IInterface)null));
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterSingleton((Func<IInterface>)null));
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterSingleton(null, new object()));
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterSingleton(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterSingleton(typeof(object), null));
        }

        [Fact]
        public void RegisterSingletonRegistersConcreteTypeAsSingletonAgainstInterface()
        {
            var concreteViaFunc = new Concrete();
            this._provider.RegisterSingleton<IInterface>(() => concreteViaFunc);
            Assert.Equal(concreteViaFunc, this._provider.Resolve<IInterface>());
            Assert.Same(this._provider.Resolve<IInterface>(), this._provider.Resolve<IInterface>());

            var concreteInstance = new Concrete();
            this._provider.RegisterSingleton<IInterface>(concreteInstance);
            Assert.Equal(concreteInstance, this._provider.Resolve<IInterface>());
            Assert.Same(this._provider.Resolve<IInterface>(), this._provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeRegistersConcreteTypeAgainstInterface()
        {
            this._provider.RegisterType<IInterface, Concrete>();
            var instance = this._provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, this._provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoFromOrToTypeArgument()
        {
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterType(null, typeof(object)));
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterType(typeof(object), (Type)null));
        }

        [Fact]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterType((Func<object>)null));
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterType(null, () => new object()));
            Assert.Throws<ArgumentNullException>(() => this._provider.RegisterType(typeof(object), (Func<object>)null));
        }

        [Fact]
        public void RegisterTypeWithDelegateAndTypeParameterRegistersConcreteTypeAgainstInterface()
        {
            this._provider.RegisterType(typeof(IInterface), () => new Concrete());
            var instance = this._provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, this._provider.Resolve<IInterface>());
        }

        [Fact]
        public void RegisterTypeWithDelegateRegistersConcreteTypeAgainstInterface()
        {
            this._provider.RegisterType<IInterface>(() => new Concrete());
            var instance = this._provider.Resolve<IInterface>();
            Assert.IsType<Concrete>(instance);
            Assert.NotSame(instance, this._provider.Resolve<IInterface>());
            this.
                        _provider.RegisterType(typeof(IInterface), () => new Concrete());
            Assert.NotSame(this._provider.Resolve<IInterface>(), this._provider.Resolve<IInterface>());
        }

        [Fact]
        public void ResolveCreateAndIoCConstructReturnsRegisteredType()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(this._container);

            Assert.IsType<object>(this._provider.Resolve<object>());
            Assert.IsType<object>(this._provider.Create<object>());
            Assert.IsType<object>(this._provider.IoCConstruct<object>());
        }

        [Fact]
        public void ResolveCreateAndIoCConstructThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.Throws<ArgumentNullException>(() => this._provider.Resolve(null));
            Assert.Throws<ArgumentNullException>(() => this._provider.Create(null));
            Assert.Throws<ArgumentNullException>(() => this._provider.IoCConstruct(null));
        }

        [Fact]
        public void ResolveCreateAndIoCConstructThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.Throws<ComponentNotRegisteredException>(() => this._provider.Resolve<object>());
            Assert.Throws<ComponentNotRegisteredException>(() => this._provider.Create<object>());
            Assert.Throws<ComponentNotRegisteredException>(() => this._provider.IoCConstruct<object>());
        }

        [Fact]
        public void TryResolveResolvesOutParameterWhenMatchingTypeRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(this._container);

            object foo;
            bool success = this._provider.TryResolve(out foo);

            Assert.IsType<object>(foo);
            Assert.True(success);
        }

        private class Concrete : IInterface
        {
        }
    }
}
