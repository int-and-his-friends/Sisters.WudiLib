using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Sisters.WudiLib.Builders.Annotations;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.Builders
{
#nullable enable
    internal class PostTreeNode
    {
        private readonly Type _type;
        private readonly ISet<string> _definedKeys;
        private readonly IReadOnlyDictionary<string, string> _fields;
        private readonly LinkedList<PostTreeNode> _children = new();
        private bool _isSpecified;

        /// <summary>
        /// 从上报数据类型构造 <see cref="PostTreeNode"/>。
        /// </summary>
        /// <param name="type">上报数据类型。</param>
        /// <param name="isSpecified">是否要初始化此类型。</param>
        /// <exception cref="WudiLibBuilderException">构造时出现异常。</exception>
        /// <exception cref="ArgumentException"><c>type</c> 不是 <see cref="Post"/> 的子类。</exception>
        public PostTreeNode(Type type, bool isSpecified = true)
        {
            if (!typeof(Post).IsAssignableFrom(type))
            {
                throw new ArgumentException($"传入的类型必须是 {typeof(Post).FullName} 的子类。", nameof(type));
            }
            if (isSpecified && type.IsAbstract)
            {
                throw new ArgumentException($"无法接受抽象类型 {type.FullName}。", nameof(type));
            }
            _type = type;
            var attributes = type.GetCustomAttributes<PostAttribute>(false);
            var definedKeys = attributes.Select(a => a.Field);
            _definedKeys = new HashSet<string>();
            foreach (var fieldName in definedKeys)
            {
                if (!_definedKeys.Add(fieldName))
                {
                    throw new WudiLibBuilderException($"在上报类型 {type.FullName} 中重复出现了字段 {fieldName} 的约束。");
                }
            }
            _fields = attributes.ToDictionary(a => a.Field, a => a.Value);
            _isSpecified = isSpecified;
        }

        /// <summary>
        /// 把相应的 PostTreeNode 直接添加到此 PostTreeNode 下面。
        /// </summary>
        /// <param name="node"></param>
        private void AddNodeToThis(PostTreeNode node)
        {
            Debug.Assert(node._type.BaseType == _type);

            // 在链表 _children 中，指定了更多字段（更具体）的上报类型应该排在更前面。
            for (var llNode = _children.First; llNode != null; llNode = llNode.Next)
            {
                var existingNoMoreGeneric = IsNoMoreGeneric(llNode.Value._definedKeys, node._definedKeys);
                if (existingNoMoreGeneric == true)
                {
                    // 当前遍历到的更具体（或同样具体），因此检查是否定义了完全一样的字段。
                    if (node._definedKeys.SetEquals(llNode.Value._definedKeys))
                    {
                        bool identical = true;
                        foreach (var kvp in node._fields)
                        {
                            if (llNode.Value._fields[kvp.Key] != kvp.Value)
                            {
                                identical = false;
                                break;
                            }
                        }
                        if (identical)
                            throw new WudiLibBuilderException($"Types {node._type.FullName} and {llNode.Value._type.FullName} have identical field constraint.");
                    }
                    continue;
                }
                if (existingNoMoreGeneric == null)
                    throw new WudiLibBuilderException($"Types {node._type.FullName} and {llNode.Value._type.FullName} have conflict.");

                _children.AddBefore(llNode, node);
                return;
            }
            _children.AddLast(node);
        }

        public void AddType(Type type)
        {
            if (!type.IsSubclassOf(_type))
            {
                throw new ArgumentException("必须添加指示的子类。", nameof(type));
            }
            if (type == _type)
            {
                _isSpecified = _isSpecified
                    ? throw new InvalidOperationException("此类型已经添加，不能再次添加。")
                    : true;
            }

            // 获取从当前类型到要添加类型的继承链。
            var chain = new Stack<Type>();
            for (Type t = type; t != _type; t = t.BaseType)
            {
                chain.Push(t);
            }

            // 添加缺失类型并获取代表要添加的类型的 PostTreeNode。
            var deepest = this;
            while (chain.Count > 0)
            {
                var current = chain.Pop();
                var node = deepest._children.FirstOrDefault(n => n._type == current);
                if (node == null)
                {
                    node = new PostTreeNode(current, false);
                    deepest.AddNodeToThis(node);
                }

                deepest = node;
            }

            // 最深的节点就是要添加的类型对应的 PostTreeNode。
            deepest.AddType(type);
        }

        /// <summary>
        /// 传入两组字段名，判断 <c>compared</c> 是否不如 <c>comparing</c>
        /// 范围广。如果包含的字段少，则范围更广。
        /// </summary>
        /// <param name="compared">被比较的。</param>
        /// <param name="comparing">比较基准。</param>
        /// <returns>如果前者范围更广或者相同，则为 <c>true</c>；如果后者范围更广，则为
        /// <c>false</c>；否则为 <c>null</c>。</returns>
        public static bool? IsNoMoreGeneric(
            ISet<string> compared,
            ISet<string> comparing)
        {
            if (comparing.IsSubsetOf(compared))
                return true;
            if (comparing.IsSupersetOf(compared))
                return false;
            return null;
        }
    }
#nullable restore
}
