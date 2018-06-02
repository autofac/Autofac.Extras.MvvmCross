// <copyright file="ChildAutofacMvxIocProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Autofac.Extras.MvvmCross
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Autofac.Builder;
    using Autofac.Core;
    using Autofac.Core.Lifetime;
    using Autofac.Core.Registration;
    using global::MvvmCross.Base;
    using global::MvvmCross.Exceptions;
    using global::MvvmCross.IoC;

    /// <summary>
    /// Child inversion of control provider for the MvvmCross framework backed by Autofac.
    /// This class is used as the implementation for <see cref="AutofacMvxIocProvider"/> as well as for creating child containers.
    /// </summary>
    public class ChildAutofacMvxIocProvider : IMvxIoCProvider, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildAutofacMvxIocProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which dependencies should be resolved.
        /// </param>
        /// <param name="propertyInjectionOptions">propertyInjectionOptions</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> is <see langword="null"/>.
        /// </exception>
        public ChildAutofacMvxIocProvider(ILifetimeScope container, IMvxPropertyInjectorOptions propertyInjectionOptions)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this.Container = container;
            this.PropertyInjectionOptions = propertyInjectionOptions ?? throw new ArgumentNullException(nameof(propertyInjectionOptions));
            this.PropertyInjectionEnabled = propertyInjectionOptions.InjectIntoProperties != MvxPropertyInjection.None;

            if (propertyInjectionOptions.ThrowIfPropertyInjectionFails)
            {
                throw new NotSupportedException("Autofac does not support throwing an exception in case a service could not be injected into a property!");
            }

            this.Container = container;
            this.PropertyInjectionOptions = propertyInjectionOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildAutofacMvxIocProvider"/> class.
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
        public ChildAutofacMvxIocProvider(ILifetimeScope container, IAutofacPropertyInjectorOptions propertyInjectorOptions)
            : this(container, (IMvxPropertyInjectorOptions)propertyInjectorOptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildAutofacMvxIocProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which dependencies should be resolved.
        /// </param>
        public ChildAutofacMvxIocProvider(ILifetimeScope container)
            : this(container, new MvxPropertyInjectorOptions())
        {
        }

        /// <summary>
        /// Gets a container.
        /// </summary>
        public ILifetimeScope Container { get; private set; }

        /// <summary>
        /// Gets a value indicating whether if property injection is enabled.
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
            if (!this.Container.IsRegistered(type))
            {
                this.RegisterType(type, type);
            }

            return this.Resolve(type);
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

#pragma warning disable CS0618 // Type or member is obsolete. To be fixed as part of https://github.com/autofac/Autofac.Extras.MvvmCross/issues/8
            cb.Update(this.Container.ComponentRegistry);
#pragma warning restore CS0618 // Type or member is obsolete
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
            var regType = cb.RegisterType(type).As(tInterface).AsSelf().SingleInstance();
            if (this.PropertyInjectionEnabled)
            {
                this.SetPropertyInjection(regType);
            }

            var regInterface = cb.Register(cc => theConstructor()).As(tInterface).AsSelf().SingleInstance();
            if (this.PropertyInjectionEnabled)
            {
                this.SetPropertyInjection(regInterface);
            }

#pragma warning disable CS0618 // Type or member is obsolete. To be fixed as part of https://github.com/autofac/Autofac.Extras.MvvmCross/issues/8
            cb.Update(this.Container.ComponentRegistry);
#pragma warning restore CS0618 // Type or member is obsolete
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
            var x = cb.Register(c => constructor()).AsSelf();
            if (this.PropertyInjectionEnabled)
            {
                this.SetPropertyInjection(x);
            }

#pragma warning disable CS0618 // Type or member is obsolete. To be fixed as part of https://github.com/autofac/Autofac.Extras.MvvmCross/issues/8
            cb.Update(this.Container.ComponentRegistry);
#pragma warning restore CS0618 // Type or member is obsolete
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
            if (this.PropertyInjectionEnabled)
            {
                this.SetPropertyInjection(x);
            }

#pragma warning disable CS0618 // Type or member is obsolete. To be fixed as part of https://github.com/autofac/Autofac.Extras.MvvmCross/issues/8
            cb.Update(this.Container.ComponentRegistry);
#pragma warning restore CS0618 // Type or member is obsolete
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
            if (this.PropertyInjectionEnabled)
            {
                this.SetPropertyInjection(x);
            }

#pragma warning disable CS0618 // Type or member is obsolete. To be fixed as part of https://github.com/autofac/Autofac.Extras.MvvmCross/issues/8
            cb.Update(this.Container.ComponentRegistry);
#pragma warning restore CS0618 // Type or member is obsolete
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
                return this.Container.Resolve(type);
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
        /// Returns a new child container off the current container.
        /// </summary>
        /// <returns>Returns a new child container.</returns>
        public IMvxIoCProvider CreateChildContainer()
        {
            var childContainer = this.Container.BeginLifetimeScope();
            return new ChildAutofacMvxIocProvider(childContainer);
        }

        /// <summary>
        /// Disposes the container.
        /// </summary>
        public void Dispose()
        {
            if (this.Container != null)
            {
                this.Container.Dispose();
                this.Container = null;
            }
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
        public virtual void SetPropertyInjection<TLimit, TActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var options = this.PropertyInjectionOptions as IAutofacPropertyInjectorOptions;
            var mode = this.PropertyInjectionOptions.InjectIntoProperties;

            if (mode == MvxPropertyInjection.None)
            {
                return;
            }

            if (mode == MvxPropertyInjection.MvxInjectInterfaceProperties)
            {
                registration.PropertiesAutowired(this.SelectAllMvxInject);
            }
            else if (options?.PropertyInjectionSelector == null)
            {
                registration.PropertiesAutowired();
            }
            else
            {
                registration.PropertiesAutowired(options.PropertyInjectionSelector);
            }
        }

        private bool SelectAllMvxInject(PropertyInfo pi, object obj)
        {
            var options = this.PropertyInjectionOptions as IAutofacPropertyInjectorOptions;
            var type = typeof(MvxInjectAttribute);

            // if there is the custom or an MvxInject attribute on the property, accept
            var accept = pi.GetCustomAttributes(type).Any();

            // and if there is also a selector, call the selector as well
            if (accept && options?.PropertyInjectionSelector != null)
            {
                return options.PropertyInjectionSelector.InjectProperty(pi, obj);
            }

            return accept;
        }
    }
}
