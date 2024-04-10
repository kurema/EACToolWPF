using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EACToolWpf
{
	public class RegexEntry
	{
		public RegexEntry()
		{
		}

		public RegexEntry([System.Diagnostics.CodeAnalysis.StringSyntax(System.Diagnostics.CodeAnalysis.StringSyntaxAttribute.Regex)] string regexFrom, string regexTo)
		{
			RegexFrom = regexFrom ?? throw new ArgumentNullException(nameof(regexFrom));
			RegexTo = regexTo ?? throw new ArgumentNullException(nameof(regexTo));
		}

		public string RegexFrom { get; set; } = string.Empty;
		public string RegexTo { get; set; } = string.Empty;
	}
}
