using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LamiaSharp.Exceptions;
using LamiaSharp.Values;

namespace LamiaSharp.Expressions
{
    public class ExpressionList : Expression, IEnumerable<IExpression>
    {
        private readonly IList<IExpression> _values = new List<IExpression>();

        public readonly string Op;

        // TODO: Update Type to actual
        public override string Type => Types.Any;

        public ExpressionListNode First { get; set; }

        public ExpressionListNode Last { get; set; }

        public int Count { get; private set; }

        public int Tokens { get; private set; }

        public ExpressionList(string op)
        {
            Op = op;

            AddFirst(new Symbol(op));
        }

        public ExpressionList AddFirst(ExpressionListNode node)
        {
            if (First == null)
            {
                First = node;
                Last = First;
            }
            else
            {
                First.Previous = node;
                node.Next = First;

                First = node;
            }

            _values.Insert(0, node.Value);

            Count++;

            if (node.Value is ExpressionList sub)
            {
                Tokens += sub.Tokens;
            }
            else
            {
                Tokens++;
            }

            return this;
        }

        public ExpressionList AddFirst(IExpression value)
        {
            return AddFirst(new ExpressionListNode(value));
        }

        public ExpressionList AddLast(ExpressionListNode node)
        {
            Last.Next = node;
            node.Previous = Last;

            Last = node;

            _values.Add(node.Value);

            Count++;

            if (node.Value is ExpressionList sub)
            {
                Tokens += sub.Tokens;
            }
            else
            {
                Tokens++;
            }

            return this;
        }

        public ExpressionList AddLast(IExpression value)
        {
            return AddLast(new ExpressionListNode(value));
        }

        public ExpressionList Enter()
        {
            Tokens++;

            return this;
        }

        public ExpressionList Return()
        {
            Tokens++;

            return this;
        }

        public IEnumerator<IExpression> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override IExpression Evaluate(Environment env)
        {
            if (!env.TryGetValue(Op, out var express))
            {
                throw new RuntimeException($"Call to undefined symbol '{Op}'");
            }

            var value = express.Evaluate(env);

            if (!(value is Closure closure))
            {
                throw new RuntimeException($"Except closure, got '{value}'");
            }

            var arguments = _values.Skip(1).Select(p => p.Evaluate(env)).OfType<IValue>();

            return closure.Call(arguments);
        }

        public override string ToString()
        {
            var buf = Parser.Boc;

            foreach (var value in _values)
            {
                buf += value.ToString();
                buf += " ";
            }

            buf = buf.TrimEnd();

            buf += Parser.Eoc;

            return buf;
        }
    }
}
