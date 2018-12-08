using System.Threading.Tasks;

namespace Sisters.WudiLib.Api
{
    public static class CqHttpClientGroupExtensions
    {
        private const string KickGroupMemberAction = "set_group_kick";

        /// <summary>
        /// 群组踢人。
        /// </summary>
        /// <param name="groupId">群号。</param>
        /// <param name="userId">要踢的 QQ 号。</param>
        /// <returns>是否成功。注意：酷 Q 未处理错误，所以无论是否成功都会返回<c>true</c>。</returns>
        public static async Task<bool> KickGroupMemberAsync(this ICqHttpClient cqHttpClient, long groupId, long userId)
        {
            var data = new
            {
                group_id = groupId,
                user_id = userId,
            };
            var success = await cqHttpClient.CallAsync<object>(KickGroupMemberAction, data);
            return success.IsOk;
        }
    }
}
