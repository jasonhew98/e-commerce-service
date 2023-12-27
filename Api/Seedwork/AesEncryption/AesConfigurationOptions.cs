namespace Api.Seedwork.AesEncryption
{
    public class AesConfigurationOptions
    {
        public string Password { get; set; }
        public string Salt { get; set; }
        public int Iterations { get; set; }
        public int KeySize { get; set; }
    }
}
