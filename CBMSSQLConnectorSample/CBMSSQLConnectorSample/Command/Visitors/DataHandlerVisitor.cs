using CBTestConnector.Command.Handlers.Resources;
using MG.CB.Command.DataHandler;
using MG.CB.Command.DataHandler.Interfaces;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBTestConnector.Command.Visitors
{
    public class DataHandlerVisitor : IVisitor<IDataHandler, IQueryableElement>
    {
        public IQueryableElement Visit(IDataHandler obj)
        {
            switch (obj)
            {
                case TableSource source:
                    return source.Arguments.Metadata;
                case TableFunctionSource source:
                    return source.Arguments.Metadata;
                default:
                    return obj.Previous != null ? Visit(obj.Previous) : null;
            }
        }

        #region Singleton - usage: DataHandlerVisitor.Instance 

        private DataHandlerVisitor()
        {
        }

        public static DataHandlerVisitor Instance { get; } = new DataHandlerVisitor();

        #endregion
    }
}