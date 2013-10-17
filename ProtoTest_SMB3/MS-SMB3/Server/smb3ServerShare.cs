//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ServerShare
// Description: A stucture contains information about share
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// A stucture contains information about share
    /// </summary>
    public class smb3ServerShare
    {
        private string name;
        private string localPath;
        private List<string> connectSecurity;
        private List<string> fileSecurity;
        private ShareFlags_Values cscFlags;
        private bool isDfs;
        private bool doAccessBasedDirectoryEnumeration;
        private bool allowNamespaceCaching;
        private bool forceSharedDelete;
        private bool restrictExclusiveOpens;

        /// <summary>
        /// A unique name for the shared resource on this server.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// A path that describes the local resource that is being shared.
        /// </summary>
        public string LocalPath
        {
            get
            {
                return localPath;
            }
        }

        /// <summary>
        /// An authorization policy such as an access control list that describes 
        /// which users are allowed to connect to this share.
        /// </summary>
        public ReadOnlyCollection<string> ConnectSecurity
        {
            get
            {
                if (connectSecurity != null)
                {
                    return connectSecurity.AsReadOnly();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// An authorization policy such as an access control list that describes what actions users 
        /// that connect to this share are allowed to perform on the shared resource
        /// </summary>
        public ReadOnlyCollection<string> FileSecurity
        {
            get
            {
                if (fileSecurity != null)
                {
                    return fileSecurity.AsReadOnly();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The configured offline caching policy for this share.
        /// </summary>
        public ShareFlags_Values CscFlags
        {
            get
            {
                return cscFlags;
            }
        }

        /// <summary>
        /// A Boolean that, if set, indicates that this share is configured for DFS.
        /// </summary>
        public bool IsDfs
        {
            get
            {
                return isDfs;
            }
        }

        /// <summary>
        /// A Boolean that, if set, indicates that the results of directory enumerations on this share 
        /// must be trimmed to include only the files and directories that the calling user has access to
        /// </summary>
        public bool DoAccessBasedDirectoryEnumeration
        {
            get
            {
                return doAccessBasedDirectoryEnumeration;
            }
        }

        /// <summary>
        /// A Boolean that, if set, indicates that clients are allowed to cache directory enumeration results 
        /// for better performance.
        /// </summary>
        public bool AllowNamespaceCaching
        {
            get
            {
                return allowNamespaceCaching;
            }
        }

        /// <summary>
        /// A Boolean that, if set, indicates that all opens on this share MUST include FILE_SHARE_DELETE in the sharing access
        /// </summary>
        public bool ForceSharedDelete
        {
            get
            {
                return forceSharedDelete;
            }
        }

        /// <summary>
        /// A Boolean that, if set, indicates that users who request read-only access to a file 
        /// are not allowed to deny other readers.
        /// </summary>
        public bool RestrictExclusiveOpens
        {
            get
            {
                return restrictExclusiveOpens;
            }
        }
    }
}
