using System;
using Nito.AsyncEx;

namespace Autofac.Extras.MvvmCross.Test
{
    /// <summary>
    /// Base class for all tests, as the AutofacMvxIocProvider must be an "MvxSingleton"
    /// Multiple tests running in parallel would cause multiple providers to be instanciated leading to the following exception:
    /// MvvmCross.Platform.Exceptions.MvxException : You cannot create more than one instance of MvxSingleton
    /// </summary>
    public abstract class AutofacMvxTestBase : IDisposable
    {
        private static readonly AsyncLock _lock = new AsyncLock();
        private readonly IDisposable _key;

        protected AutofacMvxTestBase()
        {
            _key = _lock.Lock();
        }

        protected virtual void DisposeOverride()
        {
        }

        public void Dispose()
        {
            DisposeOverride();

            _key.Dispose();
        }
    }
}
