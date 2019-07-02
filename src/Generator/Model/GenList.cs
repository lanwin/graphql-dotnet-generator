namespace GraphQLGen
{
    class GenList : IGenReference
    {
        public GenList(IGenReference element)
        {
            Element = element;
        }

        public IGenReference Element { get; set; }
        
        public GenSelectionSet GetSelectionSet() => Element.GetSelectionSet();
    }
}