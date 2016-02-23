namespace Antigen.Objects
{
    /// <summary>
    /// Interface for objects that need to perform some cleanup
    /// (f.ex. event unregistering) when they are destroyed.
    /// Objects implementing this interface will likely not
    /// be garbage-collected until <see cref="Destroy"/>
    /// is called.
    /// </summary>
    interface IDestroy
    {
        /// <summary>
        /// Prepares the object for garbage collection, releasing
        /// any held resources, unregistering events etc. Must also
        /// destroy any objects that implement <code>IDestroy</code>
        /// and are owned by the implementor of this method.
        /// </summary>
        void Destroy();
    }
}
