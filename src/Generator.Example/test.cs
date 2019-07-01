using System;
using GraphQL.Client;
using Newtonsoft.Json.Linq;
using GraphQL.Common.Request;
using System.Threading.Tasks;

namespace Pokemon.GraphQL.ModelFragments
{
  public interface IPokemonName
  {
    String Name {get; set;}
  }
}

namespace Pokemon.GraphQL.Model
{
  public class Query
  {
    public Pokemon1 Pokemon {get; set;}
  }
  public class Pokemon1 : global::Pokemon.GraphQL.ModelFragments.IPokemonName
  {
    public String Name {get; set;}
    public String Id {get; set;}
    public String Number {get; set;}
    public PokemonAttack1 Attacks {get; set;}
    public System.Collections.Generic.List<Pokemon2> Evolutions {get; set;}
  }
  public class PokemonAttack1
  {
    public System.Collections.Generic.List<Attack1> Special {get; set;}
  }
  public class Attack1
  {
    public String Name {get; set;}
    public String Type {get; set;}
    public Int64 Damage {get; set;}
  }
  public class Pokemon2
  {
    public String Id {get; set;}
    public String Number {get; set;}
    public String Name {get; set;}
    public PokemonDimension Weight {get; set;}
    public PokemonAttack2 Attacks {get; set;}
  }
  public class PokemonDimension
  {
    public String Minimum {get; set;}
    public String Maximum {get; set;}
  }
  public class PokemonAttack2
  {
    public System.Collections.Generic.List<Attack2> Fast {get; set;}
  }
  public class Attack2
  {
    public String Type {get; set;}
    public Int64 Damage {get; set;}
  }
}

namespace Pokemon.GraphQL.ModelOnlyName
{
  public class Query
  {
    public Pokemon Pokemon {get; set;}
  }
  public class Pokemon : global::Pokemon.GraphQL.ModelFragments.IPokemonName
  {
    public String Name {get; set;}
  }
}

namespace Pokemon.GraphQL
{
  public class PokemonClient
  {
    readonly GraphQLClient _client;
    const string Query = @"
      fragment pokemonName on Pokemon {
        name
      }
      {
        pokemon(name: ""Pikachu"") {
          id
          number
          ... pokemonName
          attacks {
            special {
              name
              type
              damage
            }
          }
          evolutions {
            id
            number
            name
            weight {
              minimum
              maximum
            }
            attacks {
              fast {
                type
                damage
              }
            }
          }
        }
      }
    ";
    const string QueryOnlyName = @"
      fragment pokemonName on Pokemon {
        name
      }
      query OnlyName {
        pokemon(name: ""Pikachu"") {
          ... pokemonName
        }
      }
    ";
    public PokemonClient(string url)
    {
      _client = new GraphQLClient(url);
    }
    public async Task<global::Pokemon.GraphQL.Model.Query> GetQuery()
    {
      var response = await _client.PostAsync(new GraphQLRequest()
      {
        OperationName = "",
        Query = Query
      });
      return ( (JObject)response.Data ).ToObject<global::Pokemon.GraphQL.Model.Query>();
    }
    public async Task<global::Pokemon.GraphQL.ModelOnlyName.Query> GetQueryOnlyName()
    {
      var response = await _client.PostAsync(new GraphQLRequest()
      {
        OperationName = "OnlyName",
        Query = QueryOnlyName
      });
      return ( (JObject)response.Data ).ToObject<global::Pokemon.GraphQL.ModelOnlyName.Query>();
    }
  }
}
