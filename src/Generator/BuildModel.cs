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

            var rootSet = ns.CreateSelectionSet(context.Schema.Query, context.Document);
            Run(context, operation.SelectionSet, rootSet);
            ns.Root = rootSet;
            
            foreach(var variable in operation.Variables)
            {
                var name = variable.Type.Name();
                var type = context.Schema.FindType(name);
                var reference = ns.CreateReference(type, variable.NameNode);
                rootSet.AddVariable(variable.Name, reference);
            }

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