using System.Collections.Generic;

namespace Mensch.Id.API.ViewModels
{
    public class NewProfileViewModel
    {
        public NewProfileViewModel(
            List<string> idCandidates,
            List<string> anonymousIdCandidates)
        {
            IdCandidates = idCandidates;
            AnonymousIdCandidates = anonymousIdCandidates;
        }

        public List<string> IdCandidates { get; }
        public List<string> AnonymousIdCandidates { get; }
    }
}