using MG.CB.Command.Interfaces;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBGmailConnectorSample.Translator
{
    /// <summary> Class representing required information for context Translator. </summary>
    public class ContextTranslatorInfo
    {
        public ContextTranslatorInfo(IExecutionContext executionContext, IDataType type)
        {
            ExecutionContext = executionContext;
            Type = type;
        }

        /// <summary>
        /// Gets the execution context in which the parsed statement executes.
        /// The execution context used to extract values from variables (e.g. @p1, @p2).
        /// </summary>
        public IExecutionContext ExecutionContext { get; }

        /// <summary> Gets or sets the data type for the argument being translated. </summary>
        public IDataType Type { get; }
    }
}