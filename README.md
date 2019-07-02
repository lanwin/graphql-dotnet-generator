# GraphQL-Dotnet-Generator

Generates statically typed client code for use in dotnet/sharp clients.

Status: Prototype

-----

Given the schema 

https://github.com/lucasbento/graphql-pokemon/blob/master/schemas/schema.graphql

And given the graphql query

```
fragment pokemonName on Pokemon {
  name
}

{
  pokemon(name: "Pikachu") {
    id
    number
    ...pokemonName
    attacks {
      special {
        name
        type
        damage
      }
    }
}
```

It generates a client like this

```c#
var client = new PokemonClient("https://graphql-pokemon.now.sh/graphql");
var data = await client.GetQuery();
var name = (IPokemonName)data.Pokemon;
Console.WriteLine(name.Name);
```

#### Goals:

* input schema (SDL, Url, Json)
* input .graphql file (multiple queries, mutations, fragments)
* output *.cs file or assembly with matching types and a typed client for all queries and mutations
* output is placed beside your graphql file and is specific to it
* plain dotnet (no node or npm required)
* possibly dotnet global tool
* possibly support for https://github.com/prisma/graphql-config so vscode toolings works out of the box with graphql files
* possibly format .cs files with dotnet format after generation
* possibly msbuild task
* get an official graphql-dotnet project

#### What needs to be done before release

* [ ] ensure the obj naming makes sens (is hard to change after release without breaking users)
* [x] impl queries
* [ ] impl mutations
* [x] impl arguments
* [ ] client error handling
* [ ] build time error reporting 
* [ ] readOnly model
* [ ] tests, tests, tests
* [ ] impl dll output via Roslyn compiler so refactoring tools dont mess with the generated code
* [ ] impl multiple queries files
* [ ] impl schema from server url

#### Contributions
If you want to contribute, you are highly welcome.
