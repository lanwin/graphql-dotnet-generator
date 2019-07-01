namespace GraphQLGen
{
    class GenField
    {
        public string Name { get; set; }
        public GenReference Reference { get; set; }
        public GenSelectionSet SelectionSet { get; set; }
    }
}