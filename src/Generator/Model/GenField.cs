namespace GraphQLGen
{
    class GenField
    {
        public string Name { get; set; }
        public IGenReference Type { get; set; }
        public GenSelectionSet Parent { get; set; }
    }
}