//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3CreateRequestPacket
// Description: smb3CreateRequestPacket defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 CREATE Request packet is sent by a client to request either creation of or access to a file.
    /// </summary>
    public class smb3CreateRequestPacket : smb3StandardPacket<CREATE_Request>
    {
        /// <summary>
        /// Get create contexts structure from payload.Buffer.
        /// </summary>
        /// <returns>The lease key or null</returns>
        public CREATE_CONTEXT_Values[] GetCreateContexts()
        {
            if (PayLoad.CreateContextsLength == 0)
            {
                return null;
            }
            else
            {
                byte[] createContextArray = new byte[PayLoad.CreateContextsLength];

                Array.Copy(PayLoad.Buffer, (int)(PayLoad.CreateContextsOffset - smb3Consts.CreateRequestBufferStartIndex),
                    createContextArray, 0, createContextArray.Length);

                return smb3Utility.ConvertByteArrayToCreateContexts(createContextArray);
            }
        }


        /// <summary>
        /// Get path name from buffer
        /// </summary>
        /// <returns>The path name</returns>
        public string RetreivePathName()
        {
            byte[] nameArray = new byte[PayLoad.NameLength];

            Array.Copy(PayLoad.Buffer, PayLoad.NameOffset - smb3Consts.CreateRequestBufferStartIndex, nameArray, 0, nameArray.Length);

            return Encoding.Unicode.GetString(nameArray);
        }
    }
}
