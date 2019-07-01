using System;
using GraphQL.Client;
using Newtonsoft.Json.Linq;
using GraphQL.Common.Request;
using System.Threading.Tasks;

namespace Pokemon.GraphQL.ModelFragments
{
  public interface IPokemonName
  {
    String Name {get;}
  }
}

namespace Pokemon.GraphQL.Model
{
  public class Query
  {
    public Pokemon1 Pokemon {get;}
  }
  public abstract class Pokemon1 : global::Pokemon.GraphQL.ModelFragments.IPokemonName
  {
    public String Name {get;}
    public String Id {get;}
    public String Number {get;}
    public PokemonAttack1 Attacks {get;}
    public System.Collections.Generic.List<Pokemon2> Evolutions {get;}
  }
  public class PokemonAttack1
  {
    public System.Collections.Generic.List<Attack1> Special {get;}
  }
  public class Attack1
  {
    public String Name {get;}
    public String Type {get;}
    public Int64 Damage {get;}
  }
  public class Pokemon2
  {
    public String Id {get;}
    public String Number {get;}
    public String Name {get;}
    public PokemonDimension Weight {get;}
    public PokemonAttack2 Attacks {get;}
  }
  public class PokemonDimension
  {
    public String Minimum {get;}
    public String Maximum {get;}
  }
  public class PokemonAttack2
  {
    public System.Collections.Generic.List<Attack2> Fast {get;}
  }
  public class Attack2
  {
    public String Type {get;}
    public Int64 Damage {get;}
  }
}

namespace Pokemon.GraphQL.ModelOnlyName
{
  public class Query
  {
    public Pokemon Pokemon {get;}
  }
  public class Pokemon : global::Pokemon.GraphQL.ModelFragments.IPokemonName
  {
    public String Name {get;}
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
