using System;
using System.Linq;
using CB.Connector.Exceptions;
using CBGmailConnectorSample.Metadata;
using MG.CB.Command.DataHandler.Argument;
using MG.CB.Command.DataHandler.FilterCriteria;
using MG.CB.Command.DataHandler.FilterCriteria.ComparisonCriteria;
using MG.CB.Command.Translator;
using Newtonsoft.Json.Linq;

namespace CBGmailConnectorSample.Translator
{
    /// <summary> Component for translating the SQL filter criteria to string representation. </summary>
    public class ArgumentTranslator : Translator<object, ContextTranslatorInfo>
    {
        #region Singleton - usage: ArgumentTranslator.Instance 

        private ArgumentTranslator()
        {
        }

        /// <summary>
        /// Gets the single instance for <see cref="ArgumentTranslator"/>.
        /// </summary> 
        public static ArgumentTranslator Instance { get; } = new ArgumentTranslator();

        #endregion

        protected override object Translate(NotCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(BlockCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(AndCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(OrCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(EqualCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(NotEqualCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(LikeCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(NotLikeCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(GreaterCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(GreaterOrEqualCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(LessCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(LessOrEqualCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(InCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(NotInCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(ExistsCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(NotExistsCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(IsNullCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(IsNotNullCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(BetweenCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(AllCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(AnyCriteria criteria, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedFilterCriteriaException, criteria.GetType().Name);
        }

        protected override object Translate(Constant argument, ContextTranslatorInfo context)
        {
            switch (argument.Value)
            {
                case string value when context.Type is CollectionType || context.Type is ComplexType:
                    try
                    {
                        return JToken.Parse(value);
                    }
                    catch (Exception e)
                    {
                        throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnexpectedException, "Invalid JSON", e);
                    }
                case byte[] byteArray:
                    return Convert.ToBase64String(byteArray);
                default: return argument.Value;
            }
        }

        protected override object Translate(Set argument, ContextTranslatorInfo context)
        {
            return string.Join(",", argument.Args.Select(arg => Translate(arg, context)).ToList());
        }

        protected override object Translate(Variable argument, ContextTranslatorInfo context)
        {
            // The execution context is used to extract values from the variable.
            var variableValue = context.ExecutionContext.GetVariableValue(argument);
            switch (variableValue)
            {
                case string value when context.Type is CollectionType || context.Type is ComplexType:
                    try
                    {
                        return JToken.Parse(value);
                    }
                    catch (Exception e)
                    {
                        throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnexpectedException, "Invalid JSON", e);
                    }
                case byte[] byteArray:
                    return Convert.ToBase64String(byteArray);
                default: return variableValue;
            }
        }

        protected override object Translate(ColumnArgument argument, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedOperationException, argument.GetType().Name);
        }

        protected override object Translate(Block argument, ContextTranslatorInfo context)
        {
            return $"({string.Join(",", argument.Args.Select(arg => Translate(arg, context)).ToList())})";
        }

        protected override object Translate(ScalarValuedFunction argument, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedOperationException, argument.GetType().Name);
        }

        protected override object Translate(AggregateFunction argument, ContextTranslatorInfo context)
        {
            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnsupportedOperationException, argument.GetType().Name);
        }
    }
}