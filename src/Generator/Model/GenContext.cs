using System.Collections.Generic;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQLGen
{
    class GenContext
    {
        public ISchema Schema { get; set; }
        public Document Document { get; set; }
        public GenNamespace Fragments { get; } = new GenNamespace { Name = "Fragments" };
        public List<GenNamespace> Namespaces { get; } = new List<GenNamespace>();
        public GenConfig Config { get; set; }
    }
}