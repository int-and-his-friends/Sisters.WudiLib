using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib
{
    partial class CoolQHttpApi
    {
        private static readonly string PrivatePath = "/send_private_msg";
        private static readonly string GroupPath = "/send_group_msg";
        private static readonly string DiscussPath = "/send_discuss_msg";
        private static readonly string KickGroupMemberPath = "/set_group_kick";
        private static readonly string RecallPath = "/delete_msg";
        private static readonly string LoginInfoPath = "/get_login_info";

        private string PrivateUrl => apiAddress + PrivatePath;
        private string GroupUrl => apiAddress + GroupPath;
        private string DiscussUrl => apiAddress + DiscussPath;
        private string KickGroupMemberUrl => apiAddress + KickGroupMemberPath;
        private string RecallUrl => apiAddress + RecallPath;
        private string LoginInfoUrl => apiAddress + LoginInfoPath;
    }
}
