using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.Responses;
#nullable enable
/// <summary>
/// 获取消息 API 的响应数据。
/// </summary>
public class GetMessageResponseData
{
    [JsonProperty("message")]
    private object _message = default!;
    [JsonIgnore]
    private readonly Lazy<ReceivedMessage> _messageLazy;
    [JsonIgnore]
    private bool _isMessageManualSet;

    /// <summary>
    /// 构造实例。
    /// </summary>
    public GetMessageResponseData()
    {
        _messageLazy = new Lazy<ReceivedMessage>(() => new ReceivedMessage(_message));
    }

    /// <summary>
    /// 发送时间。
    /// </summary>
    [JsonConverter(typeof(UnixDateTimeConverter))]
    [JsonProperty("time")]
    public DateTimeOffset Time { get; set; }
    /// <summary>
    /// 消息类型。
    /// </summary>
    [JsonProperty("message_type")]
    public required string MessageType { get; set; }
    /// <summary>
    /// 消息id。
    /// </summary>
    [JsonProperty("message_id")]
    public int MessageId { get; set; }
    /// <summary>
    /// 消息真实id。
    /// </summary>
    [JsonProperty("real_id")]
    public int RealId { get; set; }
    /// <summary>
    /// 发送人信息。
    /// </summary>
    [JsonProperty("sender")]
    public required SenderInfo Sender { get; set; }
    /// <summary>
    /// 消息内容。
    /// </summary>
    [JsonIgnore]
    public ReceivedMessage Message
    {
        get => _isMessageManualSet ? (ReceivedMessage)_message : _messageLazy.Value;
        set
        {
            _isMessageManualSet = true;
            _message = value;
        }
    }
}
