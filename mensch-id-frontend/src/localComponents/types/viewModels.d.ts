import { Models } from './models.d';
import * as Enums from './enums.d';

export namespace ViewModels {
    interface AssignerAccountViewModel {
        name: string;
        logoUrl: string;
    }

    interface NewProfileViewModel {
        idCandidates: string[];
    }

    interface ProfileViewModel {
        id: string;
        loginProviders: Enums.LoginProvider[];
        verifications: Models.Verification[];
    }

    interface SignedAssignerControlledProfile extends Models.AssignerControlledProfile {
        timestamp: Date;
        signature: string;
    }
}
