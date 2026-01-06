
namespace HashcatWrapper
{
    public class ShadowEntry
    {
        public required string Login { get; set; }
        public required string Salt { get; set; }
        public required string HashBase64 { get; set; }
    }
}
