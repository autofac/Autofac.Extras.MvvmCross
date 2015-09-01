using System;
using Cirrious.CrossCore.IoC;

namespace Autofac.Extras.MvvmCross
{
    /// <summary>
    /// Autofac property injection options.
    /// </summary>
    public interface IAutofacPropertyInjectorOptions : IMvxPropertyInjectorOptions
    {
        /// <summary>
        /// An additional attribute used to mark a property as requiring property injection.
        /// </summary>
        Type CustomInjectorAttributeType { get; set; }
    }

    /// <summary>
    /// Autofac property injection options.
    /// </summary>
    public class AutofacPropertyInjectionOptions : MvxPropertyInjectorOptions, IAutofacPropertyInjectorOptions
    {
        /// <summary>
        /// An additional attribute used to mark a property as requiring property injection.
        /// </summary>
        public Type CustomInjectorAttributeType { get; set; }
    }
}
