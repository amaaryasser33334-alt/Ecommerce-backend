using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.user
{
    public class UserQueryParameters
    {
            public string? Search { get; set; }

            public int PageNumber { get; set; } = 1;

            private int _pageSize = 10;

            public int PageSize
            {
                get
                {
                    return _pageSize;
                }
            set
            {
                if (value > 50)
                {
                    _pageSize = 50;
                }
                else if (value < 1)
                {
                    _pageSize = 10;
                }
                else
                {
                    _pageSize = value;
                }

            }
        }
    }
}
