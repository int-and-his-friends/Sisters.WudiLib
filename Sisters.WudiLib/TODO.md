## Recent
- 优化整个事件处理流程

    1. 将反序列化与事件处理分离，便于实现共同事件。可以通过 `is` 运算符判断事件类型。
    2. 分离数据传输、反序列化及业务逻辑的代码，使得在不同阶段发生的异常可以由不同事件处理。

- 使转发功能支持设置 Secret。

- 实现 `ReceivedMessage` 的判等。

## Future
- 优化基础类，实现不同客户端使用不同 Token。

    1. 将 `Utilities` 类的大部分代码移到 Client 类中。改为实例方法，这样就可以读取 Token 了。
	2. 给 API 设计一个基类，大部分 API 调用放在基类，具体再传给一个虚方法。这个虚方法将在子类里重写，这样可以实现 WebSocket 调用 API。
	3. 将转义移到别的合适的类。
	4. 不再使用 `Utilities` 类。

- 修改异常、客户端和上报监听器的类名，使其更符合逻辑。暂定 `CqHttpClient`/`CqHttpListener`/`CqHttpException`
- 优化 `Message` 类的继承结构。

    1. 可以实现 `IEnumerable` 等接口，这样应该可以直接序列化。
	2. 设计 `IMessage` 接口，发消息时传入。暂定有 `Serializing`、`Raw` 等属性。
	3. 修改 `Post.Message` 类名，减少同时 `using` 两个命名空间时的麻烦。暂定一律改为 `PostContext`、`MessageContext` 等。
	4. `Message` 类为构造的消息；`RawMessage` 类不变（但不再继承 `Message`，`ReceivedMessage` 也是）；取消冗余的 `SectionMessage` 和 `SendingMessage`；`ReceivedMessage` 保留 `Raw`、`Sections`（可能改为 `SectionMessage`，毕竟 `Message` 实现了 `IEnumerable`）等属性，可判等；其余的消息是否可判等我还没想好；均实现 `IMessage` 接口。

- 支持异步的事件处理器。
- 将一些 Type 改为枚举。使用 `EnumMember` 特性标记。
- 反序列化时传入静态的 setting 或者 serializer，避免潜在的全局 setting 影响。

- 加入运行平台选项，发送本地图片时，如果检测到和酷 Q 运行在不同的机器上，可以尝试先读取，再以 base64 的方式发送，以便多机使用。

## 已取消
- 使得 `Post.AnonymousInfo` 可以比较。