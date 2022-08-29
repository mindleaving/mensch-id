import { Models } from './models.d';
import * as Enums from './enums.d';

export namespace ViewModels {
    interface NewProfileViewModel {
        idCandidates: string[];
    }

    interface ProfileViewModel extends Models.Person {
        loginProviders: Enums.LoginProvider[];
        verifications: Models.Verification[];
    }
}
