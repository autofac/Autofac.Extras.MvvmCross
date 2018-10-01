using Xunit;

// Multiple tests running in parallel would cause multiple providers to be instantiated leading to the following exception:
// MvvmCross.Platform.Exceptions.MvxException : You cannot create more than one instance of MvxSingleton.
[assembly: CollectionBehavior(DisableTestParallelization = true)]