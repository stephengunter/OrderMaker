using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Views
{
	public class BaseOption
	{
		public string Value { get; set; }
		public string Text { get; set; }

		public override string ToString() => Text;
    }
}
