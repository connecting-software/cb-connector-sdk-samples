namespace CBTestConnector.Command.Handlers.Resources
{
    public interface IVisitor<in TInput, out TOutput>
    {
        TOutput Visit(TInput obj);
    }
}