using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Formic
{
    public partial class Blog
    {
        public Blog()
        {
            Post = new HashSet<Post>();
        }

        [UIHint("ID")]
        public int PK { get; set; }

        [Display(Name = "Blog Name")]
        public string Name { get; set; }

        public virtual ICollection<Post> Post { get; set; }

        public override string ToString() => Name;
    }
}
