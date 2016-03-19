using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formic.Models
{
	public class RecordSet
	{
		public IProperty[] Properties { get; set; }
		public INavigation[] NavigationProperties { get; set; }
		public object[] Data { get; set; }
	}

	public class RecordSetT<T>
	{
		public IProperty[] Properties { get; set; }
		public INavigation[] NavigationProperties { get; set; }
		public T[] Data { get; set; }
	}
}
