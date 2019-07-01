using System;
using System.Threading.Tasks;
using Pokemon.GraphQL;
using Pokemon.GraphQL.ModelFragments;

namespace GraphQLGen.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new PokemonClient("https://graphql-pokemon.now.sh/graphql");
            var data = await client.GetQuery();

            var name = (IPokemonName)data.Pokemon;
            Console.WriteLine(name.Name);

            var data2 = await client.GetQueryOnlyName();
            Console.WriteLine(data2.Pokemon.Name);
        }
    }
}
