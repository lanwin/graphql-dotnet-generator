using System;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQLGen
{
    static class BuildModel
    {
        public static void Run(GenContext context)
        {
            foreach(var fragment in context.Document.Fragments)
            {
                var graphType = fragment.Type.GraphTypeFromType(context.Schema);
                var genType = context.Fragments.GetSelectionSet(graphType, fragment);
                genType.Name = fragment.Name;
                Run(context, fragment.SelectionSet, genType);
            }

            foreach(var operation in context.Document.Operations)
                Run(context, operation);
        }

        static void Run(GenContext context, Operation operation)
        {
            var ns = new GenNamespace
            {
                Operation = operation,
                Name = operation.Name
            };

            var sharpType = ns.GetSelectionSet(context.Schema.Query, context.Document);
            Run(context, operation.SelectionSet, sharpType);
            ns.Root = sharpType.GetReference();

            context.Namespaces.Add(ns);
        }

        static void Run(GenContext context, SelectionSet set, GenSelectionSet selectionSet)
        {
            foreach(var selection in set.Selections)
            {
                switch(selection)
                {
                    case Field field:
                    {
                        var fieldType = ((ObjectGraphType)selectionSet.GraphType).GetField(field.Name);
                        var graphType = fieldType.ResolvedType;

                        if(graphType is NonNullGraphType nonNull)
                            graphType = nonNull.ResolvedType;

                        if(graphType is GraphQLTypeReference typeRef)
                            graphType = context.Schema.FindType(typeRef.TypeName);

                        var realType = graphType;
                        if(graphType is ListGraphType listType)
                            realType = listType.ResolvedType;
                            
                        var propertyType = selectionSet.GetSelectionSet(realType, field);
                        var references = propertyType.GetReference();
                        if(graphType is ListGraphType)
                            references.IsList = true;
                        selectionSet.AddField(field.Alias ?? field.Name, references);

                        Run(context, field.SelectionSet, propertyType);

                        break;
                    }

                    case FragmentSpread fragment:
                    {
                        var fragmentType = context.Fragments.SelectionSets.Find(t => t.Name == fragment.Name);
                        selectionSet.AddFragment(fragmentType);
                        break;
                    }

                    case InlineFragment inline:
                    {
                        throw new NotImplementedException();
                    }

                    default:
                        throw new InvalidOperationException("Uh whats that?");
                }
            }
        }
    }
}