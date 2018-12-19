using System;
using System.Linq;
using CB.Connector.Exceptions;
using MG.CB.Command.DataHandler.Argument;
using MG.CB.Command.DataHandler.FilterCriteria;
using MG.CB.Command.DataHandler.FilterCriteria.ComparisonCriteria;
using MG.CB.Command.Interfaces;
using MG.CB.Command.Translator;

namespace CBTestConnector.Command.Translator
{
    /// <summary> Component for translating the SQL filter criteria to <see cref="string"/>. </summary>
    /// <seealso cref="Translator{TOutput,TContext}" />
    public class SqlTranslator : Translator<string, object>
    {
        protected override string Translate(NotCriteria criteria, object context)
        {
            return $"NOT {Translate(criteria.FilterCriteria, context)}";
        }

        protected override string Translate(BlockCriteria criteria, object context)
        {
            return $"({Translate(criteria.FilterCriteria, context)})";
        }

        protected override string Translate(AndCriteria criteria, object context)
        {
            return $"{Translate(criteria.Left, context)} AND {Translate(criteria.Right, context)}";
        }

        protected override string Translate(OrCriteria criteria, object context)
        {
            return $"{Translate(criteria.Left, context)} OR {Translate(criteria.Right, context)}";
        }

        protected override string Translate(EqualCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} = {Translate(criteria.Args.Last(), context)}";
        }

        protected override string Translate(NotEqualCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} != {Translate(criteria.Args.Last(), context)}";
        }

        protected override string Translate(LikeCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} LIKE {Translate(criteria.Args.Last(), context)}";
        }

        protected override string Translate(NotLikeCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} NOT LIKE {Translate(criteria.Args.Last(), context)}";
        }

        protected override string Translate(GreaterCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} >  {Translate(criteria.Args.Last(), context)}";
        }

        protected override string Translate(GreaterOrEqualCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} >=  {Translate(criteria.Args.Last(), context)}";
        }

        protected override string Translate(LessCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} <  {Translate(criteria.Args.Last(), context)}";
        }

        protected override string Translate(LessOrEqualCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} <=  {Translate(criteria.Args.Last(), context)}";
        }

        protected override string Translate(InCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} IN  ({Translate(criteria.Args.Last(), context)})";
        }

        protected override string Translate(NotInCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} NOT IN  ({Translate(criteria.Args.Last(), context)})";
        }

        protected override string Translate(ExistsCriteria criteria, object context)
        {
            return $"EXISTS {Translate(criteria.Args.First(), context)}";
        }

        protected override string Translate(NotExistsCriteria criteria, object context)
        {
            return $"NOT EXISTS {Translate(criteria.Args.First(), context)}";
        }

        protected override string Translate(IsNullCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} IS NULL";
        }

        protected override string Translate(IsNotNullCriteria criteria, object context)
        {
            return $"{Translate(criteria.Args.First(), context)} IS NULL";
        }

        protected override string Translate(BetweenCriteria criteria, object context)
        {
            var args = criteria.Args.ToList();
            return
                $"{Translate(args[0], context)} BETWEEN {Translate(args[1], context)} AND {Translate(args[2], context)}";
        }

        protected override string Translate(AllCriteria criteria, object context)
        {
            return $"ALL {Translate(criteria.Args.First(), context)}";
        }

        protected override string Translate(AnyCriteria criteria, object context)
        {
            return $"ANY {Translate(criteria.Args.First(), context)}";
        }

        protected override string Translate(Constant argument, object context)
        {
            string result;
            var constType = argument.Type;            
            switch (constType.Name)
            {
                case "MgString":
                    result = $"N'{argument.Value}'";
                    break;
                case "MgInt32":
                    result = $"{argument.Value}";
                    break;
                case "MgDecimal":
                    result = FormattableString.Invariant($"{argument.Value}");
                    break;
                case "MgByteArray":
                    result = ByteArrayToHexViaLookup32((byte[]) argument.Value);
                    break;
                default:
                    result = $"'{argument.Value}'";
                    break;
            }
            return result;
        }

        protected override string Translate(Set argument, object context)
        {
            return $"{string.Join(",", argument.Args.Select(arg => Translate(arg, context)).ToList())}";
        }

        protected override string Translate(Variable argument, object context)
        {//Represents @p1 parameter
            if (context is IExecutionContext executionContext)
            {
                object value = executionContext.GetVariableValue(argument);
                string result;
                switch (value)
                {
                    case string _:
                        result = $"N'{value}'";
                        break;
                    case int _:
                        result = $"{value}";
                        break;
                    case long _:
                        result = $"{value}";
                        break;
                    case decimal _:
                        result = FormattableString.Invariant($"{value}");
                        break;
                    case byte[] _:
                        result = ByteArrayToHexViaLookup32((byte[])value);
                        break;
                    default:
                        result = $"'{value}'";
                        break;
                }
                return result;
            }
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.NullException, "context");
        }

        protected override string Translate(ColumnArgument argument, object context)
        {
            if (argument.IsSourceColumn)
                return argument.Owner == null ? $"[{argument.Name}]" : $"[{argument.Owner.Name}].[{argument.Name}]";
            if (argument.Name == "*")
                return "*";
            return Translate(argument.ArgumentColumn, context);
        }

        protected override string Translate(Block argument, object context)
        {
            return $"({string.Join(",", argument.Args.Select(arg => Translate(arg, context)).ToList())})";
        }

        protected override string Translate(ScalarValuedFunction argument, object context)
        {
            string oper;
            switch (argument.Name.ToLower())
            {
                case "add":
                    oper = "+";
                    break;
                case "div":
                    oper = "/";
                    break;
                case "mult":
                    oper = "*";
                    break;
                case "sub":
                    oper = "-";
                    break;
                case "mod":
                    oper = "%";
                    break;
                default:
                    return $"{argument.Metadata.QualifiedName}{Translate((Set)argument.Parameter, context)}";
            }

            return $"({string.Join(oper, ((Set)argument.Parameter).Args.Select(arg => Translate(arg, context)).ToList())})";
        }

        protected override string Translate(AggregateFunction argument, object context)
        {
            return $"{argument.Name} {Translate(argument.Parameter, context)}";
        }

        #region Singleton - usage: SqlTranslator.Instance

        private SqlTranslator()
        {
        }

        public static SqlTranslator Instance { get; } = new SqlTranslator();

        #endregion

        #region Auxiliary Methods

        private static readonly uint[] Lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = Lookup32;
            var result = new char[bytes.Length * 2 + 2];
            result[0] = '0';
            result[1] = 'x';
            for (int i = 1; i < bytes.Length + 1; i++)
            {
                var val = lookup32[bytes[i - 1]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }

        #endregion
    }
}