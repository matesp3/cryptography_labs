
namespace dictionary_attack
{
    public enum CrackType
    {
        NameWithOneUpper,
        Letters6To7,
        DigitsAndLetters4To5
    }

    public static class CrackTypeExtensions
    {
        public static string GetDescription(this CrackType crackType) =>
            crackType switch
            {
                CrackType.NameWithOneUpper => "Name with one uppercase letter",
                CrackType.Letters6To7 => "Common trigram based passwords, length 6-7",
                CrackType.DigitsAndLetters4To5 => "Digits and letters, length 4-5",
                _ => string.Empty
            };
    }

    public class CrackedHash
    {
        public required string Login { get; set; }
        public required string Password { get; set; }

        public required string Source { get; set; }
        public required CrackType UsedCrackType { get; set; }
    }
}
