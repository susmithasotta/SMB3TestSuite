using System;

namespace Microsoft.Protocols.TestTools.StackSdk
{
    /// <summary>
    /// StackRoleContext provide abstract methods for all protocols to update the context.
    /// </summary>
    public abstract class StackRoleContext
    {
        /// <summary>
        /// update the Transport state. it will be invoked every time connect or disconnect occurs. 
        /// </summary>
        /// <param name="connectionId">the connection identity.</param>
        /// <param name="state">the latest connection state.</param>
        public abstract void UpdateTransportState(int connectionId, StackTransportState state);

        /// <summary>
        /// this faunction will be invoked every time a packet is sent or received. all states of 
        /// the protocol role will be maitened here.
        /// </summary>
        /// <param name="connectionId">the connection identity.</param>
        /// <param name="packet">the sended or received packet in stack transport.</param>
        public abstract void UpdateRoleContext(int connectionId, StackPacket packet);
    }
}
