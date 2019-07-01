using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GraphQL.Execution;
using GraphQL.Utilities;
using Newtonsoft.Json;

namespace GraphQLGen
{
    class Program
    {
        static void Main(string[] args)
        {
            var configFile = args.FirstOrDefault() ?? "generator.json";
            var config = JsonConvert.DeserializeObject<GenConfig>(File.ReadAllText(configFile, Encoding.UTF8));

            var schemaString = File.ReadAllText(config.Schema, Encoding.UTF8);

            // remove """ style comments 
            schemaString = Regex.Replace(schemaString, "\"\"\"(.|(\\r ?\\n))*?\"\"\"", "", RegexOptions.Multiline);

            var document = new GraphQLDocumentBuilder().Build(File.ReadAllText(config.GraphQLFile, Encoding.UTF8));
            var schema = new SchemaBuilder().Build(schemaString);

            var context = new GenContext
            {
                Schema = schema,
                Document = document,
                Config = config
            };

            BuildModel.Run(context);

            CSharpTransformer.Apply(context);
            var text = CSharpOutput.RenderSingleFile(context);

            File.WriteAllText(config.OutputFile, text, Encoding.UTF8);

            Console.WriteLine("Output file {0}", config.OutputFile);
        }

    }
}
