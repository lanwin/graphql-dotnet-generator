using System;
using System.Collections.Generic;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQLGen
{
    class GenSelectionSet
    {
        public INode Node { get; set; }
        public IGraphType GraphType { get; set; }
        public string Name { get; set; }
        public GenNamespace Namespace { get; set; }
        public List<GenField> Fields { get; } = new List<GenField>();
        public List<GenReference> RefFragments { get; } = new List<GenReference>();

        public void AddField(string name, GenReference reference)
        {
            var existingField = Fields.Find(p => p.Name == name);
            if(existingField != null)
                return;

            Fields.Add(new GenField
            {
                Name = name,
                SelectionSet = this,
                Reference = reference
            });
        }

        public bool MapsToBuildInType
        {
            get
            {
                switch(GraphType)
                {
                    case IntGraphType _:
                    case FloatGraphType _:
                    case DecimalGraphType _:
                    case StringGraphType _:
                    case BooleanGraphType _:
                    case DateGraphType _:
                    case DateTimeOffsetGraphType _:
                    case TimeSpanSecondsGraphType _:
                    case IdGraphType _:
                    case ShortGraphType _:
                    case UShortGraphType _:
                    case ULongGraphType _:
                    case UIntGraphType _:
                    return true;
                    default:
                    return false;
                }
            }
        }
        
        public void AddFragment(GenSelectionSet fragment)
            => RefFragments.Add(new GenReference { SelectionSet = fragment });

        public override string ToString()
            => Name;

        public GenSelectionSet GetSelectionSet(IGraphType graphType, INode node)
            => Namespace.GetSelectionSet(graphType, node);

        public GenReference GetReference() =>
            new GenReference
            {
                SelectionSet = this
            };
    }
}