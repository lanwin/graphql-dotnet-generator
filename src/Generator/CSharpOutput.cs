using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphQL.Types;

namespace GraphQLGen
{
    static class CSharpOutput
    {
        public static string RenderSingleFile(GenContext context)
        {
            var text = new StringBuilder();
            text.AppendLine("using System;");
            text.AppendLine("using GraphQL.Client;");
            text.AppendLine("using Newtonsoft.Json.Linq;");
            text.AppendLine("using GraphQL.Common.Request;");
            text.AppendLine("using System.Threading.Tasks;");

            RenderNamespace(text, context, context.Fragments);

            foreach(var ns in context.Namespaces)
            {
                RenderNamespace(text, context, ns);
            }

            RenderClient(text, context);

            return text.ToString();
        }

        static void RenderClient(StringBuilder text, GenContext context)
        {
            text.AppendLine();
            text.AppendLine("namespace " + context.Config.Namespace);
            text.AppendLine("{");

            text.AppendLine("  public class " + context.Config.ClientName);
            text.AppendLine("  {");
            text.AppendLine("    readonly GraphQLClient _client;");

            text.AppendLine("    public " + context.Config.ClientName + "(string url)");
            text.AppendLine("    {");
            text.AppendLine("      _client = new GraphQLClient(url);");
            text.AppendLine("    }");

            foreach(var ns in context.Namespaces)
            {
                var name = (ns.Name ?? "Query").Replace(".", "");
                var variableName = "Query" + UpperCaseFirst(name);

                text.AppendLine("    const string " + variableName + " = @\"");

                foreach(var fragment in ns.UsedFragments)
                {
                    text.AppendLine("      " + AstPrinterExtended.Print(fragment.Node).Replace("\"", "\"\"").Replace("\n", Environment.NewLine + "      "));
                }

                text.AppendLine("      " + AstPrinterExtended.Print(ns.Operation).Replace("\"", "\"\"").Replace("\n", Environment.NewLine + "      "));
                text.AppendLine("    \";");

                var typeName = TypeReferenceName(context, ns.Root, true);

                var variables = string.Join(", ", ns.Root.Varaibles.Select(v => LowerCaseFirst(v.Name)));
                var variableDeclaration = string.Join(", ", ns.Root.Varaibles.Select(v => TypeReferenceName(context, v.Type) + " " + LowerCaseFirst(v.Name)));

                text.AppendLine("    public async Task<" + typeName + "> " + name + "(" + variableDeclaration + ")");
                text.AppendLine("    {");
                text.AppendLine("      var response = await _client.PostAsync(new GraphQLRequest()");
                text.AppendLine("      {");
                text.AppendLine("        OperationName = \"" + ns.Operation.Name + "\",");

                if(variables.Length > 0)
                    text.AppendLine("        Variables = new {" + variables + "},");


                text.AppendLine("        Query = " + variableName);
                text.AppendLine("      });");
                text.AppendLine("      return ( (JObject)response.Data ).ToObject<" + typeName + ">();");
                text.AppendLine("    }");
            }

            text.AppendLine("  }");

            text.AppendLine("}");
        }

        static void RenderNamespace(StringBuilder text, GenContext context, GenNamespace ns)
        {
            text.AppendLine();
            text.AppendLine("namespace " + NamespaceDeclarionName(context, ns));
            text.AppendLine("{");

            var useInterface = ns == context.Fragments;
            foreach(var genType in ns.SelectionSets)
            {
                if(useInterface)
                    RenderInterface(text, context, genType);
                else
                    RenderClass(text, context, genType);
            }

            text.AppendLine("}");
        }

        static void RenderClass(StringBuilder text, GenContext context, GenSelectionSet selectionSet)
        {
            text.Append("  public class " + TypeName(selectionSet));

            if(selectionSet.RefFragments.Any())
            {
                text.Append(" : ");
                text.AppendJoin(", ", selectionSet.RefFragments.Select(f => TypeReferenceName(context, f, true)));
            }

            text.AppendLine();
            text.AppendLine("  {");

            foreach(var fragment in selectionSet.RefFragments)
            {
                foreach(var property in fragment.Fields)
                {
                    text.AppendLine("    public " + TypeReferenceName(context, property.Type) + " " + UpperCaseFirst(property.Name) + " " + PropertyBody(context));
                }
            }

            foreach(var property in selectionSet.Fields)
            {
                text.AppendLine("    public " + TypeReferenceName(context, property.Type) + " " + UpperCaseFirst(property.Name) + " " + PropertyBody(context));
            }

            text.AppendLine("  }");
        }

        static void RenderInterface(StringBuilder text, GenContext context, GenSelectionSet selectionSet)
        {
            text.AppendLine("  public interface " + TypeName(selectionSet));
            text.AppendLine("  {");

            foreach(var property in selectionSet.Fields)
            {
                text.AppendLine("    " + TypeReferenceName(context, property.Type) + " " + UpperCaseFirst(property.Name) + " " + PropertyBody(context));
            }

            text.AppendLine("  }");
        }

        static string PropertyBody(GenContext context)
            => context.Config.ReadOnly ? "{get;}" : "{get; set;}";

        static string UpperCaseFirst(string name) => char.ToUpperInvariant(name[0]) + name.Substring(1);
        static string LowerCaseFirst(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);

        static string NamespaceDeclarionName(GenContext context, GenNamespace ns)
        {
            var name = context.Config.Namespace + ".Model";
            if(!string.IsNullOrWhiteSpace(ns.Name))
                name += "." + ns.Name;
            return name;
        }

        static string TypeReferenceName(GenContext context, IGenReference reference, bool fullname = false)
        {
            switch(reference)
            {
                case GenSelectionSet set:
                    {
                        var name = TypeName(set);

                        if(fullname)
                            name = "global::" + NamespaceDeclarionName(context, set.Namespace) + "." + name;

                        return name;
                    }

                case GenList list:
                    {
                        var element = TypeReferenceName(context, list.Element, fullname);
                        return ClrTypeFullName(typeof(List<>)) + "<" + element + ">";
                    }

                case GenNonNull nonNull:
                    {
                        return TypeReferenceName(context, nonNull.Element, fullname);
                    }
                default:
                throw new NotImplementedException(reference.GetType().FullName);
            }
        }

        static string TypeName(GenSelectionSet selectionSet)
        {
            var name = selectionSet.Name;

            var clrType = GetClrType(selectionSet);
            if(clrType != null)
                name = ClrTypeFullName(clrType);

            name = UpperCaseFirst(name);
            return name;
        }


        static Type GetClrType(GenSelectionSet selectionSet)
        {
            switch(selectionSet.GraphType)
            {
                case IntGraphType _:
                return typeof(long);
                case FloatGraphType _:
                return typeof(double);
                case DecimalGraphType _:
                return typeof(decimal);
                case StringGraphType _:
                return typeof(string);
                case BooleanGraphType _:
                return typeof(bool);
                case DateGraphType _:
                return typeof(DateTime);
                case DateTimeOffsetGraphType _:
                return typeof(DateTimeOffset);
                case TimeSpanSecondsGraphType _:
                return typeof(TimeSpan);
                case IdGraphType _:
                return typeof(string);
                case ShortGraphType _:
                return typeof(short);
                case UShortGraphType _:
                return typeof(ushort);
                case ULongGraphType _:
                return typeof(ulong);
                case UIntGraphType _:
                return typeof(uint);
            }
            return null;
        }

        static string ClrTypeFullName(Type clrType)
        {
            var name = clrType.Namespace == "System" ? clrType.Name : clrType.FullName;
            var gen = name.IndexOf('`');
            if(gen != -1)
                name = name.Substring(0, gen);
            return name;
        }
    }
}