# 无敌lib
为方便 C# 调用酷 Q [HTTP API](https://github.com/richardchien/coolq-http-api) 插件而开发的lib。

- Named by [int100](https://github.com/1004121460)

## 如何使用
### 发送消息、调用 API
``` C#
var httpApi = new HttpApiClient();
httpApi.ApiAddress = "http://127.0.0.1:5700/";
var privateResponse = await httpApi.SendPrivateMessageAsync(12345678, "hello");
```
### 监听事件
``` C#
var listener = new ApiPostListener();
listener.ApiClient = httpApi; // 上面所示客户端的实例，除了作为参数传给事件处理器之外并没有什么卵用。
listener.PostAddress = "http://127.0.0.1:8080/";
listener.ForwardTo = "http://[::1]:10202"; // 转发路径，监听到的事件会被转发到此处。
listener.StartListen();
listener.AnonymousMessageEvent += ApiPostListener.RepeatAsync; // 复读匿名消息。
```

## 其他
暂时不支持认证和签名。