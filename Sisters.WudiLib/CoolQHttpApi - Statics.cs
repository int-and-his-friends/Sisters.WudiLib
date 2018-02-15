using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib
{
    partial class CoolQHttpApi
    {
        private static readonly string PrivatePath = "CoolQHttpApi";

        private string PrivateUrl => apiAddress + PrivatePath;
    }
}
