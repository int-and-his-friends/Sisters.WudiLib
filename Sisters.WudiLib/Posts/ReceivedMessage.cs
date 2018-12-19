using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using static Sisters.WudiLib.SectionMessage;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 从上报中收到的消息。
    /// </summary>
    public class ReceivedMessage : WudiLib.Message
    {
        /// <summary>
        /// 也在 <see cref="Section.CqCodeTypePattern"/> 中提到。
        /// </summary>
        private const string CqCodePattern = @"\[CQ:([\w\-\.]+?)(?:,([\w\-\.]+?)=(.+?))*\]";
        private static readonly Regex CqCodeRegex = new Regex(CqCodePattern, RegexOptions.Compiled);

        private readonly bool _isString;
        /// <summary>
        /// 如果上报格式是 string，则表示原始内容；否则为 <c>null</c>。
        /// </summary>
        private readonly string _message;

        private readonly IReadOnlyList<Section> _sections;

        private readonly Lazy<IReadOnlyList<Section>> _sectionListLazy;

        private IReadOnlyList<Section> SectionListFunction()
        {
            if (!_isString)
                return _sections;
            int pos = 0;
            var regex = CqCodeRegex;
            var result = new List<Section>();
            while (pos < _message.Length)
            {
                var match = regex.Match(_message, pos);
                if (!match.Success)
                {
                    result.Add(Section.Text(_message.Substring(pos).AfterReceive()));
                    pos = _message.Length;
                }
                else
                {
                    if (match.Index > pos)
                    {
                        result.Add(Section.Text(_message.Substring(pos, match.Index - pos)));
                    }
                    pos = match.Index + match.Length;

                    string type = match.Groups[1].Value.AfterReceive();
                    var paras = match.Groups[2].Captures.Cast<Capture>().Zip(
                        match.Groups[3].Captures.Cast<Capture>(),
                        (capKey, capVal) => (capKey.Value.AfterReceive(), capVal.Value.AfterReceive())
                    ).ToArray();
                    result.Add(new Section(type, paras));
                }
            }
            return result.AsReadOnly();
        }

        /// <summary>
        /// 获取 <see cref="Section"/> 列表。
        /// </summary>
        public IReadOnlyList<Section> Sections => _sectionListLazy.Value;

        /// <exception cref="InvalidOperationException">传入参数不符合要求。</exception>
        /// <param name="o">应为 <see cref="string"/> 类型或者 <see cref="JArray"/> 类型。</param>
        internal ReceivedMessage(object o)
        {
            _sectionListLazy = new Lazy<IReadOnlyList<Section>>(SectionListFunction);

            if (o is string s)
            {
                _isString = true;
                _message = s;
                return;
            }

            if (o is JArray jObjectArray)
            {
                var sections = jObjectArray.Select(jo => new Section((JObject)jo));
                _sections = sections.ToList().AsReadOnly();
            }
            throw new InvalidOperationException("用于构造消息的对象即不是字符，也不是数组。可能是上报数据有错误。");
        }

        /// <summary>
        /// 获取消息是否是纯文本。
        /// </summary>
        public bool IsPlaintext
        {
            get
            {
                if (_isString)
                {
                    return !CqCodeRegex.IsMatch(_message);
                }

                return _sections.All(s => s.Type == "text");
            }
        }

        protected internal override object Serializing => this.Forward().Serializing;

        /// <summary>
        /// 获取不经处理的原始消息内容。
        /// </summary>
        public override string Raw => _isString ? _message : GetRaw(_sections);

        /// <summary>
        /// 获取固定消息。可以将此字符串保存到本地，在任何时候发送时，都可以发送此消息，不用担心缓存被清或者文件过期。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AggregateException">多半是网络错误。</exception>
        public string Fix()
        {
            if (_isString)
            {
                return CqCodeRegex.Replace(_message, m =>
                {
                    if (m.Groups[1].Value == Section.ImageType)
                    {
                        for (int i = 0; i < m.Groups[2].Captures.Count; i++)
                        {
                            if (m.Groups[2].Captures[i].Value == "url")
                            {
                                string url = m.Groups[3].Captures[i].Value.AfterReceive();
                                return GetFixedImageSection(url);
                            }
                        }
                    }
                    return m.Value;
                });
            }
            else
            {
                var result = new StringBuilder();
                foreach (var section in _sections)
                {
                    if (section.Type == Section.ImageType)
                    {
                        result.Append(section.Data.TryGetValue("url", out string url)
                            ? GetFixedImageSection(url)
                            : section.Raw);
                    }
                    else
                    {
                        result.Append(section.Raw);
                    }
                }
                return result.ToString();
            }
        }

        /// <summary>
        /// 获取<c>url</c>指向图片的固定消息。
        /// </summary>
        /// <param name="url">指向图片的url。</param>
        /// <returns>可以发送该图片的消息。</returns>
        /// <exception cref="AggregateException">网络错误。</exception>
        /// <exception cref="ArgumentNullException"><c>url</c>是<c>null</c>。</exception>
        private static string GetFixedImageSection(string url)
        {
            if (url is null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            using (var http = new HttpClient())
            {
                var imageBytes = http.GetByteArrayAsync(url).Result;
                var base64 = Convert.ToBase64String(imageBytes);
                return $"[CQ:image,file=base64://{base64.BeforeSend(true)}]";
            }
        }

        /// <summary>
        /// 获取消息的文本部分。
        /// </summary>
        public string Text
        {
            get
            {
                return _isString
                    ? CqCodeRegex.Replace(_message, string.Empty).AfterReceive()
                    : string.Concat(_sections.Where(s => s.Type == Section.TextType)
                        .Select(s => s.Data[Section.TextParamName]));
                // 下面是新方法，可能更快。
                //string newText = _sections
                //    .Where(s => s.Type == Section.TextType)
                //    .Aggregate(
                //        seed: new StringBuilder(),
                //        func: (sb, s) => sb.Append(s.Data[Section.TextParamName]),
                //        resultSelector: sb => sb.ToString());
            }
        }

        /// <summary>
        /// 判断消息内容是否是纯文本，如果是纯文本，则获取此文本内容。使用 string
        /// 上报类型时比查询两次属性快；array 上报类型时与先查询 <see cref="IsPlaintext"
        /// /> 属性，再查询 <see cref="Text"/> 属性没有区别。
        /// </summary>
        /// <param name="text">如果是纯文本，则为文本内容；否则为 <c>null</c>。</param>
        /// <returns>是否为纯文本。</returns>
        public bool TryGetPlainText(out string text)
        {
            text = IsPlaintext
                ? (_isString ? Raw.AfterReceive() : Text)
                : null;
            return !(text is null);
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

        /// <summary>
        /// 获取 <see cref="Section"/> 。
        /// </summary>
        /// <returns><see cref="Section"/> 列表。如果上报格式不是数组，则为 <c>null</c>。</returns>
        [Obsolete("请使用 Sections 属性。")]
        public IReadOnlyList<Section> GetSections() => _isString ? null : new List<Section>(_sections);
    }
}
