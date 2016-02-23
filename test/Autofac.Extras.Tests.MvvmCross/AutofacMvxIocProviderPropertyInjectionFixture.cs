using System;
using System.Configuration;
using Autofac.Extras.MvvmCross;
using Autofac.Core.Registration;
using Autofac.Core;
using MvvmCross.Platform;
using MvvmCross.Platform.Exceptions;
using MvvmCross.Platform.IoC;
using NUnit.Framework;

namespace Autofac.Extras.Tests.MvvmCross
{
    [TestFixture]
    public class AutofacMvxIocProviderPropertyInjectionFixture
    {
        IContainer _container;
        AutofacMvxIocProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _container = new ContainerBuilder().Build();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }


        [Test]
        public void InjectsPropertiesIfEnabled()
        {
            // Arrange
            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties});
            Mvx.RegisterType<IInterface, Concrete>();
            Mvx.RegisterType<IInterface2, Concrete2>();
            
            // Act
            var obj = Mvx.IocConstruct<HasDependantProperty>();
            
            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Dependency);
            Assert.IsNotNull(obj.MarkedDependency);
        }

        [Test]
        public void InjectsOnlyMarkedPropertiesIfEnabled()
        {
            // Arrange
            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties });
            Mvx.RegisterType<IInterface, Concrete>();
            Mvx.RegisterType<IInterface2, Concrete2>();

            // Act
            var obj = Mvx.IocConstruct<HasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Dependency);
            Assert.IsNotNull(obj.MarkedDependency);
        }

        [Test]
        public void InjectsOnlyMarkedPropertiesIfEnabled_Lazy()
        {
            // Arrange
            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties });
            Mvx.RegisterType<IInterface, Concrete>();
            Mvx.RegisterType<IInterface2, Concrete2>();
            Mvx.RegisterSingleton<IHasDependantProperty>(Mvx.IocConstruct<HasDependantProperty>);

            // Act
            var obj = Mvx.Resolve<IHasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Dependency);
            Assert.IsNotNull(obj.MarkedDependency);
        }


        [Test]
        public void InjectsOnlyMarkedProperties_WithCustomAttribute_IfEnabled()
        {
            // Arrange
            _provider = new AutofacMvxIocProvider(_container, new AutofacPropertyInjectionOptions()
            {
                InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                CustomInjectorAttributeType = typeof(MyInjectionAttribute),
            });
            Mvx.RegisterType<IInterface, Concrete>();
            Mvx.RegisterType<IInterface2, Concrete2>();
            // Act
            var obj = Mvx.IocConstruct<HasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Dependency);
            Assert.IsNotNull(obj.MarkedDependency);
        }


        [Test]
        public void InjectsOnlyMarkedProperties_WithCustomAttribute_IfEnabled_Lazy()
        {
            // Arrange
            _provider = new AutofacMvxIocProvider(_container, new AutofacPropertyInjectionOptions()
            {
                InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                CustomInjectorAttributeType = typeof(MyInjectionAttribute),
            });
            Mvx.RegisterType<IInterface, Concrete>();
            Mvx.RegisterType<IInterface2, Concrete2>();
            Mvx.RegisterSingleton<IHasDependantProperty>(Mvx.IocConstruct<HasDependantProperty>);

            // Act
            var obj = Mvx.Resolve<IHasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Dependency);
            Assert.IsNotNull(obj.MarkedDependency);
        }


        [Test]
        public void IgnoresNonResolvableProperty()
        {
            // Arrange
            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties });
            
            // Act
            var obj = Mvx.IocConstruct<HasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Dependency);
            Assert.IsNull(obj.MarkedDependency);
        }

        [Test]
        public void IfSetInOptions_OnNonResolvableProperty_Throws()
        {
            // Arrange
            _provider = new AutofacMvxIocProvider(_container, new MvxPropertyInjectorOptions()
            {
                ThrowIfPropertyInjectionFails = true,
                InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties
            });

            // Act
            MvxIoCResolveException exception = null;
            try
            {
                var obj = Mvx.IocConstruct<HasDependantProperty>();
            }
            catch (MvxIoCResolveException x)
            {
                exception = x;
            }

            // Assert
            Assert.IsNotNull(exception, "Exception expected");
            Assert.IsTrue(exception.InnerException is DependencyResolutionException, "Autofac exception is not forwarded!");
        }

        private interface IInterface
        {
        }

        private class Concrete : IInterface
        {
        }

        private interface IInterface2
        {
        }


        private class Concrete2 : IInterface2
        {
        }

        private class MyInjectionAttribute : Attribute
        {
            
        }

        private class HasDependantProperty : IHasDependantProperty
        {
            [MyInjectionAttribute]
            public IInterface Dependency { get; set; }

            [MvxInject]
            public IInterface2 MarkedDependency { get; set; }
        }
        private interface IHasDependantProperty
        {
            IInterface Dependency { get; set; }
            IInterface2 MarkedDependency { get; set; }
        }
    }

}
