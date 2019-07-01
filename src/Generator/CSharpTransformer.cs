using System.Linq;

namespace GraphQLGen
{
    static class CSharpTransformer
    {
        public static void Apply(GenContext context)
        {
            //Todo: Join equal SelectionSets into one

            EnsureSelectionSetsHaveUniqueName(context);

            MakeFragmentsStartWithICauseItsAnInterface(context);
        }

        static void MakeFragmentsStartWithICauseItsAnInterface(GenContext context)
        {
            foreach(var fragment in context.Fragments.SelectionSets)
            {
                var name = char.ToUpperInvariant(fragment.Name[0]) + fragment.Name.Substring(1);
                fragment.Name = "I" + name;
            }
        }

        static void EnsureSelectionSetsHaveUniqueName(GenContext context)
        {
            foreach(var ns in context.Namespaces)
            {
                var douplicates = ns.SelectionSets.GroupBy(g => g.Name)
                    .Where(g => g.Count() > 1)
                    .ToArray();

                foreach(var douplicateGroup in douplicates)
                {
                    var douplicate = douplicateGroup.ToList();
                    foreach(var genType in douplicate)
                        genType.Name += douplicate.IndexOf(genType) + 1;
                }
            }
        }
    }
}