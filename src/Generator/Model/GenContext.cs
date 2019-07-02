using System.Collections.Generic;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQLGen
{
    class GenContext
    {
        public GenContext()
        {
            Fragments = new GenNamespace { Name = "Fragments", Context = this };
        }

        public ISchema Schema { get; set; }
        public Document Document { get; set; }
        public GenNamespace Fragments { get; }
        public List<GenNamespace> Namespaces { get; } = new List<GenNamespace>();
        public GenConfig Config { get; set; }
    }
}