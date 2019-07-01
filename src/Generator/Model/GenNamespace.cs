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
        public GenReference Root { get; set; }

        public IEnumerable<GenReference> UsedFragments
        {
            get { return SelectionSets.SelectMany(t => t.RefFragments).GroupBy(g => g.SelectionSet).Select(s => s.First()); }
        }

        public GenSelectionSet GetSelectionSet(IGraphType graphType, INode node)
        {
            if(graphType == null)
                return null;
            var selectionSet = new GenSelectionSet { Namespace = this, Name = graphType.Name, GraphType = graphType, Node=node };
            if(!selectionSet.MapsToBuildInType)
                SelectionSets.Add(selectionSet);
            return selectionSet;
        }

        public GenSelectionSet FindSelectionSet(string name) => SelectionSets.Find(t => t.Name == name);
    }
}