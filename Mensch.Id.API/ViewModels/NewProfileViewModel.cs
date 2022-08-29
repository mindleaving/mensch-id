using System.Collections.Generic;

namespace Mensch.Id.API.ViewModels
{
    public class NewProfileViewModel
    {
        public NewProfileViewModel(
            List<string> idCandidates)
        {
            IdCandidates = idCandidates;
        }

        public List<string> IdCandidates { get; }
    }
}