using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib
{
    public partial class CoolQHttpApi : IQq
    {
        private string apiAddress;

        public string ApiAddress
        {
            get => apiAddress;
            set => apiAddress = value.TrimEnd('/');
        }

        
    }
}
