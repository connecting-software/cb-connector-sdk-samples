using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CBTestConnector.Connector;

namespace CBTestConnector.Command.Builder
{
    public class SqlCommandBuilder
    {
        public SqlCommandBuilder(ContextSqlCommandInfo context)
        {
            Context = context;
        }

        protected ContextSqlCommandInfo Context { get; }

        /// <summary> Gets the <c>delete</c> command required to perform deletions on the database. </summary>
        /// <returns>The <c>delete</c> command required to perform deletions.</returns>
        public string GetDeleteCommand()
        {
            var builder = new StringBuilder();
            builder.Append($"DELETE FROM {Context.From.Single()}");

            if (!string.IsNullOrEmpty(Context.Where))
            {
                builder.Append($" WHERE {Context.Where}");
            }

            return builder.ToString();
        }

        /// <summary> Gets the <c>insert</c> command required to perform insertions on the database. </summary>
        /// <returns>The <c>insert</c> command required to perform insertions.</returns>
        public IList<string> GetInsertCommand()
        {
            IList<string> batch = new List<string>();

            int maxBatchSize = 1000;
            int count = Context.Insert.Values.Count;
            var values = Context.Insert.Values.ToArray();
            for (var i = 0; i < count; i += maxBatchSize)
            {
                int batchSize = i + maxBatchSize > count ? count - i : maxBatchSize;
                var builder = new StringBuilder();
                builder.Append($"INSERT INTO {Context.From.Single()}");
                builder.Append($" ({string.Join(",", Context.Insert.Fields)})");
                builder.Append(" VALUES");
                builder.Append($" {string.Join(",", values, i, batchSize)}");
                if (!string.IsNullOrEmpty(Context.Where))
                {
                    builder.Append($" WHERE {Context.Where}");
                }
                builder.Append(";");
                batch.Add(builder.ToString());
            }

            return batch;
        }

        /// <summary> Gets the <c>update</c> command required to perform updates on the database. </summary>
        /// <returns>The <c>update</c> command required to perform updates.</returns>
        public string GetUpdateCommand()
        {
            var builder = new StringBuilder();
            builder.Append($"UPDATE {Context.From.Single()} VALUES {string.Join(",", Context.Set)}");
            if (!string.IsNullOrEmpty(Context.Where))
            {
                builder.Append($" WHERE {Context.Where}");
            }

            return builder.ToString();
        }

        /// <summary> Gets the <c>select</c> command required to perform selections on the database. </summary>
        /// <returns>The <c>update</c> command required to perform selections.</returns>
        public string GetSelectCommand()
        {
            var builder = new StringBuilder();
            builder.Append("SELECT");
            if (Context.Distinct) builder.Append(" DISTINCT");
            if (Context.Top > 0) builder.Append($" TOP({Context.Top})");
            if (!string.IsNullOrEmpty(Context.Aggregate))
            {
                builder.Append(Context.Aggregate);
                if (!string.IsNullOrEmpty(Context.Select))
                {
                    builder.Append(",");
                    builder.Append(Context.Select);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Context.Select))
                {
                    builder.Append($" {Context.Select}");
                }
            }

            builder.Append(Context.Join != null ? $" FROM {Parse(Context.Join)}" : $" FROM {string.Join(",", Context.From)}");
            if (!string.IsNullOrEmpty(Context.Where)) builder.Append($" WHERE {Context.Where}");
            if (!string.IsNullOrEmpty(Context.GroupBy)) builder.Append($" GROUP BY {Context.Where}");
            if (!string.IsNullOrEmpty(Context.OrderBy)) builder.Append($" ORDER BY {Context.OrderBy}");
            if (!string.IsNullOrEmpty(Context.Having)) builder.Append($" HAVING {Context.Having}");

            return builder.ToString();
        }

        #region Auxiliary Methods

        private static string Parse(Join join)
        {
            var builder = new StringBuilder();
            switch (join.Left)
            {
                case string left:
                    builder.Append(left);
                    break;
                case Join left:
                    builder.Append(Parse(left));
                    break;
            }
            builder.Append($" {join.JoinType.ToUpper()} JOIN ");
            switch (join.Right)
            {
                case string right:
                    builder.Append(right);
                    break;
                case Join right:
                    builder.Append(Parse(right));
                    break;
            }

            builder.Append(" ON ");
            builder.Append(join.OnCriteria);
            return builder.ToString();
        }

        #endregion
    }
}