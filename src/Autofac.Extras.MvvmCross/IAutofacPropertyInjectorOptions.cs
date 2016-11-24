using Autofac.Core;
using MvvmCross.Platform.IoC;

namespace Autofac.Extras.MvvmCross
{
    /// <summary>
    /// Defines additional customization for Autofac property injection.
    /// </summary>
    public interface IAutofacPropertyInjectorOptions : IMvxPropertyInjectorOptions
    {
        /// <summary>
        /// Gets or sets the mechanism that determines properties to inject.
        /// </summary>
        /// <value>
        /// An <see cref="IPropertySelector"/> that allows for custom determination of
        /// which properties to inject when property injection is enabled.
        /// </value>
        IPropertySelector PropertyInjectionSelector { get; set; }
    }
}
