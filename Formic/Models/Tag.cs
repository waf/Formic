using System;
using System.Collections.Generic;

namespace Formic
{
    //TODO: use this when we figure out many-to-many support
    public partial class Tag
    {
        public Guid Id { get; set; }
        public string TagName { get; set; }
        public virtual List<PostTag> PostTags { get; set; }
    }

    public class PostTag
    {
        public int PostId { get; set; }
        public Post Post { get; set; }

        public Guid TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
