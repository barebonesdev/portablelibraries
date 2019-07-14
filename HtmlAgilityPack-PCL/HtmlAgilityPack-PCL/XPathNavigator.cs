using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlAgilityPack
{
    public class XPathNavigator
    {
        public enum InstructionType
        {
            /// <summary>
            /// Select every node in the document (only happens at first one)
            /// </summary>
            //SelectAll,

            /// <summary>
            /// Select direct children from current node
            /// </summary>
            SelectChildren,

            /// <summary>
            /// Select everything below the current node
            /// </summary>
            SelectDescendants,


            SelectCurrent,


            SelectParent,


            MatchName,


            MatchAttribute
        }

        public class Instruction
        {
            public Instruction(InstructionType type)
            {
                Type = type;
            }

            public InstructionType Type;

            public string Name;
            public string Value;
        }

        private string _xpath;
        private int _index;

        public XPathNavigator(string xpath)
        {
            _xpath = xpath;
        }

        public bool HasMore()
        {
            return _index < _xpath.Length;
        }

        public Instruction NextInstruction()
        {
            prepare();

            char c = peek();

            //finished
            if (c == 0)
                return null;

            if (c == '/')
            {
                _index++;

                if (peek() == '/')
                {
                    _index++;
                    return new Instruction(InstructionType.SelectDescendants);
                }

                return new Instruction(InstructionType.SelectChildren);
            }

            if (c == '.')
            {
                _index++;

                if (peek() == '.')
                {
                    _index++;
                    return new Instruction(InstructionType.SelectParent);
                }

                return new Instruction(InstructionType.SelectCurrent);
            }

            if (c == '[')
            {
                advance();

                //fall through to the attribute one
                c = peek();
            }

            if (c == '@')
            {
                advance();

                Instruction instr = new Instruction(InstructionType.MatchAttribute);

                instr.Name = nextAttributeName();


                //either like [@class] or [@class='reply']
                if (peek() == '=')
                {
                    advance();

                    if (pop() != '\'')
                        throw new Exception("Poorly formatted @attribute, missing start quote on attribute value");

                    instr.Value = nextAttributeValue();

                    if (pop() != '\'')
                        throw new Exception("Poorly formatted @attribute, missing end quote on attribute value");
                }

                if (peek() == ']')
                {
                    advance();
                }

                return instr;
            }

            return new Instruction(InstructionType.MatchName) { Name = nextElementName() };
        }

        private string nextElementName()
        {
            StringBuilder builder = new StringBuilder();

            char c;
            while (true)
            {
                c = peek();

                if (c == 0 || char.IsWhiteSpace(c) || c == '/' || c == '[')
                    break;

                builder.Append(c);
                advance();
            }

            return builder.ToString();
        }

        private string nextAttributeName()
        {
            StringBuilder builder = new StringBuilder();

            char c;
            while (true)
            {
                c = peek();

                if (c == 0 || char.IsWhiteSpace(c) || c == '=' || c == '@' || c == ']' || c == '/')
                    break;

                builder.Append(c);
                advance();
            }

            return builder.ToString();
        }

        private string nextAttributeValue()
        {
            StringBuilder builder = new StringBuilder();

            char c;
            while (true)
            {
                c = peek();

                if (c == 0 || c == '\'')
                    break;

                builder.Append(c);
                advance();
            }

            return builder.ToString();
        }

        private void advance()
        {
            _index++;
        }

        /// <summary>
        /// Skips past whitespace
        /// </summary>
        private void prepare()
        {
            while (_index < _xpath.Length && char.IsWhiteSpace(_xpath[_index]))
                _index++;
        }

        private char peek()
        {
            if (_index >= _xpath.Length)
                return (char)0;

            return _xpath[_index];
        }

        /// <summary>
        /// Returns next char, or 0 if at end. Advances index.
        /// </summary>
        /// <returns></returns>
        private char pop()
        {
            if (_index >= _xpath.Length)
                return (char)0;

            return _xpath[_index++];
        }
    }
}
