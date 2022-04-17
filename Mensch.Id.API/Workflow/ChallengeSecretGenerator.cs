namespace Mensch.Id.API.Workflow
{
    public class ChallengeSecretGenerator
    {
        public static string Generate(int challengeLength = 6)
        {
            var passwordGenerator = new TemporaryPasswordGenerator { AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789" };
            return passwordGenerator.Generate(challengeLength).ToUpper();
        }
    }
}