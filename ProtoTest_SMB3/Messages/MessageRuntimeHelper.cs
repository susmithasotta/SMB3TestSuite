using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Win32;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages
{
    public static class MessageRuntimeHelper
    {
        /// <summary>
        /// Try use reflection to get the Microsoft.Modeling.Sequence type.
        /// </summary>
        /// <param name="typeArg"></param>
        /// <param name="genericSequence"></param>
        /// <returns></returns>
        public static bool TryGetModelingSequence(Type typeArg, out Type genericSequence)
        {
            genericSequence = null;

            if (genericSequence == null)
            {
                //resolve modeling sequence type from current domain or GAC.
                try
                {
                    Assembly xrtRuntimeAssembly = LoadXrtRuntimeAssembly();
                    Type genericContainer = xrtRuntimeAssembly.GetType("Microsoft.Modeling.Sequence`1", false);
                    if (genericContainer != null)
                    {
                        genericSequence = genericContainer.MakeGenericType(typeArg);
                        return true;
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    //cannot get generic modeling sequence type
                    return false;
                }
            }

            return false;
        }


        /// <summary>
        /// Describes a (possibly symbolic) value.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="value">(possibly symbolic) value</param>
        /// <returns>description</returns>
        public static string Describe<T>(T value)
        {
            if (value == null)
                return "null";
            Type type = value.GetType();
            if (IsStruct(type))
            {

                MethodInfo methodInfo = type.GetMethod("ToString", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
                if (methodInfo == null)
                {
                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    bool first = true;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0}(", type.Name);
                    foreach (FieldInfo field in fields)
                    {
                        if (!first)
                            sb.Append(",");
                        else
                            first = false;
                        sb.AppendFormat("{0}=", field.Name);
                        object fieldValue = field.GetValue((object)value);
                        sb.AppendFormat("{0}", Describe<object>(fieldValue));
                    }
                    sb.Append(")");
                    return sb.ToString();
                }
                else
                {
                    object result = methodInfo.Invoke((object)value, null);
                    return result as string;
                }
            }

            return value.ToString();
        }    


        #region helper methods
        /// <summary>
        /// Check if the given type is a Struct
        /// </summary>
        /// <param name="type">type to check</param>
        /// <returns>True if the type is a struct; False if not.</returns>
        private static bool IsStruct(Type type)
        {
            if (type.IsValueType && !type.IsPrimitive && !type.IsEnum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Load Microsoft.Xrt.Runtime.dll from file
        /// </summary>
        /// <returns>the loaded assembly</returns>
        private static Assembly LoadXrtRuntimeAssembly()
        {
            //prasanna - using local Xrt instead of PT3 supplied one
            string CurrentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string XrtRuntime = Path.Combine(CurrentPath, @"..\..\..\LIBS\Xrt\Microsoft.Xrt.Runtime.dll");
            return Assembly.LoadFile(XrtRuntime);

            //// a hint message for user
            //string message = "Microsoft.Xrt.Runtime.dll is not properly registered in GAC";

            //// get local GAC_MSIL path for xrt.runtime - commented by prasanna
            //string systemPath = Environment.GetEnvironmentVariable("SystemRoot");
            //string gacPath = Path.Combine(systemPath, @"assembly\GAC_MSIL\Microsoft.Xrt.Runtime");
            //DirectoryInfo gacInfo = new DirectoryInfo(gacPath);
                              
            //if (!gacInfo.Exists)
            //{
            //    throw new NullReferenceException(message);
            //}

            //// get the lastest installed version
            //DirectoryInfo[] infos = gacInfo.GetDirectories();
            //DirectoryInfo latestInfo = null;
            //DateTime latestDateTime = DateTime.MinValue;
            //foreach (DirectoryInfo info in infos)
            //{
            //    if (info.LastWriteTime > latestDateTime)
            //    {
            //        latestInfo = info;
            //        latestDateTime = info.LastWriteTime;
            //    }
            //}
            //if (null == latestInfo)
            //{
            //    throw new NullReferenceException(message);
            //}

            //// load the latest installed version
            //string latestXrtRuntime = Path.Combine(latestInfo.FullName, @"Microsoft.Xrt.Runtime.dll"); //prasanna
            //return Assembly.LoadFile(latestXrtRuntime);
        }
        #endregion helper methods
    }
}
