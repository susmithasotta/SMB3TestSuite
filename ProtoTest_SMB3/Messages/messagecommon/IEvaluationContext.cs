using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    /// <summary>
    /// An interface which the evaluation-enabled class must derived from.
    /// </summary>
    public interface IEvaluationContext
    {
        /// <summary>
        /// Trys to resolve a given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to be resolved</param>
        /// <param name="value">The value</param>
        /// <returns>Returns true if it resolves the symbol successfully.</returns>
        bool TryResolveSymbol(string symbol, out int value);

        /// <summary>
        /// Trys to resolve a custom type.
        /// </summary>
        /// <param name="typeName">The custom type to be resolved</param>
        /// <param name="typeInfo">The infomation(typename or size, etc.) of the custom type</param>
        /// <returns>Returns true if it resolves the type successfully.</returns>
        bool TryResolveCustomType(string typeName, out string typeInfo);

        /// <summary>
        /// Trys to resolve a given pointer.
        /// </summary>
        /// <param name="variable">The pointer to be resolved</param>
        /// <param name="value">The dereferened value</param>
        /// <param name="pointerValue">The pointer value</param>
        /// <returns></returns>
        bool TryResolveDereference(string variable, out int value, out int pointerValue);

        /// <summary>
        /// Reports the error when evaluating the symbol.
        /// </summary>
        /// <param name="message">The error message.</param>
        void ReportError(string message);

        /// <summary>
        /// Provides defined variables.
        /// </summary>
        IDictionary<string, object> Variables { get; }
    }
}
