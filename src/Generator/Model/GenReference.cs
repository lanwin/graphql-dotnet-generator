namespace GraphQLGen
{
    class GenReference
    {
        public GenSelectionSet SelectionSet { get; set; }
        public bool IsList { get; set; }

        public override string ToString() => IsList ? "[" + SelectionSet + "]" : SelectionSet.ToString();
    }
}