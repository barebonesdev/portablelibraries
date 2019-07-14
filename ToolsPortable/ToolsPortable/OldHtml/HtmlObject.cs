using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace ToolsPortable.OldHtml
{
    [DataContract]
    public class HtmlObject
    {
        private int i = 0;

        public HtmlItem Root { get; private set; }

        /// <summary>
        /// Constructs a HtmlObject from the html text data.
        /// </summary>
        /// <param name="data"></param>
        public HtmlObject(string data)
        {
            if (!data.TrimEnd().EndsWith("</html>", StringComparison.CurrentCultureIgnoreCase))
                throw new Exception("The provided string wasn't a full html document.\n\n" + data + "\n\n");

            HtmlItem king = new HtmlItem()
            {
                Type = "",
                Attributes = new Dictionary<string, string>(),
                Children = new List<HtmlItem>()
            };

            parse(data, king);

            if (king.Children.Count > 0)
                Root = king.Children[0];
        }

        private void trimSpace(string data)
        {
            for (; i < data.Length && StringTools.IsSpace(data[i]); i++) ;
        }

        private bool parseInner(string data, HtmlItem parent, HtmlItem item)
        {
            while (i + 1 < data.Length)
            {
                //trim any leading space
                trimSpace(data);

                //if there's an inner tag
                if (isStart(data))
                {
                    //read inner tags
                    parse(data, item);

                    if (i + 1 < data.Length && isEnd(data, 1))
                    {
                        //if (!StringTools.Grab(data, i, "</~>", 1)[0].Equals(item.Type))
                        if (!item.IsType(StringTools.Grab(data, i, "</~>", 1)[0]))
                            return true;

                        break;
                    }
                }

                //otherwise it just had a value
                else
                {
                    //find end tag
                    for (int x = i; x < data.Length; x++)
                        if (isEnd(data, x))
                        {
                            item.Value = data.Substring(i, x - i);
                            i = x;
                            break;
                        }

                    //if (!StringTools.Grab(data, i, "</~>", 1)[0].Equals(item.Type))
                    if (!item.IsType(StringTools.Grab(data, i, "</~>", 1)[0]))
                        return true;

                    break;
                }
            }

            //now advance past the end tag
            for (; i < data.Length; i++)
                if (isClose(data, i))
                {
                    i++;
                    break;
                }

            return false;
        }

        private string[] arr = new string[1];
        private string[] items = new string[2];
        private HtmlItem createItem(string data, HtmlItem parent)
        {
            HtmlItem item = new HtmlItem()
            {
                Attributes = new Dictionary<string, string>(),
                Children = new List<HtmlItem>(),
                Parent = parent
            };

            //move to end of tag while grabbing contents inside tag
            int origI = i;
            i = StringTools.Grab(data, i, "<~>", arr);

            if (i == -1)
            {
                string str = data.Substring(origI);
                return new HtmlItem();
            }

            int index = 0;
            string inner = arr[0];

            //grab the type
            if (inner.Contains(" "))
            {
                index = StringTools.Grab(inner, index, "~ ", arr);
                item.Type = arr[0].Trim();

                //grab the others
                while ((index = StringTools.Grab(inner, index, "~=\"~\"", items)) != -1)
                {
                    item.Attributes[items[0].Trim()] = items[1];

                    if (index < inner.Length && inner[index] == ' ')
                        index++;
                }
            }

            else
                item.Type = inner.Trim();

            return item;
        }

        private string parse(string data, HtmlItem parent)
        {
            string value = null;

            while (i < data.Length)
            {
                //if at opening tag
                if (isStart(data))
                {
                    //create item
                    HtmlItem item = createItem(data, parent);

                    //if it's a list item, and the parent is also a list item
                    if (item.IsType("li") && parent.IsType("li"))
                    {
                        //add to parent's parent
                        parent.Parent.Children.Add(item);

                        //reassign the parent
                        item.Parent = parent.Parent;

                        //scan next stuff
                        item.Value = parse(data, item);

                        //return the value to exit this inner
                        return value;
                    }

                    //add to parent
                    parent.Children.Add(item);

                    //if it's not self closing and it's not a break tag
                    if (data[i - 2] != '/' && !item.IsType("br") && !item.IsType("img"))
                    {
                        //set item value to inner data
                        item.Value = parse(data, item);
                    }
                }

                //else if at closing tag
                else if (isEnd(data, i))
                {
                    int x;

                    //if tag type matches parent
                    if ((x = StringTools.Grab(data, i, "</~>", arr)) != -1)
                    {
                        //if it was a </br> tag
                        if (arr[0].Equals("br", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //add the break to the parent
                            parent.Children.Add(new HtmlItem()
                            {
                                Type = arr[0],
                                Attributes = new Dictionary<string,string>(),
                                Children = new List<HtmlItem>(),
                                Parent = parent
                            });

                            //advance past close
                            i = x;

                            continue;
                        }

                        //if tag type matches parent
                        //else if (arr[0].Equals(parent.Type))
                        else if (parent.IsType(arr[0]))
                        {
                            //advance past close
                            i = x;
                        }
                    }

                    //return the value
                    return value;
                }

                else if (i + 1 < data.Length && data[i] == '<' && data[i + 1] == '!')
                {
                    //move forward past the '>'
                    for (; i < data.Length && data[i] != '>'; i++) ;
                    i++;
                }

                //else we just have the value
                else
                {
                    StringBuilder builder = new StringBuilder();

                    //move forward until '<'
                    bool firstWhitespace = true;
                    for (; i < data.Length && data[i] != '<'; i++)
                    {
                        //only include one whitespace character (removes multiple consecutive spaces) like proper HTML
                        if (char.IsWhiteSpace(data[i]))
                        {
                            if (firstWhitespace)
                            {
                                firstWhitespace = false;
                                builder.Append(' ');
                            }
                        }

                        else
                        {
                            firstWhitespace = true;
                            builder.Append(data[i]);
                        }
                    }

                    //temp = temp.Trim();

                    parent.Children.Add(new HtmlItem()
                    {
                        Type = "Text",
                        Value = builder.ToString(),
                        Attributes = new Dictionary<string, string>(),
                        Children = new List<HtmlItem>(),
                        Parent = parent
                    });

                }
            }

            return value;
        }

        private int advanceComment(string data, int i)
        {
            if (StringTools.StartsWith(data, i, "<!--"))
            {
                //advance to end of comment
                for (i += 4; !StringTools.StartsWith(data, i, "-->"); i++) ;

                return i;
            }

            return -1;
        }

        private bool isStart(string data)
        {
            trimSpace(data);

            if (i + 1 < data.Length)
            {
                if (data[i] == '<')
                {
                    if (data[i + 1] != '!' && data[i + 1] != '/')
                        return true;

                    //if it's a comment
                    else if (StringTools.StartsWith(data, i + 1, "!--"))
                    {
                        //advance to end of comment
                        for (i += 4; !StringTools.StartsWith(data, i, "-->"); i++) ;

                        //skip past the rest of it
                        i += 3;

                        //trim space
                        trimSpace(data);

                        //see if it's a start now
                        return isStart(data);
                    }
                }

                return data[i] == '<' && StringTools.IsAlpha(data[i + 1]);
            }

            return false;
        }

        private bool isClose(string data, int i)
        {
            //if comment
            int x = advanceComment(data, i);
            if (x != -1)
                return isClose(data, x);

            return data[i] == '>';
        }

        private bool isEnd(string data, int i)
        {
            //skip past whitespace
            for (; i < data.Length && StringTools.IsSpace(data[i]); i++) ;

            //if comment
            int x = advanceComment(data, i);
            if (x != -1)
                return isEnd(data, x);

            if (i + 1 < data.Length)
                return data[i] == '<' && data[i + 1] == '/';

            return false;
        }

        /// <summary>
        /// Finds the first match for the type, and must match the attributes if they're specified. Option to disable recursive search will only search on the next level if set to false.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public HtmlItem Get(string type, Dictionary<string, string> attributes = null, bool continueUp = false, bool recursive = true)
        {
            if (Root == null)
                return null;

            return Root.Get(type, attributes, continueUp, recursive);
        }

        /// <summary>
        ///  Calls Get(type, null) to get the HtmlItem with the type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public HtmlItem this[string type]
        {
            get
            {
                return Get(type, null);
            }
        }
    }

    /// <summary>
    /// When serialized, the initial item's parent is not saved.
    /// </summary>
    [DataContract]
    public class HtmlItem
    {
        /// <summary>
        /// Set the children's parent to this current item so it has the right references.
        /// </summary>
        /// <param name="c"></param>
        [OnDeserialized]
        public void onDeserialized(StreamingContext c)
        {
            foreach (HtmlItem i in Children)
                if (i != null)
                    i.Parent = this;
        }

        /// <summary>
        /// The type, like blockquote, button, etc.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Items like id="toc_rows" or class="ban". Key would be "id" or "class", value would be "toc_rows" or "ban".
        /// </summary>
        [DataMember]
        public Dictionary<string, string> Attributes { get; set; }

        [DataMember]
        public List<HtmlItem> Children { get; set; }

        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Parent is not serialized.
        /// </summary>
        public HtmlItem Parent { get; set; }

        /// <summary>
        /// Just gets the immediate text from the item, doesn't recursively go down. Always returns initialized string.
        /// 
        /// Like for <span>Tacos</span>, it'd return Tacos
        /// </summary>
        /// <returns></returns>
        public string GetImmediateText()
        {
            string answer = "";

            if (Value != null)
                answer += Value;

            for (int i = 0; i < Children.Count; i++)
                if (Children[i].IsType("Text") && Children[i].Value != null)
                    answer += Children[i].Value;

            return answer;
        }

        /// <summary>
        /// Gets any text contained in the item (including children's text). Returns "Value" followed by anything inside. Always returns an initialized string.
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            bool lastCharWasSpace = false;
            StringBuilder answer;

            if (Value != null)
            {
                answer = new StringBuilder(Value);
                lastCharWasSpace = Value.Length > 0 && char.IsWhiteSpace(Value[Value.Length - 1]);
            }

            else
                answer = new StringBuilder();


            for (int i = 0; i < Children.Count; i++)
            {
                string childText = Children[i].GetText();

                if (childText.Length > 0)
                {
                    //if the new text starts with a space and we already have a space, ignore the space
                    if (lastCharWasSpace && char.IsWhiteSpace(childText[0]))
                        answer.Append(childText, 1, childText.Length - 1);

                    //otherwise append like normal
                    else
                        answer.Append(childText);

                    lastCharWasSpace = char.IsWhiteSpace(childText[childText.Length - 1]);
                }
            }

            return answer.ToString();
        }

        /// <summary>
        /// Compares the Type with the provided type, returns true if they match (ignoring case), otherwise false. If null, it doesn't check the match and returns true.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsType(string type)
        {
            if (type == null)
                return true;

            return Type.Equals(type, StringComparison.CurrentCultureIgnoreCase);
        }

        public string ToDocument(int level)
        {
            string answer = tabs(level) + "<" + Type;

            foreach (var pair in Attributes)
                answer += " " + pair.Key + "=\"" + pair.Value + "\"";

            if (Children.Count == 0 && Value == null)
                answer += "/>";
                
            else
            {
                answer += ">";

                for (int i = 0; i < Children.Count; i++)
                    answer += "\n" + Children[i].ToDocument(level + 1);

                if (Children.Count > 0)
                    answer += '\n';

                if (Value != null)
                    answer += Value + "</" + Type + ">";
                else
                    answer += tabs(level) + "</" + Type + ">";
            }

            return answer;
        }

        private string tabs(int level)
        {
            string answer = "";

            for (int i = 0; i < level; i++)
                answer += '\t';

            return answer;
        }

        public LinkedList<HtmlItem> GetRange(string type, Dictionary<string, string> attributes)
        {
            HtmlItem stop = Next(false);

            HtmlItem item = this;
            LinkedList<HtmlItem> list = new LinkedList<HtmlItem>();

            while ((item = item.Next()) != stop)
            {
                if (item.IsType(type) && item.HasAttributes(attributes))
                    list.AddLast(item);
            }

            return list;
        }

        public override string ToString()
        {
            string answer = "<" + Type;

            foreach (var pair in Attributes)
                answer += " " + pair.Key + "=\"" + pair.Value + "\"";

            if (Children.Count == 0 && Value == null)
                answer += "/>";

            else
            {
                answer += ">";

                if (Children.Count > 0)
                    answer += "[" + Children.Count + "]";

                if (Value != null)
                    answer += Value;

                answer += "</" + Type + ">";
            }

            return answer;
        }

        /// <summary>
        /// Returns true if...
        ///     attributes is null
        ///     The item has all the same attributes AND the attribute values match
        ///     
        /// Providing a null value in the attributes dictionary signifies that ANY value will match.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public bool HasAttributes(Dictionary<string, string> attributes)
        {
            if (attributes == null)
                return true;

            string str;
            foreach (var pair in attributes)
            {
                if (!Attributes.TryGetValue(pair.Key, out str) || (pair.Value != null && !str.Equals(pair.Value)))
                    return false;
            }

            return true;
        }

        private HtmlItem next()
        {
            if (Parent == null)
                return null;

            int index = Parent.Children.IndexOf(this);

            //if there's a next item on the same level
            if (index + 1 < Parent.Children.Count)
                return Parent.Children[index + 1];

            return Parent.next();
        }

        public enum NextType
        {
            Down = 1,      //0001
            Up = 2,        //0010
            SameLevel = 8, //0100
            All = 15       //0111
        }

        public HtmlItem PreviousSibling()
        {
            if (Parent == null)
                return null;

            int index = Parent.Children.IndexOf(this);

            if (index == 0)
                return null;

            return Parent.Children[index - 1];
        }

        //public HtmlItem Previous(NextType recursiveType)
        //{
        //    //if no parent, there's no previous
        //    if (Parent == null)
        //        return null;

        //    HtmlItem previousSibling = PreviousSibling();

            
        //}

        public HtmlItem Next(NextType recursiveType)
        {
            if ((recursiveType & NextType.Down) == NextType.Down)
            {
                //pick first child if it exists
                if (Children.Count > 0)
                    return Children[0];
            }

            //otherwise look at the parent for the next
            if (Parent == null)
                return null;

            int index = Parent.Children.IndexOf(this);

            //if there's a next item on the same level
            if ((recursiveType & NextType.SameLevel) == NextType.SameLevel)
            {
                if (index + 1 < Parent.Children.Count)
                    return Parent.Children[index + 1];
            }


            //otherwise if we can look recursive up higher
            if ((recursiveType & NextType.Up) == NextType.Up)
            {
                return Parent.next();
            }

            return null;
        }

        /// <summary>
        /// Returns the next HtmlItem that it needs to stop at
        /// </summary>
        /// <returns></returns>
        public HtmlItem Stop()
        {
            return Next(NextType.SameLevel | NextType.Up);
        }

        public HtmlItem Next(bool recursive = true)
        {
            if (recursive)
                return Next(NextType.All);

            else
                return Next(NextType.SameLevel);
        }

        private HtmlItem get(string type, Dictionary<string, string> attributes, bool recursive)
        {
            //look through children
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].IsType(type))
                {
                    if (Children[i].HasAttributes(attributes))
                        return Children[i];
                }

                if (recursive)
                {
                    HtmlItem item = Children[i].get(type, attributes, recursive);

                    if (item != null)
                        return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the first matching item.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public HtmlItem Get(HtmlMatch match)
        {
            HtmlItem item = Get(match.Type, match.Attributes.ToDictionary(i => i.AttributeName, i => i.AttributeValue));

            if (item == null)
                return null;

            if (match.Matches(item))
                return item;

            //right now if the attributes match but the count didn't match, it'll throw an exception. I should in the future add support for having it search for the next possible item, but then I have to add in a little extra magic.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the first match for the type, and must match the attributes if they're specified. Option to disable recursive search will only search on the next level if set to false.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public HtmlItem Get(string type, Dictionary<string, string> attributes = null, bool continueUp = false, bool recursive = true)
        {
            //look through children
            HtmlItem i;
            if ((i = get(type, attributes, recursive)) != null)
                return i;

            //if we can continue up
            if (continueUp)
            {
                HtmlItem n = next();
                if (n != null)
                {
                    //first check if it matches
                    if (n.IsType(type) && n.HasAttributes(attributes))
                        return n;

                    return n.Get(type, attributes, continueUp, recursive);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the attribute from the map. Calls Attributes[type] so might throw exception if not found.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string this[string type]
        {
            get
            {
                return Attributes[type];
            }
        }

        /// <summary>
        /// Returns true if it has the attribute and the attribute's value isn't an empty string
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public bool HasAttribute(string attribute)
        {
            string str;
            if (Attributes.TryGetValue(attribute, out str))
                return str.Length > 0;

            return false;
        }

        /// <summary>
        /// Returns true if the attribute is found and the value matches the provided value, else false.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public bool HasAttribute(string attribute, string value, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            string str;
            if (Attributes.TryGetValue(attribute, out str))
                return str.Equals(value, comparisonType);

            return false;
        }
    }

    /// <summary>
    /// A class for giving instructions on picking which HtmlItem to open. Make sure to assign Type, and possibly Attributes.
    /// </summary>
    [DataContract]
    public class HtmlMatch
    {
        /// <summary>
        /// Make sure to assign AttributeName, and possibly assign AttributeValue.
        /// </summary>
        [DataContract]
        public class AttributeMatch
        {
            public AttributeMatch() { }

            public AttributeMatch(string attributeName, string attributeValue)
            {
                AttributeName = attributeName;
                AttributeValue = attributeValue;
            }

            /// <summary>
            /// Must be assigned! Cannot be null.
            /// </summary>
            [DataMember]
            public string AttributeName { get; set; }

            /// <summary>
            /// Leave this as null if it only should check for the property name, not the value
            /// </summary>
            [DataMember]
            public string AttributeValue { get; set; }

            public bool Matches(HtmlItem item)
            {
                //if we're just checking for the property existing
                if (AttributeValue == null)
                    return item.Attributes.ContainsKey(AttributeName);

                //otherwise we need to check if the value matches too
                return item.Attributes.Contains(new KeyValuePair<string, string>(AttributeName, AttributeValue));
            }
        }

        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// How many attributes there should be. Initially set to -1 meaning it'll ignore checking the count.
        /// </summary>
        [DataMember]
        public int AttributesCount { get; set; }

        [DataMember]
        public AttributeMatch[] Attributes { get; set; }

        public HtmlMatch()
        {
            AttributesCount = -1;
        }

        public bool Matches(HtmlItem item)
        {
            //if it's the right type
            if (item.IsType(Type))
            {
                //check if there's the right number of attributes
                if (AttributesCount != -1 && item.Attributes.Count != AttributesCount)
                    return false;
                    

                //if we have to check the properties
                if (Attributes != null)
                {
                    //if any property fails to match, the whole thing doesn't match
                    for (int i = 0; i < Attributes.Length; i++)
                        if (!Attributes[i].Matches(item))
                            return false;
                }

                return true;
            }

            return false;
        }
    }

    [DataContract]
    public class HtmlInstruction
    {
        public abstract class Creator
        {
            /// <summary>
            /// Default is true. If this returns true, it'll use the attribute values when giving instructions to get to the next value. Can be overriden.
            /// </summary>
            protected virtual bool UseAttributeValues
            {
                get { return true; }
            }

            /// <summary>
            /// Default is true. If this returns true, it'll use the attribute keys when giving instructions to get the next value. Can be overriden.
            /// </summary>
            protected virtual bool UseAttributeKeys
            {
                get { return true; }
            }

            protected abstract HtmlInstruction Found(HtmlItem item);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="fromStart">This would be the top item like the "p" item from the search results.</param>
            /// <returns></returns>
            public HtmlInstruction CreateInstructions(HtmlItem fromStart)
            {
                return createInstructions(fromStart.Next(HtmlItem.NextType.Down));
            }

            private HtmlInstruction createInstructions(HtmlItem item)
            {
                if (item == null)
                    return null;

                //if (nextInstruction != null)
                //{
                //    HtmlInstruction instruction = HtmlInstruction.CreateGetNextItem(item.Type, null);
                //    instruction.Next = nextInstruction;
                //    return instruction;
                //}

                do
                {
                    //if this is our end point
                    HtmlInstruction nextInstruction = Found(item);

                    if (nextInstruction == null)
                        nextInstruction = createInstructions(item.Next(HtmlItem.NextType.Down));

                    if (nextInstruction != null)
                    {
                        Dictionary<string, string> attributes = null;

                        if (UseAttributeKeys && item.Attributes.Count > 0)
                        {
                            if (UseAttributeValues)
                                attributes = copyAttributesWithValues(item);

                            else
                                attributes = copyAttributeKeys(item);
                        }

                        HtmlInstruction instruction = HtmlInstruction.CreateGetNextItem(item.Type, attributes);
                        instruction.Next = nextInstruction;
                        return instruction;
                    }

                } while ((item = item.Next(HtmlItem.NextType.SameLevel)) != null);

                return null;
            }

            /// <summary>
            /// Can be overriden. Typically returns a dictionary with all the attributes and values, but can be overriden to leave out select values like a unique link value.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            protected virtual Dictionary<string, string> copyAttributesWithValues(HtmlItem item)
            {
                return new Dictionary<string, string>(item.Attributes);
            }

            /// <summary>
            /// Returns a new dictionary with only the keys, not the values
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            protected Dictionary<string, string> copyAttributeKeys(HtmlItem item)
            {
                Dictionary<string, string> attributes = new Dictionary<string, string>();

                foreach (var pair in item.Attributes)
                    attributes[pair.Key] = null;

                return attributes;
            }
        }

        public enum InstructionType
        {
            GetNextItem,
            GetAnyItem,
            GetAttributeValue,
            GetValue,
            GetImmediateValue,
            Start
        }

        [DataMember]
        public HtmlInstruction Next = null;

        [DataMember]
        public string Type = null;

        [DataMember]
        public InstructionType Instruction = InstructionType.Start;

        [DataMember]
        public string AttributeToFetch = null;

        [DataMember]
        public Dictionary<string, string> AttributesToMatch;


        public static HtmlInstruction CreateGetAttributeValue(string attributeName)
        {
            return new HtmlInstruction()
            {
                AttributeToFetch = attributeName,
                Instruction = InstructionType.GetAttributeValue
            };
        }

        public static HtmlInstruction CreateGetValue()
        {
            return new HtmlInstruction()
            {
                Instruction = InstructionType.GetValue
            };
        }

        public static HtmlInstruction CreateGetNextItem(string type, Dictionary<string, string> attributesToMatch)
        {
            return new HtmlInstruction()
            {
                Instruction = InstructionType.GetNextItem,
                Type = type,
                AttributesToMatch = attributesToMatch
            };
        }

        /// <summary>
        /// Processes all the instructions. Returns null if failed to grab it.
        /// </summary>
        /// <returns></returns>
        public string Get(HtmlItem item)
        {
            switch (Instruction)
            {
                case InstructionType.GetAnyItem:
                case InstructionType.GetNextItem:

                    //if there's no next instruction, there was an error
                    if (Next == null)
                        return null;

                    //if we get any item, we search as far down as necessary. Otherwise we just search one level
                    HtmlItem nextItem = item.Get(Type, AttributesToMatch, false, Instruction == InstructionType.GetAnyItem);

                    if (nextItem == null)
                        return null;

                    return Next.Get(nextItem);



                case InstructionType.GetAttributeValue:

                    string answer;

                    if (item.Attributes.TryGetValue(AttributeToFetch, out answer))
                        return answer;

                    return null;




                case InstructionType.GetValue:

                    return item.GetText();

                case InstructionType.GetImmediateValue:
                    return item.GetImmediateText();



                case InstructionType.Start:

                    if (Next == null)
                        return null;

                    return Next.Get(item);
            }

            return null;
        }
    }
}
