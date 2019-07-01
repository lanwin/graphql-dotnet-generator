namespace GraphQLGen
{
    class GenConfig
    {
        public string Schema { get; set; }
        public string GraphQLFile { get; set; }
        public string OutputFile { get; set; }
        public string Namespace { get; set; }
        public string ClientName { get; set; }
        public bool ReadOnly { get; set; } = true;
    }
}