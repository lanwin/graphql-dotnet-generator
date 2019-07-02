using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQLGen
{
    class GenNamespace
    {
        public string Name { get; set; }
        public List<GenSelectionSet> SelectionSets { get; } = new List<GenSelectionSet>();
        public Operation Operation { get; set; }
        public GenSelectionSet Root { get; set; }
        public GenContext Context { get; set; }

        public IEnumerable<GenSelectionSet> UsedFragments
        {
            get { return SelectionSets.SelectMany(t => t.RefFragments).GroupBy(g => g).Select(s => s.First()); }
        }

        public GenSelectionSet CreateSelectionSet(IGraphType graphType, INode node)
        {
            if(graphType == null)
                return null;
            var selectionSet = new GenSelectionSet { Namespace = this, Name = graphType.Name, GraphType = graphType, Node=node };
            if(!selectionSet.MapsToBuildInType)
                SelectionSets.Add(selectionSet);
            return selectionSet;
        }

        public GenSelectionSet FindSelectionSet(string name) => SelectionSets.Find(t => t.Name == name);

        public IGenReference CreateReference(IGraphType graphType, INode node)
        {
            switch(graphType)
            {
                case GraphQLTypeReference reference:
                    return CreateReference(Context.Schema.FindType(reference.TypeName), node);
                case NonNullGraphType nonNull:
                    return new GenNonNull(CreateReference(nonNull.ResolvedType, node));
                case ListGraphType list:
                    return new GenList(CreateReference(list.ResolvedType, node));
                default:
                    return CreateSelectionSet(graphType, node);
            }
        }
    }
}