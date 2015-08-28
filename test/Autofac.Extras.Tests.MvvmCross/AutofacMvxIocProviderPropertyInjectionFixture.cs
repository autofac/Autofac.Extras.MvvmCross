using System;
using System.Configuration;
using Autofac.Extras.MvvmCross;
using Autofac.Core.Registration;
using Autofac.Core;
using Cirrious.CrossCore;
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
            _provider = new AutofacMvxIocProvider(_container, true);
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
            Mvx.RegisterType<IInterface, Concrete>();
            
            // Act
            var obj = Mvx.IocConstruct<HasDependantProperty>();
            
            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Dependency);
        }

        private interface IInterface
        {
        }

        private class Concrete : IInterface
        {
        }

        private class HasDependantProperty
        {
            public IInterface Dependency { get; set; }
        }
    }
}
