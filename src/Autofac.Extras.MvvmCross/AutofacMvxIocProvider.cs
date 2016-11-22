// This software is part of the Autofac IoC container
// Copyright © 2014 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.IoC;

namespace Autofac.Extras.MvvmCross
{
    /// <summary>
    /// Inversion of control provider for the MvvmCross framework backed by Autofac.
    /// </summary>
    [SuppressMessage("CA2213", "CA2213", Justification = "The container gets disposed by the owner.")]
    public class AutofacMvxIocProvider : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider
    {
        readonly IContainer _container;
        private Type _customInjectorAttributeType;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacMvxIocProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which dependencies should be resolved.
        /// </param>
        /// <param name="propertyInjectionOptions"></param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> is <see langword="null"/>.
        /// </exception>
        public AutofacMvxIocProvider(IContainer container, IMvxPropertyInjectorOptions propertyInjectionOptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacMvxIocProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which dependencies should be resolved.
        /// </param>
        /// <param name="propertyInjectorOptions">
        /// An <see cref="IAutofacPropertyInjectorOptions"/> that defines how property
        /// injection should be handled.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="propertyInjectorOptions" /> is <see langword="null"/>.
        /// </exception>
        public AutofacMvxIocProvider(IContainer container, IAutofacPropertyInjectorOptions propertyInjectorOptions)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (propertyInjectionOptions == null) 
                throw new ArgumentNullException("propertyInjectionOptions");

            _container = container;
            PropertyInjectionOptions = propertyInjectionOptions;
            PropertyInjectionEnabled = propertyInjectionOptions.InjectIntoProperties != MvxPropertyInjection.None;

            var autofacOptions = propertyInjectionOptions as IAutofacPropertyInjectorOptions;
            if (autofacOptions != null && autofacOptions.CustomInjectorAttributeType != null && autofacOptions.CustomInjectorAttributeType.IsAssignableTo<Attribute>())
                _customInjectorAttributeType = autofacOptions.CustomInjectorAttributeType;
        }

            this.Container = container;
            this.PropertyInjectorOptions = propertyInjectorOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacMvxIocProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which dependencies should be resolved.
        /// </param>
        public AutofacMvxIocProvider(IContainer container)
            : this(container, new MvxPropertyInjectorOptions())
        {
        }

        /// <summary>
        /// Determines if property injection is enabled.
        /// </summary>
        public bool PropertyInjectionEnabled { get; private set; }

        /// <summary>
        /// Gets the property injection options.
        /// </summary>
        public IMvxPropertyInjectorOptions PropertyInjectionOptions { get; private set; }

        /// <summary>
        /// Registers an action to occur when a specific type is registered.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> that should raise the callback when registered.
        /// </typeparam>
        /// <param name="action">
        /// The <see cref="Action"/> to call when the specified type is registered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public virtual void CallbackWhenRegistered<T>(Action action)
        {
            this.CallbackWhenRegistered(typeof(T), action);
        }

        /// <summary>
        /// Registers an action to occur when a specific type is registered.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> that should raise the callback when registered.
        /// </param>
        /// <param name="action">
        /// The <see cref="Action"/> to call when the specified type is registered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> or <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public virtual void CallbackWhenRegistered(Type type, Action action)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            this.Container.ComponentRegistry.Registered += (sender, args) =>
            {
                if (args.ComponentRegistration.Services.OfType<TypedService>().Any(x => x.ServiceType == type))
                {
                    action();
                }
            };
        }

        /// <summary>
        /// Determines whether an instance of a specified type can be resolved.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> to check for resolution.
        /// </typeparam>
        /// <returns>
        /// <see langword="true"/> if the instance can be resolved; <see langword="false"/> if not.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Technically this implementation determines if the type <typeparamref name="T"/>
        /// is registered with the Autofac container. This method returning
        /// <see langword="true"/> does not guarantee that no exception will
        /// be thrown if the type is resolved but there
        /// are missing dependencies for constructing the instance.
        /// </para>
        /// </remarks>
        public virtual bool CanResolve<T>()
            where T : class
        {
            return this.CanResolve(typeof(T));
        }

        /// <summary>
        /// Determines whether an instance of a specified type can be resolved.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> to check for resolution.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the instance can be resolved; <see langword="false"/> if not.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Technically this implementation determines if the <paramref name="type"/>
        /// is registered with the Autofac container. This method returning
        /// <see langword="true"/> does not guarantee that no exception will
        /// be thrown if the <paramref name="type"/> is resolved but there
        /// are missing dependencies for constructing the instance.
        /// </para>
        /// </remarks>
        public virtual bool CanResolve(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return this.Container.IsRegistered(type);
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public virtual T Create<T>()
            where T : class
        {
            return (T)this.Create(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public virtual object Create(Type type)
        {
            return this.Resolve(type);
        }

        /// <summary>
        /// Resolves a singleton service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved singleton instance of type <typeparamref name="T"/>.
        /// </returns>
        public virtual T GetSingleton<T>()
            where T : class
        {
            return (T)this.GetSingleton(typeof(T));
        }

        /// <summary>
        /// Resolves a singleton service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved singleton instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="DependencyResolutionException">
        /// Thrown if the <paramref name="type"/> is not registered as a singleton.
        /// </exception>
        public virtual object GetSingleton(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var service = new TypedService(type);
            IComponentRegistration registration;
            if (!this.Container.ComponentRegistry.TryGetRegistration(service, out registration))
            {
                throw new ComponentNotRegisteredException(service);
            }

            if (registration.Sharing != InstanceSharing.Shared || !(registration.Lifetime is RootScopeLifetime))
            {
                // Ensure the dependency is registered as a singleton WITHOUT resolving the dependency twice.
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, AutofacMvxIocProviderResources.TypeNotRegisteredAsSingleton, type));
            }

            return this.Resolve(type);
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public virtual T IoCConstruct<T>()
            where T : class
        {
            return (T)this.IoCConstruct(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public virtual object IoCConstruct(Type type)
        {
            if (!_container.IsRegistered(type))
                RegisterType(type, type);

            return Resolve(type);
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="theObject">The instance to register.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="theObject"/> is <see langword="null"/>.
        /// </exception>
        public virtual void RegisterSingleton<TInterface>(TInterface theObject)
            where TInterface : class
        {
            this.RegisterSingleton(typeof(TInterface), theObject);
        }

        /// <summary>
        /// Register a delegate as a singleton component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="theConstructor">
        /// The construction function/delegate to call to create the singleton.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="theConstructor"/> is <see langword="null"/>.
        /// </exception>
        public virtual void RegisterSingleton<TInterface>(Func<TInterface> theConstructor)
            where TInterface : class
        {
            this.RegisterSingleton(typeof(TInterface), theConstructor);
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <param name="tInterface">
        /// The type of the instance. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="theObject">The instance to register.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tInterface"/> or <paramref name="theObject"/> is <see langword="null"/>.
        /// </exception>
        public virtual void RegisterSingleton(Type tInterface, object theObject)
        {
            if (tInterface == null)
            {
                throw new ArgumentNullException(nameof(tInterface));
            }

            if (theObject == null)
            {
                throw new ArgumentNullException(nameof(theObject));
            }

            var cb = new ContainerBuilder();

            // You can't inject properties on a pre-constructed instance.
            cb.RegisterInstance(theObject).As(tInterface).AsSelf().SingleInstance();

            cb.Update(_container);
        }

        /// <summary>
        /// Register a delegate as a singleton component.
        /// </summary>
        /// <param name="tInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="theConstructor">
        /// The construction function/delegate to call to create the singleton.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tInterface"/> or <paramref name="theConstructor"/> is <see langword="null"/>.
        /// </exception>
        public virtual void RegisterSingleton(Type tInterface, Func<object> theConstructor)
        {
            if (tInterface == null)
            {
                throw new ArgumentNullException(nameof(tInterface));
            }

            if (theConstructor == null)
            {
                throw new ArgumentNullException(nameof(theConstructor));
            }

            var cb = new ContainerBuilder();

            var type = theConstructor.GetMethodInfo().ReturnType;
            var x = cb.RegisterType(type).As(tInterface).AsSelf().SingleInstance();
            if (PropertyInjectionEnabled)
                EnablePropertyInjection(type, x);

            var y = cb.Register(cc => theConstructor()).As(tInterface).AsSelf().SingleInstance();
            if (PropertyInjectionEnabled)
                EnablePropertyInjection(tInterface, y);


            cb.Update(_container);
        }

        /// <summary>
        /// Registers a reflection-based component to service mapping.
        /// </summary>
        /// <typeparam name="TFrom">
        /// The component type that implements the service to register.
        /// </typeparam>
        /// <typeparam name="TTo">
        /// The service type that will be resolved from the container.
        /// </typeparam>
        /// <remarks>
        /// <para>
        /// This method updates the container to include a new reflection-based
        /// registration that maps <typeparamref name="TFrom"/> to its own implementing
        /// type as well as to the service type <typeparamref name="TTo"/>.
        /// </para>
        /// </remarks>
        public virtual void RegisterType<TFrom, TTo>()
            where TFrom : class
            where TTo : class, TFrom
        {
            this.RegisterType(typeof(TFrom), typeof(TTo));
        }

        /// <summary>
        /// Register a delegate for creating a component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="constructor">
        /// The construction function/delegate to call to create the instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="constructor"/> is <see langword="null"/>.
        /// </exception>
        public virtual void RegisterType<TInterface>(Func<TInterface> constructor)
            where TInterface : class
        {
            if (constructor == null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }

            var cb = new ContainerBuilder();
            var type = constructor.GetMethodInfo().ReturnType;
            var x = cb.Register(c => constructor()).AsSelf();
            if (PropertyInjectionEnabled)
                EnablePropertyInjection(type, x);

            cb.Update(_container);
        }

        /// <summary>
        /// Register a delegate for creating a component.
        /// </summary>
        /// <param name="t">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="constructor">
        /// The construction function/delegate to call to create the instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="t"/> or <paramref name="constructor"/> is <see langword="null"/>.
        /// </exception>
        public virtual void RegisterType(Type t, Func<object> constructor)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (constructor == null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }

            var cb = new ContainerBuilder();
            var type = constructor.GetMethodInfo().ReturnType;
            var x = cb.Register(c => constructor()).As(t).AsSelf();
            if (PropertyInjectionEnabled)
                EnablePropertyInjection(type, x);

            cb.Update(_container);
        }

        /// <summary>
        /// Registers a reflection-based component to service mapping.
        /// </summary>
        /// <param name="tFrom">
        /// The component type that implements the service to register.
        /// </param>
        /// <param name="tTo">
        /// The service type that will be resolved from the container.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tFrom"/> or <paramref name="tTo"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method updates the container to include a new reflection-based
        /// registration that maps <paramref name="tFrom"/> to its own implementing
        /// type as well as to the service type <paramref name="tTo"/>.
        /// </para>
        /// </remarks>
        public virtual void RegisterType(Type tFrom, Type tTo)
        {
            if (tFrom == null)
            {
                throw new ArgumentNullException(nameof(tFrom));
            }

            if (tTo == null)
            {
                throw new ArgumentNullException(nameof(tTo));
            }

            var cb = new ContainerBuilder();
            var x = cb.RegisterType(tTo).As(tFrom).AsSelf();
            if (PropertyInjectionEnabled)
                EnablePropertyInjection(tFrom, x);

            cb.Update(_container);
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public virtual T Resolve<T>()
            where T : class
        {
            return (T)this.Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public virtual object Resolve(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            try
            {
                return _container.Resolve(type);
            }
            catch (DependencyResolutionException ex)
            {
                throw new MvxIoCResolveException(ex, "Could not resolve {0}. See InnerException for details", type.FullName);
            }
        }

        /// <summary>
        /// Tries to retrieve a service of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The service <see cref="System.Type"/> to resolve.
        /// </typeparam>
        /// <param name="resolved">
        /// The resulting component instance providing the service, or default(T) if resolution is not possible.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a component providing the service is available; <see langword="false"/> if not.
        /// </returns>
        public virtual bool TryResolve<T>(out T resolved)
            where T : class
        {
            return this.Container.TryResolve(out resolved);
        }

        /// <summary>
        /// Tries to retrieve a service of a specified type.
        /// </summary>
        /// <param name="type">
        /// The service <see cref="System.Type"/> to resolve.
        /// </param>
        /// <param name="resolved">
        /// The resulting component instance providing the service, or <see langword="null"/> if resolution is not possible.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a component providing the service is available; <see langword="false"/> if not.
        /// </returns>
        public virtual bool TryResolve(Type type, out object resolved)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return this.Container.TryResolve(type, out resolved);
        }

        /// <summary>
        /// Sets the property injection on a registration based on options.
        /// </summary>
        /// <typeparam name="TLimit">The most specific type to which instances of the registration
        /// can be cast.</typeparam>
        /// <typeparam name="TActivatorData">Activator builder type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
        /// <param name="registration">The registration to update.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        protected virtual void SetPropertyInjection<TLimit, TActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (this.PropertyInjectorOptions.InjectIntoProperties == MvxPropertyInjection.None)
            {
                return;
            }

            if (this.PropertyInjectorOptions.PropertyInjectionSelector == null)
            {
                registration.PropertiesAutowired();
            }
            else
            {
                registration.PropertiesAutowired(this.PropertyInjectorOptions.PropertyInjectionSelector);
            }
        }

        private void EnablePropertyInjection(Type type, IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registrationBuilder)
        {
            var injectableProperties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(p => p.PropertyType.GetTypeInfo().IsInterface)
                .Where(p => p.IsConventional())
                .Where(p => p.CanWrite);

            switch (PropertyInjectionOptions.InjectIntoProperties)
            {
                case MvxPropertyInjection.MvxInjectInterfaceProperties:
                    if(_customInjectorAttributeType != null)
                        injectableProperties = injectableProperties
                            .Where(p => p.GetCustomAttributes(typeof(MvxInjectAttribute), false).Any() ||
                                        p.GetCustomAttributes(_customInjectorAttributeType, false).Any());
                    else
                        injectableProperties = injectableProperties
                            .Where(p => p.GetCustomAttributes(typeof(MvxInjectAttribute), false).Any());
                    break;
                case MvxPropertyInjection.AllInterfaceProperties:
                    break;
                case MvxPropertyInjection.None:
                    Mvx.Error("Internal error - should not call FindInjectableProperties with MvxPropertyInjection.None");
                    injectableProperties = new PropertyInfo[0];
                    break;
                default:
                    throw new MvxException("unknown option for InjectIntoProperties {0}", PropertyInjectionOptions.InjectIntoProperties);
            }

            foreach (var prop in injectableProperties)
            {
                var property = prop;
                registrationBuilder.OnActivating(e =>
                {
                    object dependency = null;
                    try
                    {
                        dependency = e.Context.Resolve(property.PropertyType);
                    }
                    catch (Exception x)
                    {
                        if (PropertyInjectionOptions.ThrowIfPropertyInjectionFails)
                        {
                            throw;
                        }
                        else
                        {
                            Mvx.Warning("Could not resolve property {0} of type {1} on {2}", property.Name, property.PropertyType.FullName, type.FullName);
                        }
                    }

                    if(dependency != null)
                        property.SetValue(e.Instance, dependency);
                });
            }
        }

    }
}
