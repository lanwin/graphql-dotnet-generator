using System;
using GraphQL.Client;
using Newtonsoft.Json.Linq;
using GraphQL.Common.Request;
using System.Threading.Tasks;

namespace Pokemon.GraphQL.Model.Fragments
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
  public class Pokemon1 : global::Pokemon.GraphQL.Model.Fragments.IPokemonName
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

namespace Pokemon.GraphQL.Model.GetPokemon
{
  public class Query
  {
    public Pokemon Pokemon {get; set;}
  }
  public class Pokemon : global::Pokemon.GraphQL.Model.Fragments.IPokemonName
  {
    public String Name {get; set;}
    public String Id {get; set;}
  }
}

namespace Pokemon.GraphQL.Model.GetPokemons
{
  public class Query
  {
    public System.Collections.Generic.List<Pokemon> Pokemons {get; set;}
  }
  public class Pokemon
  {
    public String Name {get; set;}
  }
}

namespace Pokemon.GraphQL
{
  public class PokemonClient
  {
    readonly GraphQLClient _client;
    public PokemonClient(string url)
    {
      _client = new GraphQLClient(url);
    }
    const string QueryQuery = @"
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
    public async Task<global::Pokemon.GraphQL.Model.Query> Query()
    {
      var response = await _client.PostAsync(new GraphQLRequest()
      {
        OperationName = "",
        Query = QueryQuery
      });
      return ( (JObject)response.Data ).ToObject<global::Pokemon.GraphQL.Model.Query>();
    }
    const string QueryGetPokemon = @"
      fragment pokemonName on Pokemon {
        name
      }
      query GetPokemon($name: String) {
        pokemon(name: $name) {
          id
          ... pokemonName
        }
      }
    ";
    public async Task<global::Pokemon.GraphQL.Model.GetPokemon.Query> GetPokemon(String name)
    {
      var response = await _client.PostAsync(new GraphQLRequest()
      {
        OperationName = "GetPokemon",
        Variables = new {name},
        Query = QueryGetPokemon
      });
      return ( (JObject)response.Data ).ToObject<global::Pokemon.GraphQL.Model.GetPokemon.Query>();
    }
    const string QueryGetPokemons = @"
      query GetPokemons {
        pokemons(first: 10) {
          name
        }
      }
    ";
    public async Task<global::Pokemon.GraphQL.Model.GetPokemons.Query> GetPokemons()
    {
      var response = await _client.PostAsync(new GraphQLRequest()
      {
        OperationName = "GetPokemons",
        Query = QueryGetPokemons
      });
      return ( (JObject)response.Data ).ToObject<global::Pokemon.GraphQL.Model.GetPokemons.Query>();
    }
  }
}
