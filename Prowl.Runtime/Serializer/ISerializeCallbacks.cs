﻿namespace Prowl.Runtime
{
    /// <summary>
    /// Sometimes you dont want a Constructor to be called when deserializing, but still need todo some work after the object has been created
    /// This interface allows you to do that
    /// </summary>
    public interface ISerializeCallbacks
    {
        /// <summary>
        /// Called right before the TagSerializer serializes this object
        /// </summary>
        public void PreSerialize();

        /// <summary>
        /// Called right after the TagSerializer serializes this object
        /// </summary>
        public void PostSerialize();

        /// <summary>
        /// Called right before the TagSerializer deserializes this object
        /// </summary>
        public void PreDeserialize();

        /// <summary>
        /// Called right after the TagSerializer deserializes this object
        /// </summary>
        public void PostDeserialize();
    }
}
