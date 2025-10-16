using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application_lib.Gitlab
{
    public interface ITextFormatterService
    {
        public string FormatTextToHtml(string? text);
    }
}
