using System.Collections.Generic;

namespace CBTestConnector.Connector
{
    public class Join
    {
        public string JoinType { get; set; }
        public object Left { get; set; }
        public object Right { get; set; }
        public string OnCriteria { get; set; }
    }

    public class Insert
    {
        public IList<string> Fields { get; } = new List<string>();
        public IList<string> Values { get; } = new List<string>();
    }
    
    /// <summary> Class representing required information for context SqlCommand. </summary>
    public class ContextSqlCommandInfo
    {
        /// <summary> Represents the select_list in the <c>select</c> statement. </summary>
        public string Select { get; set; }

        /// <summary> Represents the top_expression in the <c>select</c> statement. </summary>
        public int Top { get; set; } = 0;

        /// <summary> Represents the <c>DISTINCT</c> in the <c>select</c> statement. </summary>
        public bool Distinct { get; set; }

        /// <summary> Represents aggregate functions in the <c>select</c> statement (e.g count, max, min, etc.). </summary>
        public string Aggregate { get; set; }

        /// <summary> Represents group_by_clause in the <c>select</c> statement. </summary>
        public string GroupBy { get; set; }

        /// <summary> Represents table_source in the <c>select</c> statement. </summary>
        public IList<string> From { get; } = new List<string>();

        /// <summary> Represents where_clause search_condition in the <c>select</c> statement. </summary>
        public string Where { get; set; }

        /// <summary> Represents having_clause search_condition in the <c>select</c> statement. </summary>
        public string Having { get; set; }

        /// <summary> Represents order_by_clause in the <c>select</c> statement. </summary>
        public string OrderBy { get; set; }

        /// <summary> Represents inner_join in the <c>select</c> statement. </summary>
        public Join Join { get; set; }

        /// <summary> Represents values and fields in the <c>insert</c> statement. </summary>
        public Insert Insert { get; set; }

        /// <summary> Represents <c>set</c> in the <c>update</c> statement. </summary>
        public IList<string> Set { get; } = new List<string>();
    }
}