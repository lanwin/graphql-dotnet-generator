using System;
using System.Linq;
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
                Name = operation.Name,
                Context = context
            };

            var rootType = context.Schema.FindType(ns.Operation.OperationType.ToString());
            var rootSet = ns.CreateSelectionSet(rootType, context.Document);
            Run(context, operation.SelectionSet, rootSet);
            ns.Root = rootSet;

            foreach(var variable in operation.Variables)
            {
                var graphType = context.Schema.FindType(variable.Type.Name());
                var reference = ns.CreateReference(graphType, variable.NameNode);
                rootSet.AddVariable(variable.Name, reference);
                Run(reference.GetSelectionSet());
            }

            context.Namespaces.Add(ns);
        }

        static void Run(GenSelectionSet set)
        {
            switch(set.GraphType)
            {
                case IComplexGraphType complexType:
                    {
                        foreach(var fieldType in complexType.Fields)
                        {
                            var fieldReference = set.Namespace.CreateReference(fieldType.ResolvedType, null);
                            set.AddField(fieldType.Name, fieldReference);
                            Run(fieldReference.GetSelectionSet());
                        }
                        break;
                    }
                default:
                return;
            }
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

                            var propertyType = selectionSet.Namespace.CreateReference(fieldType.ResolvedType, field);
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