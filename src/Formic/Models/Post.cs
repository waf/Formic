using System;
using System.Collections.Generic;

namespace Formic
{
    public partial class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }

        public int BlogPK { get; set; }
        public virtual Blog Blog { get; set; }

        public Guid AuthorFK { get; set; }
        public virtual Author Author { get; set; }
        public virtual List<PostTag> PostTags { get; set; }
    }
}
