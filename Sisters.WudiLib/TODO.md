## Recent
- 实现 `ReceivedMessage` 的判等。
- `SectionMessage` 中的属性改为抽象，在子类实现。`GetRaw()` 移到 `Section` 类？
- 补齐 `GroupMemberInfo` 中的属性。

## Future
- 优化基础类

	4. 不再使用 `Utilities` 类。

- 优化整个事件处理流程

    1. 将反序列化与事件处理分离，便于实现共同事件。可以通过 `is` 运算符判断事件类型。
    2. 分离数据传输、反序列化及业务逻辑的代码，使得在不同阶段发生的异常可以由不同事件处理。

- 修改异常、客户端和上报监听器的类名，使其更符合逻辑。暂定 `CqHttpClient`/`CqHttpListener`/`CqHttpException`
- 优化 `Message` 类的继承结构。

    1. 可以实现 `IEnumerable` 等接口，这样应该可以直接序列化。
	2. 设计 `IMessage` 接口，发消息时传入。暂定有 `Serializing`、`Raw` 等属性。
	3. 修改 `Post.Message` 类名，减少同时 `using` 两个命名空间时的麻烦。暂定一律改为 `PostContext`、`MessageContext` 等。
	4. `Message` 类为构造的消息；`RawMessage` 类不变（但不再继承 `Message`，`ReceivedMessage` 也是）；取消冗余的 `SectionMessage` 和 `SendingMessage`；`ReceivedMessage` 保留 `Raw`、`Sections`（可能改为 `SectionMessage`，毕竟 `Message` 实现了 `IEnumerable`）等属性，可判等；其余的消息是否可判等我还没想好；均实现 `IMessage` 接口。

- `Message` 类的其他修改

    1. 要么把本地图片文件删掉（改用 Byte 数组图片），要么改名，以 `ImageLocal`/`ImageRemote` 区分？

- 优化 `Section` 类的序列化和反序列化过程。

	3. 做完这些工作以后，考虑让 `ReceivedMessage` 直接反序列化。

- 支持异步的事件处理器。
- 将一些 Type 改为枚举。使用 `EnumMember` 特性标记。
- 反序列化时传入静态的 setting 或者 serializer，避免潜在的全局 setting 影响。

- 加入运行平台选项，发送本地图片时，如果检测到和酷 Q 运行在不同的机器上，可以尝试先读取，再以 base64 的方式发送，以便多机使用。

- 把下面这种消息拼接方式抄过来
```C#
// 戳一戳
_mahuaApi.SendPrivateMessage("472158246")
    .Shake()
    .Done();

// 讨论组发送消息
_mahuaApi.SendDiscussMessage("472158246")
    .Text("嘤嘤嘤：")
    .Newline()
    .Text("文章无聊，不如来局游戏http://www.newbe.pro")
    .Image(@"D:\logo.png")
    .Done();

// 群内at发送消息
_mahuaApi.SendGroupMessage("610394020")
    .At("472158246")
    .Text("我想充钱")
    .Newline()
    .Done();
```

- 当前转发时会统一转换成 UTF-8 编码。如果上报编码不是 UTF-8 会导致 `byte` 数组发生变化，从而导致 `X-Signature` 头部不正确。如有必要，在未来版本中修复。

## 已取消
- 使得 `Post.AnonymousInfo` 可以比较。