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
                var genType = context.Fragments.CreateSelectionSet(graphType, fragment);
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

            var sharpType = ns.CreateSelectionSet(context.Schema.Query, context.Document);
            Run(context, operation.SelectionSet, sharpType);
            ns.Root = sharpType;

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

                            IGenReference Unfold(IGraphType g)
                            {
                                switch(g)
                                {
                                    case GraphQLTypeReference reference:
                                    return Unfold(context.Schema.FindType(reference.TypeName));
                                    case NonNullGraphType nonNull:
                                    return new GenNonNull(Unfold(nonNull.ResolvedType));
                                    case ListGraphType list:
                                    return new GenList(Unfold(list.ResolvedType));
                                    default:
                                    return selectionSet.Namespace.CreateSelectionSet(g, field);
                                }

                            }

                            var propertyType = Unfold(fieldType.ResolvedType);
                            selectionSet.AddField(field.Alias ?? field.Name, propertyType);

                            Run(context, field.SelectionSet, propertyType.GetSelectionSet());

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