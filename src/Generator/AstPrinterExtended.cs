using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Utilities;

namespace GraphQLGen
{
    static class AstPrinterExtended
    {
        static readonly AstPrintVisitor Visitor = new AstPrintVisitor();

        static AstPrinterExtended()
        {
            // Current master AstPrinter ignores fragments and spreads

            string Join(IEnumerable<object> nodes, string separator)
            {
                return nodes != null
                    ? string.Join(
                        separator,
                        nodes.Where(n => n != null)
                            .Where(n => !string.IsNullOrWhiteSpace(n.ToString()))
                            .Select(n => n.ToString()))
                    : "";
            }

            Visitor.Config<FragmentDefinition>(c =>
            {
                c.Field(x => x.Directives);
                c.Field(x => x.SelectionSet);
                c.Field(x => x.Name);
                c.Field(x => x.Type);
                c.Print(p =>
                {
                    var directives = Join(p.ArgArray(x => x.Directives), " ");
                    var selectionSet = p.Arg(x => x.SelectionSet);
                    var name = p.Arg(x => x.Name);
                    var typename = p.Arg(x => x.Type);
                    var body = string.IsNullOrWhiteSpace(directives)
                        ? selectionSet
                        : Join(new[] { directives, selectionSet }, " ");

                    return $"fragment {name} on {typename} {body}";
                });
            });
            Visitor.Config<FragmentSpread>(c =>
            {
                c.Field(x => x.Directives);
                c.Field(x => x.Name);
                c.Print(p =>
                {
                    //Todo: not sure how to print directives here
                    //var directives = Join(p.ArgArray(x => x.Directives), " ");
                    var name = p.Arg(x => x.Name);
                    return $"... {name}";
                });
            });
        }

        public static string Print(INode node)
        {
            var result = Visitor.Visit(node);
            return result?.ToString() ?? string.Empty;
        }
    }
}