//using System;
//using Autofac.Core;
//using MvvmCross.Platform;
//using MvvmCross.Platform.Exceptions;
//using MvvmCross.Platform.IoC;
//using Xunit;

//namespace Autofac.Extras.MvvmCross.Test
//{
//    public class AutofacMvxIocProviderPropertyInjectionFixture : IDisposable
//    {
//        IContainer _container;
//        AutofacMvxIocProvider _provider;

        
//        public AutofacMvxIocProviderPropertyInjectionFixture()
//        {
//            _container = new ContainerBuilder().Build();
//        }
        
//        public void Dispose()
//        {
//            _provider.Dispose();
//        }


//        [Fact]
//        public void InjectsPropertiesIfEnabled()
//        {
//            // Arrange
//            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties});
//            Mvx.RegisterType<IInterface, Concrete>();
//            Mvx.RegisterType<IInterface2, Concrete2>();
            
//            // Act
//            var obj = Mvx.IocConstruct<HasDependantProperty>();
            
//            // Assert
//            Assert.NotNull(obj);
//            Assert.NotNull(obj.Dependency);
//            Assert.NotNull(obj.MarkedDependency);
//        }

//        [Fact]
//        public void InjectsOnlyMarkedPropertiesIfEnabled()
//        {
//            // Arrange
//            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties });
//            Mvx.RegisterType<IInterface, Concrete>();
//            Mvx.RegisterType<IInterface2, Concrete2>();

//            // Act
//            var obj = Mvx.IocConstruct<HasDependantProperty>();

//            // Assert
//            Assert.NotNull(obj);
//            Assert.Null(obj.Dependency);
//            Assert.NotNull(obj.MarkedDependency);
//        }

//        [Fact]
//        public void InjectsOnlyMarkedPropertiesIfEnabled_Lazy()
//        {
//            // Arrange
//            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties });
//            Mvx.RegisterType<IInterface, Concrete>();
//            Mvx.RegisterType<IInterface2, Concrete2>();
//            Mvx.RegisterSingleton<IHasDependantProperty>(Mvx.IocConstruct<HasDependantProperty>);

//            // Act
//            var obj = Mvx.Resolve<IHasDependantProperty>();

//            // Assert
//            Assert.NotNull(obj);
//            Assert.Null(obj.Dependency);
//            Assert.NotNull(obj.MarkedDependency);
//        }


//        [Fact]
//        public void InjectsOnlyMarkedProperties_WithCustomAttribute_IfEnabled()
//        {
//            // Arrange
//            _provider = new AutofacMvxIocProvider(_container, new AutofacPropertyInjectorOptions()
//            {
//                InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
//                CustomInjectorAttributeType = typeof(MyInjectionAttribute),
//            });
//            Mvx.RegisterType<IInterface, Concrete>();
//            Mvx.RegisterType<IInterface2, Concrete2>();
//            // Act
//            var obj = Mvx.IocConstruct<HasDependantProperty>();

//            // Assert
//            Assert.NotNull(obj);
//            Assert.NotNull(obj.Dependency);
//            Assert.NotNull(obj.MarkedDependency);
//        }


//        [Fact]
//        public void InjectsOnlyMarkedProperties_WithCustomAttribute_IfEnabled_Lazy()
//        {
//            // Arrange
//            _provider = new AutofacMvxIocProvider(_container, new AutofacPropertyInjectorOptions()
//            {
//                InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
//                CustomInjectorAttributeType = typeof(MyInjectionAttribute),
//            });
//            Mvx.RegisterType<IInterface, Concrete>();
//            Mvx.RegisterType<IInterface2, Concrete2>();
//            Mvx.RegisterSingleton<IHasDependantProperty>(Mvx.IocConstruct<HasDependantProperty>);

//            // Act
//            var obj = Mvx.Resolve<IHasDependantProperty>();

//            // Assert
//            Assert.NotNull(obj);
//            Assert.NotNull(obj.Dependency);
//            Assert.NotNull(obj.MarkedDependency);
//        }


//        [Fact]
//        public void IgnoresNonResolvableProperty()
//        {
//            // Arrange
//            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties });
            
//            // Act
//            var obj = Mvx.IocConstruct<HasDependantProperty>();

//            // Assert
//            Assert.NotNull(obj);
//            Assert.Null(obj.Dependency);
//            Assert.Null(obj.MarkedDependency);
//        }

//        [Fact]
//        public void IfSetInOptions_OnNonResolvableProperty_Throws()
//        {
//            // Arrange
//            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions()
//            {
//                ThrowIfPropertyInjectionFails = true,
//                InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties
//            });

//            // Act
//            MvxIoCResolveException exception = null;
//            try
//            {
//                var obj = Mvx.IocConstruct<HasDependantProperty>();
//            }
//            catch (MvxIoCResolveException x)
//            {
//                exception = x;
//            }

//            // Assert
//            Assert.NotNull(exception);
//            Assert.True(exception.InnerException is DependencyResolutionException, "Autofac exception is not forwarded!");
//        }

//        private interface IInterface
//        {
//        }

//        private class Concrete : IInterface
//        {
//        }

//        private interface IInterface2
//        {
//        }


//        private class Concrete2 : IInterface2
//        {
//        }

//        private class MyInjectionAttribute : Attribute
//        {
            
//        }

//        private class HasDependantProperty : IHasDependantProperty
//        {
//            [MyInjection]
//            public IInterface Dependency { get; set; }

//            [MvxInject]
//            public IInterface2 MarkedDependency { get; set; }
//        }
//        private interface IHasDependantProperty
//        {
//            IInterface Dependency { get; set; }
//            IInterface2 MarkedDependency { get; set; }
//        }
//    }

//}
