using MG.CB.Command.DataHandler.Interfaces;
using MG.CB.Command.Interfaces;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;

namespace CBTestConnector.Command.Handlers
{
    /// <summary> Base interface for all <see cref="IDataHandler"/>s. </summary>
    public interface IHandler
    {
        /// <summary>
        /// Performs, partially, the execution of the parsed statement according to the operation represented by this instance.
        /// </summary>
        /// <param name="loader">The loader used to load a valid result expected by the CB Server.</param>
        /// <param name="context">The execution context in which the parsed statement executes.</param>
        void ExecuteInternal(IResultSetLoader loader, IExecutionContext context);
    }
}