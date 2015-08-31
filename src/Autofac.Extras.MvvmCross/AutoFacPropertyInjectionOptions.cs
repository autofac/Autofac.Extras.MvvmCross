using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore.IoC;

namespace Autofac.Extras.MvvmCross
{
    /// <summary>
    /// Adds some additional features to autofacpropertyinjection
    /// </summary>
    public interface IAutofacPropertyInjectorOptions : IMvxPropertyInjectorOptions
    {
        /// <summary>
        /// An additiona attribute to use to mark a property to be injected by Property Injection
        /// </summary>
        Type CustomInjectorAttributeType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AutoFacPropertyInjectionOptions : MvxPropertyInjectorOptions, IAutofacPropertyInjectorOptions
    {
        /// <summary>
        /// An additiona attribute to use to mark a property to be injected by Property Injection
        /// </summary>
        public Type CustomInjectorAttributeType { get; set; }
    }
}
