using System;
using System.Collections.Generic;

namespace Formic
{
    public class Author
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Post> Posts { get; internal set; }

        public override string ToString() => Name;
    }
}
