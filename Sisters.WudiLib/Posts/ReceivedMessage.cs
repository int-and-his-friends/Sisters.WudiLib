using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Sisters.WudiLib.SectionMessage;

namespace Sisters.WudiLib.Posts
{
    public class ReceivedMessage : WudiLib.Message
    {
        private const string CqCodePattern = @"\[CQ:([\w\-\.]+?)(?:,([\w\-\.]+?)=(.+?))\]";

        //private const string NotImplementedMessage = "暂时不支持数组格式的上报数据。";
        private readonly bool _isString;
        private readonly string _message;

        private readonly IList<Section> _sections;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">传入参数不符合要求。</exception>
        /// <param name="o"></param>
        internal ReceivedMessage(object o)
        {
            if (o is string s)
            {
                _isString = true;
                _message = s;
                return;
            }
            if (!(o is JArray jObjectArray))
                throw new InvalidOperationException("用于构造消息的对象即不是字符，也不是数组。可能是上报数据有错误。");
            var sections = jObjectArray.Select(jo => new Section((JObject)jo));
            _sections = sections.ToList();
        }

        public bool IsPlaintext
        {
            get
            {
                if (_isString)
                {
                    return !Regex.IsMatch(_message, CqCodePattern);
                }
                return _sections.All(s => s.Type == "text");
            }
        }

        internal override object Serializing => this.Forward().Serializing;

        public override string Raw => _isString ? _message : GetRaw(_sections);

        public string Text
        {
            get
            {
                return _isString
                    ? Regex.Replace(_message, CqCodePattern, string.Empty).AfterReceive()
                    : string.Concat(_sections.Where(s => s.Type == Section.TextType)
                        .Select(s => s.Data[Section.TextParamName]));
            }
        }

        /// <summary>
        /// 转发：转换成可以发送的格式。
        /// </summary>
        /// <returns></returns>
        public WudiLib.Message Forward()
        {
            if (_isString)
            {
                //string sendingRaw = Regex.Replace(
                //    _message,
                //    $@"\[CQ:{Section.ImageType},file=.+?,url=(.+?)\]",
                //    m => $"[CQ:{Section.ImageType},file={m.Groups[1].Value}]"
                //);
                //return new RawMessage(sendingRaw);
                return new RawMessage(_message);
            }
            //return new SendingMessage(_sections.Select(section =>
            //{
            //    if (section.Type != Section.ImageType) return section;
            //    try
            //    {
            //        return Section.NetImage(section.Data["url"]);
            //    }
            //    catch (KeyNotFoundException)
            //    {
            //        return section;
            //    }
            //}), true);
            return new SendingMessage(_sections);
        }
    }
}
