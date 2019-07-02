﻿using System;
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
            var operations = context.Namespaces.Select(ns => new
            {
                ns.Root,
                Fragments = ns.UsedFragments,
                VariableName = ("Query" + ns.Name).Replace(".", ""),
                ns.Operation
            }).ToArray();


            text.AppendLine();
            text.AppendLine("namespace " + context.Config.Namespace);
            text.AppendLine("{");

            text.AppendLine("  public class " + context.Config.ClientName);
            text.AppendLine("  {");
            text.AppendLine("    readonly GraphQLClient _client;");

            foreach(var operation in operations)
            {
                text.AppendLine("    const string " + operation.VariableName + " = @\"");

                foreach(var fragment in operation.Fragments)
                {
                    text.AppendLine("      " + AstPrinterExtended.Print(fragment.SelectionSet.Node).Replace("\"", "\"\"").Replace("\n", Environment.NewLine + "      "));
                }

                text.AppendLine("      " + AstPrinterExtended.Print(operation.Operation).Replace("\"", "\"\"").Replace("\n", Environment.NewLine + "      "));
                text.AppendLine("    \";");
            }

            text.AppendLine("    public " + context.Config.ClientName + "(string url)");
            text.AppendLine("    {");
            text.AppendLine("      _client = new GraphQLClient(url);");
            text.AppendLine("    }");

            foreach(var operation in operations)
            {
                var typeName = TypeReferenceName(context, operation.Root, true);
                text.AppendLine("    public async Task<" + typeName + "> Get" + operation.VariableName + "()");
                text.AppendLine("    {");
                text.AppendLine("      var response = await _client.PostAsync(new GraphQLRequest()");
                text.AppendLine("      {");
                text.AppendLine("        OperationName = \"" + operation.Operation.Name + "\",");
                text.AppendLine("        Query = " + operation.VariableName);
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
                foreach(var property in fragment.SelectionSet.Fields)
                {
                    text.AppendLine("    public " + TypeReferenceName(context, property.Reference) + " " + CamelCase(property.Name) + " " + PropertyBody(context));
                }
            }

            foreach(var property in selectionSet.Fields)
            {
                text.AppendLine("    public " + TypeReferenceName(context, property.Reference) + " " + CamelCase(property.Name) + " " + PropertyBody(context));
            }

            text.AppendLine("  }");
        }

        static void RenderInterface(StringBuilder text, GenContext context, GenSelectionSet selectionSet)
        {
            text.AppendLine("  public interface " + TypeName(selectionSet));
            text.AppendLine("  {");

            foreach(var property in selectionSet.Fields)
            {
                text.AppendLine("    " + TypeReferenceName(context, property.Reference) + " " + CamelCase(property.Name) + " " + PropertyBody(context));
            }

            text.AppendLine("  }");
        }

        static string PropertyBody(GenContext context)
            => context.Config.ReadOnly ? "{get;}" : "{get; set;}";

        static string CamelCase(string name) => char.ToUpperInvariant(name[0]) + name.Substring(1);

        static string NamespaceDeclarionName(GenContext context, GenNamespace ns)
        {
            var name = context.Config.Namespace + ".Model";
            if(!string.IsNullOrWhiteSpace(ns.Name))
                name += "." + ns.Name;
            return name;
        }

        static string TypeReferenceName(GenContext context, GenReference reference, bool fullname = false)
        {
            var name = TypeName(reference.SelectionSet);

            if(fullname)
                name = "global::" + NamespaceDeclarionName(context, reference.SelectionSet.Namespace) + "." + name;

            if(reference.IsList)
                name = ClrTypeFullName(typeof(List<>)) + "<" + name + ">";

            return name;
        }

        static string TypeName(GenSelectionSet selectionSet)
        {
            var name = selectionSet.Name;

            var clrType = GetClrType(selectionSet);
            if(clrType != null)
                name = ClrTypeFullName(clrType);

            name = CamelCase(name);
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