using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlAgilityPack
{
    public class HtmlCDATANode : HtmlNode
    {
        #region Fields

        protected string _data;

        #endregion

        #region Constructors

        internal HtmlCDATANode(HtmlDocument ownerdocument, int index)
            :
                base(HtmlNodeType.CDATA, ownerdocument, index)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the HTML between the start and end tags of the object. In the case of a text node, it is equals to OuterHtml.
        /// </summary>
        public override string InnerHtml
        {
            get { return Data; }
            set { Data = value; }
        }

        /// <summary>
        /// Gets the object and its content in HTML.
        /// </summary>
        public override string OuterHtml
        {
            get
            {
                return Data;

                //if (_text == null)
                    //return base.OuterHtml;

                //return _text;
            }
        }

        /// <summary>
        /// Gets or Sets the data of the node. Does NOT decode.
        /// </summary>
        public string Data
        {
            get
            {
                if (_data == null)
                {
                    _data = base.OuterHtml;

                    //if (_text.StartsWith(""))

                    //return base.OuterHtml;
                }

                return _data;
            }
            set { _data = value; }
        }

        #endregion
    }
}
