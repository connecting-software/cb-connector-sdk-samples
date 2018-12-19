using MG.CB.Collections;
using MG.CB.Collections.Classes;
using MG.CB.Metadata.MetaModel.Classes;
using MG.CB.Metadata.MetaModel.Interfaces;
using RestSharp;

namespace CBGmailConnectorSample.Metadata
{
    /// <summary> Provides a base class for defining a SQL procedure w/ additional information. </summary>
    public class Procedure : QueryableElement, IProcedure
    {
        public Procedure(string name) : base(name)
        {
            Elements = new ElementCollection<INamedElement>();
            Parameters = new SubCollection<INamedElement, IParameter>(Elements);
            // This collection does not represent a sub collection of Elements. Avoid replicates.
            ResultColumns = new ElementCollection<IColumn>();
        }

        public IElementCollection<IParameter> Parameters { get; }
        public IElementCollection<IColumn> ResultColumns { get; }
        public sealed override IElementCollection<INamedElement> Elements { get; }

        /// <summary> The URI path of this REST method. </summary>
        public string Path { get; set; }

        /// <summary> HTTP method used by this method. </summary>
        public Method Method { get; set; }

        /// <summary> Gets or sets the object that contains data about this instance. </summary>
        public object Tag { get; set; }
    }
}