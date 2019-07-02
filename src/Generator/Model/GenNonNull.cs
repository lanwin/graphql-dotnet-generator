namespace GraphQLGen
{
    class GenNonNull : IGenReference
    {
        public GenNonNull(IGenReference element)
        {
            Element = element;
        }

        public IGenReference Element { get; set; }
        
        public GenSelectionSet GetSelectionSet() => Element.GetSelectionSet();
    }
}