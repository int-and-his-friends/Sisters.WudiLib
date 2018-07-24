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
    public class ReceivedMessage : WudiLib.Message
    {
        private const string CqCodePattern = @"\[CQ:([\w\-\.]+?)(?:,([\w\-\.]+?)=(.+?))\]";

        private readonly bool _isString;
        private readonly string _message;

        private readonly IReadOnlyList<Section> _sections;

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
            _sections = sections.ToList().AsReadOnly();
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

        /// <summary>
        /// 获取固定消息。可以将此字符串保存到本地，在任何时候发送时，都可以发送此消息，不用担心缓存被清或者文件过期。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AggregateException">多半是网络错误。</exception>
        public string Fix()
        {
            if (_isString)
            {
                return Regex.Replace(_message, CqCodePattern, m =>
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
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            using (var http = new HttpClient())
            {
                var imageBytes = http.GetByteArrayAsync(url).Result;
                var base64 = Convert.ToBase64String(imageBytes);
                return $"[CQ:image,file=base64://{base64.BeforeSend()}]";
            }
        }

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

        /// <summary>
        /// 获取 <see cref="Section"/> 。
        /// </summary>
        /// <returns><see cref="Section"/> 列表。如果上报格式不是数组，则为 <c>null</c>。</returns>
        public IReadOnlyList<Section> GetSections() => _isString ? null : new List<Section>(_sections);
    }
}
