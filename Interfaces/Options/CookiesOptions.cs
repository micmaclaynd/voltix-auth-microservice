namespace Voltix.AuthMicroservice.Interfaces.Options;

public class ICookiesOptions {
    public class ITokenOptions {
        public required string Name { get; set; }
        public required double Expires { get; set; }
        public required string Domain { get; set; }
    }

    public required ITokenOptions Token { get; set; }
}

